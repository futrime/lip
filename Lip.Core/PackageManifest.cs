using DotNet.Globbing;
using Flurl;
using Lip.Migration;
using Scriban;
using Semver;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lip.Core;

/// <summary>
/// Represents the package manifest.
/// </summary>
public record PackageManifest
{
    public record Asset
    {
        public enum TypeEnum
        {
            Self,
            Tar,
            Tgz,
            Uncompressed,
            Zip,
        }

        public required TypeEnum Type { get; init; }

        public required List<Url> Urls { get; init; }

        public required List<Placement> Placements { get; init; }
    }

    public record InfoType
    {
        public required string Name { get; init; }

        public required string Description { get; init; }

        public required List<string> Tags
        {
            get => _tags;
            init => _tags = value.ConvertAll(tag => StringValidator.CheckTag(tag)
                ? tag
                : throw new SchemaViolationException("info.tags[]", $"Tag '{tag}' is invalid."));
        }
        private readonly List<string> _tags = [];

        public required Url AvatarUrl { get; init; }
    }

    public record Placement
    {
        public enum TypeEnum
        {
            File,
            Dir,
        }

        public required TypeEnum Type { get; init; }

        public required string Src { get; init; }

        public required string Dest
        {
            get => _dest;
            init => _dest = StringValidator.CheckPlaceDestPath(value)
                ? value
                : throw new SchemaViolationException(
                    "variants[].assets[].placements[].dest",
                    $"Path '{value}' is unsafe to place.");
        }
        private readonly string _dest = string.Empty;
    }

    public record ScriptsType
    {
        public required List<string> PreInstall { get; init; }

        public required List<string> Install { get; init; }

        public required List<string> PostInstall { get; init; }

        public required List<string> PrePack { get; init; }

        public required List<string> PostPack { get; init; }

        public required List<string> PreUninstall { get; init; }

        public required List<string> Uninstall { get; init; }

        public required List<string> PostUninstall { get; init; }

        public required Dictionary<string, List<string>> AdditionalScripts
        {
            get => _additionalScripts;
            init => _additionalScripts = value.ToDictionary(
                kvp => StringValidator.CheckScriptName(kvp.Key)
                    ? kvp.Key
                    : throw new SchemaViolationException(
                        $"variants[].assets[].scripts.'{kvp.Key}'",
                        $"Invalid script name '{kvp.Key}'"
                    ),
                kvp => kvp.Value
            );
        }
        private readonly Dictionary<string, List<string>> _additionalScripts = [];
    }

    public record Variant
    {
        // We do not validate label because it may be a glob.
        public required string Label { get; init; }

        public required string Platform { get; init; }

        public required Dictionary<PackageIdentifier, SemVersionRange> Dependencies { get; init; }

        public required List<Asset> Assets { get; init; }

        public required List<string> PreserveFiles
        {
            get => _preserveFiles;
            init => _preserveFiles = value.ConvertAll(
                preserveFile => StringValidator.CheckPlaceDestPath(preserveFile)
                    ? preserveFile
                    : throw new SchemaViolationException(
                        "variants[].preserve_files[]",
                        $"Invalid preserve file path '{preserveFile}'"
                    )
            );
        }
        private readonly List<string> _preserveFiles = [];

        public required List<string> RemoveFiles
        {
            get => _removeFiles;
            init => _removeFiles = value.ConvertAll(
                removeFile => StringValidator.CheckPlaceDestPath(removeFile)
                    ? removeFile
                    : throw new SchemaViolationException(
                        "variants[].remove_file[]",
                        $"Invalid remove file path '{removeFile}'"
                    )
            );
        }
        private readonly List<string> _removeFiles = [];

        public required ScriptsType Scripts { get; init; }

        public bool Match(string targetLabel, string targetPlatform)
        {
            // Check if the variant label matches the specified label.
            if (Label != targetLabel)
            {
                if (Label == string.Empty)
                {
                    return false;
                }

                if (!Glob.Parse(Label).IsMatch(targetLabel))
                {
                    return false;
                }
            }

            // Check if the platform matches the specified platform.
            if (Platform != targetPlatform)
            {
                if (Platform == string.Empty)
                {
                    return false;
                }

                if (!Glob.Parse(Platform).IsMatch(targetPlatform))
                {
                    return false;
                }
            }

            return true;
        }
    }

    public const int DefaultFormatVersion = 3;
    public const string DefaultFormatUuid = "289f771f-2c9a-4d73-9f3f-8492495a924d";

