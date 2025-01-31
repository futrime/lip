namespace Lip.Tests;

public class StringValidatorTests
{
    [Theory]
    [InlineData("folder/subfolder")]
    [InlineData("path")]
    public void CheckSafePlacePath_SafePath_ReturnsTrue(string path)
    {
        Assert.True(StringValidator.CheckPlaceDestPath(path));
    }

    [Fact]
    public void CheckSafePlacePath_PathWithDoubleDots_ReturnsFalse()
    {
        Assert.False(StringValidator.CheckPlaceDestPath("folder/../escape"));
    }

    [Fact]
    public void CheckSafePlacePath_PathWithRoot_ReturnsFalse()
    {
        string path = OperatingSystem.IsWindows() ? "C:\\root" : "/root";
        Assert.False(StringValidator.CheckPlaceDestPath(path));
    }

    [Theory]
    [InlineData("example.com/pkg#variant@1.0.0")]
    [InlineData("example.com/pkg@2.0.0")]
    public void CheckPackageSpecifier_ValidSpecifier_ReturnsTrue(string specifier)
    {
        Assert.True(StringValidator.CheckPackageSpecifier(specifier));
    }

    [Theory]
    [InlineData("")]
    [InlineData("example.com/pkg")]
    [InlineData("example.com/pkg#variant@invalid")]
    [InlineData("invalid//pkg#variant@1.0.0")]
    public void CheckPackageSpecifier_InvalidSpecifier_ReturnsFalse(string specifier)
    {
        Assert.False(StringValidator.CheckPackageSpecifier(specifier));
    }

    [Theory]
    [InlineData("example.com/pkg")]
    [InlineData("example.com/pkg#variant")]
    public void CheckPackageSpecifierWithoutVersion_ValidSpecifier_ReturnsTrue(string specifier)
    {
        Assert.True(StringValidator.CheckPackageSpecifierWithoutVersion(specifier));
    }

    [Theory]
    [InlineData("")]
    [InlineData("example.com//pkg")]
    [InlineData("example.com/pkg#invalid!variant")]
    [InlineData("example.com/pkg#invalid#variant")]
    public void CheckPackageSpecifierWithoutVersion_InvalidSpecifier_ReturnsFalse(string specifier)
    {
        Assert.False(StringValidator.CheckPackageSpecifierWithoutVersion(specifier));
    }

    [Theory]
    [InlineData("script")]
    [InlineData("script_name")]
    public void CheckScriptName_CommonInput_ReturnsTrue(string scriptName)
    {
        Assert.True(StringValidator.CheckScriptName(scriptName));
    }

    [Theory]
    [InlineData("script-name")]
    [InlineData("script name")]
    [InlineData("script_name!")]
    public void CheckScriptName_InvalidInput_ReturnsFalse(string scriptName)
    {
        Assert.False(StringValidator.CheckScriptName(scriptName));
    }

    [Theory]
    [InlineData("tag")]
    [InlineData("tag:subtag")]
    public void CheckTag_CommonInput_ReturnsTrue(string tag)
    {
        Assert.True(StringValidator.CheckTag(tag));
    }

    [Theory]
    [InlineData("tag name")]
    [InlineData("tag!")]
    public void CheckTag_InvalidInput_ReturnsFalse(string tag)
    {
        Assert.False(StringValidator.CheckTag(tag));
    }

    [Theory]
    [InlineData("example123.example-domain/example-pkg.example_pkg~Example123")]
    [InlineData("example.com/~a12")]
    [InlineData("github.com/user/repo")]
    public void CheckToothPath_ValidPath_ReturnsTrue(string path)
    {
        Assert.True(StringValidator.CheckToothPath(path));
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
    public void CheckToothPath_InvalidPath_ReturnsFalse(string path)
    {
        Assert.False(StringValidator.CheckToothPath(path));
    }

    [Theory]
    [InlineData("http://example.com")]
    [InlineData("https://example.com/path")]
    public void CheckUrl_ValidUrl_ReturnsTrue(string url)
    {
        Assert.True(StringValidator.CheckUrl(url));
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("http:invalid")]
    public void CheckUrl_InvalidUrl_ReturnsFalse(string url)
    {
        Assert.False(StringValidator.CheckUrl(url));
    }

    [Theory]
    [InlineData("variant")]
    [InlineData("variant_name")]
    public void CheckVariantLabel_ValidLabel_ReturnsTrue(string label)
    {
        Assert.True(StringValidator.CheckVariantLabel(label));
    }

    [Theory]
    [InlineData("invalid-variant")]
    [InlineData("invalid!variant")]
    public void CheckVariantLabel_InvalidLabel_ReturnsFalse(string label)
    {
        Assert.False(StringValidator.CheckVariantLabel(label));
    }

    [Theory]
    [InlineData("1.0.0")]
    [InlineData("1.0.0-alpha")]
    public void CheckVersion_CommonInput_ReturnsTrue(string version)
    {
        Assert.True(StringValidator.CheckVersion(version));
    }

    [Theory]
    [InlineData("1.0.0.0")]
    [InlineData("1.0.0-alpha!")]
    public void CheckVersion_InvalidInput_ReturnsFalse(string version)
    {
        Assert.False(StringValidator.CheckVersion(version));
    }

    [Theory]
    [InlineData("^1.0.0")]
    [InlineData("~2.0.0")]
    [InlineData(">=1.0.0 <2.0.0")]
    public void CheckVersionRange_ValidRange_ReturnsTrue(string range)
    {
        Assert.True(StringValidator.CheckVersionRange(range));
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("1.0.xx")]
    public void CheckVersionRange_InvalidRange_ReturnsFalse(string range)
    {
        Assert.False(StringValidator.CheckVersionRange(range));
    }
}
