using System.IO.Abstractions;
using SharpCompress.Archives;

namespace Lip.Core.Sources;

public class ArchiveSource(IFileInfo archiveFileInfo) : ISource {
  private readonly IFileInfo _archiveFileInfo = archiveFileInfo;

  public IEnumerable<string> Keys {
    get {
      using Stream archiveStream = _archiveFileInfo.OpenRead();
      using IArchive archive = ArchiveFactory.OpenArchive(archiveStream);

      return [.. archive.Entries
                .Where(e => !e.IsDirectory && e.Key is not null)
                .Select(e => e.Key!)];
    }
  }

  public virtual async Task<Stream> OpenRead(string key) {
    using Stream archiveStream = _archiveFileInfo.OpenRead();
    using IArchive archive = ArchiveFactory.OpenArchive(archiveStream);

    IArchiveEntry entry = archive.Entries.FirstOrDefault(e => e.Key == key && !e.IsDirectory)
        ?? throw new ArgumentException($"Key not found: {key}", nameof(key));

    MemoryStream memoryStream = new();

    await entry.WriteToAsync(memoryStream);

    memoryStream.Position = 0;

    return memoryStream;
  }
}
