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
    public void GetBaseCacheDir_WithoutBaseCacheDir_Throws()
    {
        // Arrange.
        MockFileSystem fileSystem = new();
        PathManager pathManager = new(fileSystem);

        // Act & assert.
        InvalidOperationException invalidOperationException = Assert.Throws<InvalidOperationException>(() => pathManager.BaseCacheDir);
        Assert.Equal("Base cache directory is not provided.", invalidOperationException.Message);
    }

    [Fact]
    public void GetBaseCacheDir_WithBaseCacheDir_ReturnsFullPath()
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
    public void GetBaseDownloadedFileCacheDir_WithoutBaseCacheDir_Throws()
    {
        // Arrange.
        MockFileSystem fileSystem = new();

        PathManager pathManager = new(fileSystem);

        // Act & assert.
        InvalidOperationException invalidOperationException = Assert.Throws<InvalidOperationException>(() => pathManager.BaseDownloadedFileCacheDir);
        Assert.Equal("Base cache directory is not provided.", invalidOperationException.Message);
    }

    [Fact]
    public void GetBaseDownloadedFileCacheDir_WithBaseCacheDir_ReturnsCorrectPath()
    {
        // Arrange.
        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { s_cacheDir, new MockDirectoryData() },
        });

        PathManager pathManager = new(fileSystem, baseCacheDir: s_cacheDir);

        // Act.
        string baseAssetCacheDir = pathManager.BaseDownloadedFileCacheDir;

        // Assert.
        Assert.Equal(Path.Join(s_cacheDir, "downloaded_files"), baseAssetCacheDir);
    }

    [Fact]
    public void GetBaseGitRepoCacheDir_WithoutBaseCacheDir_Throws()
    {
        // Arrange.
        MockFileSystem fileSystem = new();
        PathManager pathManager = new(fileSystem);

        // Act & assert.
        InvalidOperationException invalidOperationException = Assert.Throws<InvalidOperationException>(() => pathManager.BaseGitRepoCacheDir);
        Assert.Equal("Base cache directory is not provided.", invalidOperationException.Message);
    }

    [Fact]
    public void GetBaseGitRepoCacheDir_WithBaseCacheDir_ReturnsCorrectPath()
    {
        // Arrange.
        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { s_cacheDir, new MockDirectoryData() },
        });

        PathManager pathManager = new(fileSystem, baseCacheDir: s_cacheDir);

        // Act.
        string baseGitRepoCacheDir = pathManager.BaseGitRepoCacheDir;

        // Assert.
        Assert.Equal(Path.Join(s_cacheDir, "git_repos"), baseGitRepoCacheDir);
    }

    [Fact]
    public void GetBabsePackageManifestCacheDir_WithoutBaseCacheDir_Throws()
    {
        // Arrange.
        MockFileSystem fileSystem = new();
        PathManager pathManager = new(fileSystem);

        // Act & assert.
        InvalidOperationException invalidOperationException = Assert.Throws<InvalidOperationException>(() => pathManager.BasePackageManifestCacheDir);
        Assert.Equal("Base cache directory is not provided.", invalidOperationException.Message);
    }

    [Fact]
    public void GetBasePackageManifestCacheDir_WithBaseCacheDir_ReturnsCorrectPath()
    {
        // Arrange.
        MockFileSystem fileSystem = new();
        PathManager pathManager = new(fileSystem, baseCacheDir: s_cacheDir);

        // Act.
        string basePackageCacheDir = pathManager.BasePackageManifestCacheDir;

        // Assert.
        Assert.Equal(Path.Join(s_cacheDir, "package_manifests"), basePackageCacheDir);
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
    public void GetPackageLockPath_WhenCalled_ReturnsCorrectPath()
    {
        // Arrange.
        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { s_workingDir, new MockDirectoryData() },
        }, s_workingDir);
        PathManager pathManager = new(fileSystem);

        // Act.
        string recordPath = pathManager.PackageLockPath;

        // Assert.
        Assert.Equal(Path.Join(s_workingDir, "tooth_lock.json"), recordPath);
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
            Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "lip", "liprc.json"),
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
    public void GetDownloadedFileCacheDir_ArbitraryString_ReturnsEscapedPath(string url, string expectedFileName)
    {
        // Arrange.
        MockFileSystem fileSystem = new();
        PathManager pathManager = new(fileSystem, baseCacheDir: s_cacheDir);

        // Act.
        string cachePath = pathManager.GetDownloadedFileCachePath(url);

        // Assert.
        Assert.Equal(
            Path.Join(s_cacheDir, "downloaded_files", expectedFileName),
            cachePath);
    }

    [Theory]
    [InlineData("https://example.com/asset?v=1", "https%3A%2F%2Fexample.com%2Fasset%3Fv%3D1")]
    [InlineData("/path/to/asset", "%2Fpath%2Fto%2Fasset")]
    [InlineData("", "")]
    [InlineData(" ", "%20")]
    [InlineData("!@#$%^&*()", "%21%40%23%24%25%5E%26%2A%28%29")]
    [InlineData("../path/test", "..%2Fpath%2Ftest")]
    [InlineData("\\special\\chars", "%5Cspecial%5Cchars")]
    public void GetGitRepoCachePath_ArbitraryString_ReturnsEscapedPath(string repoUrl, string expectedDirName)
    {
        // Arrange.
        MockFileSystem fileSystem = new();
        PathManager pathManager = new(fileSystem, baseCacheDir: s_cacheDir);

        // Act.
        string repoCacheDir = pathManager.GetGitRepoCachePath(repoUrl);

        // Assert.
        Assert.Equal(
            Path.Join(s_cacheDir, "git_repos", expectedDirName),
            repoCacheDir);
    }

    [Theory]
    [InlineData("https://example.com/asset?v=1", "https%3A%2F%2Fexample.com%2Fasset%3Fv%3D1.json")]
    [InlineData("/path/to/asset", "%2Fpath%2Fto%2Fasset.json")]
    [InlineData("", ".json")]
    [InlineData(" ", "%20.json")]
    [InlineData("!@#$%^&*()", "%21%40%23%24%25%5E%26%2A%28%29.json")]
    [InlineData("../path/test", "..%2Fpath%2Ftest.json")]
    [InlineData("\\special\\chars", "%5Cspecial%5Cchars.json")]
    public void GetPackageManifestCachePath_ArbitraryString_ReturnsEscapedPath(string packageName, string expectedFileName)
    {
        // Arrange.
        MockFileSystem fileSystem = new();
        PathManager pathManager = new(fileSystem, baseCacheDir: s_cacheDir);

        // Act.
        string packageCacheDir = pathManager.GetPackageManifestCachePath(packageName);

        // Assert.
        Assert.Equal(
            Path.Join(s_cacheDir, "package_manifests", expectedFileName),
            packageCacheDir);
    }
}
