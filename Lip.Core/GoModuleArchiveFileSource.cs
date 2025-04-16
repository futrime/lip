using Golang.Org.X.Mod;
using Semver;
using SharpCompress.Common;
using System.IO.Abstractions;

namespace Lip.Core;

public class GoModuleArchiveFileSource(
    IFileSystem fileSystem,
    string archiveFilePath,
    string goModulePath,
    SemVersion version) : ArchiveFileSource(fileSystem, archiveFilePath)
{
    // When major >= 2 and there's no go.mod, GoProxy will add +incompatible in version
    // Reference: https://stackoverflow.com/questions/57355929/what-does-incompatible-in-go-mod-mean-will-it-cause-harm
    private readonly string _prefix =
        $"{goModulePath}@{Module.CanonicalVersion("v" + (version.MetadataIdentifiers.Count == 0 && version.Major >= 2 ? version + "+incompatible" : version.ToString()))}/";

    public override async IAsyncEnumerable<IFileSourceEntry> GetAllEntries()
    {
        await foreach (var entry in base.GetAllEntries())
        {
            if (entry.Key.StartsWith(_prefix))
            {
                yield return new GoModuleArchiveFileSourceEntry(
                    entry.Key[_prefix.Length..],
                    entry.Key,
                    await entry.OpenRead());
            }
            else
            {
                await entry.DisposeAsync();
            }
        }
    }

    public override async Task<IFileSourceEntry?> GetEntry(string key)
    {
        string archiveEntryKey = _prefix + key;
        var entry = await base.GetEntry(archiveEntryKey);
        if (entry is null)
        {
            return null;
        }

        return new GoModuleArchiveFileSourceEntry(
            key,
            archiveEntryKey,
            await entry.OpenRead());
    }
}

public class GoModuleArchiveFileSourceEntry(
    string key,
    string archiveEntryKey,
    Stream contentStream) : ArchiveFileSourceEntry(archiveEntryKey, contentStream)
{
    private readonly string _key = key;

    public override string Key => _key;
}