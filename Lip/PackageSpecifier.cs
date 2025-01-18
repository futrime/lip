using Semver;

namespace Lip;

public partial record PackageSpecifier
{
    public required string ToothPath
    {
        get => _tooth;
        init
        {
            if (!StringValidator.CheckToothPath(value))
            {
                throw new ArgumentException("Invalid tooth path.");
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
                throw new ArgumentException("Invalid variant label.");
            }

            _variantLabel = value;
        }
    }
    public required SemVersion Version { get; init; }

    private string _tooth = "";
    private string _variantLabel = "";

    public static PackageSpecifier Parse(string specifierText)
    {
        if (!StringValidator.CheckPackageSpecifier(specifierText))
        {
            throw new ArgumentException("Invalid package specifier.", nameof(specifierText));
        }

        string[] parts = specifierText.Split('@');
        string[] toothPathAndVariantLabel = parts[0].Split('#');

        return new PackageSpecifier
        {
            ToothPath = toothPathAndVariantLabel[0],
            VariantLabel = toothPathAndVariantLabel[1],
            Version = SemVersion.Parse(parts[1])
        };
    }
}
