using Semver;

namespace Lip.Tests;

public class PackageSpecifierWithoutVersionTests
{
    [Fact]
    public void With_Passes()
    {
        // Arrange.
        PackageSpecifierWithoutVersion packageSpecifier = new()
        {
            ToothPath = "example.com/pkg",
            VariantLabel = "variant",
        };

        // Act.
        packageSpecifier = packageSpecifier with { };
    }

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
        Assert.Equal("ToothPath", exception.ParamName);
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

    [Theory]
    [InlineData("example.com/pkg", "variant", "example.com/pkg#variant")]
    [InlineData("example.com/pkg", "", "example.com/pkg")]
    public void GetSpecifier_ValidValues_Passes(string toothPath, string variantLabel, string specifier)
    {
        // Arrange
        var packageSpecifier = new PackageSpecifierWithoutVersion
        {
            ToothPath = toothPath,
            VariantLabel = variantLabel,
        };

        // Act
        string result = packageSpecifier.Specifier;

        // Assert
        Assert.Equal(specifier, result);
    }

    [Theory]
    [InlineData("example.com/pkg#variant", "example.com/pkg", "variant")]
    [InlineData("example.com/pkg", "example.com/pkg", "")]
    public void Parse_ValidSpecifierText_Passes(string specifier, string toothPath, string variantLabel)
    {
        // Arrange & Act
        var packageSpecifier = PackageSpecifierWithoutVersion.Parse(specifier);

        // Assert
        Assert.Equal(specifier, packageSpecifier.Specifier);
        Assert.Equal(toothPath, packageSpecifier.ToothPath);
        Assert.Equal(variantLabel, packageSpecifier.VariantLabel);
    }

    [Fact]
    public void Parse_InvalidSpecifierText_Throws()
    {
        // Arrange & Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => PackageSpecifierWithoutVersion.Parse("invalid"));
        Assert.Equal("Invalid package specifier 'invalid'. (Parameter 'specifierText')", exception.Message);
    }

    [Fact]
    public void ToString_ValidValues_Passes()
    {
        // Arrange
        var packageSpecifier = new PackageSpecifierWithoutVersion
        {
            ToothPath = "example.com/pkg",
            VariantLabel = "variant",
        };

        // Act
        string specifierText = packageSpecifier.ToString();

        // Assert
        Assert.Equal("example.com/pkg#variant", specifierText);
    }
}

public class PackageSpecifierTests
{
    [Fact]
    public void With_Passes()
    {
        // Arrange.
        PackageSpecifier packageSpecifier = new()
        {
            ToothPath = "example.com/pkg",
            VariantLabel = "variant",
            Version = SemVersion.Parse("1.0.0")
        };

        // Act.
        packageSpecifier = packageSpecifier with { };
    }

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
    public void GetSpecifier_ValidValues_Passes()
    {
        // Arrange
        var packageSpecifier = new PackageSpecifier
        {
            ToothPath = "example.com/pkg",
            VariantLabel = "variant",
            Version = SemVersion.Parse("1.0.0")
        };

        // Act
        string specifier = packageSpecifier.Specifier;

        // Assert
        Assert.Equal("example.com/pkg#variant@1.0.0", specifier);
    }

    [Fact]
    public void GetSpecifierWithoutVariant_ValidValues_Passes()
    {
        // Arrange
        var packageSpecifier = new PackageSpecifier
        {
            ToothPath = "example.com/pkg",
            VariantLabel = "variant",
            Version = SemVersion.Parse("1.0.0")
        };

        // Act
        string specifier = packageSpecifier.SpecifierWithoutVariant;

        // Assert
        Assert.Equal("example.com/pkg@1.0.0", specifier);
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
    public void Parse_EmptyVariantLabel_Passes()
    {
        // Arrange & Act
        var packageSpecifier = PackageSpecifier.Parse("example.com/pkg@1.0.0");

        // Assert
        Assert.Equal("example.com/pkg@1.0.0", packageSpecifier.Specifier);
        Assert.Equal("example.com/pkg", packageSpecifier.ToothPath);
        Assert.Equal("", packageSpecifier.VariantLabel);
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
    public void ToString_ValidValues_Passes()
    {
        // Arrange
        var packageSpecifier = new PackageSpecifier
        {
            ToothPath = "example.com/pkg",
            VariantLabel = "variant",
            Version = SemVersion.Parse("1.0.0")
        };

        // Act
        string specifierText = packageSpecifier.ToString();

        // Assert
        Assert.Equal("example.com/pkg#variant@1.0.0", specifierText);
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
