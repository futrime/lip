using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace Lip.CLI;

[Description("Install packages and their dependencies from various sources.")]
class InstallCommand : AsyncCommand<InstallCommand.Settings>
{
    public class Settings : BaseCommandSettings
    {
        [CommandArgument(0, "[package ...]")]
        [Description("The packages to install. If no packages are specified, lip will install the package in the current directory.")]
        public required string[]? Packages { get; init; }

        [CommandOption("--dry-run")]
        [Description("Do not actually install any packages. Be aware that files will still be downloaded and cached.")]
        public required bool DryRun { get; init; }

        [CommandOption("-f|--force")]
        [Description("Force the installation of the package. When a package is already installed but its version is different from the specified version, lip will reinstall the package.")]
        public required bool Force { get; init; }

        [CommandOption("--ignore-scripts")]
        [Description("Do not run any scripts during installation.")]
        public required bool IgnoreScripts { get; init; }

        [CommandOption("--no-dependencies")]
        [Description("Bypass dependency resolution and only install the specified packages.")]
        public required bool NoDependencies { get; init; }

        [CommandOption("-U|--update")]
        [Description("Update the package to the specified version if the installed version is older.")]
        public required bool Update { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        (Core.Lip lip, ILogger logger, UserInteraction userInteraction) = await CommandRoot.Prepare(settings);

        await lip.Install(settings.Packages?.ToList(), new()
        {
            DryRun = settings.DryRun,
            Force = settings.Force,
            IgnoreScripts = settings.IgnoreScripts,
            NoDependencies = settings.NoDependencies,
            Update = settings.Update,
        });

        return 0;
    }
}