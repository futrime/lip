using System.Runtime.InteropServices;

namespace Lip;

public partial class Lip
{
    public record ListItem
    {
        public required PackageManifest Manifest { get; init; }
        public required bool Locked { get; init; }
    }

    public record ListArgs { }

    public async Task<List<ListItem>> List(ListArgs args)
    {
        PackageLock packageLock = await GetPackageLock();

        List<ListItem> listItems = [.. packageLock.Packages
            .Select(package => new ListItem
            {
                Manifest = package,
                Locked = packageLock.Locks.Any(l =>
                {
                    return package.ToothPath == l.ToothPath
                        && package.Version == l.Version
                        && package.GetSpecifiedVariant(
                            l.VariantLabel,
                            RuntimeInformation.RuntimeIdentifier) is not null;
                })
            })];

        return listItems;
    }

    private async Task<PackageLock> GetPackageLock()
    {
        string packageLockFilePath = _pathManager.CurrentPackageLockPath;

        // If the package lock file does not exist, return an empty package lock.
        if (!await _context.FileSystem.File.ExistsAsync(packageLockFilePath))
        {
            return new()
            {
                FormatVersion = PackageLock.DefaultFormatVersion,
                FormatUuid = PackageLock.DefaultFormatUuid,
                Packages = [],
                Locks = []
            };
        }

        byte[] packageLockBytes = await _context.FileSystem.File.ReadAllBytesAsync(packageLockFilePath);

        return PackageLock.FromJsonBytes(packageLockBytes);
    }
}
