using System.Text;
using System.Text.Json;

namespace Lip.Tests;

public class RuntimeConfigTests
{
    [Fact]
    public void GitHubProxies_InitAndGet_Passes()
    {
        // Arrange.
        RuntimeConfig runtimeConfig = new();

        // Act.
        runtimeConfig = runtimeConfig with { GitHubProxies = ["github_proxy", "github_proxy2"] };

        // Assert.
        Assert.Equal(["github_proxy", "github_proxy2"], runtimeConfig.GitHubProxies);
    }

    [Fact]
    public void GoModuleProxies_InitAndGet_Passes()
    {
        // Arrange.
        RuntimeConfig runtimeConfig = new();

        // Act.
        runtimeConfig = runtimeConfig with { GoModuleProxies = ["go_module_proxy", "go_module_proxy2"] };

        // Assert.
        Assert.Equal(["go_module_proxy", "go_module_proxy2"], runtimeConfig.GoModuleProxies);
    }

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
        Assert.Equal([], runtimeConfiguration.GitHubProxies);
        Assert.Equal(["https://proxy.golang.org"], runtimeConfiguration.GoModuleProxies);
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
                ""github_proxies"": ""github_proxy"",
                ""go_module_proxies"": ""go_module_proxy"",
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
        Assert.Equal(["github_proxy"], runtimeConfiguration.GitHubProxies);
        Assert.Equal(["go_module_proxy"], runtimeConfiguration.GoModuleProxies);
        Assert.Equal("https_proxy", runtimeConfiguration.HttpsProxy);
        Assert.Equal("noproxy", runtimeConfiguration.NoProxy);
        Assert.Equal("proxy", runtimeConfiguration.Proxy);
    }

    [Fact]
    public void FromJsonBytes_NullJson_Throws()
    {
        // Arrange.
        byte[] jsonBytes = Encoding.UTF8.GetBytes("null");

        // Act & assert.
        JsonException exception = Assert.Throws<JsonException>(() => RuntimeConfig.FromJsonBytes(jsonBytes));
        Assert.IsType<JsonException>(exception);
    }

    [Fact]
    public void ToJsonBytes_DefaultValues_Passes()
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
                "github_proxies": "",
                "go_module_proxies": "https://proxy.golang.org",
                "https_proxy": "",
                "noproxy": "",
                "proxy": ""
            }
            """.ReplaceLineEndings(), Encoding.UTF8.GetString(jsonBytes).ReplaceLineEndings());
    }
}
