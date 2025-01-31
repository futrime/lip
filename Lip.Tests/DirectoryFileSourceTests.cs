using System.IO.Abstractions.TestingHelpers;

namespace Lip.Tests;

public class DirectoryFileSourceTests
{
    [Fact]
    public async Task GetAllFiles_ReturnsDirectoryFileSourceEntry()
    {
        // Arrange
        string rootDirPath = "/path/to/dir";

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { $"{rootDirPath}/file1", new MockFileData("Test content 1") },
            { $"{rootDirPath}/file2", new MockFileData("Test content 2") },
            { $"{rootDirPath}/dir/file3", new MockFileData("Test content 3") }
        });

        var source = new DirectoryFileSource(fileSystem, rootDirPath);

        // Act
        List<IFileSourceEntry> entries = await source.GetAllEntries();

        // Assert
        Assert.Equal(3, entries.Count);
        Assert.Equal("file1", entries[0].Key);
        Assert.Equal("file2", entries[1].Key);
        Assert.Equal("dir/file3", entries[2].Key);
        Assert.Equal("Test content 1", new StreamReader(await entries[0].OpenRead()).ReadToEnd());
        Assert.Equal("Test content 2", new StreamReader(await entries[1].OpenRead()).ReadToEnd());
        Assert.Equal("Test content 3", new StreamReader(await entries[2].OpenRead()).ReadToEnd());
    }

    [Fact]
    public async Task GetEntry_PathExists_ReturnsDirectoryFileSourceEntry()
    {
        // Arrange
        string filePath = "/path/to/file";

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { filePath, new MockFileData("Test content") }
        });

        var source = new DirectoryFileSource(fileSystem, filePath);

        // Act
        IFileSourceEntry? entry = await source.GetEntry(string.Empty);

        // Assert
        Assert.NotNull(entry);
        Assert.Equal("Test content", new StreamReader(await entry.OpenRead()).ReadToEnd());
    }

    [Fact]
    public async Task GetEntry_PathDoesNotExist_ReturnsNull()
    {
        // Arrange
        string filePath = "/path/to/file";

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { filePath, new MockFileData("Test content") }
        });

        var source = new DirectoryFileSource(fileSystem, filePath);

        // Act
        IFileSourceEntry? entry = await source.GetEntry("non-empty-key");

        // Assert
        Assert.Null(entry);
    }

    [Theory]
    [InlineData("C:\\Windows\\System32\\cmd.exe")]
    [InlineData("/etc/passwd")]
    [InlineData("dir/../file")]
    public async Task GetEntry_UnsafeKey_ReturnsNull(string key)
    {
        // Arrange
        string rootDirPath = "/path/to/dir";

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { $"{rootDirPath}/file", new MockFileData("Test content") }
        });

        var source = new DirectoryFileSource(fileSystem, rootDirPath);

        // Act
        IFileSourceEntry? entry = await source.GetEntry(key);

        // Assert
        Assert.Null(entry);
    }
}

public class DirectoryFileSourceEntryTests
{
    [Fact]
    public void Key_ReturnsKey()
    {
        // Arrange
        string filePath = "/path/to/file";

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { filePath, new MockFileData("Test content") }
        });

        string expectedKey = "key";

        var entry = new DirectoryFileSourceEntry(fileSystem, filePath, expectedKey);

        // Act
        string key = entry.Key;

        // Assert
        Assert.Equal("key", key);
    }

    [Fact]
    public async Task OpenRead_IsFile_ReturnsFileStream()
    {
        // Arrange
        string filePath = "/path/to/file";

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { filePath, new MockFileData("Test content") }
        });

        string key = "file";

        var entry = new DirectoryFileSourceEntry(fileSystem, filePath, key);

        // Act
        using Stream stream = await entry.OpenRead();

        // Assert
        Assert.Equal("Test content", new StreamReader(stream).ReadToEnd());
    }
}
