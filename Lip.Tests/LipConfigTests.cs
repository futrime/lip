using System.IO.Abstractions.TestingHelpers;
using Lip.Context;
using Microsoft.Extensions.Logging;
using Moq;

namespace Lip.Tests;

public class LipConfigTests
{
    private static readonly string s_runtimeConfigPath = Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "lip", "liprc.json");

    [Fact]
    public async Task ConfigDelete_SingleItem_ResetsToDefault()
    {
        // Arrange.
        RuntimeConfig initialRuntimeConfig = new()
        {
            Cache = "/custom/cache",
            Color = false,
        };

        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { s_runtimeConfigPath, new MockFileData(initialRuntimeConfig.ToBytes()) },
        });

        Mock<IContext> context = new();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        Lip lip = new(initialRuntimeConfig, context.Object);

        // Act.
        await lip.ConfigDelete(["color"], new Lip.ConfigDeleteArgs());

        // Assert.
        Assert.True(fileSystem.File.Exists(s_runtimeConfigPath));

        Assert.Equal($$"""
        {
            "cache": "/custom/cache",
            "color": true,
            "git": "git",
            "github_proxy": "",
            "go_module_proxy": "https://goproxy.io",
            "https_proxy": "",
            "noproxy": "",
            "proxy": "",
            "script_shell": "{{(OperatingSystem.IsWindows() ? "cmd.exe" : "/bin/sh")}}"
        }
        """, fileSystem.File.ReadAllText(s_runtimeConfigPath));
    }

    [Fact]
    public async Task ConfigDelete_MultipleItems_ResetsToDefault()
    {
        // Arrange.
        RuntimeConfig initialRuntimeConfig = new()
        {
            Cache = "/custom/cache",
            Color = false,
            Git = "custom-git"
        };

        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { s_runtimeConfigPath, new MockFileData(initialRuntimeConfig.ToBytes()) },
        });

        Mock<IContext> context = new();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        Lip lip = new(initialRuntimeConfig, context.Object);

        // Act.
        await lip.ConfigDelete(["color", "git"], new Lip.ConfigDeleteArgs());

        // Assert.
        Assert.True(fileSystem.File.Exists(s_runtimeConfigPath));

        Assert.Equal($$"""
        {
            "cache": "/custom/cache",
            "color": true,
            "git": "git",
            "github_proxy": "",
            "go_module_proxy": "https://goproxy.io",
            "https_proxy": "",
            "noproxy": "",
            "proxy": "",
            "script_shell": "{{(OperatingSystem.IsWindows() ? "cmd.exe" : "/bin/sh")}}"
        }
        """, fileSystem.File.ReadAllText(s_runtimeConfigPath));
    }

    [Fact]
    public async Task ConfigDelete_EmptyKeys_ThrowsArgumentException()
    {
        // Arrange.
        RuntimeConfig initialRuntimeConfig = new();

        Mock<IContext> context = new();

        Lip lip = new(initialRuntimeConfig, context.Object);

        // Act & Assert.
        ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(
            () => lip.ConfigDelete([], new Lip.ConfigDeleteArgs()));
        Assert.Equal("No configuration keys provided. (Parameter 'keys')", exception.Message);
    }

    [Fact]
    public async Task ConfigDelete_UnknownKey_ThrowsArgumentException()
    {
        // Arrange.
        RuntimeConfig initialRuntimeConfig = new();

        Mock<IContext> context = new();

        Lip lip = new(initialRuntimeConfig, context.Object);

        // Act & Assert.
        ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(
            () => lip.ConfigDelete(["unknown"], new Lip.ConfigDeleteArgs()));
        Assert.Equal("Unknown configuration key: 'unknown'. (Parameter 'keys')", exception.Message);
    }

    [Fact]
    public async Task ConfigDelete_PartialUnknownItem_ThrowsArgumentException()
    {
        // Arrange.
        RuntimeConfig initialRuntimeConfig = new();

        Mock<IContext> context = new();

        Lip lip = new(initialRuntimeConfig, context.Object);

        // Act & Assert.
        ArgumentException argumentException = await Assert.ThrowsAsync<ArgumentException>(
            () => lip.ConfigDelete(["cache", "unknown", "color"], new Lip.ConfigDeleteArgs()));
        Assert.Equal("Unknown configuration key: 'unknown'. (Parameter 'keys')", argumentException.Message);
    }

    [Fact]
    public void ConfigGet_SingleItem_Passes()
    {
        // Arrange.
        RuntimeConfig initialRuntimeConfig = new() { Cache = "/custom/cache" };

        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { s_runtimeConfigPath, new MockFileData(initialRuntimeConfig.ToBytes()) },
        });

        Mock<IContext> context = new();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        Lip lip = new(initialRuntimeConfig, context.Object);

        // Act.
        Dictionary<string, string> result = lip.ConfigGet(["cache"], new Lip.ConfigGetArgs());

        // Assert.
        Assert.Single(result);
        Assert.Equal("/custom/cache", result["cache"]);
    }

    [Fact]
    public void ConfigGet_MultipleItems_Passes()
    {
        // Arrange.
        RuntimeConfig initialRuntimeConfig = new()
        {
            Cache = "/custom/cache",
            Color = false,
        };

        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { s_runtimeConfigPath, new MockFileData(initialRuntimeConfig.ToBytes()) },
        });

        Mock<IContext> context = new();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        Lip lip = new(initialRuntimeConfig, context.Object);

        // Act.
        Dictionary<string, string> result = lip.ConfigGet(
            ["cache", "color", "git"],
            new Lip.ConfigGetArgs());

        // Assert.
        Assert.Equal(3, result.Count);
        Assert.Equal("/custom/cache", result["cache"]);
        Assert.Equal("False", result["color"]);
    }

    [Fact]
    public void ConfigGet_UnknownKey_ThrowsArgumentException()
    {
        // Arrange.
        RuntimeConfig initialRuntimeConfig = new();

        Mock<IContext> context = new();

        Lip lip = new(initialRuntimeConfig, context.Object);

        // Act & Assert.
        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => lip.ConfigGet(["unknown"], new Lip.ConfigGetArgs()));
        Assert.Equal("Unknown configuration key: 'unknown'. (Parameter 'keys')", exception.Message);
    }

    [Fact]
    public void ConfigGet_PartialUnknownItem_ThrowsArgumentException()
    {
        // Arrange.
        RuntimeConfig initialRuntimeConfig = new();

        Mock<IContext> context = new();

        Lip lip = new(initialRuntimeConfig, context.Object);

        // Act & Assert.
        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => lip.ConfigGet(["cache", "unknown", "color"], new Lip.ConfigGetArgs()));
        Assert.Equal("Unknown configuration key: 'unknown'. (Parameter 'keys')", exception.Message);
    }

    [Fact]
    public void ConfigGet_EmptyKeys_ThrowsArgumentException()
    {
        // Arrange.
        RuntimeConfig initialRuntimeConfig = new();

        Mock<IContext> context = new();

        Lip lip = new(initialRuntimeConfig, context.Object);

        // Act & Assert.
        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => lip.ConfigGet([], new Lip.ConfigGetArgs()));
        Assert.Equal("No configuration keys provided. (Parameter 'keys')", exception.Message);
    }

    [Fact]
    public void ConfigList_ReturnsAllConfigurations()
    {
        // Arrange.
        RuntimeConfig initialRuntimeConfig = new()
        {
            Cache = "/custom/cache",
            Color = false,
            Git = "custom-git",
            GitHubProxy = "https://github-proxy.com",
            GoModuleProxy = "https://custom-proxy.io",
            HttpsProxy = "https://https-proxy.com",
            NoProxy = "localhost",
            Proxy = "http://custom-proxy.com",
            ScriptShell = "/custom/shell"
        };

        Mock<IContext> context = new();

        Lip lip = new(initialRuntimeConfig, context.Object);

        // Act.
        Dictionary<string, string> result = lip.ConfigList(new Lip.ConfigListArgs());

        // Assert.
        Assert.Equal(9, result.Count);
        Assert.Equal("/custom/cache", result["cache"]);
        Assert.Equal("False", result["color"]);
        Assert.Equal("custom-git", result["git"]);
        Assert.Equal("https://github-proxy.com", result["github_proxy"]);
        Assert.Equal("https://custom-proxy.io", result["go_module_proxy"]);
        Assert.Equal("https://https-proxy.com", result["https_proxy"]);
        Assert.Equal("localhost", result["noproxy"]);
        Assert.Equal("http://custom-proxy.com", result["proxy"]);
        Assert.Equal("/custom/shell", result["script_shell"]);
    }

    [Fact]
    public async Task ConfigSet_SingleItem_Passes()
    {
        // Arrange.
        RuntimeConfig initialRuntimeConfig = new();

        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { s_runtimeConfigPath, new MockFileData(initialRuntimeConfig.ToBytes()) },
        });

        Mock<IContext> context = new();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        Lip lip = new(initialRuntimeConfig, context.Object);

        Dictionary<string, string> keyValuePairs = new()
        {
            { "cache", "/path/to/cache" },
        };

        // Act.
        await lip.ConfigSet(keyValuePairs, new Lip.ConfigSetArgs());

        // Assert.
        Assert.True(fileSystem.File.Exists(s_runtimeConfigPath));

        Assert.Equal($$"""
        {
            "cache": "/path/to/cache",
            "color": true,
            "git": "git",
            "github_proxy": "",
            "go_module_proxy": "https://goproxy.io",
            "https_proxy": "",
            "noproxy": "",
            "proxy": "",
            "script_shell": "{{(OperatingSystem.IsWindows() ? "cmd.exe" : "/bin/sh")}}"
        }
        """, fileSystem.File.ReadAllText(s_runtimeConfigPath));
    }

    [Fact]
    public async Task ConfigSet_MultipleItems_Passes()
    {
        // Arrange.
        RuntimeConfig initialRuntimeConfig = new();

        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { s_runtimeConfigPath, new MockFileData(initialRuntimeConfig.ToBytes()) },
        });

        Mock<IContext> context = new();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        Lip lip = new(initialRuntimeConfig, context.Object);

        Dictionary<string, string> keyValuePairs = new()
        {
            { "cache", "/path/to/cache" },
            { "color", "false" },
            { "git", "git" },
            { "github_proxy", "https://github.com" },
            { "go_module_proxy", "https://goproxy.io" },
            { "https_proxy", "https://proxy.com" },
            { "noproxy", "localhost" },
            { "proxy", "http://proxy.com" },
            { "script_shell", "/bin/bash" },
        };

        // Act.
        await lip.ConfigSet(keyValuePairs, new Lip.ConfigSetArgs());

        // Assert.
        Assert.True(fileSystem.File.Exists(s_runtimeConfigPath));

        Assert.Equal($$"""
        {
            "cache": "/path/to/cache",
            "color": false,
            "git": "git",
            "github_proxy": "https://github.com",
            "go_module_proxy": "https://goproxy.io",
            "https_proxy": "https://proxy.com",
            "noproxy": "localhost",
            "proxy": "http://proxy.com",
            "script_shell": "/bin/bash"
        }
        """, fileSystem.File.ReadAllText(s_runtimeConfigPath));
    }

    [Fact]
    public async Task ConfigSet_NoItems_ThrowsArgumentException()
    {
        // Arrange.
        RuntimeConfig initialRuntimeConfig = new();

        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { s_runtimeConfigPath, new MockFileData(initialRuntimeConfig.ToBytes()) },
        });

        Mock<IContext> context = new();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        Lip lip = new(initialRuntimeConfig, context.Object);

        Dictionary<string, string> keyValuePairs = [];

        // Act.
        ArgumentException argumentException = await Assert.ThrowsAsync<ArgumentException>(() => lip.ConfigSet(keyValuePairs, new Lip.ConfigSetArgs()));

        // Assert.
        Assert.Equal("No configuration items provided. (Parameter 'keyValuePairs')", argumentException.Message);
    }

    [Fact]
    public async Task ConfigSet_UnknownItem_ThrowsArgumentException()
    {
        // Arrange.
        RuntimeConfig initialRuntimeConfig = new();

        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { s_runtimeConfigPath, new MockFileData(initialRuntimeConfig.ToBytes()) },
        });

        Mock<IContext> context = new();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        Lip lip = new(initialRuntimeConfig, context.Object);

        Dictionary<string, string> keyValuePairs = new()
        {
            { "unknown", "value" },
        };

        Lip.ConfigSetArgs args = new();

        // Act.
        ArgumentException argumentException = await Assert.ThrowsAsync<ArgumentException>(() => lip.ConfigSet(keyValuePairs, args));

        // Assert.
        Assert.Equal("Unknown configuration key: 'unknown'. (Parameter 'keyValuePairs')", argumentException.Message);
    }

    [Fact]
    public async Task ConfigSet_PartialUnknownItem_ThrowsArgumentException()
    {
        // Arrange.
        RuntimeConfig initialRuntimeConfig = new();
        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { s_runtimeConfigPath, new MockFileData(initialRuntimeConfig.ToBytes()) },
        });

        Mock<IContext> context = new();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        Lip lip = new(initialRuntimeConfig, context.Object);

        Dictionary<string, string> keyValuePairs = new()
        {
            { "cache", "/path/to/cache" },
            { "unknown", "value" },
            { "color", "false" },
        };

        // Act & Assert.
        ArgumentException argumentException = await Assert.ThrowsAsync<ArgumentException>(
            () => lip.ConfigSet(keyValuePairs, new Lip.ConfigSetArgs()));
        Assert.Equal("Unknown configuration key: 'unknown'. (Parameter 'keyValuePairs')", argumentException.Message);
    }
}
