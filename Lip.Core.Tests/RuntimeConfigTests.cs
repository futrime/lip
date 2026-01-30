using System.IO.Abstractions.TestingHelpers;
using System.Text;
using System.Text.Json;

namespace Lip.Core.Tests;

public class RuntimeConfigTests
{
    private static readonly string s_runtimeConfigPath = Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "lip", "liprc.json");

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
        Assert.Equal(["https://github.com", "https://github.levimc.org"], runtimeConfiguration.GitHubProxies);
        Assert.Equal(["https://goproxy.io"], runtimeConfiguration.GoModuleProxies);
    }

    [Fact]
    public void FromBytes_MaximumJson_Passes()
    {
        // Arrange.
        byte[] jsonBytes = Encoding.UTF8.GetBytes(
            @"
            {
                ""cache"": ""cache"",
                ""github_proxies"": ""github_proxy"",
                ""go_module_proxies"": ""go_module_proxy""
            }
            "
        );

        // Act.
        var runtimeConfiguration = RuntimeConfig.FromJsonBytes(jsonBytes);

        // Arrange.
        Assert.Equal("cache", runtimeConfiguration.Cache);
        Assert.Equal(["github_proxy"], runtimeConfiguration.GitHubProxies);
        Assert.Equal(["go_module_proxy"], runtimeConfiguration.GoModuleProxies);
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
                "github_proxies": "https://github.com,https://github.levimc.org",
                "go_module_proxies": "https://goproxy.io"
            }
            """.ReplaceLineEndings(), Encoding.UTF8.GetString(jsonBytes).ReplaceLineEndings());
    }

    [Fact]
    public async Task LoadAsync_FileExists_ReturnsConfig()
    {
        // Arrange.
        RuntimeConfig expectedConfig = new()
        {
            Cache = "/custom/cache",
            GitHubProxies = ["https://example.com"],
            GoModuleProxies = ["https://example-proxy.io"],
        };
        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { s_runtimeConfigPath, new MockFileData(expectedConfig.ToJsonBytes()) },
        });

        // Act.
        RuntimeConfig runtimeConfig = await RuntimeConfig.LoadAsync(fileSystem);

        // Assert.
        Assert.Equal(expectedConfig.Cache, runtimeConfig.Cache);
        Assert.Equal(expectedConfig.GitHubProxies, runtimeConfig.GitHubProxies);
        Assert.Equal(expectedConfig.GoModuleProxies, runtimeConfig.GoModuleProxies);
    }

    [Fact]
    public async Task LoadAsync_FileMissing_ReturnsDefault()
    {
        // Arrange.
        MockFileSystem fileSystem = new();

        // Act.
        RuntimeConfig runtimeConfig = await RuntimeConfig.LoadAsync(fileSystem);

        // Assert.
        RuntimeConfig defaultConfig = new();
        Assert.Equal(defaultConfig.Cache, runtimeConfig.Cache);
        Assert.Equal(defaultConfig.GitHubProxies, runtimeConfig.GitHubProxies);
        Assert.Equal(defaultConfig.GoModuleProxies, runtimeConfig.GoModuleProxies);
    }
}
