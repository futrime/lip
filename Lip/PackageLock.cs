using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lip;

public record PackageLock
{
    public record LockType
    {
        [JsonPropertyName("package")]
        public required string Package { get; init; }

        [JsonPropertyName("variant")]
        public required string Variant { get; init; }

        [JsonPropertyName("version")]
        public required string Version { get; init; }
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
            : throw new ArgumentException($"Format version '{value}' is not equal to {DefaultFormatVersion}.", nameof(value));
    }

    [JsonPropertyName("format_uuid")]
    public required string FormatUuid
    {
        get => DefaultFormatUuid;
        init => _ = value == DefaultFormatUuid ? 0
            : throw new ArgumentException($"Format UUID '{value}' is not equal to {DefaultFormatUuid}.", nameof(value));
    }

    [JsonPropertyName("packages")]
    public required List<PackageManifest> Packages { get; init; }

    [JsonPropertyName("locks")]
    public required List<LockType> Locks { get; init; }

    public static PackageLock FromBytes(byte[] bytes)
    {
        return JsonSerializer.Deserialize<PackageLock>(bytes, s_jsonSerializerOptions)
            ?? throw new ArgumentException("Failed to deserialize package manifest.", nameof(bytes));
    }

    public byte[] ToBytes()
    {
        return JsonSerializer.SerializeToUtf8Bytes(this, s_jsonSerializerOptions);
    }
}
