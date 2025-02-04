using System.IO.Abstractions.TestingHelpers;

namespace Lip.Tests;

public class IFileSystemExtensionsTests
{
    [Fact]
    public void CreateParentDirectory_WhenPathIsRoot_DoesNotCreateParentDirectory()
    {
        // Arrange.
        MockFileSystem fileSystem = new();

        // Act.
        fileSystem.CreateParentDirectory("/");
    }

    [Fact]
    public void CreateParentDirectory_WhenParentDirectoryDoesNotExist_CreatesParentDirectory()
    {
        // Arrange.
        MockFileSystem fileSystem = new();

        // Act.
        fileSystem.CreateParentDirectory("/path/to/file");

        // Assert.
        Assert.True(fileSystem.Directory.Exists("/path/to"));
    }

    [Fact]
    public void CreateParentDirectory_WhenParentDirectoryExists_DoesNotCreateParentDirectory()
    {
        // Arrange.
        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { "/path/to", new MockDirectoryData() },
        });

        // Act.
        fileSystem.CreateParentDirectory("/path/to/file");

        // Assert.
        Assert.True(fileSystem.Directory.Exists("/path/to"));
    }
}
