using System.Runtime.InteropServices;

namespace Lip;

public partial class Lip
{
    public record UninstallArgs
    {
        public required bool DryRun { get; init; }
        public required bool IgnoreScripts { get; init; }
        public required bool Save { get; init; }
    }

    public async Task Uninstall(List<string> packageTexts, UninstallArgs args)
    {
        List<PackageSpecifierWithoutVersion> packageSpecifiers = [.. packageTexts
            .Select(PackageSpecifierWithoutVersion.Parse)];

        // Check if all packages to uninstall are installed.

        foreach (PackageSpecifierWithoutVersion packageSpecifier in packageSpecifiers)
        {
            if (await _packageManager.GetInstalledPackageManifest(
                packageSpecifier.ToothPath,
                packageSpecifier.VariantLabel) is null)
            {
                throw new InvalidOperationException($"Package '{packageSpecifier}' is not installed.");
            }
        }

        // Uninstall all packages.

        foreach (PackageSpecifierWithoutVersion packageSpecifier in packageSpecifiers)
        {
            await _packageManager.Uninstall(packageSpecifier, args.DryRun, args.IgnoreScripts);
        }

        // Update the package lock file.

        PackageLock packageLock = await _packageManager.GetCurrentPackageLock();

        PackageLock newPackageLock = packageLock with
        {
            Locks = [.. packageLock.Locks
                    .Where(lockType => !packageSpecifiers.Contains(new()
                    {
                        ToothPath = lockType.Package.ToothPath,
                        VariantLabel = lockType.VariantLabel
                    }))]
        };

        if (!args.DryRun)
        {
            await _packageManager.SaveCurrentPackageLock(newPackageLock);
        }

        // If to save, update the package manifest file.

        if (args.Save)
        {
            PackageManifest packageManifest = await _packageManager.GetCurrentPackageManifestWithTemplate()
                ?? throw new InvalidOperationException("Package manifest is not found.");

            PackageManifest newPackagemanifest = packageManifest with
            {
                Variants = packageManifest.Variants?
                    .Select(variant =>
                    {
                        if (!packageSpecifiers.Any(packageSpecifier => variant.Match(
                            string.Empty,
                            RuntimeInformation.RuntimeIdentifier)))
                        {
                            return variant;
                        }

                        return variant with
                        {
                            Dependencies = variant.Dependencies?
                                .Where(dependency => !packageSpecifiers.Any(
                                    packageSpecifier => packageSpecifier.ToothPath == dependency.Key))
                                .ToDictionary()
                        };
                    }).ToList()
            };

            if (!args.DryRun)
            {
                await _packageManager.SaveCurrentPackageManifest(newPackagemanifest);
            }
        }
    }
}
