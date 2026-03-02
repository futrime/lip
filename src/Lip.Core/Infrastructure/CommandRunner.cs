using CliWrap;
using System.Text;

namespace Lip.Core.Infrastructure;

public interface ICommandRunner
{
    Task Run(string command);
}

public class CommandRunner : ICommandRunner
{
    public async Task Run(string command)
    {
        StringBuilder stdout = new();
        StringBuilder stderr = new();

        CommandResult result = await Cli.Wrap(OperatingSystem.IsWindows() ? "cmd.exe" : "sh")
            .WithArguments([
                OperatingSystem.IsWindows() ? "/c" : "-c",
                command
            ])
            .WithValidation(CommandResultValidation.None)
            .WithStandardInputPipe(PipeSource.Null)
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdout))
            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stderr))
            .ExecuteAsync()
            .ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            string stderrText = stderr.ToString().Trim();
            string stdoutText = stdout.ToString().Trim();

            if (!string.IsNullOrEmpty(stderrText))
            {
                throw new InvalidOperationException(
                    $"Command failed with exit code {result.ExitCode}: {stderrText}");
            }

            if (!string.IsNullOrEmpty(stdoutText))
            {
                throw new InvalidOperationException(
                    $"Command failed with exit code {result.ExitCode}: {stdoutText}");
            }

            throw new InvalidOperationException($"Command failed with exit code {result.ExitCode}");
        }
    }
}
