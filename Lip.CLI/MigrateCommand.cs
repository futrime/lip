using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace Lip.CLI;

[Description("Migrate a tooth.json file to the latest schema version.")]
class MigrateCommand : AsyncCommand<MigrateCommand.Settings>
{
    public class Settings : BaseCommandSettings
    {
        [CommandArgument(0, "<path>")]
        [Description("The path to the tooth.json file to migrate.")]
        public required string Path { get; init; }

        [CommandArgument(1, "[output]")]
        [Description("The path to write the migrated tooth.json file to. Defaults to the input path.")]
        public string? Output { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        (Core.Lip lip, ILogger logger, UserInteraction userInteraction) = await CommandRoot.Prepare(settings);

        await lip.Migrate(settings.Path, settings.Output, new());

        return 0;
    }
}
