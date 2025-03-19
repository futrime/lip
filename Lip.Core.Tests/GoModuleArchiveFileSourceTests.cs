using Semver;
using SharpCompress.Common;
using SharpCompress.Writers;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Text;

namespace Lip.Core.Tests;

public class GoModuleArchiveFileSourceTests
{
    [Fact]
    public void GetAllEntries_ReturnsEntries()
    {
        // Arrange.
        MockFileSystem fileSystem = new();

        CreateTestFiles(
            fileSystem,
            ArchiveType.Zip,
            CompressionType.Deflate,
            new()
            {
                { "example.com/pkg@v1.0.0/path/to/entry1", "test content 1" },
                { "example.com/pkg@v1.0.0/path/to/entry2", "test content 2" }
            });

        GoModuleArchiveFileSource fileSource = new(fileSystem, "archive", "example.com/pkg", SemVersion.Parse("1.0.0"));

        // Act.
        IAsyncEnumerable<IFileSourceEntry> files = fileSource.GetAllEntries();

        // Assert.
        Assert.Collection(
            files.ToBlockingEnumerable(),
            async file =>
            {
                Assert.Equal("path/to/entry1", file.Key);
                Assert.Equal("test content 1", new StreamReader(await file.OpenRead()).ReadToEnd());
            },
            async file =>
            {
                Assert.Equal("path/to/entry2", file.Key);
                Assert.Equal("test content 2", new StreamReader(await file.OpenRead()).ReadToEnd());
            });
    }

    [Fact]
    public async Task GetEntry_EntryExists_ReturnsEntry()
    {
        // Arrange.
        MockFileSystem fileSystem = new();

        CreateTestFiles(
            fileSystem,
            ArchiveType.Zip,
            CompressionType.Deflate,
            new()
            {
                { "example.com/pkg@v1.0.0/path/to/entry1", "test content 1" },
                { "example.com/pkg@v1.0.0/path/to/entry2", "test content 2" }
            });

        GoModuleArchiveFileSource fileSource = new(fileSystem, "archive", "example.com/pkg", SemVersion.Parse("1.0.0"));

        // Act.
        IFileSourceEntry? file = await fileSource.GetEntry("path/to/entry1");

        // Assert.
        Assert.NotNull(file);
        Assert.Equal("path/to/entry1", file.Key);
        Assert.Equal("test content 1", new StreamReader(await file.OpenRead()).ReadToEnd());
    }

    [Fact]
    public async Task GetEntry_EntryDoesNotExist_ReturnsNull()
    {
        // Arrange.
        MockFileSystem fileSystem = new();

        CreateTestFiles(
            fileSystem,
            ArchiveType.Zip,
            CompressionType.Deflate,
            new()
            {
                { "example.com/pkg@v1.0.0/path/to/entry1", "test content 1" },
                { "example.com/pkg@v1.0.0/path/to/entry2", "test content 2" }
            });

        GoModuleArchiveFileSource fileSource = new(fileSystem, "archive", "example.com/pkg", SemVersion.Parse("1.0.0"));

        // Act.
        IFileSourceEntry? file = await fileSource.GetEntry("path/to/entry3");

        // Assert.
        Assert.Null(file);
    }

    [Fact]
    public void Entry_Key_ReturnsKey()
    {
        // Arrange.
        GoModuleArchiveFileSourceEntry fileSourceEntry = new("key", "path/to/entry", new MemoryStream());

        // Act.
        string key = fileSourceEntry.Key;

        // Assert.
        Assert.Equal("key", key);
    }

    private static void CreateTestFiles(
        MockFileSystem fileSystem,
        ArchiveType archiveType,
        CompressionType compressionType,
        Dictionary<string, string> entries)
    {
        using FileSystemStream fileStream = fileSystem.File.Create("archive");

        using IWriter writer = WriterFactory.Open(fileStream, archiveType, new(compressionType));

        foreach (KeyValuePair<string, string> entry in entries)
        {
            using Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(entry.Value));
            writer.Write(entry.Key, stream);
        }
    }
}
