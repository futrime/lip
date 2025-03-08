using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace Lip.CLI;

class CacheSettings : BaseCommandSettings
{
}

[Description("Add a package to the cache.")]
class CacheAddCommand : AsyncCommand<CacheAddCommand.Settings>
{
    public class Settings : CacheSettings
    {
        [CommandArgument(0, "<package>")]
        [Description("The package to add.")]
        public required string Package { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        (Core.Lip lip, ILogger logger, UserInteraction userInteraction) = await CommandRoot.Prepare(settings);

        await lip.CacheAdd(settings.Package, new());

        return 0;
    }
}

[Description("Remove all items from the cache.")]
class CacheCleanCommand : AsyncCommand<CacheCleanCommand.Settings>
{
    public class Settings : CacheSettings
    {
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        (Core.Lip lip, ILogger logger, UserInteraction userInteraction) = await CommandRoot.Prepare(settings);

        await lip.CacheClean(new());

        return 0;
    }
}

[Description("List items in the cache.")]
class CacheListCommand : AsyncCommand<CacheListCommand.Settings>
{
    public class Settings : CacheSettings
    {
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        (Core.Lip lip, ILogger logger, UserInteraction userInteraction) = await CommandRoot.Prepare(settings);

        Core.Lip.CacheListResult result = await lip.CacheList(new());

        Tree root = new("Cache");

        root
            .AddNode("Downloaded Files")
            .AddNode(new Rows(result.DownloadedFiles.Select(file => new Text(file))));

        root
            .AddNode("Git Repos")
            .AddNode(new Rows(result.GitRepos.Select(repo => new Text(repo))));

        AnsiConsole.Write(root);

        return 0;
    }
}
