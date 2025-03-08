using Golang.Org.X.Mod;
using Semver;
using System.IO.Abstractions;

namespace Lip.Core;

public class GoModuleArchiveFileSource(
    IFileSystem fileSystem,
    string archiveFilePath,
    string goModulePath,
    SemVersion version) : ArchiveFileSource(fileSystem, archiveFilePath)
{
    private readonly string _archiveFilePath = archiveFilePath;

    private readonly IFileSystem _fileSystem = fileSystem;

    // When major >= 2 and there's no go.mod, GoProxy will add +incompatible in version
    // Reference: https://stackoverflow.com/questions/57355929/what-does-incompatible-in-go-mod-mean-will-it-cause-harm
    private readonly string _prefix =
        $"{goModulePath}@{Module.CanonicalVersion("v" + (version.MetadataIdentifiers.Count == 0 && version.Major >= 2 ? version + "+incompatible" : version.ToString()))}/";

    public override async Task<List<IFileSourceEntry>> GetAllEntries()
    {
        List<IFileSourceEntry> entries =
        [
            .. (await base.GetAllEntries())
            .Where(entry => entry.Key.StartsWith(_prefix))
            .Select(entry => new GoModuleArchiveFileSourceEntry(
                _fileSystem,
                _archiveFilePath,
                entry.Key[_prefix.Length..],
                entry.Key))
        ];

        return entries;
    }

    public override async Task<IFileSourceEntry?> GetEntry(string key)
    {
        string archiveEntryKey = _prefix + key;

        if (await base.GetEntry(archiveEntryKey) is null)
        {
            return null;
        }

        return new GoModuleArchiveFileSourceEntry(
            _fileSystem,
            _archiveFilePath,
            key,
            archiveEntryKey);
    }
}

public class GoModuleArchiveFileSourceEntry(
    IFileSystem fileSystem,
    string archiveFilePath,
    string key,
    string archiveEntryKey) : ArchiveFileSourceEntry(fileSystem, archiveFilePath, archiveEntryKey)
{
    private readonly string _key = key;

    public override string Key => _key;
}
