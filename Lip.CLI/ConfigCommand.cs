using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace Lip.CLI;

class ConfigSettings : BaseCommandSettings
{
}

[Description("Delete a configuration value.")]
class ConfigDeleteCommand : AsyncCommand<ConfigDeleteCommand.Settings>
{
    public class Settings : ConfigSettings
    {
        [CommandArgument(0, "<key ...>")]
        [Description("The keys to delete.")]
        public required string[] Keys { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        (Core.Lip lip, ILogger logger, UserInteraction userInteraction) = await CommandRoot.Prepare(settings);

        await lip.ConfigDelete([.. settings.Keys], new());

        return 0;
    }
}

[Description("Get a configuration value.")]
class ConfigGetCommand : AsyncCommand<ConfigGetCommand.Settings>
{
    public class Settings : ConfigSettings
    {
        [CommandArgument(0, "<key ...>")]
        [Description("The keys to get.")]
        public required string[] Keys { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        (Core.Lip lip, ILogger logger, UserInteraction userInteraction) = await CommandRoot.Prepare(settings);

        Dictionary<string, string> value = lip.ConfigGet([.. settings.Keys], new());

        Table table = new()
        {
            Title = new TableTitle("Configuration")
        };

        table.AddColumns("Key", "Value");

        foreach (KeyValuePair<string, string> entry in value)
        {
            table.AddRow(entry.Key.EscapeMarkup(), entry.Value.EscapeMarkup());
        }

        AnsiConsole.Write(table);

        return 0;
    }
}

[Description("List all configuration values.")]
class ConfigListCommand : AsyncCommand<ConfigListCommand.Settings>
{
    public class Settings : ConfigSettings { }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        (Core.Lip lip, ILogger logger, UserInteraction userInteraction) = await CommandRoot.Prepare(settings);

        Dictionary<string, string> value = lip.ConfigList(new());

        Table table = new()
        {
            Title = new TableTitle("Configuration")
        };

        table.AddColumns("Key", "Value");

        foreach (KeyValuePair<string, string> entry in value)
        {
            table.AddRow(entry.Key.EscapeMarkup(), entry.Value.EscapeMarkup());
        }

        AnsiConsole.Write(table);

        return 0;
    }
}

[Description("Set a configuration value.")]
class ConfigSetCommand : AsyncCommand<ConfigSetCommand.Settings>
{
    public class Settings : ConfigSettings
    {
        [CommandArgument(0, "<key=value ...>")]
        [Description("The key-value pairs to set.")]
        public required string[] Entries { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        (Core.Lip lip, ILogger logger, UserInteraction userInteraction) = await CommandRoot.Prepare(settings);

        Dictionary<string, string> entries = settings.Entries
            .Select(entry => entry.Split('=', 2))
            .ToDictionary(entry => entry[0], entry => entry[1]);

        await lip.ConfigSet(entries, new());

        return 0;
    }
}