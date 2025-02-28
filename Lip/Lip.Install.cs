using Microsoft.Extensions.Logging;
using Semver;
using SharpCompress.Archives;
using System.Runtime.InteropServices;

namespace Lip;

public partial class Lip
{
    public record InstallArgs
    {
        public required bool DryRun { get; init; }
        public required bool Force { get; init; }
        public required bool IgnoreScripts { get; init; }
        public required bool NoDependencies { get; init; }
        public required bool Update { get; init; }
    }

    private record PackageInstallDetail : TopoSortedPackageList<PackageInstallDetail>.IItem
    {
        public Dictionary<PackageIdentifier, SemVersionRange> Dependencies
        {
            get
            {
                return Manifest.GetVariant(
                        VariantLabel,
                        RuntimeInformation.RuntimeIdentifier)?
                        .Dependencies ?? [];
            }
        }

        public required IFileSource FileSource { get; init; }

        public required PackageManifest Manifest { get; init; }

        public PackageSpecifier Specifier => new()
        {
            ToothPath = Manifest.ToothPath,
            VariantLabel = VariantLabel,
            Version = Manifest.Version
        };

        public required string VariantLabel { get; init; }
    }

    public async Task Install(List<string> userInputPackageTexts, InstallArgs args)
    {
        // Parse user input package texts for packages and check conflicts.

        TopoSortedPackageList<PackageInstallDetail> packageInstallDetails = [];
        List<PackageSpecifier> packageSpecifiersToInstallSpecified = [];

        foreach (string packageText in userInputPackageTexts)
        {
            PackageInstallDetail installDetail = await GetPackageInstallDetailFromUserInput(packageText);

            PackageManifest? installedPackageManifest = await _packageManager.GetPackageManifestFromLock(
                installDetail.Specifier.Identifier);

            // If not installed, add to install details.
            if (installedPackageManifest is null)
            {
                packageInstallDetails.Add(installDetail);
                packageSpecifiersToInstallSpecified.Add(installDetail.Specifier);

                continue;
            }

            // If force, add to install details.
            if (args.Force)
            {
                packageInstallDetails.Add(installDetail);
                packageSpecifiersToInstallSpecified.Add(installDetail.Specifier);

                continue;
            }

            // If installed with the same version, skip.
            if (installedPackageManifest.Version == installDetail.Manifest.Version)
            {
                _context.Logger.LogWarning(
                    "Package '{specifier}' is already installed. Skipping.",
                    new PackageSpecifier()
                    {
                        ToothPath = installDetail.Manifest.ToothPath,
                        VariantLabel = installDetail.VariantLabel,
                        Version = installDetail.Manifest.Version
                    }
                );

                continue;
            }

            // If installed with a previous version and Update is specified, add to install details.
            if (args.Update && installedPackageManifest.Version.ComparePrecedenceTo(
                installDetail.Manifest.Version) < 0)
            {
                packageInstallDetails.Add(installDetail);
                packageSpecifiersToInstallSpecified.Add(installDetail.Specifier);

                continue;
            }

            // Otherwise, there is a conflict.

            PackageSpecifier specifier = new()
            {
                ToothPath = installDetail.Manifest.ToothPath,
                VariantLabel = installDetail.VariantLabel,
                Version = installDetail.Manifest.Version
            };

            throw new InvalidOperationException(
                $"Package '{specifier}' is already installed with a different version '{installedPackageManifest.Version}.");
        }

        // Solve dependencies.

        PackageLock packageLock = await _packageManager.GetCurrentPackageLock();

        List<PackageSpecifier> primaryPackageSpecifiers = [
            .. packageInstallDetails
            .Select(detail => new PackageSpecifier()
            {
                ToothPath = detail.Manifest.ToothPath,
                VariantLabel = detail.VariantLabel,
                Version = detail.Manifest.Version
            }),
            .. packageLock.Locks
            .Where(@lock => @lock.Locked)
            .Select(@lock => @lock.Specifier)];

        List<PackageSpecifier> packageSpecifiersToInstall = (args.NoDependencies
            ? primaryPackageSpecifiers
            : await _dependencySolver.ResolveDependencies(
                primaryPackageSpecifiers,
                [.. packageLock.Locks.Select(@lock => @lock.Specifier)]))
                ?? throw new InvalidOperationException("Cannot resolve dependencies.");

        // Prepare package install details and uninstall details.

        TopoSortedPackageList<PackageUninstallDetail> packageUninstallDetails = [];

        foreach (PackageSpecifier packageSpecifierToInstall in packageSpecifiersToInstall)
        {
            // Skip primary package specifiers because they are already in the install details.

            if (primaryPackageSpecifiers.Contains(packageSpecifierToInstall))
            {
                continue;
            }

            // If installed with the same version, skip.

            PackageManifest? installedPackageManifest = await _packageManager.GetPackageManifestFromLock(
                packageSpecifierToInstall.Identifier);

            if (installedPackageManifest?.Version == packageSpecifierToInstall.Version)
            {
                _context.Logger.LogInformation(
                    "Dependency package '{specifier}' is already installed. Skipping.",
                    new PackageSpecifier()
                    {
                        ToothPath = packageSpecifierToInstall.ToothPath,
                        VariantLabel = packageSpecifierToInstall.VariantLabel,
                        Version = packageSpecifierToInstall.Version
                    }
                );
                continue;
            }

            // If installed with different version, add to uninstall details.

            if (installedPackageManifest is not null
                && installedPackageManifest.Version != packageSpecifierToInstall.Version)
            {
                packageUninstallDetails.Add(new PackageUninstallDetail
                {
                    Manifest = installedPackageManifest,
                    VariantLabel = packageSpecifierToInstall.VariantLabel
                });
            }

            // Add to install details.

            IFileSource fileSource = await _cacheManager.GetPackageFileSource(packageSpecifierToInstall);

            PackageManifest packageManifest = await _packageManager.GetPackageManifestFromFileSource(fileSource)
                ?? throw new InvalidOperationException($"Cannot get package manifest from package '{packageSpecifierToInstall}'.");

            packageInstallDetails.Add(new PackageInstallDetail
            {
                FileSource = fileSource,
                Manifest = packageManifest,
                VariantLabel = packageSpecifierToInstall.VariantLabel
            });
        }

        // Uninstall packages in topological order.

        foreach (PackageUninstallDetail packageUninstallDetail in packageUninstallDetails)
        {
            await _packageManager.UninstallPackage(
                packageUninstallDetail.Specifier.Identifier,
                args.DryRun,
                args.IgnoreScripts);
        }

        // Install packages in reverse topological order.

        foreach (PackageInstallDetail packageInstallDetail
            in packageInstallDetails.AsEnumerable().Reverse())
        {
            // Lock the package if it is a primary package specifier.
            await _packageManager.InstallPackage(
                packageInstallDetail.FileSource,
                packageInstallDetail.VariantLabel,
                args.DryRun,
                args.IgnoreScripts,
                locked: primaryPackageSpecifiers.Contains(packageInstallDetail.Specifier));
        }
    }

