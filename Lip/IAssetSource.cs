namespace Lip;

public interface IAssetSource
{
    IEnumerable<IAssetSourceEntry> Entries { get; }

    IAssetSourceEntry? GetEntry(string key);
}

public interface IAssetSourceEntry
{
    bool IsDirectory { get; }
    string Key { get; }

    Stream OpenRead();
}
