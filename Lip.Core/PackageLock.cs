using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lip.Core;

public record PackageLock
{
    public record Package
    {
        public required List<string> Files { get; init; }

        public required bool Locked { get; init; }

        public required PackageManifest Manifest { private get; init; }

        public PackageSpecifier Specifier => new()
        {
            ToothPath = Manifest.ToothPath,
            VariantLabel = VariantLabel,
            Version = Manifest.Version,
        };

        public PackageManifest.Variant Variant => Manifest.GetVariant(
            VariantLabel,
            RuntimeInformation.RuntimeIdentifier)!;

        public required string VariantLabel
        {
            private get => _variantLabel;
            init
            {
                if (!StringValidator.CheckVariantLabel(value))
                {
                    throw new SchemaViolationException(
                        "packages[].variant_format",
                        $"Variant label '{value}' does not meet the required format."
                    );
                }
                if (Manifest.GetVariant(value, RuntimeInformation.RuntimeIdentifier) is null)
                {
                    throw new SchemaViolationException(
                        "packages[].variant",
                        $"No matching variant found for label '{value}' with runtime identifier '{RuntimeInformation.RuntimeIdentifier}'."
                    );
                }
                _variantLabel = value;
            }
        }
        private readonly string _variantLabel = string.Empty;
    }

    public const int DefaultFormatVersion = 3;
    public const string DefaultFormatUuid = "289f771f-2c9a-4d73-9f3f-8492495a924d";

    public required List<Package> Packages { get; init; }

    public static async Task<PackageLock> FromStream(Stream stream)
    {
        RawPackageLock rawPackageLock = await RawPackageLock.FromStream(stream);

        // Validate format version and UUID.

        if (rawPackageLock.FormatVersion != DefaultFormatVersion)
        {
            throw new SchemaViolationException(
                "format_version",
                $"Expected format version {DefaultFormatVersion}, but got {rawPackageLock.FormatVersion}.");
        }

        if (rawPackageLock.FormatUuid != DefaultFormatUuid)
        {
            throw new SchemaViolationException(
                "format_uuid",
                $"Expected format UUID '{DefaultFormatUuid}', but got '{rawPackageLock.FormatUuid}'.");
        }

        return new PackageLock
        {
            Packages = [.. rawPackageLock.Packages
                .Select(rawPackage =>
                {
                    var manifest = PackageManifest.FromJsonElement(rawPackage.Manifest);
                    return (manifest, rawPackage.Files, rawPackage.Locked, rawPackage.Variant);
                })
                // Filter out packages without matching variants.
                .Where(x => x.manifest.GetVariant(x.Variant, RuntimeInformation.RuntimeIdentifier) != null)
                .Select(x => new Package
                {
                    Files = x.Files,
                    Locked = x.Locked,
                    Manifest = x.manifest,
                    VariantLabel = x.Variant,
                })],
        };
    }

    public async Task ToStream(Stream stream)
    {
        RawPackageLock rawPackageLock = new()
        {
            FormatVersion = DefaultFormatVersion,
            FormatUuid = DefaultFormatUuid,
            Packages = Packages
                .ConvertAll(package => new RawPackageLock.Package
                {
                    Files = package.Files,
                    Locked = package.Locked,
                    Manifest = new PackageManifest()
                    {
                        ToothPath = package.Specifier.ToothPath,
                        Version = package.Specifier.Version,
                        Info = new()
                        {
                            Name = string.Empty,
                            Description = string.Empty,
                            Tags = [],
                            AvatarUrl = new(),
                        },
                        Variants = [package.Variant],
                    }.ToJsonElement(),
                    Variant = package.Variant.Label,
                }),
        };

        await rawPackageLock.ToStream(stream);
    }
}

[ExcludeFromCodeCoverage]
file record RawPackageLock
{
    public record Package
    {
        [JsonPropertyName("files")]
        public required List<string> Files { get; init; }

        [JsonPropertyName("locked")]
        public required bool Locked { get; init; }

        [JsonPropertyName("manifest")]
        public required JsonElement Manifest { get; init; }

        [JsonPropertyName("variant")]
        public required string Variant { get; init; }
    }

    [JsonPropertyName("format_version")]
    public required int FormatVersion { get; init; }

    [JsonPropertyName("format_uuid")]
    public required string FormatUuid { get; init; }

    [JsonPropertyName("packages")]
    public required List<Package> Packages { get; init; }

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        AllowTrailingCommas = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        IndentSize = 4,
        ReadCommentHandling = JsonCommentHandling.Skip,
        WriteIndented = true,
    };

    public static async Task<RawPackageLock> FromStream(Stream stream)
    {
        return await JsonSerializer.DeserializeAsync<RawPackageLock>(
            stream,
            _jsonSerializerOptions)
            ?? throw new SchemaViolationException("", "JSON bytes deserialized to null.");
    }

    public async Task ToStream(Stream stream)
    {
        await JsonSerializer.SerializeAsync(stream, this, _jsonSerializerOptions);
    }
}