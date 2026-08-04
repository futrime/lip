using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Lip.Core.Sources;

namespace Lip.Core.Tests.Sources;

public class DirectorySourceTests {
  [Fact]
  public void Keys_ReturnsAllFiles() {
    // Arrange
    string root = Path.GetPathRoot(Environment.CurrentDirectory) ?? "/";
    string testDir = Path.Combine(root, "test");
    MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
    {
            { Path.Combine(testDir, "file1.txt"), new MockFileData("content1") },
            { Path.Combine(testDir, "subdir", "file2.txt"), new MockFileData("content2") }
        });
    IDirectoryInfo dirInfo = mockFileSystem.DirectoryInfo.New(testDir);
    DirectorySource provider = new(dirInfo);

    // Act
    List<string> keys = [.. provider.Keys];

    // Assert
    Assert.Equal(2, keys.Count);
    Assert.Contains("file1.txt", keys);
    Assert.Contains(@"subdir" + Path.DirectorySeparatorChar + "file2.txt", keys);
  }

  [Fact]
  public async Task OpenRead_ValidKey_ReturnsStream() {
    // Arrange
    string root = Path.GetPathRoot(Environment.CurrentDirectory) ?? "/";
    string testDir = Path.Combine(root, "test");
    MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
    {
            { Path.Combine(testDir, "file.txt"), new MockFileData("test content") }
        });
    IDirectoryInfo dirInfo = mockFileSystem.DirectoryInfo.New(testDir);
    DirectorySource provider = new(dirInfo);

    // Act
    using Stream stream = await provider.OpenRead("file.txt");
    using StreamReader reader = new(stream);
    string content = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);

    // Assert
    Assert.Equal("test content", content);
  }

  [Fact]
  public async Task OpenRead_InvalidKey_ThrowsArgumentException() {
    // Arrange
    string root = Path.GetPathRoot(Environment.CurrentDirectory) ?? "/";
    string testDir = Path.Combine(root, "test");
    MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
    {
            { Path.Combine(testDir, "file.txt"), new MockFileData("content") }
        });
    IDirectoryInfo dirInfo = mockFileSystem.DirectoryInfo.New(testDir);
    DirectorySource provider = new(dirInfo);

    // Act & Assert
    await Assert.ThrowsAsync<ArgumentException>(() => provider.OpenRead("nonexistent.txt"));
  }
}
