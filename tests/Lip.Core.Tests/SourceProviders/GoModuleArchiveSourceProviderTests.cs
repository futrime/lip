using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.IO.Compression;
using System.Text;
using Lip.Core.Sources;

namespace Lip.Core.Tests.Sources;

public class GoModuleArchiveSourceTests {
  [Fact]
  public async Task Keys_FiltersAndExtractsKeys() {
    // Arrange
    // Go module archives typically have structure: module@version/path/to/file
    using MemoryStream memoryStream = new();
    using (ZipArchive archive = new(memoryStream, System.IO.Compression.ZipArchiveMode.Create, true)) {
      ZipArchiveEntry entry = archive.CreateEntry("github.com/user/repo@v1.0.0/file1.txt");
      using (Stream entryStream = entry.Open()) {
        entryStream.Write(Encoding.UTF8.GetBytes("c1"));
      }

      ZipArchiveEntry entry2 = archive.CreateEntry("github.com/user/repo@v1.0.0/dir/file2.txt");
      using (Stream entryStream2 = entry2.Open()) {
        entryStream2.Write(Encoding.UTF8.GetBytes("c2"));
      }

      ZipArchiveEntry entry3 = archive.CreateEntry("other/file.txt");
      using Stream entryStream3 = entry3.Open();
      entryStream3.Write(Encoding.UTF8.GetBytes("c3"));
    }
    memoryStream.Position = 0;
    byte[] archiveBytes = memoryStream.ToArray();

    MockFileSystem mockFileSystem = new();
    string path = @"C:\archive.zip";
    mockFileSystem.AddFile(path, new MockFileData(archiveBytes));
    IFileInfo fileInfo = mockFileSystem.FileInfo.New(path);

    GoModuleArchiveSource provider = new(fileInfo);

    // Act
    List<string> keys = [.. provider.Keys];

    // Assert
    // keys should be relative to the module root
    Assert.Contains("file1.txt", keys);
    Assert.Contains("dir/file2.txt", keys);
    Assert.DoesNotContain("other/file.txt", keys);
  }

  [Fact]
  public async Task OpenRead_MapsKeyToArchiveEntry() {
    // Arrange
    using MemoryStream memoryStream = new();
    using (ZipArchive archive = new(memoryStream, System.IO.Compression.ZipArchiveMode.Create, true)) {
      ZipArchiveEntry entry = archive.CreateEntry("github.com/user/repo@v1.0.0/file.txt");
      using Stream entryStream = entry.Open();
      entryStream.Write(Encoding.UTF8.GetBytes("content"));
    }
    memoryStream.Position = 0;
    byte[] archiveBytes = memoryStream.ToArray();

    MockFileSystem mockFileSystem = new();
    string path = @"C:\archive.zip";
    mockFileSystem.AddFile(path, new MockFileData(archiveBytes));
    IFileInfo fileInfo = mockFileSystem.FileInfo.New(path);

    GoModuleArchiveSource provider = new(fileInfo);

    // Act
    using Stream stream = await provider.OpenRead("file.txt");
    using StreamReader reader = new(stream);
    string content = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);

    // Assert
    Assert.Equal("content", content);
  }
}
