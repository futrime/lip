using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace Lip.CLI;

[Description("Create a archive from the current directory, containing all files to place specified in the `tooth.json` file.")]
class PackCommand : AsyncCommand<PackCommand.Settings>
{
    public class Settings : BaseCommandSettings
    {
        [CommandArgument(0, "<path>")]
        [Description("The path to the archive to create.")]
        public required string OutputPath { get; init; }

        [CommandOption("--dry-run")]
        [Description("Do not actually create the archive.")]
        public required bool DryRun { get; init; }

        [CommandOption("--ignore-scripts")]
        [Description("Do not run any scripts during packaging.")]
        public required bool IgnoreScripts { get; init; }

        [CommandOption("--archive-format <format>")]
        [DefaultValue("zip")]
        [Description("The format of the archive to create. Valid formats are `zip`, `tar`, `tgz` and `tar.gz`. Defaults to `zip`.")]
        public required string ArchiveFormat { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        (Core.Lip lip, ILogger logger, UserInteraction userInteraction) = await CommandRoot.Prepare(settings);

        await lip.Pack(settings.OutputPath, new()
        {
            DryRun = settings.DryRun,
            IgnoreScripts = settings.IgnoreScripts,
            ArchiveFormat = settings.ArchiveFormat switch
            {
                "zip" => Core.Lip.PackArgs.ArchiveFormatType.Zip,
                "tar" => Core.Lip.PackArgs.ArchiveFormatType.Tar,
                "tgz" or "tar.gz" => Core.Lip.PackArgs.ArchiveFormatType.TarGz,
                _ => throw new ArgumentException($"Invalid archive format: {settings.ArchiveFormat}")
            }
        });

        return 0;
    }
}