namespace Lip.Tests;

public class GoModuleTests
{
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
        Assert.Equal($"{path} is not a valid Go module path. (Parameter 'path')", ex.Message);
    }
}
