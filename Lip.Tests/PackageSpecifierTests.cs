using Semver;

namespace Lip.Tests;

public class PackageSpecifierWithoutVersionTests
{
    [Fact]
    public void Constructor_ValidValues_Passes()
    {
        // Arrange & Act
        var packageSpecifier = new PackageSpecifierWithoutVersion
        {
            ToothPath = "example.com/pkg",
            VariantLabel = "variant",
        };

        // Assert
        Assert.Equal("example.com/pkg#variant", packageSpecifier.Specifier);
        Assert.Equal("example.com/pkg", packageSpecifier.ToothPath);
        Assert.Equal("variant", packageSpecifier.VariantLabel);
    }

    [Fact]
    public void Constructor_InvalidToothPath_Throws()
    {
        // Arrange & Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => new PackageSpecifierWithoutVersion
        {
            ToothPath = "invalid/tooth",
            VariantLabel = "variant",
        });
        Assert.Equal("Invalid tooth path. (Parameter 'ToothPath')", exception.Message);
    }

    [Fact]
    public void Constructor_InvalidVariantLabel_Throws()
    {
        // Arrange & Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => new PackageSpecifierWithoutVersion
        {
            ToothPath = "example.com/pkg",
            VariantLabel = "invalid-variant",
        });
        Assert.Equal("Invalid variant label. (Parameter 'VariantLabel')", exception.Message);
    }

    [Fact]
    public void Parse_ValidSpecifierText_Passes()
    {
        // Arrange & Act
        var packageSpecifier = PackageSpecifierWithoutVersion.Parse("example.com/pkg#variant");

        // Assert
        Assert.Equal("example.com/pkg#variant", packageSpecifier.Specifier);
        Assert.Equal("example.com/pkg", packageSpecifier.ToothPath);
        Assert.Equal("variant", packageSpecifier.VariantLabel);
    }

    [Fact]
    public void Parse_InvalidSpecifierText_Throws()
    {
        // Arrange & Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => PackageSpecifierWithoutVersion.Parse("invalid"));
        Assert.Equal("Invalid package specifier 'invalid'. (Parameter 'specifierText')", exception.Message);
    }
}

public class PackageSpecifierTests
{
    [Fact]
    public void Constructor_ValidValues_Passes()
    {
        // Arrange & Act
        var packageSpecifier = new PackageSpecifier
        {
            ToothPath = "example.com/pkg",
            VariantLabel = "variant",
            Version = SemVersion.Parse("1.0.0")
        };

        // Assert
        Assert.Equal("example.com/pkg#variant@1.0.0", packageSpecifier.Specifier);
        Assert.Equal("example.com/pkg", packageSpecifier.ToothPath);
        Assert.Equal("variant", packageSpecifier.VariantLabel);
        Assert.Equal("1.0.0", packageSpecifier.Version.ToString());
    }

    [Fact]
    public void Parse_ValidSpecifierText_Passes()
    {
        // Arrange & Act
        var packageSpecifier = PackageSpecifier.Parse("example.com/pkg#variant@1.0.0");

        // Assert
        Assert.Equal("example.com/pkg#variant@1.0.0", packageSpecifier.Specifier);
        Assert.Equal("example.com/pkg", packageSpecifier.ToothPath);
        Assert.Equal("variant", packageSpecifier.VariantLabel);
        Assert.Equal("1.0.0", packageSpecifier.Version.ToString());
    }

    [Fact]
    public void Parse_InvalidSpecifierText_Throws()
    {
        // Arrange & Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => PackageSpecifier.Parse("invalid"));
        Assert.Equal("Invalid package specifier 'invalid'. (Parameter 'specifierText')", exception.Message);
    }

    [Fact]
    public void WithoutVersion_Passes()
    {
        // Arrange
        var packageSpecifier = new PackageSpecifier
        {
            ToothPath = "example.com/pkg",
            VariantLabel = "variant",
            Version = SemVersion.Parse("1.0.0")
        };

        // Act
        PackageSpecifierWithoutVersion packageSpecifierWithoutVersion = packageSpecifier.WithoutVersion();

        // Assert
        Assert.Equal("example.com/pkg#variant", packageSpecifierWithoutVersion.Specifier);
        Assert.Equal("example.com/pkg", packageSpecifierWithoutVersion.ToothPath);
        Assert.Equal("variant", packageSpecifierWithoutVersion.VariantLabel);
    }
}
