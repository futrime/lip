using Flurl;
using Microsoft.Extensions.Logging;
using Moq;
using Semver;
using SharpCompress.Common;
using SharpCompress.Writers;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Text;

namespace Lip.Core.Tests;

public class CacheManagerTests
{
    private static readonly string s_cacheDir = OperatingSystem.IsWindows()
        ? Path.Join("C:", "path", "to", "cache")
        : Path.Join("/", "path", "to", "cache");

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

        CacheManager cacheManager = new(context.Object, pathManager, [], []);

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

        CacheManager cacheManager = new(context.Object, pathManager, [], []);

        // Act.
        await cacheManager.Clean();

        // Assert.
        Assert.False(fileSystem.Directory.Exists(s_cacheDir));
    }

    [Fact]
    public async Task GetFileFromUrl_ValidUrl_Returns()
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

        CacheManager cacheManager = new(context.Object, pathManager, [], []);

        Url url = Url.Parse("https://example.com/test.file");

        // Act.
        IFileInfo file = await cacheManager.GetFileFromUrl(url);

        // Assert.
        Assert.Equal("test", new StreamReader(file.OpenRead()).ReadToEnd());
        downloader.Verify(d => d.DownloadFile(url, Path.Join(s_cacheDir, "downloaded_files", "https%3A%2F%2Fexample.com%2Ftest.file")), Times.Once);
    }

    [Fact]
    public async Task GetFileFromUrl_WithGitHubProxy_Returns()
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
            [Url.Parse("https://example.com/github-proxy")],
            []);

        Url url = Url.Parse("https://github.com/user/repo/test.file");

        // Act.
        IFileInfo file = await cacheManager.GetFileFromUrl(url);

        // Assert.
        Assert.Equal("test", new StreamReader(file.OpenRead()).ReadToEnd());
        downloader.Verify(d => d.DownloadFile(
            Url.Parse("https://example.com/github-proxy/user/repo/test.file"),
            Path.Join(
                s_cacheDir,
                "downloaded_files",
                "https%3A%2F%2Fexample.com%2Fgithub-proxy%2Fuser%2Frepo%2Ftest.file")), Times.Once);
    }

    [Fact]
    public async Task GetFileFromUrl_FileExists_Returns()
    {
        // Arrange.
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { Path.Join(s_cacheDir, "downloaded_files", "https%3A%2F%2Fexample.com%2Ftest.file"), new MockFileData("test") }
        });

        var context = new Mock<IContext>();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        PathManager pathManager = new(fileSystem, s_cacheDir);

        CacheManager cacheManager = new(context.Object, pathManager, [], []);

        Url url = Url.Parse("https://example.com/test.file");

        // Act.
        IFileInfo file = await cacheManager.GetFileFromUrl(url);

        // Assert.
        Assert.Equal("test", new StreamReader(file.OpenRead()).ReadToEnd());
        context.Verify(c => c.Downloader.DownloadFile(It.IsAny<Url>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetFileFromUrls_MultipleUrls_Returns()
    {
        // Arrange.
        var fileSystem = new MockFileSystem();

        var downloader = new Mock<IDownloader>();
        downloader.Setup(d => d.DownloadFile(
            Url.Parse("https://example.com/test.file"),
            Path.Join(s_cacheDir, "downloaded_files", "https%3A%2F%2Fexample.com%2Ftest.file")))
            .Callback<Url, string>((url, path) => fileSystem.AddFile(path, new MockFileData("test 1")));

        downloader.Setup(d => d.DownloadFile(
            Url.Parse("https://backup.example.com/test.file"),
            Path.Join(s_cacheDir, "downloaded_files", "https%3A%2F%2Fbackup.example.com%2Ftest.file")))
            .Callback<Url, string>((url, path) => fileSystem.AddFile(path, new MockFileData("test 2")));

        var context = new Mock<IContext>();
        context.SetupGet(c => c.Downloader).Returns(downloader.Object);
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        PathManager pathManager = new(fileSystem, s_cacheDir);

        CacheManager cacheManager = new(context.Object, pathManager, [], []);

        Url url1 = Url.Parse("https://example.com/test.file");
        Url url2 = Url.Parse("https://backup.example.com/test.file");

        // Act.
        IFileInfo file = await cacheManager.GetFileFromUrls([url1, url2]);

        // Assert.
        Assert.Equal("test 1", new StreamReader(file.OpenRead()).ReadToEnd());
        downloader.Verify(d => d.DownloadFile(url1, Path.Join(s_cacheDir, "downloaded_files", "https%3A%2F%2Fexample.com%2Ftest.file")), Times.Once);
        downloader.Verify(d => d.DownloadFile(url2, Path.Join(s_cacheDir, "downloaded_files", "https%3A%2F%2Fbackup.example.com%2Ftest.file")), Times.Never);
    }

    [Fact]
    public async Task GetFileFromUrls_FirstUrlFailed_Returns()
    {
        // Arrange.
        var fileSystem = new MockFileSystem();

        var downloader = new Mock<IDownloader>();
        downloader.Setup(d => d.DownloadFile(
            Url.Parse("https://example.com/test.file"),
            Path.Join(s_cacheDir, "downloaded_files", "https%3A%2F%2Fexample.com%2Ftest.file")))
            .Throws(new InvalidOperationException());

        downloader.Setup(d => d.DownloadFile(
            Url.Parse("https://backup.example.com/test.file"),
            Path.Join(s_cacheDir, "downloaded_files", "https%3A%2F%2Fbackup.example.com%2Ftest.file")))
            .Callback<Url, string>((url, path) => fileSystem.AddFile(path, new MockFileData("test 2")));

        var context = new Mock<IContext>();
        context.SetupGet(c => c.Downloader).Returns(downloader.Object);
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);
        context.SetupGet(c => c.Logger).Returns(new Mock<ILogger>().Object);

        PathManager pathManager = new(fileSystem, s_cacheDir);

        CacheManager cacheManager = new(context.Object, pathManager, [], []);

        Url url1 = Url.Parse("https://example.com/test.file");
        Url url2 = Url.Parse("https://backup.example.com/test.file");

        // Act.
        IFileInfo file = await cacheManager.GetFileFromUrls([url1, url2]);

        // Assert.
        Assert.Equal("test 2", new StreamReader(file.OpenRead()).ReadToEnd());
        downloader.Verify(d => d.DownloadFile(url1, Path.Join(s_cacheDir, "downloaded_files", "https%3A%2F%2Fexample.com%2Ftest.file")), Times.Once);
        downloader.Verify(d => d.DownloadFile(url2, Path.Join(s_cacheDir, "downloaded_files", "https%3A%2F%2Fbackup.example.com%2Ftest.file")), Times.Once);
    }

    [Fact]
    public async Task GetFileFromUrls_AllUrlsFailed_Throws()
    {
        // Arrange.
        var fileSystem = new MockFileSystem();

        var downloader = new Mock<IDownloader>();
        downloader.Setup(d => d.DownloadFile(
            Url.Parse("https://example.com/test.file"),
            Path.Join(s_cacheDir, "downloaded_files", "https%3A%2F%2Fexample.com%2Ftest.file")))
            .Throws(new InvalidOperationException());

        downloader.Setup(d => d.DownloadFile(
            Url.Parse("https://backup.example.com/test.file"),
            Path.Join(s_cacheDir, "downloaded_files", "https%3A%2F%2Fbackup.example.com%2Ftest.file")))
            .Throws(new InvalidOperationException());

        var context = new Mock<IContext>();
        context.SetupGet(c => c.Downloader).Returns(downloader.Object);
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);
        context.SetupGet(c => c.Logger).Returns(new Mock<ILogger>().Object);

        PathManager pathManager = new(fileSystem, s_cacheDir);

        CacheManager cacheManager = new(context.Object, pathManager, [], []);

        Url url1 = Url.Parse("https://example.com/test.file");
        Url url2 = Url.Parse("https://backup.example.com/test.file");

        // Act and assert.
        await Assert.ThrowsAsync<InvalidOperationException>(() => cacheManager.GetFileFromUrls([url1, url2]));
    }

    [Fact]
    public async Task GetPackageFileSource_NoAvailableRemoteSource_Throws()
    {
        // Arrange.
        var fileSystem = new MockFileSystem();

        var context = new Mock<IContext>();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        PathManager pathManager = new(fileSystem, s_cacheDir);

        CacheManager cacheManager = new(context.Object, pathManager, [], []);

        var packageSpecifier = PackageSpecifier.Parse("example.com/repo@1.0.0");

        // Act and assert.
        await Assert.ThrowsAsync<InvalidOperationException>(() => cacheManager.GetPackageFileSource(packageSpecifier));
    }

    [Fact]
    public async Task GetPackageFileSource_GitRepoCached_Returns()
    {
        // Arrange.
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { Path.Join(s_cacheDir, "git_repos", "https%3A%2F%2Fexample.com%2Frepo", "v1.0.0", "file"), new MockFileData("content") }
        });

        Mock<IContext> context = new();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);
        context.SetupGet(c => c.Git).Returns(new Mock<IGit>().Object);

        PathManager pathManager = new(fileSystem, s_cacheDir);

        CacheManager cacheManager = new(context.Object, pathManager, [], []);

        var packageSpecifier = PackageSpecifier.Parse("example.com/repo@1.0.0");

        // Act.
        IFileSource fileSource = await cacheManager.GetPackageFileSource(packageSpecifier);

        // Assert.
        Assert.Equal("content", new StreamReader(await fileSource.GetFileStream("file") ?? Stream.Null).ReadToEnd());
    }

    [Fact]
    public async Task GetPackageFileSource_GitRepoNotCached_Returns()
    {
        // Arrange.
        var fileSystem = new MockFileSystem();

        var git = new Mock<IGit>();
        git.Setup(g => g.Clone(
            "https://example.com/repo",
            Path.Join(s_cacheDir, "git_repos", "https%3A%2F%2Fexample.com%2Frepo", "v1.0.0"),
            It.IsAny<string?>(),
            It.IsAny<int?>()))
            .Callback<string, string, string?, int?>((url, path, _, _)
                => fileSystem.AddFile(Path.Join(path, "file"), new MockFileData("content")));

        var context = new Mock<IContext>();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);
        context.SetupGet(c => c.Git).Returns(git.Object);

        PathManager pathManager = new(fileSystem, s_cacheDir);

        CacheManager cacheManager = new(context.Object, pathManager, [], []);

        var packageSpecifier = PackageSpecifier.Parse("example.com/repo@1.0.0");

        // Act.
        IFileSource fileSource = await cacheManager.GetPackageFileSource(packageSpecifier);

        // Assert.
        Assert.Equal("content", new StreamReader(await fileSource.GetFileStream("file") ?? Stream.Null).ReadToEnd());
    }

    [Fact]
    public async Task GetPackageFileSource_GoModuleCached_Returns()
    {
        // Arrange.
        var fileSystem = new MockFileSystem();

        CreateGoModuleArchive(
            fileSystem,
            Path.Join(
                s_cacheDir,
                "downloaded_files",
                "https%3A%2F%2Fexample.com%2Fgo-proxy%2Fexample.com%2Frepo%2F%40v%2Fv1.0.0.zip"),
            "example.com/repo",
            new SemVersion(1, 0, 0),
            new Dictionary<string, string>
            {
                { "file", "content" }
            });

        var context = new Mock<IContext>();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        PathManager pathManager = new(fileSystem, s_cacheDir);

        CacheManager cacheManager = new(context.Object, pathManager, [], [Url.Parse("https://example.com/go-proxy")]);

        var packageSpecifier = PackageSpecifier.Parse("example.com/repo@1.0.0");

        // Act.
        IFileSource fileSource = await cacheManager.GetPackageFileSource(packageSpecifier);

        // Assert.
        Assert.Equal("content", new StreamReader((await fileSource.GetFileStream("file"))!).ReadToEnd());
    }

    [Fact]
    public async Task List_NoCacheDirectories_ReturnsEmptyList()
    {
        // Arrange.
        var fileSystem = new MockFileSystem();

        var context = new Mock<IContext>();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        PathManager pathManager = new(fileSystem, s_cacheDir);

        CacheManager cacheManager = new(context.Object, pathManager, [], []);

        // Act.
        ICacheManager.ICacheSummary listResult = await cacheManager.List();

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

        CacheManager cacheManager = new(context.Object, pathManager, [], []);

        // Act.
        ICacheManager.ICacheSummary listResult = await cacheManager.List();

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

        CacheManager cacheManager = new(context.Object, pathManager, [], []);

        // Act.
        ICacheManager.ICacheSummary listResult = await cacheManager.List();

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

        CacheManager cacheManager = new(context.Object, pathManager, [], []);

        // Act.
        ICacheManager.ICacheSummary listResult = await cacheManager.List();

        // Assert.
        Assert.Empty(listResult.DownloadedFiles);
        Assert.Empty(listResult.GitRepos);
    }

    private static void CreateGoModuleArchive(
        MockFileSystem fileSystem,
        string archiveFilePath,
        string goModulePath,
        SemVersion version,
        Dictionary<string, string> entries)
    {
        fileSystem.Directory.CreateDirectory(fileSystem.Path.GetDirectoryName(archiveFilePath)!);
        using FileSystemStream fileStream = fileSystem.File.Create(archiveFilePath);

        using IWriter writer = WriterFactory.Open(fileStream, ArchiveType.Zip, new(CompressionType.Deflate));

        foreach (KeyValuePair<string, string> entry in entries)
        {
            using Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(entry.Value));

            string archiveEntryKey = $"{goModulePath}@v{version}{(version.Major >= 2 ? "+incompatible" : "")}/{entry.Key}";

            writer.Write(archiveEntryKey, stream);
        }
    }
}
