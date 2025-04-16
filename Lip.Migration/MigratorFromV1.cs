using System.Text.Json;

namespace Lip.Migration;

public static class MigratorFromV1
{
    public static bool IsMigratable(JsonElement json)
    {
        try
        {
            return json.GetProperty("format_version").GetInt64() == 1;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static JsonElement Migrate(JsonElement json)
    {
        var manifestV1 = json.Deserialize<ManifestV1>()
            ?? throw new JsonException("Failed to deserialize the obsolete manifest.");

        var info = new ManifestV2.InfoType
        {
            Name = manifestV1.Information?.Data != null && manifestV1.Information.Data.TryGetValue("name", out var n)
            ? n.GetString() ?? ""
            : "",
            Description = manifestV1.Information?.Data != null && manifestV1.Information.Data.TryGetValue("description", out var d)
            ? d.GetString() ?? ""
            : "",
            Author = manifestV1.Information?.Data != null && manifestV1.Information.Data.TryGetValue("author", out var a)
            ? a.GetString() ?? ""
            : "",
            Tags = manifestV1.Information?.Data != null && manifestV1.Information.Data.TryGetValue("tags", out var t) && t.ValueKind == JsonValueKind.Array
            ? [.. t.EnumerateArray().Select(e => e.GetString() ?? "")]
            : []
        };

        static ManifestV2.CommandsType? ConvertCommands(List<ManifestV1.Command>? commands)
        {
            if (commands == null)
                return null;

            var result = new ManifestV2.CommandsType();
            foreach (var cmd in commands)
            {
                if (cmd.Type.Equals("install", StringComparison.OrdinalIgnoreCase))
                {
                    result.PostInstall ??= [];
                    result.PostInstall.AddRange(cmd.Commands);
                }
                else if (cmd.Type.Equals("uninstall", StringComparison.OrdinalIgnoreCase))
                {
                    result.PostUninstall ??= [];
                    result.PostUninstall.AddRange(cmd.Commands);
                }
            }
            return (result.PostInstall == null && result.PostUninstall == null) ? null : result;
        }

        var manifestV2 = new ManifestV2
        {
            FormatVersion = manifestV1.FormatVersion,
            Tooth = manifestV1.Tooth,
            Version = manifestV1.Version,
            Info = info,
            AssetUrl = null,
            Commands = ConvertCommands(manifestV1.Commands),
            Dependencies = manifestV1.Dependencies?.ToDictionary(
            kvp => kvp.Key,
            kvp => string.Join(" || ", kvp.Value.Select(group => string.Join(" && ", group)))
            ),
            Prerequisites = null,
            Files = manifestV1.Placement != null
            ? new ManifestV2.FilesType
            {
                Place = [.. manifestV1.Placement.Select(p => new ManifestV2.PlaceType { Src = p.Source, Dest = p.Destination.TrimEnd('*') })]
            }
            : null,
            Platforms = null // No conversion for platforms from ManifestV1
        };

        return MigratorFromV2.Migrate(JsonSerializer.SerializeToElement(manifestV2));
    }
}