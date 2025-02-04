using System.IO.Abstractions;

namespace Lip;

/// <summary>
/// A file source that reads a single file.
/// </summary>
/// <remarks>
/// This file source assumes that no changes are made to the directory while it is in use.
/// </remarks>
/// <param name="fileSystem">The file system to use.</param>
/// <param name="filePath">The path of the file.</param>
public class StandaloneFileSource(IFileSystem fileSystem, string filePath) : IFileSource
{
    private readonly string _filePath = filePath;
    private readonly IFileSystem _fileSystem = fileSystem;

    public async Task<List<IFileSourceEntry>> GetAllEntries()
    {
        await Task.Delay(0); // To avoid warning.
        return [new StandaloneFileSourceEntry(_fileSystem, _filePath)];
    }

    public async Task<IFileSourceEntry?> GetEntry(string key)
    {
        await Task.Delay(0); // To avoid warning.
        return (key == string.Empty)
            ? new StandaloneFileSourceEntry(_fileSystem, _filePath)
            : null;
    }
}

public class StandaloneFileSourceEntry(IFileSystem fileSystem, string filePath) : IFileSourceEntry
{
    private readonly string _filePath = filePath;
    private readonly IFileSystem _fileSystem = fileSystem;

    public string Key => string.Empty;

    public async Task<Stream> OpenRead()
    {
        await Task.Delay(0); // To avoid warning.

        return _fileSystem.File.OpenRead(_filePath);
    }
}
