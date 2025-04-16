using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace Lip.CLI;

[Description("Show information about a package. If not cached, lip will download the package.")]
class ViewCommand : AsyncCommand<ViewCommand.Settings>
{
    public class Settings : BaseCommandSettings
    {
        [CommandArgument(0, "<package>")]
        [Description("The package to view.")]
        public required string Package { get; init; }

        [CommandArgument(1, "[path]")]
        [Description("The path to the property to view. If not specified, the entire package information will be shown.")]
        public required string? Path { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        (Core.Lip lip, ILogger logger, UserInteraction userInteraction) = await CommandRoot.Prepare(settings);

        string result = await lip.View(settings.Package, settings.Path, new());

        AnsiConsole.MarkupLine(result.EscapeMarkup());

        return 0;
    }
}