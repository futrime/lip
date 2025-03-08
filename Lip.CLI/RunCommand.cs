using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace Lip.CLI;

[Description("Run a script specified in the `tooth.json` file. Built-in script hooks cannot be run with this command.")]
class RunCommand : AsyncCommand<RunCommand.Settings>
{
    public class Settings : BaseCommandSettings
    {
        [CommandArgument(0, "<script>")]
        [Description("he script to run.")]
        public required string Script { get; init; }

        [CommandOption("--variant")]
        [Description("The label of the variant to use. If not specified, the default variant \"\" (empty string) is used.")]
        public required string? Variant { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        (Core.Lip lip, ILogger logger, UserInteraction userInteraction) = await CommandRoot.Prepare(settings);

        await lip.Run(settings.Script, new()
        {
            VariantLabel = settings.Variant ?? string.Empty,
        });

        return 0;
    }
}
