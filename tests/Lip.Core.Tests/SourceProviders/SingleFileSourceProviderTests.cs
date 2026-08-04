using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Lip.Core.Sources;

namespace Lip.Core.Tests.Sources;

public class SingleFileSourceTests {
  [Fact]
  public void Keys_ReturnsEmptyStringKey() {
    // Arrange
    MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
    {
            { @"C:\test\file.txt", new MockFileData("content") }
        });
    IFileInfo fileInfo = mockFileSystem.FileInfo.New(@"C:\test\file.txt");
    SingleFileSource provider = new(fileInfo);

    // Act
    List<string> keys = [.. provider.Keys];

    // Assert
    Assert.Single(keys);
    Assert.Equal("", keys[0]);
  }

  [Fact]
  public async Task OpenRead_EmptyKey_ReturnsStream() {
    // Arrange
    MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
    {
            { @"C:\test\file.txt", new MockFileData("file content") }
        });
    IFileInfo fileInfo = mockFileSystem.FileInfo.New(@"C:\test\file.txt");
    SingleFileSource provider = new(fileInfo);

    // Act
    using Stream stream = await provider.OpenRead("");
    using StreamReader reader = new(stream);
    string content = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);

    // Assert
    Assert.Equal("file content", content);
  }

  [Fact]
  public async Task OpenRead_NonEmptyKey_ThrowsArgumentException() {
    // Arrange
    MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
    {
            { @"C:\test\file.txt", new MockFileData("content") }
        });
    IFileInfo fileInfo = mockFileSystem.FileInfo.New(@"C:\test\file.txt");
    SingleFileSource provider = new(fileInfo);

    // Act & Assert
    await Assert.ThrowsAsync<ArgumentException>(() => provider.OpenRead("anykey"));
  }
}
