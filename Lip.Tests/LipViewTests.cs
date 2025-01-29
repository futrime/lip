using System.IO.Abstractions.TestingHelpers;
using System.Runtime.InteropServices;
using Lip.Context;
using Moq;

namespace Lip.Tests;

public class LipViewTests
{
    private static readonly string s_cacheDir = OperatingSystem.IsWindows()
        ? Path.Join("C:", "path", "to", "cache")
        : Path.Join("/", "path", "to", "cache");

    private static readonly string s_packageManifestData = $$"""
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
                            "type": "self"
                        },
                        {
                            "type": "zip"
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
        """.ReplaceLineEndings();

    [Fact]
    public async Task View_EmptyPath_ReturnsFullManifest()
    {
        // Arrange.
        RuntimeConfig runtimeConfig = new()
        {
            Cache = s_cacheDir,
        };

        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { Path.Join(s_cacheDir, "package_manifests", "example.com%2Frepo%401.0.0.json"), new MockFileData(s_packageManifestData) },
        });

        Mock<IContext> context = new();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        Lip lip = new(runtimeConfig, context.Object);

        // Act.
        string result = await lip.View("example.com/repo@1.0.0", string.Empty, new());

        // Assert.
        Assert.Equal(s_packageManifestData, result);
    }

    [Theory]
    [InlineData("format_version", "3")]
    [InlineData("format_uuid", "289f771f-2c9a-4d73-9f3f-8492495a924d")]
    [InlineData("variants[0].assets[2].urls[0]", "https://example.com/test.file")]
    public async Task View_ValidPath_ReturnsField(string path, string expectedField)
    {
        // Arrange.
        RuntimeConfig runtimeConfig = new()
        {
            Cache = s_cacheDir,
        };

        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { Path.Join(s_cacheDir, "package_manifests", "example.com%2Frepo%401.0.0.json"), new MockFileData(s_packageManifestData) },
        });

        Mock<IContext> context = new();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        Lip lip = new(runtimeConfig, context.Object);

        // Act.
        string result = await lip.View("example.com/repo@1.0.0", path, new());

        // Assert.
        Assert.Equal(expectedField, result);
    }

    [Fact]
    public async Task View_ComplexField_ReturnsField()
    {
        // Arrange.
        RuntimeConfig runtimeConfig = new()
        {
            Cache = s_cacheDir,
        };

        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { Path.Join(s_cacheDir, "package_manifests", "example.com%2Frepo%401.0.0.json"), new MockFileData(s_packageManifestData) },
        });

        Mock<IContext> context = new();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        Lip lip = new(runtimeConfig, context.Object);

        // Act.
        string result = await lip.View("example.com/repo@1.0.0", "variants[0].assets[0]", new());

        // Assert.
        Assert.Equal(@"{type: ""self""}", result);
    }

    [Fact]
    public async Task View_UnmatchedToothPath_ThrowsInvalidOperationException()
    {
        // Arrange.
        RuntimeConfig runtimeConfig = new()
        {
            Cache = s_cacheDir,
        };

        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { Path.Join(s_cacheDir, "package_manifests", "example.com%2Finvalid%401.0.0.json"), new MockFileData(s_packageManifestData) },
        });

        Mock<IContext> context = new();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        Lip lip = new(runtimeConfig, context.Object);

        // Act & Assert.
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await lip.View("example.com/invalid@1.0.0", string.Empty, new()));
    }

    [Fact]
    public async Task View_UnmatchedVersion_ThrowsInvalidOperationException()
    {
        // Arrange.
        RuntimeConfig runtimeConfig = new()
        {
            Cache = s_cacheDir,
        };

        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { Path.Join(s_cacheDir, "package_manifests", "example.com%2Frepo%402.0.0.json"), new MockFileData(s_packageManifestData) },
        });

        Mock<IContext> context = new();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        Lip lip = new(runtimeConfig, context.Object);

        // Act & Assert.
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await lip.View("example.com/repo@2.0.0", string.Empty, new()));
    }

    [Fact]
    public async Task View_InvalidPath_ThrowsFormatException()
    {
        // Arrange.
        RuntimeConfig runtimeConfig = new()
        {
            Cache = s_cacheDir,
        };

        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { Path.Join(s_cacheDir, "package_manifests", "example.com%2Frepo%401.0.0.json"), new MockFileData(s_packageManifestData) },
        });

        Mock<IContext> context = new();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        Lip lip = new(runtimeConfig, context.Object);

        // Act & Assert.
        await Assert.ThrowsAsync<FormatException>(async () => await lip.View("example.com/repo@1.0.0", "@#$%^", new()));
    }

    [Fact]
    public async Task View_PathNotFound_ReturnsEmpty()
    {
        // Arrange.
        RuntimeConfig runtimeConfig = new()
        {
            Cache = s_cacheDir,
        };

        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { Path.Join(s_cacheDir, "package_manifests", "example.com%2Frepo%401.0.0.json"), new MockFileData(s_packageManifestData) },
        });

        Mock<IContext> context = new();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        Lip lip = new(runtimeConfig, context.Object);

        // Act.
        string result = await lip.View("example.com/repo@1.0.0", "nonexistent", new());

        // Assert.
        Assert.Equal(string.Empty, result);
    }
}
