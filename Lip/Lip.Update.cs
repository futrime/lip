namespace Lip;

public partial class Lip
{
    public record UpdateArgs
    {
        public required bool DryRun { get; init; }
        public required bool Force { get; init; }
        public required bool IgnoreScripts { get; init; }
        public required bool NoDependencies { get; init; }
    }

    public async Task Update(List<string> userInputPackageTexts, UpdateArgs args)
    {
        await Install(userInputPackageTexts, new InstallArgs
        {
            DryRun = args.DryRun,
            Force = args.Force,
            IgnoreScripts = args.IgnoreScripts,
            NoDependencies = args.NoDependencies,
            Update = true
        });
    }
}
