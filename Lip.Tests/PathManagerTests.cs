using System.IO.Abstractions.TestingHelpers;
using Flurl;

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
    public void GetCurrentPackageManifestPath_WhenCalled_ReturnsCorrectPath()
    {
        // Arrange.
        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { s_workingDir, new MockDirectoryData() },
        }, s_workingDir);
        PathManager pathManager = new(fileSystem);

        // Act.
        string manifestPath = pathManager.CurrentPackageManifestPath;

        // Assert.
        Assert.Equal(Path.Join(s_workingDir, "tooth.json"), manifestPath);
    }

    [Fact]
    public void GetCurrentPackageLockPath_WhenCalled_ReturnsCorrectPath()
    {
        // Arrange.
        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { s_workingDir, new MockDirectoryData() },
        }, s_workingDir);
        PathManager pathManager = new(fileSystem);

        // Act.
        string recordPath = pathManager.CurrentPackageLockPath;

        // Assert.
        Assert.Equal(Path.Join(s_workingDir, "tooth_lock.json"), recordPath);
    }

    [Fact]
    public void GetPackageManifestFileName_WhenCalled_ReturnsCorrectFileName()
    {
        // Arrange.
        MockFileSystem fileSystem = new();
        PathManager pathManager = new(fileSystem);

        // Act.
        string manifestFileName = pathManager.PackageManifestFileName;

        // Assert.
        Assert.Equal("tooth.json", manifestFileName);
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
    [InlineData("https://example.com/path/to/asset", "https%3A%2F%2Fexample.com%2Fpath%2Fto%2Fasset")]
    [InlineData("https://example.com/", "https%3A%2F%2Fexample.com%2F")]
    public void GetDownloadedFileCachePath_ArbitraryString_ReturnsEscapedPath(string url, string expectedFileName)
    {
        // Arrange.
        MockFileSystem fileSystem = new();
        PathManager pathManager = new(fileSystem, baseCacheDir: s_cacheDir);

        // Act.
        string cachePath = pathManager.GetDownloadedFileCachePath(Url.Parse(url));

        // Assert.
        Assert.Equal(
            Path.Join(s_cacheDir, "downloaded_files", expectedFileName),
            cachePath);
    }

    [Theory]
    [InlineData("https://example.com/asset?v=1", "v1", "https%3A%2F%2Fexample.com%2Fasset%3Fv%3D1", "v1")]
    [InlineData("/path/to/asset", "tag", "%2Fpath%2Fto%2Fasset", "tag")]
    [InlineData("", "default", "", "default")]
    [InlineData(" ", "space", "%20", "space")]
    [InlineData("!@#$%^&*()", "special", "%21%40%23%24%25%5E%26%2A%28%29", "special")]
    [InlineData("../path/test", "test", "..%2Fpath%2Ftest", "test")]
    [InlineData("\\special\\chars", "chars", "%5Cspecial%5Cchars", "chars")]
    public void GetGitRepoDirCachePath_ArbitraryString_ReturnsEscapedPath(
        string repoUrl,
        string tag,
        string expectedRepoDir,
        string expectedTagDir)
    {
        // Arrange.
        MockFileSystem fileSystem = new();
        PathManager pathManager = new(fileSystem, baseCacheDir: s_cacheDir);

        // Act.
        string repoCacheDir = pathManager.GetGitRepoDirCachePath(repoUrl, tag);

        // Assert.
        Assert.Equal(
            Path.Join(s_cacheDir, "git_repos", expectedRepoDir, expectedTagDir),
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
    public void GetPackageManifestCachePath_ArbitraryString_ReturnsEscapedPath(string toothPath, string expectedFileName)
    {
        // Arrange.
        MockFileSystem fileSystem = new();
        PathManager pathManager = new(fileSystem, baseCacheDir: s_cacheDir);

        // Act.
        string packageCacheDir = pathManager.GetPackageManifestCachePath(toothPath);

        // Assert.
        Assert.Equal(
            Path.Join(s_cacheDir, "package_manifests", expectedFileName),
            packageCacheDir);
    }

    [Fact]
    public void GetPackageManifestPath_WhenCalled_ReturnsCorrectPath()
    {
        // Arrange.
        string baseDir = OperatingSystem.IsWindows()
            ? Path.Join("C:", "path", "to", "cache")
            : Path.Join("/", "path", "to", "cache");
        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { baseDir, new MockDirectoryData() },
        }, baseDir);
        PathManager pathManager = new(fileSystem);

        // Act.
        string manifestPath = pathManager.GetPackageManifestPath(baseDir);

        // Assert.
        Assert.Equal(Path.Join(baseDir, "tooth.json"), manifestPath);
    }
}
