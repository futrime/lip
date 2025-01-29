using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace Lip.Tests;

public class IFileSystemExtensionsTests
{
    [Fact]
    public async Task CreateParentDirectoryAsync_WhenPathIsRoot_DoesNotCreateParentDirectory()
    {
        // Arrange.
        MockFileSystem fileSystem = new();

        // Act.
        await fileSystem.CreateParentDirectoryAsync("/");
    }

    [Fact]
    public async Task CreateParentDirectoryAsync_WhenParentDirectoryDoesNotExist_CreatesParentDirectory()
    {
        // Arrange.
        MockFileSystem fileSystem = new();

        // Act.
        await fileSystem.CreateParentDirectoryAsync("/path/to/file");

        // Assert.
        Assert.True(fileSystem.Directory.Exists("/path/to"));
    }

    [Fact]
    public async Task CreateParentDirectoryAsync_WhenParentDirectoryExists_DoesNotCreateParentDirectory()
    {
        // Arrange.
        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { "/path/to", new MockDirectoryData() },
        });

        // Act.
        await fileSystem.CreateParentDirectoryAsync("/path/to/file");

        // Assert.
        Assert.True(fileSystem.Directory.Exists("/path/to"));
    }

    [Fact]
    public async Task CreateDirectoryAsync_CreatesDirectory()
    {
        // Arrange
        var fileSystem = new MockFileSystem();

        // Act
        await fileSystem.Directory.CreateDirectoryAsync("/test/dir");

        // Assert
        Assert.True(fileSystem.Directory.Exists("/test/dir"));
    }

    [Fact]
    public async Task DeleteAsync_DeletesDirectory()
    {
        // Arrange
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { "/test/dir", new MockDirectoryData() }
        });

        // Act
        await fileSystem.Directory.DeleteAsync("/test/dir", false);

        // Assert
        Assert.False(fileSystem.Directory.Exists("/test/dir"));
    }

    [Fact]
    public async Task ExistsAsync_Directory_ReturnsExpectedResult()
    {
        // Arrange
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { "/test/dir", new MockDirectoryData() }
        });

        // Act & Assert
        Assert.True(await fileSystem.Directory.ExistsAsync("/test/dir"));
        Assert.False(await fileSystem.Directory.ExistsAsync("/nonexistent"));
    }

    [Fact]
    public async Task EnumerateDirectoriesAsync_ReturnsExpectedDirectories()
    {
        // Arrange
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { "/test/dir1", new MockDirectoryData() },
            { "/test/dir2", new MockDirectoryData() }
        });
        IDirectoryInfo dirInfo = fileSystem.DirectoryInfo.New("/test");

        // Act
        IEnumerable<IDirectoryInfo> dirs = await dirInfo.EnumerateDirectoriesAsync();

        // Assert
        Assert.Equal(2, dirs.Count());
    }

    [Fact]
    public async Task EnumerateFilesAsync_ReturnsExpectedFiles()
    {
        // Arrange
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { "/test/file1.txt", new MockFileData("test1") },
            { "/test/file2.txt", new MockFileData("test2") }
        });
        IDirectoryInfo dirInfo = fileSystem.DirectoryInfo.New("/test");

        // Act
        IEnumerable<IFileInfo> files = await dirInfo.EnumerateFilesAsync();

        // Assert
        Assert.Equal(2, files.Count());
    }

    [Fact]
    public async Task CopyAsync_CopiesFile()
    {
        // Arrange
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { "/test/source.txt", new MockFileData("test") }
        });

        // Act
        await fileSystem.File.CopyAsync("/test/source.txt", "/test/dest.txt");

        // Assert
        Assert.True(fileSystem.File.Exists("/test/dest.txt"));
    }

    [Fact]
    public async Task CreateAsync_CreatesFile()
    {
        // Arrange
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>()
        {
            { "/test", new MockDirectoryData() }
        });

        // Act
        FileSystemStream stream = await fileSystem.File.CreateAsync("/test/file.txt");
        await stream.DisposeAsync();

        // Assert
        Assert.True(fileSystem.File.Exists("/test/file.txt"));
    }

    [Fact]
    public async Task ExistsAsync_File_ReturnsExpectedResult()
    {
        // Arrange
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { "/test/file.txt", new MockFileData("test") }
        });

        // Act & Assert
        Assert.True(await fileSystem.File.ExistsAsync("/test/file.txt"));
        Assert.False(await fileSystem.File.ExistsAsync("/nonexistent.txt"));
    }

    [Fact]
    public async Task OpenReadAsync_OpensFileForReading()
    {
        // Arrange
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { "/test/file.txt", new MockFileData("test content") }
        });

        // Act
        await using FileSystemStream stream = await fileSystem.File.OpenReadAsync("/test/file.txt");
        using var reader = new StreamReader(stream);
        string content = await reader.ReadToEndAsync();

        // Assert
        Assert.Equal("test content", content);
    }

    [Fact]
    public async Task ExistsAsync_Path_ReturnsExpectedResult()
    {
        // Arrange
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { "/test/file.txt", new MockFileData("test") }
        });

        // Act & Assert
        Assert.True(await fileSystem.Path.ExistsAsync("/test/file.txt"));
        Assert.False(await fileSystem.Path.ExistsAsync("/nonexistent.txt"));
    }
}
