using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace Lip.CLI;

[Description("Update packages and their dependencies from various sources. Equivalent to `lip install --update <package...>`.")]
class UpdateCommand : AsyncCommand<UpdateCommand.Settings>
{
    public class Settings : BaseCommandSettings
    {
        [CommandArgument(0, "<package ...>")]
        [Description("The package to update.")]
        public required string[] Packages { get; init; }

        [CommandOption("--dry-run")]
        [Description("Simulate the update without making any changes. Files will still be downloaded and cached.")]
        public required bool DryRun { get; init; }

        [CommandOption("-f|--force")]
        [Description("Force the installation of the package. When a package is already installed but its version is higher than the specified version, lip will still reinstall the package.")]
        public required bool Force { get; init; }

        [CommandOption("--ignore-scripts")]
        [Description("Do not run any scripts during updating.")]
        public required bool IgnoreScripts { get; init; }

        [CommandOption("--no-dependencies")]
        [Description("Bypass dependency resolution and only install the specified packages.")]
        public required bool NoDependencies { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        (Core.Lip lip, ILogger logger, UserInteraction userInteraction) = await CommandRoot.Prepare(settings);

        await lip.Update([.. settings.Packages], new()
        {
            DryRun = settings.DryRun,
            Force = settings.Force,
            IgnoreScripts = settings.IgnoreScripts,
            NoDependencies = settings.NoDependencies
        });

        return 0;
    }
}