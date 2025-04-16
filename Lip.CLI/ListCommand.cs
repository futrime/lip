using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace Lip.CLI;

[Description("List installed packages.")]
class ListCommand : AsyncCommand<ListCommand.Settings>
{
    public class Settings : BaseCommandSettings
    {
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        (Core.Lip lip, ILogger logger, UserInteraction userInteraction) = await CommandRoot.Prepare(settings);

        List<Core.Lip.ListResultItem> result = await lip.List(new());

        Table table = new()
        {
            Title = new TableTitle("Installed Packages")
        };

        table.AddColumns("Package", "Variant", "Version", "Locked");

        foreach (Core.Lip.ListResultItem item in result)
        {
            table.AddRow(
                item.Specifier.ToothPath.EscapeMarkup(),
                item.Specifier.VariantLabel.EscapeMarkup(),
                item.Specifier.Version.ToString().EscapeMarkup(),
                item.Locked.ToString().EscapeMarkup()
            );
        }

        AnsiConsole.Write(table);

        return 0;
    }
}