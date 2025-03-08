using Lip.Context;
using Lip.Core;
using Microsoft.Extensions.Logging;
using Semver;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.IO.Abstractions;
using System.Reflection;

namespace Lip.CLI;

[Description("lip is a general package manager.")]
class CommandRoot : AsyncCommand<CommandRoot.Settings>
{
    public class Settings : BaseCommandSettings
    {
        [CommandOption("-V|--version")]
        [Description("Show version and exit.")]
        public required bool Version { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        (Core.Lip lip, ILogger logger, UserInteraction userInteraction) = await Prepare(settings);

        if (settings.Version)
        {
            SemVersion version = SemVersion.FromVersion(Assembly.GetEntryAssembly()!.GetName().Version!);

            AnsiConsole.MarkupLine(version.ToString().EscapeMarkup());

            return 0;
        }

        logger.LogCritical("No command specified. Use 'lip --help' for more information.");

        return -1;
    }

    public static async Task<(Core.Lip lip, ILogger logger, UserInteraction userInteraction)> Prepare(
        BaseCommandSettings settings,
        bool doNotRunProgressService = false)
    {
        ILogger logger = CreateLogger(settings.Quiet, settings.Verbose);

        RuntimeConfig runtimeConfig = await GetRuntimeConfig();

        UserInteraction userInteraction = new();

        Core.Lip lip = Core.Lip.Create(
            runtimeConfig,
            new Context.Context
            {
                CommandRunner = new CommandRunner(),
                Downloader = new Context.Downloader(userInteraction),
                FileSystem = new FileSystem(),
                Git = await StandaloneGit.Create(),
                Logger = logger,
                UserInteraction = userInteraction,
                WorkingDir = Directory.GetCurrentDirectory()
            }
        );

        if (!doNotRunProgressService)
        {
            _ = userInteraction.RunProgressService(); // Run in the background.
        }

        return (lip, logger, userInteraction);
    }

    private static ILogger CreateLogger(bool quiet, bool verbose)
    {
        LogLevel logLevel = quiet
            ? LogLevel.Error
            : verbose
                ? LogLevel.Debug
                : LogLevel.Information;

        using ILoggerFactory factory = LoggerFactory.Create(builder =>
        {
            builder
                .AddSimpleConsole(options =>
                {
                    options.SingleLine = true;
                })
                .SetMinimumLevel(logLevel);
        });
        return factory.CreateLogger("lip");
    }

    private static async Task<RuntimeConfig> GetRuntimeConfig()
    {
        string path = Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "lip", "liprc.json");

        if (!Path.Exists(path))
        {
            return new RuntimeConfig();
        }

        byte[] json = await File.ReadAllBytesAsync(path);

        return RuntimeConfig.FromJsonBytes(json);
    }
}
