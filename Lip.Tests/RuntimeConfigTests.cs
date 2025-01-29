using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace Lip.Tests;

public class RuntimeConfigTests
{
    [Fact]
    public void FromBytes_MinimumJson_Passes()
    {
        // Arrange.
        byte[] jsonBytes = Encoding.UTF8.GetBytes("{}");

        // Act.
        var runtimeConfiguration = RuntimeConfig.FromJsonBytes(jsonBytes);

        // Assert.
        Assert.Equal(
            Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "lip", "cache"),
            runtimeConfiguration.Cache
        );
        Assert.True(runtimeConfiguration.Color);
        Assert.Equal("", runtimeConfiguration.GitHubProxy);
        Assert.Equal("https://goproxy.io", runtimeConfiguration.GoModuleProxy);
        Assert.Equal("", runtimeConfiguration.HttpsProxy);
        Assert.Equal("", runtimeConfiguration.NoProxy);
        Assert.Equal("", runtimeConfiguration.Proxy);
    }

    [Fact]
    public void FromBytes_MaximumJson_Passes()
    {
        // Arrange.
        byte[] jsonBytes = Encoding.UTF8.GetBytes(
            @"
            {
                ""cache"": ""cache"",
                ""color"": false,
                ""github_proxy"": ""github_proxy"",
                ""go_module_proxy"": ""go_module_proxy"",
                ""https_proxy"": ""https_proxy"",
                ""noproxy"": ""noproxy"",
                ""proxy"": ""proxy""
            }
            "
        );

        // Act.
        var runtimeConfiguration = RuntimeConfig.FromJsonBytes(jsonBytes);

        // Arrange.
        Assert.Equal("cache", runtimeConfiguration.Cache);
        Assert.False(runtimeConfiguration.Color);
        Assert.Equal("github_proxy", runtimeConfiguration.GitHubProxy);
        Assert.Equal("go_module_proxy", runtimeConfiguration.GoModuleProxy);
        Assert.Equal("https_proxy", runtimeConfiguration.HttpsProxy);
        Assert.Equal("noproxy", runtimeConfiguration.NoProxy);
        Assert.Equal("proxy", runtimeConfiguration.Proxy);
    }

    [Fact]
    public void FromBytes_NullJson_Throws()
    {
        // Arrange.
        byte[] jsonBytes = Encoding.UTF8.GetBytes("null");

        // Act.
        JsonException exception = Assert.Throws<JsonException>(() => RuntimeConfig.FromJsonBytes(jsonBytes));

        // Assert.
        Assert.Equal("Runtime config bytes deserialization failed.", exception.Message);
        Assert.NotNull(exception.InnerException);
        Assert.Equal("JSON bytes deserialized to null.", exception.InnerException.Message);
    }

    [Fact]
    public void ToBytes_MinimumJson_Passes()
    {
        // Arrange.
        var runtimeConfiguration = new RuntimeConfig();

        // Act.
        byte[] jsonBytes = runtimeConfiguration.ToJsonBytes();

        // Assert.
        Assert.Equal($$"""
            {
                "cache": "{{Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "lip", "cache").Replace("\\", "\\\\")}}",
                "color": true,
                "github_proxy": "",
                "go_module_proxy": "https://goproxy.io",
                "https_proxy": "",
                "noproxy": "",
                "proxy": ""
            }
            """.ReplaceLineEndings(), Encoding.UTF8.GetString(jsonBytes).ReplaceLineEndings());
    }
}
