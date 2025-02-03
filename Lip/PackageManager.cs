using System.IO.Abstractions;
using System.Runtime.InteropServices;
using Flurl;
using Lip.Context;
using Microsoft.Extensions.Logging;
using Semver;

namespace Lip;

public class PackageManager(
    IContext context,
    CacheManager cacheManager,
    PathManager pathManager
)
{
    private readonly CacheManager _cacheManager = cacheManager;
    private readonly IContext _context = context;
    private readonly PathManager _pathManager = pathManager;

    public async Task<PackageLock> GetCurrentPackageLock()
    {
        string packageLockFilePath = _pathManager.CurrentPackageLockPath;

        // If the package lock file does not exist, return an empty package lock.
        if (!await _context.FileSystem.File.ExistsAsync(packageLockFilePath))
        {
            return new()
            {
                FormatVersion = PackageLock.DefaultFormatVersion,
                FormatUuid = PackageLock.DefaultFormatUuid,
                Locks = []
            };
        }

        byte[] packageLockBytes = await _context.FileSystem.File.ReadAllBytesAsync(packageLockFilePath);

        return PackageLock.FromJsonBytes(packageLockBytes);
    }

    public async Task<PackageManifest?> GetCurrentPackageManifestParsed()
    {
        byte[]? packageManifestBytes = await GetCurrentPackageManifestBytes();

        if (packageManifestBytes == null)
        {
            return null;
        }

        return PackageManifest.FromJsonBytesParsed(packageManifestBytes);
    }

    public async Task<PackageManifest?> GetCurrentPackageManifestWithTemplate()
    {
        byte[]? packageManifestBytes = await GetCurrentPackageManifestBytes();

        if (packageManifestBytes == null)
        {
            return null;
        }

        return PackageManifest.FromJsonBytesWithTemplate(packageManifestBytes);
    }

    public async Task<SemVersion?> GetInstalledPackageVersion(string toothPath, string variantLabel)
    {
        PackageLock packageLock = await GetCurrentPackageLock();

        List<PackageLock.LockType> locks = [.. packageLock.Locks.Where(@lock => @lock.Package.ToothPath == toothPath
                                              && @lock.VariantLabel == variantLabel)];

        if (locks.Count == 0)
        {
            return null;
        }
        else
        {
            return locks[0].Package.Version;
        }
    }

    public async Task Install(IFileSource packageFileSource, string variantLabel, bool dryRun, bool ignoreScripts, bool locked)
    {
        Stream packageManifestFileStream = await packageFileSource.GetFileStream(_pathManager.PackageManifestFileName)
            ?? throw new InvalidOperationException("Package manifest not found.");

        PackageManifest packageManifest = PackageManifest.FromJsonBytesParsed(await packageManifestFileStream.ReadAsync());

        PackageSpecifier packageSpecifier = new()
        {
            ToothPath = packageManifest.ToothPath,
            Version = packageManifest.Version,
            VariantLabel = variantLabel
        };

        // If the package has already been installed, skip installing. Or if the package has been
        // installed with a different version, throw exception.

        SemVersion? installedVersion = await GetInstalledPackageVersion(packageSpecifier.ToothPath, packageSpecifier.VariantLabel);

        if (installedVersion == packageSpecifier.Version)
        {
            _context.Logger.LogInformation("Package {packageSpecifier} is already installed with version {installedVersion}", packageSpecifier, installedVersion);
            return;
        }

        if (installedVersion != null)
        {
            throw new InvalidOperationException($"Package {packageSpecifier} is already installed with version {installedVersion}.");
        }

        // If the package does not contain the variant to install, throw exception.

        PackageManifest.VariantType packageVariant = packageManifest.GetSpecifiedVariant(
            string.Empty,
            RuntimeInformation.RuntimeIdentifier)
            ?? throw new InvalidOperationException($"The package does not contain variant {variantLabel}.");

        // Run pre-install scripts.

        if (!ignoreScripts)
        {
            PackageManifest.ScriptsType? script = packageVariant.Scripts;
            List<string>? preInstallScripts = script?.PreInstall;

            preInstallScripts?.ForEach(script =>
            {
                _context.Logger.LogDebug("Running script: {script}", script);

                if (!dryRun)
                {
                    _context.CommandRunner.Run(
                        script,
                        _pathManager.WorkingDir);
                }
            });
        }

        // Place files.

        foreach (PackageManifest.AssetType asset in packageVariant.Assets ?? [])
        {
            IFileSource fileSource = await GetAssetFileSource(asset, packageSpecifier);

            foreach (IFileSourceEntry fileSourceEntry in await fileSource.GetAllEntries())
            {
                foreach (PackageManifest.PlaceType place in asset.Place ?? [])
                {
                    string? destRelative = _pathManager.GetPlacementRelativePath(place, fileSourceEntry.Key);

                    if (destRelative is null)
                    {
                        continue;
                    }

                    string destPath = _context.FileSystem.Path.Join(_pathManager.WorkingDir, destRelative);

                    if (_context.FileSystem.Path.Exists(destPath))
                    {
                        throw new InvalidOperationException($"File {destPath} already exists.");
                    }

                    _context.Logger.LogDebug("Placing file: {entryKey} -> {destPath}", fileSourceEntry.Key, destPath);

                    if (!dryRun)
                    {
                        using Stream fileSourceEntryStream = await fileSourceEntry.OpenRead();

                        await _context.FileSystem.File.WriteAllBytesAsync(
                            destPath,
                            await fileSourceEntryStream.ReadAsync());
                    }
                }
            }
        }

        // Run install scripts.

        if (!ignoreScripts)
        {
            PackageManifest.ScriptsType? script = packageVariant.Scripts;
            List<string>? installScripts = script?.Install;

            installScripts?.ForEach(script =>
            {
                _context.Logger.LogDebug("Running script: {script}", script);

                if (!dryRun)
                {
                    _context.CommandRunner.Run(
                        script,
                        _pathManager.WorkingDir);
                }
            });
        }

        // Update package lock.

        PackageLock packageLock = await GetCurrentPackageLock();

        packageLock.Locks.Add(new()
        {
            Package = packageManifest,
            VariantLabel = variantLabel,
            Locked = locked
        });

        await _context.FileSystem.File.WriteAllBytesAsync(
            _pathManager.CurrentPackageLockPath,
            packageLock.ToJsonBytes());

        // Run post-install scripts.

        if (!ignoreScripts)
        {
            PackageManifest.ScriptsType? script = packageVariant.Scripts;
            List<string>? postInstallScripts = script?.PostInstall;

            postInstallScripts?.ForEach(script =>
            {
                _context.Logger.LogDebug("Running script: {script}", script);
                if (!dryRun)
                {
                    _context.CommandRunner.Run(
                        script,
                        _pathManager.WorkingDir);
                }
            });
        }

        _context.Logger.LogInformation("Package {packageSpecifier} installed.", packageSpecifier);
    }

    public async Task Uninstall(PackageSpecifierWithoutVersion packageSpecifierWithoutVersion, bool dryRun, bool ignoreScripts)
    {
        SemVersion? installedVersion = await GetInstalledPackageVersion(packageSpecifierWithoutVersion.ToothPath, packageSpecifierWithoutVersion.VariantLabel);

        // If the package is not installed, skip uninstalling.

        if (installedVersion is null)
        {
            _context.Logger.LogInformation("Package {packageSpecifier} is not installed.", packageSpecifierWithoutVersion);
            return;
        }

        PackageSpecifier packageSpecifier = new()
        {
            ToothPath = packageSpecifierWithoutVersion.ToothPath,
            Version = installedVersion,
            VariantLabel = packageSpecifierWithoutVersion.VariantLabel
        };

        PackageManifest packageManifest = (await GetCurrentPackageLock()).Locks
            .Where(@lock => @lock.Package.ToothPath == packageSpecifier.ToothPath
                && @lock.Package.Version == packageSpecifier.Version
                && @lock.VariantLabel == packageSpecifier.VariantLabel)
            .Select(@lock => @lock.Package)
            .FirstOrDefault()!;

        // If the package does not contain the variant to install, throw exception.

        PackageManifest.VariantType packageVariant = packageManifest.GetSpecifiedVariant(
            string.Empty,
            RuntimeInformation.RuntimeIdentifier)
            ?? throw new InvalidOperationException($"The package does not contain variant {packageSpecifier.VariantLabel}.");

        // Run pre-uninstall scripts.

        if (!ignoreScripts)
        {
            PackageManifest.ScriptsType? script = packageVariant.Scripts;
            List<string>? preUninstallScripts = script?.PreUninstall;

            preUninstallScripts?.ForEach(script =>
            {
                _context.Logger.LogDebug("Running script: {script}", script);

                if (!dryRun)
                {
                    _context.CommandRunner.Run(
                        script,
                        _pathManager.WorkingDir);
                }
            });
        }

        // Run uninstall scripts.

        if (!ignoreScripts)
        {
            PackageManifest.ScriptsType? script = packageVariant.Scripts;
            List<string>? uninstallScripts = script?.Uninstall;

            uninstallScripts?.ForEach(script =>
            {
                _context.Logger.LogDebug("Running script: {script}", script);

                if (!dryRun)
                {
                    _context.CommandRunner.Run(
                        script,
                        _pathManager.WorkingDir);
                }
            });
        }

        // Remove files.

        foreach (PackageManifest.AssetType asset in packageVariant.Assets ?? [])
        {
            IFileSource fileSource = await GetAssetFileSource(asset, packageSpecifier);

            List<string> preserve = asset.Preserve ?? [];
            List<string> remove = asset.Remove ?? [];

            foreach (IFileSourceEntry fileSourceEntry in await fileSource.GetAllEntries())
            {
                foreach (PackageManifest.PlaceType place in asset.Place ?? [])
                {
                    string? destRelative = _pathManager.GetPlacementRelativePath(place, fileSourceEntry.Key);

                    if (destRelative is null)
                    {
                        continue;
                    }

                    if (preserve.Contains(destRelative))
                    {
                        continue;
                    }

                    string destPath = _context.FileSystem.Path.Join(_pathManager.WorkingDir, destRelative);

                    _context.Logger.LogDebug("Removing file: {destPath}", destPath);

                    if (!dryRun)
                    {
                        if (_context.FileSystem.File.Exists(destPath))
                        {
                            _context.FileSystem.File.Delete(destPath);
                        }

                        RemoveParentDirectoriesUntilWorkingDir(destPath);
                    }
                }
            }

            foreach (string removePath in remove)
            {
                string destPath = _context.FileSystem.Path.Join(_pathManager.WorkingDir, removePath);

                _context.Logger.LogDebug("Removing file: {destPath}", destPath);

                if (!dryRun)
                {
                    if (_context.FileSystem.File.Exists(destPath))
                    {
                        _context.FileSystem.File.Delete(destPath);
                    }

                    RemoveParentDirectoriesUntilWorkingDir(destPath);
                }
            }
        }

        // Update package lock.

        PackageLock packageLock = await GetCurrentPackageLock();

        packageLock.Locks.RemoveAll(@lock => @lock.Package.ToothPath == packageSpecifier.ToothPath
            && @lock.Package.Version == packageSpecifier.Version
            && @lock.VariantLabel == packageSpecifier.VariantLabel);

        await _context.FileSystem.File.WriteAllBytesAsync(
            _pathManager.CurrentPackageLockPath,
            packageLock.ToJsonBytes());

        // Run post-uninstall scripts.

        if (!ignoreScripts)
        {
            PackageManifest.ScriptsType? script = packageVariant.Scripts;
            List<string>? postUninstallScripts = script?.PostUninstall;

            postUninstallScripts?.ForEach(script =>
            {
                _context.Logger.LogDebug("Running script: {script}", script);

                if (!dryRun)
                {
                    _context.CommandRunner.Run(
                        script,
                        _pathManager.WorkingDir);
                }
            });
        }
    }

    private async Task<IFileSource> GetAssetFileSource(PackageManifest.AssetType asset, PackageSpecifier packageSpecifier)
    {
        if (asset.Type == PackageManifest.AssetType.TypeEnum.Self)
        {
            return await _cacheManager.GetPackageFileSource(packageSpecifier);
        }

        List<Url> urls = asset.Urls?.Select(url => new Url(url)).ToList()
            ?? throw new InvalidOperationException("Asset URLs are not specified.");

        IFileInfo assetFile = await _cacheManager.GetDownloadedFile(urls);

        if (asset.Type == PackageManifest.AssetType.TypeEnum.Uncompressed)
        {
            return new StandaloneFileSource(_context.FileSystem, assetFile.FullName);
        }
        else
        {
            return new ArchiveFileSource(_context.FileSystem, assetFile.FullName);
        }
    }

    private async Task<byte[]?> GetCurrentPackageManifestBytes()
    {
        string packageManifestFilePath = _pathManager.CurrentPackageManifestPath;

        if (!await _context.FileSystem.File.ExistsAsync(packageManifestFilePath))
        {
            return null;
        }

        return await _context.FileSystem.File.ReadAllBytesAsync(packageManifestFilePath);
    }

    private void RemoveParentDirectoriesUntilWorkingDir(string path)
    {
        path = _context.FileSystem.Path.Join(_pathManager.WorkingDir, path);

        string? parentDir = _context.FileSystem.Path.GetDirectoryName(path);

        while (parentDir != null
               && parentDir.StartsWith(_pathManager.WorkingDir)
               && parentDir != _pathManager.WorkingDir)
        {
            if (_context.FileSystem.Directory.Exists(parentDir)
                && !_context.FileSystem.Directory.EnumerateFileSystemEntries(parentDir).Any())
            {
                _context.FileSystem.Directory.Delete(parentDir);
            }

            parentDir = _context.FileSystem.Path.GetDirectoryName(parentDir);
        }
    }
}
