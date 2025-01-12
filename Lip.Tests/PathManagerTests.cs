using System.IO.Abstractions.TestingHelpers;

namespace Lip.Tests;

public class PathManagerTests
{
    private static readonly string s_cacheDir = OperatingSystem.IsWindows()
        ? Path.Join("C:", "path", "to", "cache")
        : Path.Join("/", "path", "to", "cache");
    private static readonly string s_workingDir = OperatingSystem.IsWindows()
        ? Path.Join("C:", "current", "dir")
        : Path.Join("/", "current", "dir");

    [Fact]
    public void GetBaseAssetCacheDir_WithoutRuntimeConfig_ThrowsInvalidOperationException()
    {
        // Arrange.
        MockFileSystem fileSystem = new();

        PathManager pathManager = new(fileSystem);

        // Act.
        InvalidOperationException invalidOperationException = Assert.Throws<InvalidOperationException>(() => pathManager.BaseAssetCacheDir);

        // Assert.
        Assert.Equal("Runtime configuration is not set.", invalidOperationException.Message);
    }

    [Fact]
    public void GetBaseAssetCacheDir_WithRuntimeConfig_ReturnsCorrectPath()
    {
        // Arrange.
        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { s_cacheDir, new MockDirectoryData() },
        });

        PathManager pathManager = new(fileSystem, baseCacheDir: s_cacheDir);

        // Act.
        string baseAssetCacheDir = pathManager.BaseAssetCacheDir;

        // Assert.
        Assert.Equal(Path.Join(s_cacheDir, "assets"), baseAssetCacheDir);
    }

    [Fact]
    public void GetBaseCacheDir_WithoutRuntimeConfig_ThrowsInvalidOperationException()
    {
        // Arrange.
        MockFileSystem fileSystem = new();
        PathManager pathManager = new(fileSystem);

        // Act.
        InvalidOperationException invalidOperationException = Assert.Throws<InvalidOperationException>(() => pathManager.BaseCacheDir);

        // Assert.
        Assert.Equal("Runtime configuration is not set.", invalidOperationException.Message);
    }

    [Fact]
    public void GetBaseCacheDir_WithRuntimeConfig_ReturnsFullPath()
    {
        // Arrange.
        MockFileSystem fileSystem = new();
        PathManager pathManager = new(fileSystem, baseCacheDir: s_cacheDir);

        // Act.
        string baseCacheDir = pathManager.BaseCacheDir;

        // Assert.
        Assert.Equal(s_cacheDir, baseCacheDir);
    }

    [Fact]
    public void GetBasePackageCacheDir_WithoutRuntimeConfig_ThrowsInvalidOperationException()
    {
        // Arrange.
        MockFileSystem fileSystem = new();
        PathManager pathManager = new(fileSystem);

        // Act.
        InvalidOperationException invalidOperationException = Assert.Throws<InvalidOperationException>(() => pathManager.BasePackageCacheDir);

        // Assert.
        Assert.Equal("Runtime configuration is not set.", invalidOperationException.Message);
    }

    [Fact]
    public void GetBasePackageCacheDir_WithRuntimeConfig_ReturnsCorrectPath()
    {
        // Arrange.
        MockFileSystem fileSystem = new();
        PathManager pathManager = new(fileSystem, baseCacheDir: s_cacheDir);

        // Act.
        string basePackageCacheDir = pathManager.BasePackageCacheDir;

        // Assert.
        Assert.Equal(Path.Join(s_cacheDir, "packages"), basePackageCacheDir);
    }

    [Fact]
    public void GetPackageManifestPath_WhenCalled_ReturnsCorrectPath()
    {
        // Arrange.
        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { s_workingDir, new MockDirectoryData() },
        }, s_workingDir);
        PathManager pathManager = new(fileSystem);

        // Act.
        string manifestPath = pathManager.PackageManifestPath;

        // Assert.
        Assert.Equal(Path.Join(s_workingDir, "tooth.json"), manifestPath);
    }

    [Fact]
    public void GetPackageRecordPath_WhenCalled_ReturnsCorrectPath()
    {
        // Arrange.
        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { s_workingDir, new MockDirectoryData() },
        }, s_workingDir);
        PathManager pathManager = new(fileSystem);

        // Act.
        string recordPath = pathManager.PackageRecordPath;

        // Assert.
        Assert.Equal(Path.Join(s_workingDir, "tooth.lock"), recordPath);
    }

    [Fact]
    public void GetRuntimeConfigPath_WhenCalled_ReturnsCorrectPath()
    {
        // Arrange.
        MockFileSystem fileSystem = new();
        PathManager pathManager = new(fileSystem);

        // Act.
        string runtimeConfigPath = pathManager.RuntimeConfigPath;

        // Assert.
        Assert.Equal(
            Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "lip", "runtime_config.json"),
            runtimeConfigPath);
    }

    [Fact]
    public void GetWorkingDir_WhenCalled_ReturnsCurrentDirectory()
    {
        // Arrange.
        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { s_workingDir, new MockDirectoryData() },
        }, s_workingDir);
        PathManager pathManager = new(fileSystem);

        // Act.
        string workingDir = pathManager.WorkingDir;

        // Assert.
        Assert.Equal(s_workingDir, workingDir);
    }

    [Theory]
    [InlineData("https://example.com/asset?v=1", "https%3A%2F%2Fexample.com%2Fasset%3Fv%3D1")]
    [InlineData("/path/to/asset", "%2Fpath%2Fto%2Fasset")]
    [InlineData("", "")]
    [InlineData(" ", "%20")]
    [InlineData("!@#$%^&*()", "%21%40%23%24%25%5E%26%2A%28%29")]
    [InlineData("../path/test", "..%2Fpath%2Ftest")]
    [InlineData("\\special\\chars", "%5Cspecial%5Cchars")]
    public void GetAssetCacheDir_ArbitraryString_ReturnsEscapedPath(string assetUrl, string expectedAssetDirName)
    {
        // Arrange.
        MockFileSystem fileSystem = new();
        PathManager pathManager = new(fileSystem, baseCacheDir: s_cacheDir);

        // Act.
        string assetCacheDir = pathManager.GetAssetCacheDir(assetUrl);

        // Assert.
        Assert.Equal(
            Path.Join(s_cacheDir, "assets", expectedAssetDirName),
            assetCacheDir);
    }

    [Theory]
    [InlineData("https://example.com/asset?v=1", "https%3A%2F%2Fexample.com%2Fasset%3Fv%3D1")]
    [InlineData("/path/to/asset", "%2Fpath%2Fto%2Fasset")]
    [InlineData("", "")]
    [InlineData(" ", "%20")]
    [InlineData("!@#$%^&*()", "%21%40%23%24%25%5E%26%2A%28%29")]
    [InlineData("../path/test", "..%2Fpath%2Ftest")]
    [InlineData("\\special\\chars", "%5Cspecial%5Cchars")]
    public void GetPackageCacheDir_ArbitraryString_ReturnsEscapedPath(string packageName, string expectedPackageDirName)
    {
        // Arrange.
        MockFileSystem fileSystem = new();
        PathManager pathManager = new(fileSystem, baseCacheDir: s_cacheDir);

        // Act.
        string packageCacheDir = pathManager.GetPackageCacheDir(packageName);

        // Assert.
        Assert.Equal(
            Path.Join(s_cacheDir, "packages", expectedPackageDirName),
            packageCacheDir);
    }
}
