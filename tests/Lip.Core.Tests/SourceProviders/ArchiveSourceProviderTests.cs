using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.IO.Compression;
using System.Text;
using Lip.Core.Sources;

namespace Lip.Core.Tests.Sources;

public class ArchiveSourceTests {
  [Fact]
  public async Task Keys_ReturnsAllFileKeys() {
    // Arrange
    using MemoryStream memoryStream = new();

    using (ZipArchive archive = new(memoryStream, System.IO.Compression.ZipArchiveMode.Create, true)) {
      ZipArchiveEntry entry = archive.CreateEntry("file1.txt");
      using (Stream entryStream = entry.Open()) {
        entryStream.Write(Encoding.UTF8.GetBytes("content1"));
      }

      ZipArchiveEntry entry2 = archive.CreateEntry("dir/file2.txt");
      using Stream entryStream2 = entry2.Open();
      entryStream2.Write(Encoding.UTF8.GetBytes("content2"));
    }
    memoryStream.Position = 0;
    byte[] archiveBytes = memoryStream.ToArray();

    MockFileSystem mockFileSystem = new();
    string path = @"C:\archive.zip";
    mockFileSystem.AddFile(path, new MockFileData(archiveBytes));
    IFileInfo fileInfo = mockFileSystem.FileInfo.New(path);

    ArchiveSource provider = new(fileInfo);

    // Act
    List<string> keys = [.. provider.Keys];

    // Assert
    Assert.Equal(2, keys.Count);
    Assert.Contains("file1.txt", keys);
    Assert.Contains("dir/file2.txt", keys);
  }

  [Fact]
  public async Task OpenRead_ValidKey_ReturnsContent() {
    // Arrange
    using MemoryStream memoryStream = new();

    using (ZipArchive archive = new(memoryStream, System.IO.Compression.ZipArchiveMode.Create, true)) {
      ZipArchiveEntry entry = archive.CreateEntry("file1.txt");
      using Stream entryStream = entry.Open();
      entryStream.Write(Encoding.UTF8.GetBytes("test content"));
    }
    memoryStream.Position = 0;
    byte[] archiveBytes = memoryStream.ToArray();

    MockFileSystem mockFileSystem = new();
    string path = @"C:\archive.zip";
    mockFileSystem.AddFile(path, new MockFileData(archiveBytes));
    IFileInfo fileInfo = mockFileSystem.FileInfo.New(path);

    ArchiveSource provider = new(fileInfo);

    // Act
    using Stream stream = await provider.OpenRead("file1.txt");
    using StreamReader reader = new(stream);
    string content = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);

    // Assert
    Assert.Equal("test content", content);
  }

  [Fact]
  public async Task OpenRead_InvalidKey_ThrowsArgumentException() {
    // Arrange
    using MemoryStream memoryStream = new();

    using (ZipArchive archive = new(memoryStream, System.IO.Compression.ZipArchiveMode.Create, true)) {
    }
    memoryStream.Position = 0;
    byte[] archiveBytes = memoryStream.ToArray();

    MockFileSystem mockFileSystem = new();
    string path = @"C:\archive.zip";
    mockFileSystem.AddFile(path, new MockFileData(archiveBytes));
    IFileInfo fileInfo = mockFileSystem.FileInfo.New(path);

    ArchiveSource provider = new(fileInfo);

    // Act & Assert
    await Assert.ThrowsAsync<ArgumentException>(() => provider.OpenRead("nonexistent"));
  }
}
