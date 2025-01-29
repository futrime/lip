using System.IO.Abstractions.TestingHelpers;
using System.Runtime.InteropServices;
using Flurl;
using Lip.Context;
using Moq;

namespace Lip.Tests;

public class LipCacheTests
{
    private static readonly string s_cacheDir = OperatingSystem.IsWindows()
        ? Path.Join("C:", "path", "to", "cache")
        : Path.Join("/", "path", "to", "cache");

    [Fact]
    public async Task CacheAdd_ValidPackageSpecifier_AddsCache()
    {
        // Arrange.
        string packageManifestData = $$"""
            {
                "format_version": 3,
                "format_uuid": "289f771f-2c9a-4d73-9f3f-8492495a924d",
                "tooth": "example.com/repo",
                "version": "1.0.0",
                "variants": [
                    {
                        "platform": "{{RuntimeInformation.RuntimeIdentifier}}",
                        "assets": [
                            {
                                "type": "self",
                            },
                            {
                                "type": "zip",
                            },
                            {
                                "type": "zip",
                                "urls": [
                                    "https://example.com/test.file"
                                ]
                            }
                        ]
                    }
                ]
            }
            """;

        RuntimeConfig runtimeConfig = new()
        {
            Cache = s_cacheDir,
        };

        MockFileSystem fileSystem = new();

        Mock<IDownloader> downloader = new();
        downloader.Setup(d => d.DownloadFile(
            Url.Parse("https://example.com/test.file"),
            Path.Join(s_cacheDir, "downloaded_files", "https%3A%2F%2Fexample.com%2Ftest.file")))
            .Callback<Url, string>((_, dest) =>
            {
                fileSystem.AddFile(dest, new MockFileData("test"));
            });

        Mock<IGit> git = new();
        git.Setup(g => g.Clone(
            "https://example.com/repo",
            Path.Join(s_cacheDir, "git_repos", "https%3A%2F%2Fexample.com%2Frepo", "v1.0.0"),
            "v1.0.0",
            1))
            .Callback<string, string, string?, int?>((_, dest, __, ___) =>
            {
                fileSystem.AddFile(Path.Join(dest, "tooth.json"), new MockFileData(packageManifestData));
            });

        Mock<IContext> context = new();
        context.SetupGet(c => c.Downloader).Returns(downloader.Object);
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);
        context.SetupGet(c => c.Git).Returns(git.Object);

        Lip lip = new(runtimeConfig, context.Object);

        // Act.
        await lip.CacheAdd("example.com/repo@1.0.0", new());

