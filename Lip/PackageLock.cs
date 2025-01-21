using System.Text.Json;
using System.Text.Json.Serialization;
using Semver;

namespace Lip;

public record PackageLock
{
    public record LockType
    {
        [JsonPropertyName("tooth")]
        public string ToothPath
        {
            get => _tooth;
            init => _tooth = StringValidator.CheckToothPath(value)
                ? value
                : throw new SchemaViolationException("tooth", $"Invalid tooth path '{value}'.");
        }

        [JsonPropertyName("variant")]
        public string VariantLabel
        {
            get => _variant;
            init => _variant = StringValidator.CheckVariantLabel(value)
                ? value
                : throw new SchemaViolationException("variant", $"Invalid variant label '{value}'.");
        }

        [JsonIgnore]
        public SemVersion Version => SemVersion.Parse(VersionText);

        [JsonPropertyName("version")]
        public string VersionText
        {
            get => _version;
            init => _version = StringValidator.CheckVersion(value)
                ? value
                : throw new SchemaViolationException("version", $"Invalid version '{value}'.");
        }

        private string _tooth = "";
        private string _variant = "";
        private string _version = "";
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

    [JsonPropertyName("packages")]
    public required List<PackageManifest> Packages { get; init; }

    [JsonPropertyName("locks")]
    public required List<LockType> Locks { get; init; }

    public static PackageLock FromJsonBytes(byte[] bytes)
    {
        try
        {
            return JsonSerializer.Deserialize<PackageLock>(bytes, s_jsonSerializerOptions)
                ?? throw new JsonException("JSON bytes deserialized to null.");
        }
        catch (Exception ex) when (ex is JsonException || ex is SchemaViolationException)
        {
            throw new JsonException("Package lock bytes deserialization failed.", ex);
        }
    }

    public byte[] ToJsonBytes()
    {
        return JsonSerializer.SerializeToUtf8Bytes(this, s_jsonSerializerOptions);
    }
}
