namespace Lip;

public partial class Lip
{
    public record PruneArgs
    {
        public required bool DryRun { get; init; }
        public required bool IgnoreScripts { get; init; }
    }

    public async Task Prune(PruneArgs args)
    {
        List<PackageSpecifierWithoutVersion> packageSpecifiers = await _dependencySolver.GetUnnecessaryPackages();

        // Uninstall all unnecessary packages.
        foreach (PackageSpecifierWithoutVersion packageSpecifier in packageSpecifiers)
        {
            await _packageManager.Uninstall(packageSpecifier, args.DryRun, args.IgnoreScripts);
        }
    }
}
