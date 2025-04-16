using SharpCompress.Readers;
using System.IO.Abstractions;

namespace Lip.Core;

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

    public virtual async IAsyncEnumerable<IFileSourceEntry> GetAllEntries()
    {
        await Task.CompletedTask; // Suppress warning.

        using FileSystemStream fileStream = _fileSystem.File.OpenRead(_archiveFilePath);
        using IReader reader = ReaderFactory.Open(fileStream);

        while (reader.MoveToNextEntry())
        {
            if (reader.Entry.IsDirectory)
            {
                continue;
            }

            MemoryStream memoryStream = new();

            reader.WriteEntryTo(memoryStream);

            memoryStream.Position = 0;

            yield return new ArchiveFileSourceEntry(reader.Entry.Key!, memoryStream);
        }
    }

    public virtual async Task<IFileSourceEntry?> GetEntry(string key)
    {
        await Task.CompletedTask; // Suppress warning.

        using FileSystemStream fileStream = _fileSystem.File.OpenRead(_archiveFilePath);

        using IReader reader = ReaderFactory.Open(fileStream);

        while (reader.MoveToNextEntry())
        {
            if (reader.Entry.Key != key)
            {
                continue;
            }

            MemoryStream memoryStream = new();

            reader.WriteEntryTo(memoryStream);

            memoryStream.Position = 0;

            return new ArchiveFileSourceEntry(key, memoryStream);
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
    string key,
    Stream contentStream) : IFileSourceEntry
{
    private readonly string _key = key;
    private readonly Stream _contentStream = contentStream;

    public virtual string Key => _key;

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        _contentStream.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await Task.CompletedTask; // Suppress warning.

        GC.SuppressFinalize(this);

        Dispose();
    }

    public async Task<Stream> OpenRead()
    {
        await Task.CompletedTask; // Suppress warning.

        _contentStream.Position = 0;

        MemoryStream memoryStream = new();

        await _contentStream.CopyToAsync(memoryStream);

        memoryStream.Position = 0;

        return memoryStream;
    }
}