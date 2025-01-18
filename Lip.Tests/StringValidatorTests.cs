namespace Lip.Tests;

public class StringValidatorTests
{
    [Theory]
    [InlineData("script")]
    [InlineData("script_name")]
    public void IsScriptNameValid_CommonInput_Passes(string scriptName)
    {
        Assert.True(StringValidator.CheckScriptName(scriptName));
    }

    [Theory]
    [InlineData("script-name")]
    [InlineData("script name")]
    [InlineData("script_name!")]
    public void IsScriptNameValid_InvalidInput_Fails(string scriptName)
    {
        Assert.False(StringValidator.CheckScriptName(scriptName));
    }

    [Theory]
    [InlineData("tag")]
    [InlineData("tag:subtag")]
    public void IsTagValid_CommonInput_Passes(string tag)
    {
        Assert.True(StringValidator.CheckTag(tag));
    }

    [Theory]
    [InlineData("tag name")]
    [InlineData("tag!")]
    public void IsTagValid_InvalidInput_Fails(string tag)
    {
        Assert.False(StringValidator.CheckTag(tag));
    }

    [Theory]
    [InlineData("1.0.0")]
    [InlineData("1.0.0-alpha")]
    public void IsVersionValid_CommonInput_Passes(string version)
    {
        Assert.True(StringValidator.CheckVersion(version));
    }

    [Theory]
    [InlineData("1.0.0.0")]
    [InlineData("1.0.0-alpha!")]
    public void IsVersionValid_InvalidInput_Fails(string version)
    {
        Assert.False(StringValidator.CheckVersion(version));
    }
}
