using System.IO.Abstractions.TestingHelpers;

namespace Lip.Tests;

public class StandaloneFileSourceTests
{
    [Fact]
    public async Task GetAllFiles_ReturnsStandaloneFileSourceEntry()
    {
        // Arrange
        string filePath = "/path/to/file";

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { filePath, new MockFileData("Test content") }
        });

        var source = new StandaloneFileSource(fileSystem, filePath);

        // Act
        List<IFileSourceEntry> entries = await source.GetAllEntries();

        // Assert
        Assert.Single(entries);
        Assert.Equal("Test content", new StreamReader(await entries[0].OpenRead()).ReadToEnd());
    }

    [Fact]
    public async Task GetEntry_EmptyKey_ReturnsStandaloneFileSourceEntry()
    {
        // Arrange
        string filePath = "/path/to/file";

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { filePath, new MockFileData("Test content") }
        });

        var source = new StandaloneFileSource(fileSystem, filePath);

        // Act
        IFileSourceEntry? entry = await source.GetEntry(string.Empty);

        // Assert
        Assert.NotNull(entry);
        Assert.Equal("Test content", new StreamReader(await entry.OpenRead()).ReadToEnd());
    }

    [Fact]
    public async Task GetEntry_NonEmptyKey_ReturnsNull()
    {
        // Arrange
        string filePath = "/path/to/file";

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { filePath, new MockFileData("Test content") }
        });

        var source = new StandaloneFileSource(fileSystem, filePath);

        // Act
        IFileSourceEntry? entry = await source.GetEntry("non-empty-key");

        // Assert
        Assert.Null(entry);
    }
}

public class StandaloneFileSourceEntryTests
{
    [Fact]
    public void Key_ReturnsEmptyString()
    {
        // Arrange
        string filePath = "/path/to/file";

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { filePath, new MockFileData("Test content") }
        });

        var entry = new StandaloneFileSourceEntry(fileSystem, filePath);

        // Act
        string key = entry.Key;

        // Assert
        Assert.Equal(string.Empty, key);
    }

    [Fact]
    public async Task OpenRead_ReturnsFileStream()
    {
        // Arrange
        string filePath = "/path/to/file";

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { filePath, new MockFileData("Test content") }
        });

        var entry = new StandaloneFileSourceEntry(fileSystem, filePath);

        // Act
        using Stream stream = await entry.OpenRead();

        // Assert
        Assert.Equal("Test content", new StreamReader(stream).ReadToEnd());
    }
}
