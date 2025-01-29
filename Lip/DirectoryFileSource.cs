using System.IO.Abstractions;

namespace Lip;

/// <summary>
/// A file source that reads files from a directory.
/// </summary>
/// <remarks>
/// This file source assumes that no changes are made to the directory while it is in use.
/// </remarks>
/// <param name="fileSystem">The file system to use.</param>
/// <param name="rootDirPath">The root directory path of all files.</param>
public class DirectoryFileSource(IFileSystem fileSystem, string rootDirPath) : IFileSource
{
    private readonly string _rootDirPath = fileSystem.Path.GetFullPath(rootDirPath);
    private readonly IFileSystem _fileSystem = fileSystem;

    public async Task<List<IFileSourceEntry>> GetAllFiles()
    {
        await Task.Delay(0); // To avoid warning.

        return [.. _fileSystem.Directory.EnumerateFiles(_rootDirPath)
            .Select(filePath => new DirectoryFileSourceEntry(
                _fileSystem,
                filePath,
                _fileSystem.Path.GetRelativePath(_rootDirPath, filePath)
                .Replace(_fileSystem.Path.DirectorySeparatorChar, '/')))
            .Cast<IFileSourceEntry>()];
    }

    public async Task<IFileSourceEntry?> GetFile(string key)
    {
        if (!StringValidator.CheckSafePlacePath(key))
        {
            return null;
        }

        string filePath = _fileSystem.Path.Join(_rootDirPath, key);
        filePath = _fileSystem.Path.GetFullPath(filePath);

        if (await _fileSystem.File.ExistsAsync(filePath))
        {
            return new DirectoryFileSourceEntry(_fileSystem, filePath, key);
        }

        return null;
    }
}

public class DirectoryFileSourceEntry(IFileSystem fileSystem, string filePath, string key) : IFileSourceEntry
{
    private readonly string _filePath = fileSystem.Path.GetFullPath(filePath);
    private readonly IFileSystem _fileSystem = fileSystem;

    public string Key { get; } = key;

    public async Task<Stream> OpenRead()
    {
        return await _fileSystem.File.OpenReadAsync(_filePath);
    }
}
