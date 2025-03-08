using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Lip.Migration;

public static class MigratorFromV2
{
    public static bool IsMigratable(JsonElement json)
    {
        try
        {
            return json.GetProperty("format_version").GetInt64() == 2;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static JsonElement Migrate(JsonElement json)
    {
        // Replace `$(xxx)` with `{{xxx}}` to fit Scriban template syntax with Regex.
        Regex regex = new(@"\$\(([^)]+?)\)");
        string jsonString = regex.Replace(json.GetRawText(), "{{$1}}");

        ManifestV2 manifestV2 = JsonSerializer.Deserialize<ManifestV2>(jsonString)!;

        Manifest manifest = new()
        {
            FormatVersion = 3,
            FormatUuid = "289f771f-2c9a-4d73-9f3f-8492495a924d",
            Tooth = manifestV2.Tooth,
            Version = manifestV2.Version,
            Info = new Manifest.InfoType
            {
                Name = manifestV2.Info.Name,
                Description = manifestV2.Info.Description,
                Tags = manifestV2.Info.Tags,
                AvatarUrl = manifestV2.Info.AvatarUrl
            },
            Variants =
            [
                // If manifest v2 has platforms, use properties from platforms firstly, if platforms don't have some properties, obtain them from global.
                // If platforms not exist, generating a variant with server's architecture and properties from global.
                .. manifestV2.Platforms?
                       .Select(p => new Manifest.Variant
                       {
                           Platform =
                               ConvertGOARCHAndGOOSToPlatform(
                                   p.GOARCH, p.GOOS),
                           Dependencies = p.Dependencies ?? manifestV2.Dependencies,
                           Assets = p.AssetUrl is not null
                               ?
                               [
                                   new Manifest.Asset
                                   {
                                       Type = p.AssetUrl.Split('.').Last() switch
                                       {
                                           "tar" => Manifest.Asset.TypeEnum.Tar,
                                           "tgz" => Manifest.Asset.TypeEnum.Tgz,
                                           "gz" => Manifest.Asset.TypeEnum.Tgz,
                                           "zip" => Manifest.Asset.TypeEnum.Zip,
                                           _ => Manifest.Asset.TypeEnum.Uncompressed
                                       },
                                       Urls =
                                       [
                                           p.AssetUrl
                                       ],
                                       Placements =
                                           p.Files?.Place?.Select(ConvertPlaceToPlacement)
                                               .ToList() ?? manifestV2.Files?.Place?.Select(ConvertPlaceToPlacement)
                                               .ToList(),
                                   }
                               ]
                               : manifestV2.AssetUrl is not null
                                   ?
                                   [
                                       new Manifest.Asset
                                       {
                                           Type = manifestV2.AssetUrl.Split('.').Last() switch
                                           {
                                               "tar" => Manifest.Asset.TypeEnum.Tar,
                                               "tgz" => Manifest.Asset.TypeEnum.Tgz,
                                               "gz" => Manifest.Asset.TypeEnum.Tgz,
                                               "zip" => Manifest.Asset.TypeEnum.Zip,
                                               _ => Manifest.Asset.TypeEnum.Uncompressed
                                           },
                                           Urls =
                                           [
                                               manifestV2.AssetUrl
                                           ],
                                           Placements = manifestV2.Files?.Place?.Select(ConvertPlaceToPlacement)
                                               .ToList()
                                       }
                                   ]
                                   : null,
                           PreserveFiles = p.Files?.Preserve ?? manifestV2.Files?.Preserve,
                           RemoveFiles = p.Files?.Remove ?? manifestV2.Files?.Remove,
                           Scripts = p.Commands is not null
                               ? ConvertCommandsToScripts(p.Commands)
                               : manifestV2.Commands is not null
                                   ? ConvertCommandsToScripts(manifestV2.Commands)
                                   : null
                       }) ??
                   [
                       new Manifest.Variant
                       {
                           Dependencies = manifestV2.Dependencies,
                           Assets = manifestV2.AssetUrl is not null
                               ?
                               [
                                   new Manifest.Asset
                                   {
                                       Type = manifestV2.AssetUrl.Split('.').Last() switch
                                       {
                                           "tar" => Manifest.Asset.TypeEnum.Tar,
                                           "tgz" => Manifest.Asset.TypeEnum.Tgz,
                                           "gz" => Manifest.Asset.TypeEnum.Tgz,
                                           "zip" => Manifest.Asset.TypeEnum.Zip,
                                           _ => Manifest.Asset.TypeEnum.Uncompressed
                                       },
                                       Urls =
                                       [
                                           manifestV2.AssetUrl
                                       ],
                                       Placements = manifestV2.Files?.Place?.Select(ConvertPlaceToPlacement)
                                           .ToList()
                                   }
                               ]
                               :
                               [
                                   new Manifest.Asset
                                   {
                                       Type = Manifest.Asset.TypeEnum.Self,
                                       Placements = manifestV2.Files?.Place?.Select(ConvertPlaceToPlacement)
                                           .ToList()
                                   }
                               ],
                           PreserveFiles = manifestV2.Files?.Preserve,
                           RemoveFiles = manifestV2.Files?.Remove,
                           Scripts = manifestV2.Commands is not null
                               ? ConvertCommandsToScripts(manifestV2.Commands)
                               : null
                       }
                   ]
            ]
        };

        // For satisfy one non-globbed variant must exist rule// For satisfy one non-globbed variant must exist rule
        if (manifest.Variants.Any(variant =>
                !string.IsNullOrEmpty(variant.Platform) &&
                manifest.Variants.Any(s => s.Platform is not null && s.Platform.Contains('*'))))
        {
            manifest.Variants.Insert(0, new Manifest.Variant { Platform = RuntimeInformation.RuntimeIdentifier });
        }

        return JsonSerializer.SerializeToElement(manifest);
    }

    private static Manifest.ScriptsType ConvertCommandsToScripts(ManifestV2.CommandsType commands)
    {
        return new Manifest.ScriptsType
        {
            PreInstall = commands.PreInstall,
            PostInstall = commands.PostInstall,
            PreUninstall = commands.PreUninstall,
            PostUninstall = commands.PostUninstall
        };
    }

    private static string ConvertGOARCHAndGOOSToPlatform(string? goarch, string goos)
    {
        string prefix = goos switch
        {
            "windows" => "win",
            "darwin" => "osx",
            "linux" => "linux",
            "ios" => "ios",
            "android" => "android",
            _ => throw new PlatformNotSupportedException()
        };

        string suffix = goarch switch
        {
            "amd64" => "x64",
            "386" => "x86",
            "arm64" => "arm64",
            "arm" => "arm",
            "loong64" => "loongarch64",
            null => "*",
            _ => throw new PlatformNotSupportedException()
        };

        return $"{prefix}-{suffix}";
    }

    private static Manifest.Placement ConvertPlaceToPlacement(ManifestV2.PlaceType places)
    {
        return new Manifest.Placement
        {
            Src = places.Src.TrimEnd('*'),
            Dest = places.Dest,
            // If the source ends with '*', treat it as a directory; otherwise as a file.
            Type = places.Src.EndsWith('*')
                ? Manifest.Placement.TypeEnum.Dir
                : Manifest.Placement.TypeEnum.File
        };
    }
}
