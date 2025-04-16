using Semver;
using System.Runtime.InteropServices;

namespace Lip.Core;

public partial class Lip
{
    public record PruneArgs
    {
        public required bool DryRun { get; init; }
        public required bool IgnoreScripts { get; init; }
    }

    public async Task Prune(PruneArgs args)
    {
        List<PackageIdentifier> packageSpecifiersUnnecessary = await _dependencySolver
            .GetUnnecessaryPackages();

        // Sort packages topologically.

        TopoSortedPackageList<PackageUninstallDetail> packageUninstallDetails = [];

        foreach (PackageIdentifier packageSpecifier in packageSpecifiersUnnecessary)
        {
            PackageLock.Package package = (await _packageManager.GetPackageFromLock(
                packageSpecifier))!; // We know that the package is installed.

            packageUninstallDetails.Add(new PackageUninstallDetail
            {
                Package = package
            });
        }

        // Uninstall all packages in topological order.

        foreach (PackageUninstallDetail packageUninstallDetail in packageUninstallDetails)
        {
            await _packageManager.UninstallPackage(
                packageUninstallDetail.Specifier.Identifier,
                args.DryRun,
                args.IgnoreScripts);
        }
    }
}