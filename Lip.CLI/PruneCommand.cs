using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace Lip.CLI;

[Description("Remove all unused packages. Dependencies of other packages will be preserved and skipped during pruning.")]
class PruneCommand : AsyncCommand<PruneCommand.Settings>
{
    public class Settings : BaseCommandSettings
    {
        [CommandOption("--dry-run")]
        [Description("Do not actually uninstall any packages.")]
        public required bool DryRun { get; init; }

        [CommandOption("--ignore-scripts")]
        [Description("Do not run any scripts during uninstallation.")]
        public required bool IgnoreScripts { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        (Core.Lip lip, ILogger logger, UserInteraction userInteraction) = await CommandRoot.Prepare(settings);

        await lip.Prune(new()
        {
            DryRun = settings.DryRun,
            IgnoreScripts = settings.IgnoreScripts,
        });

        return 0;
    }
}
