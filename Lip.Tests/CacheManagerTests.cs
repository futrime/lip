using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using Flurl;
using Lip.Context;
using Moq;
using Semver;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Writers;

namespace Lip.Tests;

public class CacheManagerTests
{
    private static readonly string s_cacheDir = OperatingSystem.IsWindows()
        ? Path.Join("C:", "path", "to", "cache")
        : Path.Join("/", "path", "to", "cache");

    [Fact]
    public void CacheSummary_Constructor_TrivialValues_Passes()
    {
        // Arrange.
        CacheManager.CacheSummary cacheSummary = new()
        {
            DownloadedFiles = [],
            GitRepos = [],
        };

        // Act.
        cacheSummary = cacheSummary with { };

        // Assert.
        Assert.Empty(cacheSummary.DownloadedFiles);
        Assert.Empty(cacheSummary.GitRepos);
    }

    [Fact]
    public async Task Clean_BaseCacheDirExists_Passes()
    {
        // Arrange.
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { s_cacheDir, new MockDirectoryData() }
        });

        var context = new Mock<IContext>();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        PathManager pathManager = new(fileSystem, s_cacheDir);

        CacheManager cacheManager = new(context.Object, pathManager);

        // Act.
        await cacheManager.Clean();

        // Assert.
        Assert.False(fileSystem.Directory.Exists(s_cacheDir));
    }

    [Fact]
    public async Task Clean_BaseCacheDirNotExists_Passes()
    {
        // Arrange.
        var fileSystem = new MockFileSystem();

        var context = new Mock<IContext>();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        PathManager pathManager = new(fileSystem, s_cacheDir);

        CacheManager cacheManager = new(context.Object, pathManager);

        // Act.
        await cacheManager.Clean();

        // Assert.
        Assert.False(fileSystem.Directory.Exists(s_cacheDir));
    }

    [Fact]
    public async Task GetDownloadedFile_ValidUrl_ReturnsStream()
    {
        // Arrange.
        var fileSystem = new MockFileSystem();

        var downloader = new Mock<IDownloader>();
        downloader.Setup(d => d.DownloadFile(
            Url.Parse("https://example.com/test.file"),
            Path.Join(s_cacheDir, "downloaded_files", "https%3A%2F%2Fexample.com%2Ftest.file")))
            .Callback<Url, string>((url, path) => fileSystem.AddFile(path, new MockFileData("test")));

        var context = new Mock<IContext>();
        context.SetupGet(c => c.Downloader).Returns(downloader.Object);
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        PathManager pathManager = new(fileSystem, s_cacheDir);

        CacheManager cacheManager = new(context.Object, pathManager);

        Url url = Url.Parse("https://example.com/test.file");

        // Act.
        IFileInfo file = await cacheManager.GetDownloadedFile(url);

        // Assert.
        Assert.Equal("test", new StreamReader(file.OpenRead()).ReadToEnd());
    }

    [Fact]
    public async Task GetDownloadedFIle_WithGitHubProxy_ReturnsStream()
    {
        // Arrange.
        var fileSystem = new MockFileSystem();

        var downloader = new Mock<IDownloader>();
        downloader.Setup(d => d.DownloadFile(
            Url.Parse("https://example.com/github-proxy/user/repo/test.file"),
            Path.Join(
                s_cacheDir,
                "downloaded_files",
                "https%3A%2F%2Fexample.com%2Fgithub-proxy%2Fuser%2Frepo%2Ftest.file")))
            .Callback<Url, string>((url, path) => fileSystem.AddFile(path, new MockFileData("test")));

        var context = new Mock<IContext>();
        context.SetupGet(c => c.Downloader).Returns(downloader.Object);
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        PathManager pathManager = new(fileSystem, s_cacheDir);

        CacheManager cacheManager = new(
            context.Object,
            pathManager,
            Url.Parse("https://example.com/github-proxy"));

        Url url = Url.Parse("https://github.com/user/repo/test.file");

        // Act.
        IFileInfo file = await cacheManager.GetDownloadedFile(url);

        // Assert.
        Assert.Equal("test", new StreamReader(file.OpenRead()).ReadToEnd());
    }

    [Fact]
    public async Task GetDownloadedFile_FileExists_ReturnsStream()
    {
        // Arrange.
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { Path.Join(s_cacheDir, "downloaded_files", "https%3A%2F%2Fexample.com%2Ftest.file"), new MockFileData("test") }
        });

        var context = new Mock<IContext>();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        PathManager pathManager = new(fileSystem, s_cacheDir);

        CacheManager cacheManager = new(context.Object, pathManager);

        Url url = Url.Parse("https://example.com/test.file");

        // Act.
        IFileInfo file = await cacheManager.GetDownloadedFile(url);

        // Assert.
        Assert.Equal("test", new StreamReader(file.OpenRead()).ReadToEnd());
    }

    [Fact]
    public async Task List_NoCacheDirectories_ReturnsEmptyList()
    {
        // Arrange.
        var fileSystem = new MockFileSystem();

        var context = new Mock<IContext>();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        PathManager pathManager = new(fileSystem, s_cacheDir);

        CacheManager cacheManager = new(context.Object, pathManager);

        // Act.
        CacheManager.CacheSummary listResult = await cacheManager.List();

        // Assert.
        Assert.Empty(listResult.DownloadedFiles);
        Assert.Empty(listResult.GitRepos);
    }

    [Fact]
    public async Task List_DownloadedFileExists_ReturnsListResult()
    {
        // Arrange.
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { Path.Join(s_cacheDir, "downloaded_files", "https%3A%2F%2Fexample.com%2Ftest.file"), new MockFileData("test") }
        });

        var context = new Mock<IContext>();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        PathManager pathManager = new(fileSystem, s_cacheDir);

        CacheManager cacheManager = new(context.Object, pathManager);

        // Act.
        CacheManager.CacheSummary listResult = await cacheManager.List();

        // Assert.
        Assert.Single(listResult.DownloadedFiles);
        Assert.Equal("https://example.com/test.file", listResult.DownloadedFiles.Keys.Single());
        Assert.Equal("test", new StreamReader(listResult.DownloadedFiles.Values.Single().OpenRead()).ReadToEnd());
        Assert.Empty(listResult.GitRepos);
    }

    [Fact]
    public async Task List_GitRepoExists_ReturnsListResult()
    {
        // Arrange.
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { Path.Join(s_cacheDir, "git_repos", "https%3A%2F%2Fexample.com%2Frepo", "v1.0.0"), new MockDirectoryData() }
        });

        var context = new Mock<IContext>();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        PathManager pathManager = new(fileSystem, s_cacheDir);

        CacheManager cacheManager = new(context.Object, pathManager);

        // Act.
        CacheManager.CacheSummary listResult = await cacheManager.List();

        // Assert.
        Assert.Empty(listResult.DownloadedFiles);
        Assert.Single(listResult.GitRepos);
        Assert.Equal("https://example.com/repo", listResult.GitRepos.Keys.Single().Url);
        Assert.Equal("v1.0.0", listResult.GitRepos.Keys.Single().Tag);
        Assert.Equal(Path.Join(s_cacheDir, "git_repos", "https%3A%2F%2Fexample.com%2Frepo", "v1.0.0"), listResult.GitRepos.Values.Single().FullName);
    }

    [Fact]
    public async Task List_PackageManifestFileExists_ReturnsListResult()
    {
        // Arrange.
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { Path.Join(s_cacheDir, "package_manifests", "example.com%2Frepo%401.0.0.json"), new MockFileData("test") }
        });

        var context = new Mock<IContext>();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        PathManager pathManager = new(fileSystem, s_cacheDir);

        CacheManager cacheManager = new(context.Object, pathManager);

        // Act.
        CacheManager.CacheSummary listResult = await cacheManager.List();

        // Assert.
        Assert.Empty(listResult.DownloadedFiles);
        Assert.Empty(listResult.GitRepos);
    }

    private static void CreateGoModuleArchive(
        MockFileSystem fileSystem,
        string goModulePath,
        SemVersion version,
        Dictionary<string, string> entries)
    {
        using FileSystemStream fileStream = fileSystem.File.Create("archive");

        using IWriter writer = WriterFactory.Open(fileStream, ArchiveType.Zip, new(CompressionType.Deflate));

        foreach (KeyValuePair<string, string> entry in entries)
        {
            using Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(entry.Value));

            string archiveEntryKey = $"{goModulePath}@v{version}{(version.Major >= 2 ? "+incompatible" : "")}/{entry.Key}";

            writer.Write(archiveEntryKey, stream);
        }
    }
}