        // Assert.
        Assert.True(fileSystem.File.Exists(Path.Join(s_cacheDir, "downloaded_files", "https%3A%2F%2Fexample.com%2Ftest.file")));
        Assert.True(fileSystem.File.Exists(Path.Join(s_cacheDir, "git_repos", "https%3A%2F%2Fexample.com%2Frepo", "v1.0.0", "tooth.json")));
        Assert.True(fileSystem.File.Exists(Path.Join(s_cacheDir, "package_manifests", "example.com%2Frepo%401.0.0.json")));
        Assert.Equal(packageManifestData, fileSystem.File.ReadAllText(Path.Join(s_cacheDir, "package_manifests", "example.com%2Frepo%401.0.0.json")));
    }

    [Fact]
    public async Task CacheAdd_NullVariant_AddsCache()
    {
        // Arrange.
        string packageManifestData = $$"""
            {
                "format_version": 3,
                "format_uuid": "289f771f-2c9a-4d73-9f3f-8492495a924d",
                "tooth": "example.com/repo",
                "version": "1.0.0"
            }
            """;

        RuntimeConfig runtimeConfig = new()
        {
            Cache = s_cacheDir,
        };

        MockFileSystem fileSystem = new();

        Mock<IGit> git = new();
        git.Setup(g => g.Clone(
            "https://example.com/repo",
            Path.Join(s_cacheDir, "git_repos", "https%3A%2F%2Fexample.com%2Frepo", "v1.0.0"),
            "v1.0.0",
            1))
            .Callback<string, string, string?, int?>((_, dest, __, ___) =>
            {
                fileSystem.AddFile(Path.Join(dest, "tooth.json"), new MockFileData(packageManifestData));
            });

        Mock<IContext> context = new();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);
        context.SetupGet(c => c.Git).Returns(git.Object);

        Lip lip = new(runtimeConfig, context.Object);

        // Act.
        await lip.CacheAdd("example.com/repo@1.0.0", new());

        // Assert.
        Assert.True(fileSystem.File.Exists(Path.Join(s_cacheDir, "git_repos", "https%3A%2F%2Fexample.com%2Frepo", "v1.0.0", "tooth.json")));
        Assert.True(fileSystem.File.Exists(Path.Join(s_cacheDir, "package_manifests", "example.com%2Frepo%401.0.0.json")));
        Assert.Equal(packageManifestData, fileSystem.File.ReadAllText(Path.Join(s_cacheDir, "package_manifests", "example.com%2Frepo%401.0.0.json")));
    }

    [Fact]
    public async Task CacheAdd_MismatchedToothPath_ThrowsInvalidOperationException()
    {
        // Arrange.
        string packageManifestData = $$"""
            {
                "format_version": 3,
                "format_uuid": "289f771f-2c9a-4d73-9f3f-8492495a924d",
                "tooth": "example.com/other-repo",
                "version": "1.0.0"
            }
            """;

        RuntimeConfig runtimeConfig = new()
        {
            Cache = s_cacheDir,
        };

        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { Path.Join(s_cacheDir, "package_manifests", "example.com%2Frepo%401.0.0.json"), new MockFileData(packageManifestData) },
        });

        Mock<IContext> context = new();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        Lip lip = new(runtimeConfig, context.Object);

        // Act & Assert.
        await Assert.ThrowsAsync<InvalidOperationException>(() => lip.CacheAdd("example.com/repo@1.0.0", new()));
    }

    [Fact]
    public async Task CacheAdd_MismatchedVersion_ThrowsInvalidOperationException()
    {
        // Arrange.
        string packageManifestData = $$"""
            {
                "format_version": 3,
                "format_uuid": "289f771f-2c9a-4d73-9f3f-8492495a924d",
                "tooth": "example.com/repo",
                "version": "2.0.0"
            }
            """;

        RuntimeConfig runtimeConfig = new()
        {
            Cache = s_cacheDir,
        };

        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { Path.Join(s_cacheDir, "package_manifests", "example.com%2Frepo%401.0.0.json"), new MockFileData(packageManifestData) },
        });

        Mock<IContext> context = new();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        Lip lip = new(runtimeConfig, context.Object);

        // Act & Assert.
        await Assert.ThrowsAsync<InvalidOperationException>(() => lip.CacheAdd("example.com/repo@1.0.0", new()));
    }

    [Fact]
    public async Task CacheClean_WhenCalled_CleansCache()
    {
        // Arrange.
        RuntimeConfig runtimeConfig = new()
        {
            Cache = s_cacheDir,
        };

        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { Path.Join(s_cacheDir, "file"), new MockFileData("content") },
        });

        Mock<IContext> context = new();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        Lip lip = new(runtimeConfig, context.Object);

        // Act.
        await lip.CacheClean(new());

        // Assert.
        Assert.False(fileSystem.File.Exists(Path.Join(s_cacheDir, "file")));
    }

    [Fact]
    public async Task CacheList_WhenCalled_ReturnsCacheInfo()
    {
        // Arrange.
        RuntimeConfig runtimeConfig = new()
        {
            Cache = s_cacheDir,
        };

        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { Path.Join(s_cacheDir, "downloaded_files", "https%3A%2F%2Fexample.com%2Ftest.file"), new MockFileData("test") },
            { Path.Join(s_cacheDir, "git_repos", "https%3A%2F%2Fexample.com%2Frepo", "v1.0.0"), new MockDirectoryData() },
            { Path.Join(s_cacheDir, "package_manifests", "example.com%2Frepo%401.0.0.json"), new MockFileData("test") },
        });

        Mock<IContext> context = new();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        Lip lip = new(runtimeConfig, context.Object);

        // Act.
        Lip.CacheListResult result = await lip.CacheList(new());

        // Assert.
        Assert.Equal(new[] { "https://example.com/test.file" }, result.DownloadedFiles);
        Assert.Equal(new[] { "https://example.com/repo v1.0.0" }, result.GitRepos);
        Assert.Equal(new[] { "example.com/repo@1.0.0" }, result.PackageManifestFiles);
    }
}
