using Microsoft.Extensions.Logging;
using Semver;
using System.Diagnostics.CodeAnalysis;

namespace Lip.Core;

public partial class Lip
{
    public record UninstallArgs
    {
        public required bool DryRun { get; init; }
        public required bool IgnoreScripts { get; init; }
    }

    [ExcludeFromCodeCoverage]
    private record PackageUninstallDetail : TopoSortedPackageList<PackageUninstallDetail>.IItem
    {
        public Dictionary<PackageIdentifier, SemVersionRange> Dependencies => Package.Variant.Dependencies;

        public required PackageLock.Package Package { get; init; }

        public PackageSpecifier Specifier => Package.Specifier;
    }

    public async Task Uninstall(List<string> packageSpecifierTextsToUninstall, UninstallArgs args)
    {
        List<PackageIdentifier> packageSpecifiersToUninstallSpecified =
            packageSpecifierTextsToUninstall.ConvertAll(PackageIdentifier.Parse);

        // Remove non-installed packages and sort packages topologically.

        TopoSortedPackageList<PackageUninstallDetail> packageUninstallDetails = [];

        foreach (PackageIdentifier identifier in packageSpecifiersToUninstallSpecified)
        {
            PackageLock.Package? package = await _packageManager.GetPackageFromLock(
                identifier);

            if (package is null)
            {
                _context.Logger.LogWarning(
                    "Package '{identifier}' is not installed. Skipping.",
                    identifier);
                continue;
            }

            packageUninstallDetails.Add(new PackageUninstallDetail
            {
                Package = package
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
    }
}
