namespace Lip.Core;

public record PackageIdentifier
{
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

    public static PackageIdentifier Parse(string text)
    {
        if (!StringValidator.CheckPackageIdentifier(text))
        {
            throw new ArgumentException($"Invalid package identifier '{text}'.", nameof(text));
        }

        string[] parts = text.Split('#');

        return new PackageIdentifier
        {
            ToothPath = parts[0],
            VariantLabel = parts.ElementAtOrDefault(1) ?? string.Empty
        };
    }

    public override string ToString()
    {
        return $"{ToothPath}{(VariantLabel != string.Empty ? "#" : string.Empty)}{VariantLabel}";
    }
}