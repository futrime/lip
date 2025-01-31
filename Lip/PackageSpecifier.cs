using Semver;

namespace Lip;

public record PackageSpecifierWithoutVersion
{
    public string Specifier => $"{ToothPath}{(VariantLabel != string.Empty ? "#" : string.Empty)}{VariantLabel}";

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

    private string _tooth = string.Empty;
    private string _variantLabel = string.Empty;

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
            VariantLabel = parts.ElementAtOrDefault(1) ?? string.Empty
        };
    }

    public override string ToString()
    {
        return Specifier;
    }
}

public record PackageSpecifier : PackageSpecifierWithoutVersion
{
    public new string Specifier => $"{base.Specifier}@{Version}";
    public string SpecifierWithoutVariant => $"{new PackageSpecifierWithoutVersion()
    {
        ToothPath = ToothPath,
        VariantLabel = string.Empty
    }.Specifier}@{Version}";

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

    public override string ToString()
    {
        return Specifier;
    }

    public PackageSpecifierWithoutVersion WithoutVersion()
    {
        return new PackageSpecifierWithoutVersion
        {
            ToothPath = ToothPath,
            VariantLabel = VariantLabel
        };
    }
}
