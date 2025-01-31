using System.Runtime.InteropServices;
using Lip.Context;
using Microsoft.Extensions.Logging;

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

    public async Task<bool> CheckInstalledPackage(PackageSpecifier packageSpecifier)
    {
        PackageLock packageLock = await GetCurrentPackageLock();

        return packageLock.Locks.Any(@lock => @lock.Package.ToothPath == packageSpecifier.ToothPath
                                              && @lock.Package.Version == packageSpecifier.Version
                                              && @lock.VariantLabel == packageSpecifier.VariantLabel);
    }

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

    public async Task Install(IFileSource packageFileSource, string variantLabel, bool dryRun, bool ignoreScripts)
    {
        Stream packageManifestFileStream = await packageFileSource.GetFileStream(_pathManager.PackageManifestFileName)
            ?? throw new InvalidOperationException("Package manifest not found.");

        PackageManifest packageManifest = PackageManifest.FromJsonBytesParsed(await packageManifestFileStream.ReadAsync());

        // If the package has already been installed, skip installing.

        PackageSpecifier packageSpecifier = new()
        {
            ToothPath = packageManifest.ToothPath,
            Version = packageManifest.Version,
            VariantLabel = variantLabel
        };

        if (await CheckInstalledPackage(packageSpecifier))
        {
            return;
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

        }

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
    }

    // private async Task<IFileSource> GetAssetFileSource(PackageManifest.AssetType asset, PackageSpecifier packageSpecifier)
    // {
    //     if (asset.Type == PackageManifest.AssetType.TypeEnum.Self)
    //     {
    //         _cacheManager.GetPackageManifestFile
    //     }
    // }

    private async Task<byte[]?> GetCurrentPackageManifestBytes()
    {
        string packageManifestFilePath = _pathManager.CurrentPackageManifestPath;

        if (!await _context.FileSystem.File.ExistsAsync(packageManifestFilePath))
        {
            return null;
        }

        return await _context.FileSystem.File.ReadAllBytesAsync(packageManifestFilePath);
    }
}
