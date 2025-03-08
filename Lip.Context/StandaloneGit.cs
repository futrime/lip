using CliWrap;
using Lip.Core;
using System.Text;

namespace Lip.Context;

public class StandaloneGit : IGit
{
    private StandaloneGit() { }

    public static async Task<StandaloneGit?> Create()
    {
        try
        {
            return await CreateInternal();
        }
        catch
        {
            return null;
        }
    }

    public async Task Clone(string repository, string directory, string? branch = null, int? depth = null)
    {
        using Stream stdInStream = Console.OpenStandardInput();
        using Stream stdOutStream = Console.OpenStandardOutput();
        using Stream stdErrStream = Console.OpenStandardError();

        await Cli.Wrap("git")
            .WithArguments([
                "clone",
                .. (branch is not null) ? new[] { "--branch", branch } : [],
                .. (depth is not null) ? new[] { "--depth", depth.ToString()! } : [],
                repository,
                directory
            ])
            .WithStandardInputPipe(PipeSource.FromStream(stdInStream))
            .WithStandardOutputPipe(PipeTarget.ToStream(stdOutStream))
            .WithStandardErrorPipe(PipeTarget.ToStream(stdErrStream))
            .ExecuteAsync();
    }

    public async Task<List<IGit.IListRemoteResultItem>> ListRemote(string repository, bool refs = false, bool tags = false)
    {
        using Stream stdInStream = Console.OpenStandardInput();
        using MemoryStream outStream = new();
        using Stream stdErrStream = Console.OpenStandardError();

        await Cli.Wrap("git")
            .WithArguments([
                "ls-remote",
                .. refs ? new List<string> { "--refs" } : [],
                .. tags ? new List<string> { "--tags" } : [],
                repository
            ])
            .WithStandardInputPipe(PipeSource.FromStream(stdInStream))
            .WithStandardOutputPipe(PipeTarget.ToStream(outStream))
            .WithStandardErrorPipe(PipeTarget.ToStream(stdErrStream))
            .ExecuteAsync();

        byte[] output = outStream.ToArray();

        string outputString = Encoding.UTF8.GetString(output);

        return [.. outputString
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line =>
            {
                string[] parts = line.Split('\t');
                return new ListRemoteResultItem(Sha: parts[0], Ref: parts[1]);
            })];
    }

    private static async Task<StandaloneGit?> CreateInternal()
    {
        var result = await Cli.Wrap("git")
            .WithArguments("--version")
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync();

        return result.ExitCode == 0 ? new StandaloneGit() : null;
    }
}

file record ListRemoteResultItem(string Sha, string Ref) : IGit.IListRemoteResultItem;