    private static readonly JsonDocumentOptions _jsonDocumentOptions = new()
    {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip,
    };

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        AllowTrailingCommas = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        IndentSize = 4,
        ReadCommentHandling = JsonCommentHandling.Skip,
        WriteIndented = true,
    };

    private static readonly JsonWriterOptions _jsonWriterOptions = new()
    {
        Indented = true,
        IndentSize = 4,
    };

    public required string ToothPath
    {
        get => _toothPath;
        init => _toothPath = StringValidator.CheckToothPath(value)
            ? value
            : throw new SchemaViolationException(
                "tooth",
                $"Invalid tooth path '{value}'"
            );
    }
    private readonly string _toothPath = string.Empty;

    public required SemVersion Version { get; init; }

    public required InfoType Info { get; init; }

    public required List<Variant> Variants { get; init; }

    [ExcludeFromCodeCoverage] // TODO: Add unit tests for this method.
    public static PackageManifest FromJsonElement(JsonElement jsonElement)
    {
        RawPackageManifest rawPackageManifest = RawPackageManifest.FromJsonElement(jsonElement).WithTemplateRendered();

        // Validate format version and UUID.

        if (rawPackageManifest.FormatVersion != DefaultFormatVersion)
        {
            throw new SchemaViolationException(
                "format_version",
                $"Expected format version {DefaultFormatVersion}, but got {rawPackageManifest.FormatVersion}.");
        }

        if (rawPackageManifest.FormatUuid != DefaultFormatUuid)
        {
            throw new SchemaViolationException(
                "format_uuid",
                $"Expected format UUID '{DefaultFormatUuid}', but got '{rawPackageManifest.FormatUuid}'.");
        }

        PackageManifest packageManifest = new()
        {
            ToothPath = rawPackageManifest.Tooth,
            Version = SemVersion.Parse(rawPackageManifest.Version),
            Info = new InfoType
            {
                Name = rawPackageManifest.Info?.Name ?? "",
                Description = rawPackageManifest.Info?.Description ?? "",
                Tags = rawPackageManifest.Info?.Tags ?? [],
                AvatarUrl = Url.Parse(rawPackageManifest.Info?.AvatarUrl ?? "")
            },
            Variants = rawPackageManifest.Variants?.ConvertAll(variant => new Variant
            {
                Label = variant.Label ?? "",
                Platform = variant.Platform ?? RuntimeInformation.RuntimeIdentifier,
                Dependencies = variant.Dependencies?.ToDictionary(
                    kvp => PackageIdentifier.Parse(kvp.Key),
                    kvp => SemVersionRange.ParseNpm(kvp.Value)
                ) ?? [],
                Assets = variant.Assets?.ConvertAll(asset => new Asset
                {
                    Type = (Asset.TypeEnum)asset.Type,
                    Urls = asset.Urls?.ConvertAll(url => Url.Parse(url)) ?? [],
                    Placements = asset.Placements?.ConvertAll(placement => new Placement
                    {
                        Type = (Placement.TypeEnum)placement.Type,
                        Src = placement.Src,
                        Dest = placement.Dest
                    }) ?? [],
                }) ?? [],
                PreserveFiles = variant.PreserveFiles ?? [],
                RemoveFiles = variant.RemoveFiles ?? [],
                Scripts = new ScriptsType
                {
                    PreInstall = variant.Scripts?.PreInstall ?? [],
                    Install = variant.Scripts?.Install ?? [],
                    PostInstall = variant.Scripts?.PostInstall ?? [],
                    PrePack = variant.Scripts?.PrePack ?? [],
                    PostPack = variant.Scripts?.PostPack ?? [],
                    PreUninstall = variant.Scripts?.PreUninstall ?? [],
                    Uninstall = variant.Scripts?.Uninstall ?? [],
                    PostUninstall = variant.Scripts?.PostUninstall ?? [],
                    AdditionalScripts = variant.Scripts?.AdditionalProperties?.ToDictionary(
                        kvp => kvp.Key,
                        kvp => (kvp.Value.ValueKind == JsonValueKind.Array
                                && kvp.Value.EnumerateArray()
                                    .All(elem => elem.ValueKind == JsonValueKind.String))
                            ? kvp.Value.Deserialize<List<string>>()!
                            : throw new SchemaViolationException(
                                $"variants[].assets[].scripts.'{kvp.Key}'",
                                $"Invalid script list"
                            )
                    ) ?? []
                }
            }) ?? []
        };

        return packageManifest;
    }

    public static async Task<PackageManifest> FromStream(Stream stream)
    {
        JsonElement jsonElement = (await JsonDocument.ParseAsync(
            stream,
            _jsonDocumentOptions)).RootElement;

        // When reading from a stream, the JSON may follow an older format. So we need to migrate
        // it.
        jsonElement = Migrator.Migrate(jsonElement);

        return FromJsonElement(jsonElement);
    }

    /// <summary>
    /// Gets the specified variant.
    /// </summary>
    /// <param name="targetLabel">The label of the variant to specify.</param>
    /// <param name="targetPlatform">The runtime identifier of the variant to specify.</param>
    /// <returns></returns>
    public Variant? GetVariant(string targetLabel, string targetPlatform)
    {
        // Find the variant that matches the specified label and platform.
        List<Variant> matchedVariants = [.. Variants.Where(variant => variant.Match(targetLabel, targetPlatform))];

        // There must be at least one variant fully matched.
        if (!matchedVariants.Any(
            variant => variant.Label == targetLabel
                       && variant.Platform == targetPlatform))
        {
            return null;
        }

        // Merge all matched variants into a single variant.
        Variant mergedVariant = new()
        {
            Label = targetLabel,
            Platform = targetPlatform,
            Dependencies = matchedVariants
                .SelectMany(variant => variant.Dependencies)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            Assets = [.. matchedVariants.SelectMany(variant => variant.Assets)],
            PreserveFiles = [.. matchedVariants.SelectMany(variant => variant.PreserveFiles)],
            RemoveFiles = [.. matchedVariants.SelectMany(Variant => Variant.RemoveFiles)],
            Scripts = new ScriptsType
            {
                PreInstall = matchedVariants.Last().Scripts.PreInstall,
                Install = matchedVariants.Last().Scripts.Install,
                PostInstall = matchedVariants.Last().Scripts.PostInstall,
                PrePack = matchedVariants.Last().Scripts.PrePack,
                PostPack = matchedVariants.Last().Scripts.PostPack,
                PreUninstall = matchedVariants.Last().Scripts.PreUninstall,
                Uninstall = matchedVariants.Last().Scripts.Uninstall,
                PostUninstall = matchedVariants.Last().Scripts.PostUninstall,
                AdditionalScripts = matchedVariants
                    .SelectMany(variant => variant.Scripts.AdditionalScripts)
                    .GroupBy(kvp => kvp.Key)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Last().Value)
            }
        };

        return mergedVariant;
    }

    public JsonElement ToJsonElement()
    {
        RawPackageManifest rawPackageManifest = new()
        {
            FormatVersion = DefaultFormatVersion,
            FormatUuid = DefaultFormatUuid,
            Tooth = ToothPath,
            Version = Version.ToString(),
            Info = new()
            {
                Name = Info.Name,
                Description = Info.Description,
                Tags = Info.Tags,
                AvatarUrl = Info.AvatarUrl
            },
            Variants = Variants.ConvertAll(variant => new RawPackageManifest.Variant
            {
                Label = variant.Label,
                Platform = variant.Platform,
                Dependencies = variant.Dependencies.ToDictionary(
                    kvp => kvp.Key.ToString(),
                    kvp => kvp.Value.ToString()),
                Assets = variant.Assets.ConvertAll(asset => new RawPackageManifest.Asset
                {
                    Type = (RawPackageManifest.Asset.TypeEnum)asset.Type,
                    Urls = asset.Urls.ConvertAll(url => url.ToString()),
                    Placements = asset.Placements.ConvertAll(placement
                        => new RawPackageManifest.Placement
                        {
                            Type = (RawPackageManifest.Placement.TypeEnum)placement.Type,
                            Src = placement.Src,
                            Dest = placement.Dest,
                        })
                }),
                PreserveFiles = variant.PreserveFiles,
                RemoveFiles = variant.RemoveFiles,
                Scripts = new RawPackageManifest.ScriptsType
                {
                    PreInstall = variant.Scripts.PreInstall,
                    Install = variant.Scripts.Install,
                    PostInstall = variant.Scripts.PostInstall,
                    PrePack = variant.Scripts.PrePack,
                    PostPack = variant.Scripts.PostPack,
                    PreUninstall = variant.Scripts.PreUninstall,
                    Uninstall = variant.Scripts.Uninstall,
                    PostUninstall = variant.Scripts.PostUninstall,
                    AdditionalProperties = variant.Scripts.AdditionalScripts.ToDictionary(
                        kvp => kvp.Key,
                        kvp => JsonSerializer.SerializeToElement(kvp.Value, _jsonSerializerOptions)
                    )
                }
            })
        };

        return rawPackageManifest.ToJsonElement();
    }

    public async Task ToStream(Stream stream)
    {
        JsonElement jsonElement = ToJsonElement();
        await using Utf8JsonWriter jsonWriter = new(stream, _jsonWriterOptions);

        jsonElement.WriteTo(jsonWriter);
    }
}

