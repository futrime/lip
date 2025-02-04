using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotNet.Globbing;
using Scriban;
using Scriban.Parsing;
using Semver;

namespace Lip;

/// <summary>
/// Represents the package manifest.
/// </summary>
public record PackageManifest
{
    public record AssetType
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
        public List<string>? Urls
        {
            get => _urls;
            init
            {
                value?.ForEach(url =>
                {
                    if (!StringValidator.CheckUrl(url))
                    {
                        throw new SchemaViolationException("urls", $"URL '{url}' is invalid.");
                    }
                });

                _urls = value;
            }
        }

        [JsonPropertyName("place")]
        public List<PlaceType>? Place { get; init; }

        [JsonPropertyName("preserve")]
        public List<string>? Preserve
        {
            get => _preserve;
            init
            {
                value?.ForEach(preserve =>
                {
                    if (!StringValidator.CheckPlaceDestPath(preserve))
                    {
                        throw new SchemaViolationException("preserve", $"Path '{preserve}' is unsafe to preserve.");
                    }
                });

                _preserve = value;
            }
        }

        [JsonPropertyName("remove")]
        public List<string>? Remove
        {
            get => _remove;
            init
            {
                value?.ForEach(remove =>
                {
                    if (!StringValidator.CheckPlaceDestPath(remove))
                    {
                        throw new SchemaViolationException("remove", $"Path '{remove}' is unsafe to remove.");
                    }
                });

                _remove = value;
            }
        }

