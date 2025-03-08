using Semver;

namespace Lip.Core;


public record PackageSpecifier
{
    public PackageIdentifier Identifier => new()
    {
        ToothPath = ToothPath,
        VariantLabel = VariantLabel
    };

    public required string ToothPath
    {
        get => _toothPath;
        init
        {
            if (!StringValidator.CheckToothPath(value))
            {
                throw new ArgumentException($"Invalid tooth path {value}.", nameof(ToothPath));
            }

            _toothPath = value;
        }
    }
    private readonly string _toothPath = string.Empty;

    public required string VariantLabel
    {
        get => _variantLabel;
        init
        {
            if (!StringValidator.CheckVariantLabel(value))
            {
                throw new ArgumentException($"Invalid variant label {value}.", nameof(VariantLabel));
            }

            _variantLabel = value;
        }
    }
    private string _variantLabel = string.Empty;

    public required SemVersion Version { get; init; }

    public static PackageSpecifier FromIdentifier(PackageIdentifier identifier, SemVersion version)
    {
        return new PackageSpecifier
        {
            ToothPath = identifier.ToothPath,
            VariantLabel = identifier.VariantLabel,
            Version = version
        };
    }

    public static PackageSpecifier Parse(string text)
    {
        if (!StringValidator.CheckPackageSpecifier(text))
        {
            throw new ArgumentException(
                $"Invalid package specifier '{text}'. Expected format is 'toothPath[#variantLabel]@version'.", nameof(text));
        }

        string[] parts = text.Split('@');

        PackageIdentifier packageIdentifier = PackageIdentifier.Parse(parts[0]);

        return new PackageSpecifier
        {
            ToothPath = packageIdentifier.ToothPath,
            VariantLabel = packageIdentifier.VariantLabel,
            Version = SemVersion.Parse(parts[1])
        };
    }

    public override string ToString()
    {
        string packageIdentifierText = new PackageIdentifier
        {
            ToothPath = ToothPath,
            VariantLabel = VariantLabel
        }.ToString();

        return $"{packageIdentifierText}@{Version}";
    }
}
