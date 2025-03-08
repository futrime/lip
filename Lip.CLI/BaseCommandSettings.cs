using Spectre.Console.Cli;
using System.ComponentModel;

namespace Lip.CLI;

public class BaseCommandSettings : CommandSettings
{
    [CommandOption("-q|--quiet")]
    [Description("Show only errors.")]
    public required bool Quiet { get; init; }

    [CommandOption("-v|--verbose")]
    [Description("Show verbose output.")]
    public required bool Verbose { get; init; }
}
