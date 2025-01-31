namespace Lip;

/// <summary>
/// File source is a unified interface for file access for different file structures.
/// </summary>
public interface IFileSource
{
    /// <summary>
    /// Retrieves all entries from the file source.
    /// </summary>
    /// <returns>All files in the file source.</returns>
    Task<List<IFileSourceEntry>> GetAllEntries();

    /// <summary>
    /// Retrieves an entry from the file source by its key.
    /// </summary>
    /// <param name="key">The unique identifier of the entry to retrieve.</param>
    /// <returns>The entry if found; otherwise, null.</returns>
    Task<IFileSourceEntry?> GetEntry(string key);
}

/// <summary>
/// Represents an entry within a file source.
/// </summary>
public interface IFileSourceEntry
{
    /// <summary>
    /// Gets the unique identifier of this entry.
    /// </summary>
    /// <value>The entry's key within the file source.</value>
    string Key { get; }

    /// <summary>
    /// Opens a stream to read the entry's content.
    /// </summary>
    /// <returns>A stream containing the entry's data.</returns>
    Task<Stream> OpenRead();
}

public static class IFileSourceExtensions
{
    public static async Task<Stream?> GetFileStream(this IFileSource fileSource, string key)
    {
        IFileSourceEntry? entry = await fileSource.GetEntry(key);

        if (entry == null)
        {
            return null;
        }

        return await entry.OpenRead();
    }
}
