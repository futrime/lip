using Microsoft.Extensions.Logging;
using Semver;
using SharpCompress.Archives;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Lip.Core;

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

    [ExcludeFromCodeCoverage]
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

    public async Task Install(List<string>? userInputPackageTexts, InstallArgs args)
    {
        // Here we regulate the abbreviations of glossary terms used in this method:
        // - UIP: User input packages
        // - IP: Installed packages
        // - LP: Installed and locked packages
        // - DP: Direct and indirect dependencies of UIP and LP, along with UIP and LP themselves
        // - TUP: Packages to uninstall, IP x (-DP + UIP)
        // - TIP: Packages to install, UIP + (DP - IP)
        // - TLP: Packages to lock, UIP + LP

        //
        // Step 1: Get UIP.
        //
        // Result variables:
        // - userInputDetails (UIP)
        // - userInputSpecifiers (UIP)

        #region UIP

        _context.Logger.LogDebug("Getting user input packages...");

        // Parse package install details from user input.
        // If no input, install the current directory with the empty variant label.
        List<PackageInstallDetail> userInputDetails = [];
        foreach (string userInputPackageText in userInputPackageTexts ?? ["."])
        {
            var detail = await GetPackageInstallDetailFromUserInput(userInputPackageText);
            userInputDetails.Add(detail);
        }

        IEnumerable<PackageSpecifier> userInputSpecifiers = userInputDetails
            .Select(detail => detail.Specifier);

        _context.Logger.LogDebug("Packages from user input:");
        foreach (PackageSpecifier userInputSpecifier in userInputSpecifiers)
        {
            _context.Logger.LogDebug("  {specifier}", userInputSpecifier);
        }

        // Validate user input package install details.
        foreach (var detail in userInputDetails)
        {
            PackageLock.Package? installedPackage = await _packageManager.GetPackageFromLock(
                detail.Specifier.Identifier);

            // If (1) not installed, (2) force, or (3) update is specified and the installed
            // package is older, it is okay.
            if (installedPackage is null
                || args.Force
                || (args.Update
                    && installedPackage.Specifier.Version.ComparePrecedenceTo(detail.Manifest.Version) < 0)
            )
            {
                continue;
            }

            // Otherwise, there is a conflict.
            throw new InvalidOperationException(
                $"Package '{detail.Specifier}' from input is already installed with version '{installedPackage.Specifier.Version}'.");
        }

        #endregion

        //
        // Step 2: Get IP.
        //
        // Result variables:
        // - installedSpecifiers (IP)

        #region IP

        _context.Logger.LogDebug("Getting installed packages...");

        PackageLock packageLock = await _packageManager.GetCurrentPackageLock();

        IEnumerable<PackageSpecifier> installedSpecifiers = packageLock.Packages
            .Select(@lock => @lock.Specifier);

        _context.Logger.LogDebug("Installed packages:");
        foreach (PackageSpecifier installedSpecifier in installedSpecifiers)
        {
            _context.Logger.LogDebug("  {specifier}", installedSpecifier);
        }

        #endregion

        //
        // Step 3: Get LP.
        //
        // Result variables:
        // - lockedSpecifiers (LP)

        #region LP

        _context.Logger.LogDebug("Getting locked packages...");

        IEnumerable<PackageSpecifier> lockedSpecifiers = packageLock.Packages
            .Where(@lock => @lock.Locked)
            .Select(@lock => @lock.Specifier);

        _context.Logger.LogDebug("Locked packages:");
        foreach (PackageSpecifier lockedSpecifier in lockedSpecifiers)
        {
            _context.Logger.LogDebug("  {specifier}", lockedSpecifier);
        }

        #endregion

        //
        // Step 4: Get DP.
        //
        // Result variables:
        // - dependentSpecifiers (DP)

        #region DP

        _context.Logger.LogDebug("Getting dependent packages...");

        IEnumerable<PackageSpecifier> dependentSpecifiers = args.NoDependencies
            // If NoDependencies is specified, assume DP = UIP + IP.
            ? [
                ..userInputSpecifiers,
                ..installedSpecifiers
                    // User input packages take precedence over other packages.
                    .Where(installedSpecifier => !userInputSpecifiers
                        .Any(userInputSpecifier => userInputSpecifier.Identifier == installedSpecifier.Identifier)
                    ),
            ]
            : await _dependencySolver.ResolveDependencies(
                primaryPackageSpecifiers: [
                    ..userInputDetails.Select(detail => detail.Specifier),
                    ..lockedSpecifiers
                        // User input packages take precedence over other packages.
                        .Where(lockedSpecifier => !userInputSpecifiers
                            .Any(userInputSpecifier => userInputSpecifier.Identifier == lockedSpecifier.Identifier)
                        ),
                ],
                installedPackageSpecifiers: installedSpecifiers,
                knownPackages: [
                    ..packageLock.Packages,
                    ..userInputDetails.Select(detail => new PackageLock.Package()
                    {
                        Files = [],
                        Locked = true,
                        Manifest = detail.Manifest,
                        VariantLabel = detail.VariantLabel,
                    }),
                ]
            ) ?? throw new InvalidOperationException("Cannot resolve dependencies.");

        _context.Logger.LogDebug("Dependent packages:");
        foreach (PackageSpecifier dependentSpecifier in dependentSpecifiers)
        {
            _context.Logger.LogDebug("  {specifier}", dependentSpecifier);
        }

        #endregion

        //
        // Step 5: Get TUP = IP x (-DP + UIP).
        //
        // Result variables:
        // - uninstallDetails (TUP)

        #region TUP

        _context.Logger.LogDebug("Getting packages to uninstall...");

        TopoSortedPackageList<PackageUninstallDetail> uninstallDetails = [];
        foreach (var installedSpecifier in installedSpecifiers)
        {
            PackageLock.Package installedPackage = (await _packageManager.GetPackageFromLock(installedSpecifier.Identifier))!;

            if (!dependentSpecifiers.Any(dependentSpecifier => dependentSpecifier.Identifier == installedSpecifier.Identifier)
                || userInputSpecifiers.Any(userInputSpecifier => userInputSpecifier.Identifier == installedSpecifier.Identifier))
            {
                uninstallDetails.Add(new PackageUninstallDetail()
                {
                    Package = installedPackage
                });
            }
        }

        _context.Logger.LogDebug("Packages to uninstall:");
        foreach (PackageUninstallDetail uninstallDetail in uninstallDetails)
        {
            _context.Logger.LogDebug("  {specifier}", uninstallDetail.Package.Specifier);
        }

        #endregion

        //
        // Step 6: Get TIP = UIP + (DP - IP).
        //
        // Result variables:
        // - installDetails (TIP)

        #region TIP

        _context.Logger.LogDebug("Getting packages to install...");

        TopoSortedPackageList<PackageInstallDetail> installDetails = [];

        installDetails.AddRange(userInputDetails);

        foreach (PackageSpecifier dependentSpecifier in dependentSpecifiers)
        {
            if (installedSpecifiers.Any(installedSpecifier => installedSpecifier.Identifier == dependentSpecifier.Identifier)
                || userInputDetails.Any(userInputDetail => userInputDetail.Specifier.Identifier == dependentSpecifier.Identifier))
            {
                continue;
            }

            IFileSource fileSource = await _cacheManager.GetPackageFileSource(dependentSpecifier);

            PackageManifest manifest = await _packageManager.GetPackageManifestFromFileSource(fileSource)
                ?? throw new InvalidOperationException($"Cannot get package manifest from package '{dependentSpecifier}'.");

            installDetails.Add(new PackageInstallDetail
            {
                FileSource = fileSource,
                Manifest = manifest,
                VariantLabel = dependentSpecifier.VariantLabel,
            });
        }

        _context.Logger.LogDebug("Packages to install:");
        foreach (PackageInstallDetail installDetail in installDetails)
        {
            _context.Logger.LogDebug("  {specifier}", installDetail.Specifier);
        }

        #endregion

        //
        // Step 7: Get TLP = UIP + LP.
        //
        // Result variables:
        // - specifiersToLock (TLP)

        #region TLP

        _context.Logger.LogDebug("Getting packages to lock...");

        IEnumerable<PackageSpecifier> specifiersToLock = [
            ..userInputSpecifiers,
            ..lockedSpecifiers
                // User input packages take precedence over other packages.
                .Where(lockedSpecifier => !userInputSpecifiers
                    .Any(userInputSpecifier => userInputSpecifier.Identifier == lockedSpecifier.Identifier)
                ),
        ];

        _context.Logger.LogDebug("Packages to lock:");
        foreach (PackageSpecifier specifierToLock in specifiersToLock)
        {
            _context.Logger.LogDebug("  {specifier}", specifierToLock);
        }

        #endregion

        //
        // Step 8: Perform the installation.
        //

        // Uninstall packages in topological order.
        foreach (PackageUninstallDetail uninstallDetail in uninstallDetails)
        {
            await _packageManager.UninstallPackage(
                uninstallDetail.Package.Specifier.Identifier,
                args.DryRun,
                args.IgnoreScripts);
        }

        // Install packages in reverse topological order.
        foreach (PackageInstallDetail packageInstallDetail in installDetails.AsEnumerable().Reverse())
        {
            // Lock the package if it is a primary package specifier.
            await _packageManager.InstallPackage(
                packageInstallDetail.FileSource,
                packageInstallDetail.VariantLabel,
                args.DryRun,
                args.IgnoreScripts,
                locked: specifiersToLock.Contains(packageInstallDetail.Specifier));
        }
    }

    private async Task<PackageInstallDetail> GetPackageInstallDetailFromUserInput(string userInputPackageText)
    {
        // First, check if package text refers to a local directory containing a tooth.json file.

        string possibleDirPath = _context.FileSystem.Path.Combine(_pathManager.WorkingDir, userInputPackageText.Split('#')[0]);

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

        string possibleFilePath = _context.FileSystem.Path.Combine(_pathManager.WorkingDir, userInputPackageText.Split('#')[0]);

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

        // Third, check if package text is a package specifier.

        if (userInputPackageText.Contains('@'))
        {
            PackageSpecifier packageSpecifier = PackageSpecifier.Parse(userInputPackageText);

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

        // Fourth, assume package text is a package identifier and look for the latest version.

        {
            PackageIdentifier packageIdentifier = PackageIdentifier.Parse(userInputPackageText);

            IOrderedEnumerable<SemVersion> packageVersionList = (await _packageManager.GetPackageRemoteVersions(packageIdentifier))
                .OrderByDescending(v => v, SemVersion.PrecedenceComparer);

            if (packageVersionList.Any())
            {
                PackageSpecifier packageSpecifier = PackageSpecifier.FromIdentifier(packageIdentifier, packageVersionList.First());

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
        }

        // If none of the above, throw an exception.

        throw new ArgumentException($"Cannot resolve package text '{userInputPackageText}'.", nameof(userInputPackageText));
    }
}
