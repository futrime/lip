namespace Lip.Tests;

public class PackageIdentifierTests
{
    private const string _defaultText = "example.com/pkg#variant";
    private const string _defaultToothPath = "example.com/pkg";
    private const string _defaultVariantLabel = "variant";

    [Fact]
    public void Constructor_ValidValues_ReturnsCorrectInstance()
    {
        // Arrange & Act.
        PackageIdentifier identifier = new()
        {
            ToothPath = _defaultToothPath,
            VariantLabel = _defaultVariantLabel,
        };

        PackageIdentifier newIdentifier = identifier with { };

        // Assert.
        Assert.Equal(_defaultToothPath, newIdentifier.ToothPath);
        Assert.Equal(_defaultVariantLabel, newIdentifier.VariantLabel);
    }

    [Fact]
    public void Constructor_InvalidToothPath_ThrowsArgumentException()
    {
        // Arrange & Act & Assert.
        ArgumentException exception = Assert.Throws<ArgumentException>(() => new PackageIdentifier
        {
            ToothPath = "invalid tooth path",
            VariantLabel = _defaultVariantLabel,
        });
        Assert.Equal("ToothPath", exception.ParamName);
    }

    [Fact]
    public void Constructor_InvalidVariantLabel_ThrowsArgumentException()
    {
        // Arrange & Act & Assert.
        ArgumentException exception = Assert.Throws<ArgumentException>(() => new PackageIdentifier
        {
            ToothPath = _defaultToothPath,
            VariantLabel = "invalid-variant",
        });
        Assert.Equal("VariantLabel", exception.ParamName);
    }

    [Fact]
    public void Parse_FullText_ReturnsCorrectInstance()
    {
        // Arrange & Act.
        PackageIdentifier identifier = PackageIdentifier.Parse(_defaultText);

        // Assert.
        Assert.Equal(_defaultToothPath, identifier.ToothPath);
        Assert.Equal(_defaultVariantLabel, identifier.VariantLabel);
    }

    [Fact]
    public void Parse_WithoutVariantLabel_ReturnsCorrectInstance()
    {
        // Arrange & Act.
        PackageIdentifier identifier = PackageIdentifier.Parse(_defaultToothPath);

        // Assert.
        Assert.Equal(_defaultToothPath, identifier.ToothPath);
        Assert.Equal(string.Empty, identifier.VariantLabel);
    }

    [Fact]
    public void Parse_InvalidText_ThrowsArgumentException()
    {
        // Arrange.
        string text = "invalid text";

        // Act & Assert.
        ArgumentException exception = Assert.Throws<ArgumentException>(() => PackageIdentifier.Parse(text));
        Assert.Equal("text", exception.ParamName);
    }

    [Fact]
    public void ToString_WithVariantLabel_ReturnsCorrectText()
    {
        // Arrange.
        PackageIdentifier identifier = new()
        {
            ToothPath = _defaultToothPath,
            VariantLabel = _defaultVariantLabel,
        };

        // Act.
        string text = identifier.ToString();

        // Assert.
        Assert.Equal(_defaultText, text);
    }

    [Fact]
    public void ToString_WithoutVariantLabel_ReturnsCorrectText()
    {
        // Arrange
        PackageIdentifier identifier = new()
        {
            ToothPath = _defaultToothPath,
            VariantLabel = string.Empty,
        };

        // Act
        string text = identifier.ToString();

        // Assert
        Assert.Equal(_defaultToothPath, text);
    }
}
