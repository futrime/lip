using Semver;

namespace Lip;

public record PackageSpecifierWithoutVersion
{
    public required string ToothPath
    {
        get => _tooth;
        init
        {
            if (!StringValidator.CheckToothPath(value))
            {
                throw new ArgumentException("Invalid tooth path.", nameof(ToothPath));
            }

            _tooth = value;
        }
    }
    public required string VariantLabel
    {
        get => _variantLabel;
        init
        {
            if (!StringValidator.CheckVariantLabel(value))
            {
                throw new ArgumentException("Invalid variant label.", nameof(VariantLabel));
            }

            _variantLabel = value;
        }
    }

    private string _tooth = "";
    private string _variantLabel = "";

    public static PackageSpecifierWithoutVersion Parse(string specifierText)
    {
        if (!StringValidator.CheckPackageSpecifierWithoutVersion(specifierText))
        {
            throw new ArgumentException($"Invalid package specifier '{specifierText}'.", nameof(specifierText));
        }

        string[] parts = specifierText.Split('#');

        return new PackageSpecifierWithoutVersion
        {
            ToothPath = parts[0],
            VariantLabel = parts[1]
        };
    }
}

public record PackageSpecifier : PackageSpecifierWithoutVersion
{

    public required SemVersion Version { get; init; }

    public static new PackageSpecifier Parse(string specifierText)
    {
        if (!StringValidator.CheckPackageSpecifier(specifierText))
        {
            throw new ArgumentException($"Invalid package specifier '{specifierText}'.", nameof(specifierText));
        }

        string[] parts = specifierText.Split('@');

        PackageSpecifierWithoutVersion packageSpecifierWithoutVersion = PackageSpecifierWithoutVersion.Parse(parts[0]);

        return new PackageSpecifier
        {
            ToothPath = packageSpecifierWithoutVersion.ToothPath,
            VariantLabel = packageSpecifierWithoutVersion.VariantLabel,
            Version = SemVersion.Parse(parts[1])
        };
    }
}