    private async Task<PackageInstallDetail> GetPackageInstallDetailFromUserInput(string userInputPackageText)
    {
        // First, check if package text refers to a local directory containing a tooth.json file.

        string possibleDirPath = _context.FileSystem.Path.Join(_pathManager.WorkingDir, userInputPackageText.Split('#')[0]);

        if (_context.FileSystem.Directory.Exists(possibleDirPath))
        {
            DirectoryFileSource directoryFileSource = new(_context.FileSystem, possibleDirPath);

            PackageManifest? packageManifest = await _packageManager.GetPackageManifestFromFileSource(directoryFileSource);

            if (packageManifest is not null)
            {
                return new()
                {
                    FileSource = directoryFileSource,
                    Manifest = packageManifest,
                    VariantLabel = userInputPackageText.Split('#').ElementAtOrDefault(1) ?? string.Empty
                };
            }
        }

        // Second, check if package text refers to a local archive file.

        string possibleFilePath = _context.FileSystem.Path.Join(_pathManager.WorkingDir, userInputPackageText.Split('#')[0]);

        if (_context.FileSystem.File.Exists(possibleFilePath))
        {
            using Stream fileStream = _context.FileSystem.File.OpenRead(possibleFilePath);

            if (ArchiveFactory.IsArchive(fileStream, out _))
            {
                ArchiveFileSource archiveFileSource = new(_context.FileSystem, possibleFilePath);

                PackageManifest? packageManifest = await _packageManager.GetPackageManifestFromFileSource(archiveFileSource);

                if (packageManifest is not null)
                {
                    return new()
                    {
                        FileSource = archiveFileSource,
                        Manifest = packageManifest,
                        VariantLabel = userInputPackageText.Split('#').ElementAtOrDefault(1) ?? string.Empty
                    };
                }
            }
        }

        // Third, assume package text is a package specifier.

        {
            var packageSpecifier = PackageSpecifier.Parse(userInputPackageText);

            IFileSource fileSource = await _cacheManager.GetPackageFileSource(packageSpecifier);

            PackageManifest? packageManifest = await _packageManager.GetPackageManifestFromFileSource(fileSource);

            if (packageManifest is not null)
            {
                return new()
                {
                    FileSource = fileSource,
                    Manifest = packageManifest,
                    VariantLabel = packageSpecifier.VariantLabel
                };
            }
        }

        // If none of the above, throw an exception.

        throw new ArgumentException($"Cannot resolve package text '{userInputPackageText}'.", nameof(userInputPackageText));
    }
}
