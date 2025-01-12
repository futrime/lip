using System.Text;

namespace Lip.Tests;

public class RuntimeConfigTests
{
    [Fact]
    public void FromBytes_MinimumJson_Passes()
    {
        // Arrange.
        byte[] jsonBytes = Encoding.UTF8.GetBytes("{}");

        // Act.
        var runtimeConfiguration = RuntimeConfig.FromBytes(jsonBytes);

        // Assert.
        Assert.Equal(
            Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "lip", "cache"),
            runtimeConfiguration.Cache
        );
        Assert.True(runtimeConfiguration.Color);
        Assert.Equal("git", runtimeConfiguration.Git);
        Assert.Equal("", runtimeConfiguration.GitHubProxy);
        Assert.Equal("https://goproxy.io", runtimeConfiguration.GoModuleProxy);
        Assert.Equal("", runtimeConfiguration.HttpsProxy);
        Assert.Equal("", runtimeConfiguration.NoProxy);
        Assert.Equal("", runtimeConfiguration.Proxy);
        Assert.Equal(
            OperatingSystem.IsWindows()
                ? "cmd.exe"
                : "/bin/sh", runtimeConfiguration.ScriptShell
        );
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
                ""git"": ""git"",
                ""github_proxy"": ""github_proxy"",
                ""go_module_proxy"": ""go_module_proxy"",
                ""https_proxy"": ""https_proxy"",
                ""noproxy"": ""noproxy"",
                ""proxy"": ""proxy"",
                ""script_shell"": ""script_shell""
            }
            "
        );

        // Act.
        var runtimeConfiguration = RuntimeConfig.FromBytes(jsonBytes);

        // Arrange.
        Assert.Equal("cache", runtimeConfiguration.Cache);
        Assert.False(runtimeConfiguration.Color);
        Assert.Equal("git", runtimeConfiguration.Git);
        Assert.Equal("github_proxy", runtimeConfiguration.GitHubProxy);
        Assert.Equal("go_module_proxy", runtimeConfiguration.GoModuleProxy);
        Assert.Equal("https_proxy", runtimeConfiguration.HttpsProxy);
        Assert.Equal("noproxy", runtimeConfiguration.NoProxy);
        Assert.Equal("proxy", runtimeConfiguration.Proxy);
        Assert.Equal("script_shell", runtimeConfiguration.ScriptShell);
    }

    [Fact]
    public void FromBytes_NullJson_ThrowsArgumentException()
    {
        // Arrange.
        byte[] jsonBytes = Encoding.UTF8.GetBytes("null");

        // Act.
        ArgumentException exception = Assert.Throws<ArgumentException>("bytes", () => RuntimeConfig.FromBytes(jsonBytes));

        // Assert.
        Assert.Equal("Failed to deserialize runtime configuration. (Parameter 'bytes')", exception.Message);
    }

    [Fact]
    public void ToBytes_MinimumJson_Passes()
    {
        // Arrange.
        var runtimeConfiguration = new RuntimeConfig();

        // Act.
        byte[] jsonBytes = runtimeConfiguration.ToBytes();

        // Assert.
        Assert.Equal($$"""
            {
                "cache": "{{Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "lip", "cache").Replace("\\", "\\\\")}}",
                "color": true,
                "git": "git",
                "github_proxy": "",
                "go_module_proxy": "https://goproxy.io",
                "https_proxy": "",
                "noproxy": "",
                "proxy": "",
                "script_shell": "{{(OperatingSystem.IsWindows() ? "cmd.exe" : "/bin/sh")}}"
            }
            """.ReplaceLineEndings(), Encoding.UTF8.GetString(jsonBytes).ReplaceLineEndings());
    }
}
