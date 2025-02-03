namespace Lip.Tests;

public class GoModuleTests
{
    [Theory]
    [InlineData("v0.0.0", "v0.0.0")]
    [InlineData("v1.0.0", "v1.0.0")]
    [InlineData("1.0.0", "v1.0.0")]
    [InlineData("v1.0.0+build", "v1.0.0")]
    [InlineData("v2.0.0", "v2.0.0+incompatible")]
    public void CanonicalVersion_VariousVersionStrings_ReturnsCanonicalVersion(string version, string expectedCanonicalVersion)
    {
        // Act.
        string result = GoModule.CanonicalVersion(version);

        // Assert.
        Assert.Equal(expectedCanonicalVersion, result);
    }

    [Theory]
    [InlineData("example123.example-domain/example-pkg.example_pkg~Example123")]
    [InlineData("example.com/~a12")]
    [InlineData("github.com/user/repo")]
    public void CheckPath_ValidPath_ReturnsTrue(string path)
    {
        // Act.
        bool result = GoModule.CheckPath(path);

        // Assert.
        Assert.True(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("-example.com/pkg")]
    [InlineData("example.com//pkg")]
    [InlineData("example.com/pkg/")]
    [InlineData("example/pkg")]
    [InlineData("Example.com/pkg")]
    [InlineData("example\0.com/pkg")]
    [InlineData("example.com/../pkg")]
    [InlineData("example.com/.pkg")]
    [InlineData("example.com/pkg.")]
    [InlineData("example.com/p*kg")]
    [InlineData("example.com/con.pkg")]
    [InlineData("example.com/pkg~123")]
    public void CheckPath_InvalidPath_ReturnsFalse(string path)
    {
        // Act.
        bool result = GoModule.CheckPath(path);

        // Assert.
        Assert.False(result);
    }

    [Theory]
    [InlineData("example.com/pkg", "example.com/pkg")]
    [InlineData("example.com/PKG", "example.com/!p!k!g")]
    public void EscapePath_ValidPath_ReturnsEscapedPath(string path, string expectedEscapedPath)
    {
        // Act.
        string result = GoModule.EscapePath(path);

        // Assert
        Assert.Equal(expectedEscapedPath, result);
    }

    [Theory]
    [InlineData("example.com/!pkg")]
    [InlineData("example.com/pkg~123")]
    public void EscapePath_InvalidPath_Throws(string path)
    {
        // Act & assert.
        ArgumentException ex = Assert.Throws<ArgumentException>(() => GoModule.EscapePath(path));
        Assert.Equal("path", ex.ParamName);
    }

    [Theory]
    [InlineData("v1.0.0", "v1.0.0")]
    [InlineData("v1.0.0-BETA", "v1.0.0-!b!e!t!a")]
    public void EscapeVersion_ValidVersion_ReturnsEscapedVersion(string version, string expectedEscapedVersion)
    {
        // Act.
        string result = GoModule.EscapeVersion(version);

        // Assert.
        Assert.Equal(expectedEscapedVersion, result);
    }

    [Theory]
    [InlineData("v1.0.0!")]
    [InlineData(".v1.0.0beta")]
    public void EscapeVersion_InvalidVersion_Throws(string version)
    {
        // Act & assert.
        ArgumentException ex = Assert.Throws<ArgumentException>(() => GoModule.EscapeVersion(version));
        Assert.Equal("v", ex.ParamName);
    }
}
