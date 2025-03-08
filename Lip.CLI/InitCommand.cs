using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace Lip.CLI;

[Description("This command can be used to set up a new or existing lip package.")]
class InitCommand : AsyncCommand<InitCommand.Settings>
{
    public class Settings : BaseCommandSettings
    {
        [CommandOption("-f|--force")]
        [Description("Overwrite the existing tooth.json file.")]
        public required bool Force { get; init; }

        [CommandOption("--init-avatar-url <url>")]
        [Description("The avatar URL to use.")]
        public required string? InitAvatarUrl { get; init; }

        [CommandOption("--init-description <description>")]
        [Description("The description to use.")]
        public required string? InitDescription { get; init; }

        [CommandOption("--init-name <name>")]
        [Description("The name to use.")]
        public required string? InitName { get; init; }

        [CommandOption("--init-tooth <tooth>")]
        [Description("The package's tooth path to use.")]
        public required string? InitTooth { get; init; }

        [CommandOption("--init-version <version>")]
        [Description("The version to use.")]
        public required string? InitVersion { get; init; }

        [CommandOption("-y|--yes")]
        [Description("Skip confirmation prompts.")]
        public required bool Yes { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        (Core.Lip lip, ILogger logger, UserInteraction userInteraction) = await CommandRoot.Prepare(
            settings, doNotRunProgressService: true);

        await lip.Init(new()
        {
            Force = settings.Force,
            InitAvatarUrl = settings.InitAvatarUrl,
            InitDescription = settings.InitDescription,
            InitName = settings.InitName,
            InitTooth = settings.InitTooth,
            InitVersion = settings.InitVersion,
            Yes = settings.Yes,
        });

        return 0;
    }
}
