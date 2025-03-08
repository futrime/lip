using CliWrap;
using Lip.Core;

namespace Lip.Context;

public class CommandRunner : ICommandRunner
{
    public async Task Run(string command, string workingDirectory)
    {
        using Stream stdInStream = Console.OpenStandardInput();
        using Stream stdOutStream = Console.OpenStandardOutput();
        using Stream stdErrStream = Console.OpenStandardError();

        CommandResult result = await Cli.Wrap(OperatingSystem.IsWindows() ? "cmd.exe" : "sh")
            .WithArguments(
            [
                OperatingSystem.IsWindows() ? "/c" : "-c",
                command
            ])
            .WithWorkingDirectory(workingDirectory)
            .WithStandardInputPipe(PipeSource.FromStream(stdInStream))
            .WithStandardOutputPipe(PipeTarget.ToStream(stdOutStream))
            .WithStandardErrorPipe(PipeTarget.ToStream(stdErrStream))
            .ExecuteAsync();

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException($"Command failed with exit code {result.ExitCode}");
        }
    }
}
