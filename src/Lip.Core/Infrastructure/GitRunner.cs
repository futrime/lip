using CliWrap;
using System.Collections.Generic;
using System.Text;

namespace Lip.Core.Infrastructure;

public interface IGitRunner
{
    Task Clone(
        string repo,
        string? dir = null,
        string? branch = null,
        int? depth = null);

    Task<IEnumerable<(string Sha, string Ref)>> LsRemote(
        string repository,
        bool refs = false,
        bool tags = false);
}

public class GitRunner : IGitRunner
{
    private static IReadOnlyDictionary<string, string?> BuildNonInteractiveGitEnv()
    {
        Dictionary<string, string?> env = new()
        {
            ["GIT_TERMINAL_PROMPT"] = "0",
            ["GIT_ASKPASS"] = "",
            ["SSH_ASKPASS"] = "",
        };

        return env;
    }

    public async Task Clone(
        string repo,
        string? dir = null,
        string? branch = null,
        int? depth = null)
    {
        StringBuilder stderr = new();

        CommandResult result = await Cli.Wrap("git")
            .WithArguments(
            [
                "clone",
                .. (branch is not null) ? new[] { "--branch", branch } : [],
                .. (depth is not null) ? new[] { "--depth", depth.ToString()! } : [],
                "--",
                repo,
                .. (dir is not null) ? new[] { dir } : []
            ])
            .WithEnvironmentVariables(BuildNonInteractiveGitEnv())
            .WithValidation(CommandResultValidation.None)
            .WithStandardInputPipe(PipeSource.Null)
            .WithStandardOutputPipe(PipeTarget.Null)
            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stderr))
            .ExecuteAsync()
            .ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            string stderrText = stderr.ToString().Trim();
            throw new InvalidOperationException(
                string.IsNullOrEmpty(stderrText)
                    ? $"git clone failed with exit code {result.ExitCode}"
                    : $"git clone failed with exit code {result.ExitCode}: {stderrText}");
        }
    }

    public async Task<IEnumerable<(string Sha, string Ref)>> LsRemote(
        string repository,
        bool refs = false,
        bool tags = false)
    {
        StringBuilder stdout = new();
        StringBuilder stderr = new();

        CommandResult result = await Cli.Wrap("git")
            .WithArguments([
                "ls-remote",
                .. refs ? new List<string> { "--refs" } : [],
                .. tags ? new List<string> { "--tags" } : [],
                repository
            ])
            .WithEnvironmentVariables(BuildNonInteractiveGitEnv())
            .WithValidation(CommandResultValidation.None)
            .WithStandardInputPipe(PipeSource.Null)
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdout))
            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stderr))
            .ExecuteAsync()
            .ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            string stderrText = stderr.ToString().Trim();
            throw new InvalidOperationException(
                string.IsNullOrEmpty(stderrText)
                    ? $"git ls-remote failed with exit code {result.ExitCode}"
                    : $"git ls-remote failed with exit code {result.ExitCode}: {stderrText}");
        }

        string outputString = stdout.ToString();

        return outputString
            .Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(static line =>
            {
                string[] parts = line.Split('\t', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                return (Sha: parts[0], Ref: parts[1]);
            });
    }
}
