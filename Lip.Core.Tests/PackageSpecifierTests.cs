using Semver;

namespace Lip.Core.Tests;

public class PackageSpecifierTests
{
    private const string _defaultText = "example.com/pkg#variant@1.0.0";
    private const string _defaultToothPath = "example.com/pkg";
    private const string _defaultVariantLabel = "variant";
    private readonly SemVersion _defaultVersion = new(1, 0, 0);

    [Fact]
    public void Constructor_ValidValues_ReturnsCorrectInstance()
    {
        // Arrange & Act.
        PackageSpecifier specifier = new()
        {
            ToothPath = _defaultToothPath,
            VariantLabel = _defaultVariantLabel,
            Version = _defaultVersion
        };

        PackageSpecifier newSpecifier = specifier with { };

        // Assert.
        Assert.Equal(_defaultToothPath, newSpecifier.ToothPath);
        Assert.Equal(_defaultVariantLabel, newSpecifier.VariantLabel);
    }

    [Fact]
    public void Constructor_InvalidToothPath_ThrowsArgumentException()
    {
        // Arrange & Act & Assert.
        ArgumentException exception = Assert.Throws<ArgumentException>(() => new PackageSpecifier
        {
            ToothPath = "invalid tooth path",
            VariantLabel = _defaultVariantLabel,
            Version = _defaultVersion
        });
        Assert.Equal("ToothPath", exception.ParamName);
    }

    [Fact]
    public void Constructor_InvalidVariantLabel_ThrowsArgumentException()
    {
        // Arrange & Act & Assert.
        ArgumentException exception = Assert.Throws<ArgumentException>(() => new PackageSpecifier
        {
            ToothPath = _defaultToothPath,
            VariantLabel = "invalid-variant",
            Version = _defaultVersion
        });
        Assert.Equal("VariantLabel", exception.ParamName);
    }

    [Fact]
    public void Identifier_ReturnsCorrectIdentifier()
    {
        // Arrange.
        PackageSpecifier specifier = new()
        {
            ToothPath = _defaultToothPath,
            VariantLabel = _defaultVariantLabel,
            Version = _defaultVersion
        };

        PackageIdentifier expectedIdentifier = new()
        {
            ToothPath = _defaultToothPath,
            VariantLabel = _defaultVariantLabel
        };

        // Act.
        PackageIdentifier identifier = specifier.Identifier;

        // Assert.
        Assert.Equal(expectedIdentifier, identifier);
    }

    [Fact]
    public void FromIdentifier_ValidValues_ReturnsCorrectInstance()
    {
        // Arrange.
        PackageIdentifier identifier = new()
        {
            ToothPath = _defaultToothPath,
            VariantLabel = _defaultVariantLabel
        };

        PackageSpecifier expectedSpecifier = new()
        {
            ToothPath = _defaultToothPath,
            VariantLabel = _defaultVariantLabel,
            Version = _defaultVersion
        };

        // Act.
        PackageSpecifier specifier = PackageSpecifier.FromIdentifier(
            identifier,
            _defaultVersion);

        // Assert.
        Assert.Equal(expectedSpecifier, specifier);
    }

    [Fact]
    public void Parse_FullText_ReturnsCorrectInstance()
    {
        // Arrange & Act.
        PackageSpecifier specifier = PackageSpecifier.Parse(_defaultText);

        // Assert.
        Assert.Equal(_defaultToothPath, specifier.ToothPath);
        Assert.Equal(_defaultVariantLabel, specifier.VariantLabel);
    }

    [Fact]
    public void Parse_WithoutVariantLabel_ReturnsCorrectInstance()
    {
        // Arrange & Act.
        PackageSpecifier specifier = PackageSpecifier.Parse($"{_defaultToothPath}@{_defaultVersion}");

        // Assert.
        Assert.Equal(_defaultToothPath, specifier.ToothPath);
        Assert.Equal(string.Empty, specifier.VariantLabel);
    }

    [Fact]
    public void Parse_InvalidText_ThrowsArgumentException()
    {
        // Arrange.
        string invalidSpecifier = "invalid specifier";

        // Act & Assert.
        ArgumentException exception = Assert.Throws<ArgumentException>(() => PackageSpecifier.Parse(invalidSpecifier));
        Assert.Equal("text", exception.ParamName);
    }

    [Fact]
    public void ToString_WithVariantLabel_ReturnsCorrectText()
    {
        // Arrange.
        PackageSpecifier specifier = new()
        {
            ToothPath = _defaultToothPath,
            VariantLabel = _defaultVariantLabel,
            Version = _defaultVersion
        };

        // Act.
        string specifierText = specifier.ToString();

        // Assert.
        Assert.Equal(_defaultText, specifierText);
    }

    [Fact]
    public void ToString_WithoutVariantLabel_ReturnsCorrectText()
    {
        // Arrange
        PackageSpecifier specifier = new()
        {
            ToothPath = _defaultToothPath,
            VariantLabel = string.Empty,
            Version = _defaultVersion
        };

        // Act
        string specifierText = specifier.ToString();

        // Assert
        Assert.Equal($"{_defaultToothPath}@{_defaultVersion}", specifierText);
    }
}
