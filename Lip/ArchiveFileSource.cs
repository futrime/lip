using System.IO.Abstractions;
using SharpCompress.Readers;

namespace Lip;

/// <summary>
/// A file source that reads and writes files from an archive.
/// </summary>
/// <remarks>
/// This file source assumes that no changes are made to the directory while it is in use.
/// </remarks>
/// <param name="fileSystem">The file system to use.</param>
/// <param name="archiveFilePath">The archive file path.</param>
public class ArchiveFileSource(IFileSystem fileSystem, string archiveFilePath) : IFileSource
{
    private readonly string _archiveFilePath = archiveFilePath;
    private readonly IFileSystem _fileSystem = fileSystem;

    public virtual async Task<List<IFileSourceEntry>> GetAllEntries()
    {
        await Task.Delay(0); // Suppress warning.

        using FileSystemStream fileStream = _fileSystem.File.OpenRead(_archiveFilePath);

        using IReader reader = ReaderFactory.Open(fileStream);

        List<IFileSourceEntry> entries = [];
        while (reader.MoveToNextEntry())
        {
            // We don't know when the key is null, so we suppress the warning.
            entries.Add(new ArchiveFileSourceEntry(_fileSystem, _archiveFilePath, reader.Entry.Key!));
        }

        return entries;
    }

    public virtual async Task<IFileSourceEntry?> GetEntry(string key)
    {
        await Task.Delay(0); // Suppress warning.

        using FileSystemStream fileStream = _fileSystem.File.OpenRead(_archiveFilePath);

        using IReader reader = ReaderFactory.Open(fileStream);

        while (reader.MoveToNextEntry())
        {
            if (reader.Entry.Key == key)
            {
                return new ArchiveFileSourceEntry(_fileSystem, _archiveFilePath, key);
            }
        }

        return null;
    }
}

/// <summary>
/// A file source entry that reads and writes files from an archive entry.
/// </summary>
/// <param name="fileSystem">The file system to use.</param>
/// <param name="archiveFilePath">The archive file path.</param>
/// <param name="key">The key of the entry.</param>
public class ArchiveFileSourceEntry(
    IFileSystem fileSystem,
    string archiveFilePath,
    string key) : IFileSourceEntry
{
    private readonly string _archiveFilePath = archiveFilePath;
    private readonly IFileSystem _fileSystem = fileSystem;
    private readonly string _key = key;

    public virtual string Key => _key;

    public async Task<Stream> OpenRead()
    {
        await Task.Delay(0); // Suppress warning.

        using FileSystemStream fileStream = _fileSystem.File.OpenRead(_archiveFilePath);

        using IReader reader = ReaderFactory.Open(fileStream);

        while (reader.MoveToNextEntry())
        {
            if (reader.Entry.Key == _key)
            {
                MemoryStream memoryStream = new();

                reader.WriteEntryTo(memoryStream);

                memoryStream.Position = 0;

                return memoryStream;
            }
        }

        throw new InvalidOperationException($"Entry '{Key}' not found in archive '{_archiveFilePath}'.");
    }
}