        private List<string>? _urls;
        private List<string>? _preserve;
        private List<string>? _remove;
    }

    public partial record InfoType
    {
        [JsonPropertyName("name")]
        public string? Name { get; init; }

        [JsonPropertyName("description")]
        public string? Description { get; init; }

        [JsonPropertyName("tags")]
        public List<string>? Tags
        {
            get => _tags;
            init
            {
                value?.ForEach(tag =>
                {
                    if (!StringValidator.CheckTag(tag))
                    {
                        throw new SchemaViolationException("tags", $"Tag '{tag}' is invalid.");
                    }
                });

                _tags = value;
            }
        }

        [JsonPropertyName("avatar_url")]
        public string? AvatarUrl
        {
            get => _avatarUrl;
            init
            {
                if (value is null)
                {
                    _avatarUrl = null;
                    return;
                }

                if (!StringValidator.CheckUrl(value))
                {
                    throw new SchemaViolationException("avatar_url", $"Avatar URL '{value}' is invalid.");
                }

                _avatarUrl = value;
            }
        }

        private List<string>? _tags;
        private string? _avatarUrl;
    }

    public record PlaceType
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
        public required string Dest
        {
            get => _dest;
            init
            {
                if (!StringValidator.CheckPlaceDestPath(value))
                {
                    throw new SchemaViolationException("dest", $"Path '{value}' is unsafe to place.");
                }

                _dest = value;
            }
        }

        private string _dest = string.Empty;
    }

    public partial record ScriptsType
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

        [JsonIgnore]
        public Dictionary<string, List<string>> AdditionalScripts
        {
            get
            {
                var additionalScripts = new Dictionary<string, List<string>>();
                foreach (KeyValuePair<string, JsonElement> kvp in AdditionalProperties ?? [])
                {
                    string key = kvp.Key;
                    JsonElement value = kvp.Value;

                    // Ignore all properties that don't match the script name and value pattern.

                    if (!StringValidator.CheckScriptName(key))
                    {
                        continue;
                    }

                    if (value.ValueKind != JsonValueKind.Array)
                    {
                        continue;
                    }

                    bool allStrings = true;
                    foreach (JsonElement element in value.EnumerateArray())
                    {
                        if (element.ValueKind != JsonValueKind.String)
                        {
                            allStrings = false;
                            break;
                        }
                    }
                    if (!allStrings)
                    {
                        continue;
                    }

                    // The value will always be an array of strings, since we've checked that above.
                    List<string> scripts = value.Deserialize<List<string>>()!;

                    additionalScripts[kvp.Key] = scripts;
                }
                return additionalScripts;
            }
            init
            {
                AdditionalProperties ??= [];

                foreach (KeyValuePair<string, List<string>> kvp in value)
                {
                    if (!StringValidator.CheckScriptName(kvp.Key))
                    {
                        throw new SchemaViolationException(kvp.Key, $"Script name '{kvp.Key}' is invalid.");
                    }

                    AdditionalProperties[kvp.Key] = JsonSerializer.SerializeToElement(kvp.Value);
                }
            }
        }
    }

    public record VariantType
    {
        [JsonIgnore]
        public string VariantLabel => VariantLabelRaw ?? string.Empty;

        [JsonPropertyName("label")]
        public string? VariantLabelRaw { get; init; }

        [JsonPropertyName("platform")]
        public string? Platform { get; init; }

        [JsonPropertyName("dependencies")]
        public Dictionary<string, string>? Dependencies
        {
            get => _dependencies;
            set
            {
                if (value is null)
                {
                    _dependencies = null;
                    return;
                }

                foreach (KeyValuePair<string, string> kvp in value)
                {
                    if (!StringValidator.CheckPackageSpecifierWithoutVersion(kvp.Key))
                    {
                        throw new SchemaViolationException("dependencies", $"Package specifier '{kvp.Key}' is invalid.");
                    }

                    if (!StringValidator.CheckVersionRange(kvp.Value))
                    {
                        throw new SchemaViolationException("dependencies", $"Version range '{kvp.Value}' is invalid.");
                    }
                }

                _dependencies = value;
            }
        }

        [JsonPropertyName("assets")]
        public List<AssetType>? Assets { get; init; }

        [JsonPropertyName("scripts")]
        public ScriptsType? Scripts { get; init; }

        private Dictionary<string, string>? _dependencies;

        public bool Match(string variantLabel, string platform)
        {
            // Check if the variant label matches the specified label.
            bool isVariantLabelMatched = false;

            if (VariantLabel == variantLabel)
            {
                isVariantLabelMatched = true;
            }
            else if (VariantLabel != string.Empty)
            {
                var labelGlob = Glob.Parse(VariantLabel);

                if (labelGlob.IsMatch(variantLabel))
                {
                    isVariantLabelMatched = true;
                }
            }

            if (!isVariantLabelMatched)
            {
                return false;
            }

            // Check if the platform matches the specified platform.
            bool isPlatformMatched = false;

            if (Platform == platform)
            {
                isPlatformMatched = true;
            }
            else if (Platform != string.Empty || Platform is null)
            {
                var platformGlob = Glob.Parse(Platform ?? "*");

                if (platformGlob.IsMatch(platform))
                {
                    isPlatformMatched = true;
                }
            }

            if (!isPlatformMatched)
            {
                return false;
            }

            return true;
        }
    }

    public const int DefaultFormatVersion = 3;
    public const string DefaultFormatUuid = "289f771f-2c9a-4d73-9f3f-8492495a924d";

    private static readonly JsonSerializerOptions s_jsonSerializerOptions = new()
    {
        AllowTrailingCommas = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        IndentSize = 4,
        ReadCommentHandling = JsonCommentHandling.Skip,
        WriteIndented = true,
    };

    [JsonPropertyName("format_version")]
    public required int FormatVersion
    {
        get => DefaultFormatVersion;
        init => _ = value == DefaultFormatVersion ? 0
            : throw new SchemaViolationException("format_version", $"Format version '{value}' is not equal to {DefaultFormatVersion}.");
    }

    [JsonPropertyName("format_uuid")]
    public required string FormatUuid
    {
        get => DefaultFormatUuid;
        init => _ = value == DefaultFormatUuid ? 0
            : throw new SchemaViolationException("format_uuid", $"Format UUID '{value}' is not equal to {DefaultFormatUuid}.");
    }

    [JsonPropertyName("tooth")]
    public required string ToothPath { get; init; }

    [JsonIgnore]
    public SemVersion Version => SemVersion.Parse(VersionText);

    [JsonPropertyName("version")]
    public required string VersionText
    {
        get
        {
            return _version;
        }
        init
        {
            if (!StringValidator.CheckVersion(value))
            {
                throw new SchemaViolationException("version", $"Version '{value}' is invalid.");
            }

            _version = value;
        }
    }

    [JsonPropertyName("info")]
    public InfoType? Info { get; init; }

    [JsonPropertyName("variants")]
    public List<VariantType>? Variants { get; init; }

    private string _version = "0.0.0"; // The default value does never get used.

    public static PackageManifest FromJsonBytesParsed(byte[] bytes)
    {
        return FromJsonBytesWithTemplate(bytes).WithTemplateParsed();
    }

    /// <summary>
    /// Deserializes a package manifest from the specified byte array.
    /// </summary>
    /// <param name="bytes">The byte array to deserialize.</param>
    /// <returns>The deserialized package manifest.</returns>
    public static PackageManifest FromJsonBytesWithTemplate(byte[] bytes)
    {
        try
        {
            return JsonSerializer.Deserialize<PackageManifest>(bytes, s_jsonSerializerOptions)
                ?? throw new JsonException("JSON bytes deserialized to null.");
        }
        catch (Exception ex) when (ex is JsonException)
        {
            throw new JsonException("Package manifest bytes deserialization failed.", ex);
        }
    }

    /// <summary>
    /// Gets the specified variant.
    /// </summary>
    /// <param name="variantLabel">The label of the variant to specify.</param>
    /// <param name="platform">The runtime identifier of the variant to specify.</param>
    /// <returns></returns>
    public VariantType? GetSpecifiedVariant(string variantLabel, string platform)
    {
        // Find the variant that matches the specified label and platform.
        List<VariantType> matchedVariants = Variants?
            .Where(variant => variant.Match(variantLabel, platform))
            .ToList() ?? [];

        // However, there must exist at least one variant that matches the specified label and platform without any wildcards.
        if (!matchedVariants.Any(
            variant => variant.VariantLabel == variantLabel))
        {
            return null;
        }

        if (!matchedVariants.Any(variant => variant.Platform == platform))
        {
            return null;
        }

        // Merge all matched variants into a single variant.
        VariantType mergedVariant = new()
        {
            VariantLabelRaw = variantLabel,
            Platform = platform,
            Dependencies = matchedVariants
                .SelectMany(variant => variant.Dependencies ?? [])
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            Assets = [.. matchedVariants.SelectMany(variant => variant.Assets ?? [])],
            Scripts = new ScriptsType
            {
                PreInstall = matchedVariants
                    .LastOrDefault(variant => variant.Scripts?.PreInstall is not null)?.Scripts!.PreInstall,
                Install = matchedVariants
                    .LastOrDefault(variant => variant.Scripts?.Install is not null)?.Scripts!.Install,
                PostInstall = matchedVariants
                    .LastOrDefault(variant => variant.Scripts?.PostInstall is not null)?.Scripts!.PostInstall,
                PrePack = matchedVariants
                    .LastOrDefault(variant => variant.Scripts?.PrePack is not null)?.Scripts!.PrePack,
                PostPack = matchedVariants
                    .LastOrDefault(variant => variant.Scripts?.PostPack is not null)?.Scripts!.PostPack,
                PreUninstall = matchedVariants
                    .LastOrDefault(variant => variant.Scripts?.PreUninstall is not null)?.Scripts!.PreUninstall,
                Uninstall = matchedVariants
                    .LastOrDefault(variant => variant.Scripts?.Uninstall is not null)?.Scripts!.Uninstall,
                PostUninstall = matchedVariants
                    .LastOrDefault(variant => variant.Scripts?.PostUninstall is not null)?.Scripts!.PostUninstall,
                AdditionalProperties = matchedVariants
                    .SelectMany(variant => variant.Scripts?.AdditionalProperties ?? [])
                    .GroupBy(kvp => kvp.Key)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Last().Value)
            }
        };

        return mergedVariant;
    }

    /// <summary>
    /// Serializes the package manifest to a byte array.
    /// </summary>
    /// <returns>The serialized package manifest.</returns>
    public byte[] ToJsonBytes()
    {
        return JsonSerializer.SerializeToUtf8Bytes(this, s_jsonSerializerOptions);
    }

    /// <summary>
    /// Serializes the package manifest to a JSON element.
    /// </summary>
    /// <returns>The serialized package manifest.</returns>
    public JsonElement ToJsonElement()
    {
        return JsonSerializer.SerializeToElement(this, s_jsonSerializerOptions);
    }

    /// <summary>
    /// Parses the template and renders the package manifest.
    /// </summary>
    /// <returns>The rendered package manifest.</returns>
    public PackageManifest WithTemplateParsed()
    {
        string templateText = Encoding.UTF8.GetString(ToJsonBytes());
        Template template = Template.Parse(templateText);

        if (template.HasErrors)
        {
            StringBuilder sb = new();
            foreach (LogMessage message in template.Messages)
            {
                sb.Append(message.ToString());
            }
            throw new FormatException($"Failed to parse template: {sb}");
        }

        JsonElement json = ToJsonElement();

        string renderedText = template.Render(json);

        return FromJsonBytesWithTemplate(Encoding.UTF8.GetBytes(renderedText));
    }
}