[ExcludeFromCodeCoverage]
file record RawPackageManifest
{
    public record Asset
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum TypeEnum
        {
            [JsonStringEnumMemberName("self")]
            Self,
            [JsonStringEnumMemberName("tar")]
            Tar,
            [JsonStringEnumMemberName("tgz")]
            Tgz,
            [JsonStringEnumMemberName("uncompressed")]
            Uncompressed,
            [JsonStringEnumMemberName("zip")]
            Zip,
        }

        [JsonPropertyName("type")]
        public required TypeEnum Type { get; init; }

        [JsonPropertyName("urls")]
        public List<string>? Urls { get; init; }

        [JsonPropertyName("placements")]
        public List<Placement>? Placements { get; init; }
    }

    public record InfoType
    {
        [JsonPropertyName("name")]
        public string? Name { get; init; }

        [JsonPropertyName("description")]
        public string? Description { get; init; }

        [JsonPropertyName("tags")]
        public List<string>? Tags { get; init; }

        [JsonPropertyName("avatar_url")]
        public string? AvatarUrl { get; init; }
    }

    public record Placement
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum TypeEnum
        {
            [JsonStringEnumMemberName("file")]
            File,
            [JsonStringEnumMemberName("dir")]
            Dir,
        }

        [JsonPropertyName("type")]
        public required TypeEnum Type { get; init; }

        [JsonPropertyName("src")]
        public required string Src { get; init; }

        [JsonPropertyName("dest")]
        public required string Dest { get; init; }
    }

    public record ScriptsType
    {
        [JsonPropertyName("pre_install")]
        public List<string>? PreInstall { get; init; }

        [JsonPropertyName("install")]
        public List<string>? Install { get; init; }

        [JsonPropertyName("post_install")]
        public List<string>? PostInstall { get; init; }

        [JsonPropertyName("pre_pack")]
        public List<string>? PrePack { get; init; }

        [JsonPropertyName("post_pack")]
        public List<string>? PostPack { get; init; }

        [JsonPropertyName("pre_uninstall")]
        public List<string>? PreUninstall { get; init; }

        [JsonPropertyName("uninstall")]
        public List<string>? Uninstall { get; init; }

        [JsonPropertyName("post_uninstall")]
        public List<string>? PostUninstall { get; init; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? AdditionalProperties { get; init; }
    }

    public record Variant
    {
        [JsonPropertyName("label")]
        public string? Label { get; init; }

        [JsonPropertyName("platform")]
        public string? Platform { get; init; }

        [JsonPropertyName("dependencies")]
        public Dictionary<string, string>? Dependencies { get; init; }

        [JsonPropertyName("assets")]
        public List<Asset>? Assets { get; init; }

        [JsonPropertyName("preserve_files")]
        public List<string>? PreserveFiles { get; init; }

        [JsonPropertyName("remove_files")]
        public List<string>? RemoveFiles { get; init; }

        [JsonPropertyName("scripts")]
        public ScriptsType? Scripts { get; init; }
    }

    [JsonPropertyName("format_version")]
    public required int FormatVersion { get; init; }

    [JsonPropertyName("format_uuid")]
    public required string FormatUuid { get; init; }

    [JsonPropertyName("tooth")]
    public required string Tooth { get; init; }

    [JsonPropertyName("version")]
    public required string Version { get; init; }

    [JsonPropertyName("info")]
    public InfoType? Info { get; init; }

    [JsonPropertyName("variants")]
    public List<Variant>? Variants { get; init; }

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        AllowTrailingCommas = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        IndentSize = 4,
        ReadCommentHandling = JsonCommentHandling.Skip,
        WriteIndented = true,
    };

    public static RawPackageManifest FromJsonElement(JsonElement jsonElement)
    {
        return JsonSerializer.Deserialize<RawPackageManifest>(
            jsonElement,
            _jsonSerializerOptions)
            ?? throw new SchemaViolationException("", "JSON bytes deserialized to null.");
    }

    public JsonElement ToJsonElement()
    {
        return JsonSerializer.SerializeToElement(this, _jsonSerializerOptions);
    }

    public RawPackageManifest WithTemplateRendered()
    {
        string jsonText = JsonSerializer.Serialize(this);

        Template template = Template.Parse(jsonText);

        JsonElement jsonElement = ToJsonElement();

        string jsonTextRendered = template.Render(jsonElement);

        JsonElement jsonElementRendered = JsonDocument.Parse(jsonTextRendered).RootElement;

        return FromJsonElement(jsonElementRendered);
    }
}
