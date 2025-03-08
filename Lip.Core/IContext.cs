using Flurl;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;

namespace Lip.Core;

public interface ICommandRunner
{
    Task Run(string command, string workingDirectory);
}

public interface IContext
{
    ICommandRunner CommandRunner { get; }
    IDownloader Downloader { get; }
    IFileSystem FileSystem { get; }
    IGit? Git { get; }
    ILogger Logger { get; }
    IUserInteraction UserInteraction { get; }
    string? WorkingDir { get; }
}

public interface IDownloader
{
    Task DownloadFile(Url url, string destinationPath);
}

public interface IGit
{
    interface IListRemoteResultItem
    {
        public string Sha { get; }
        public string Ref { get; }
    }

    Task Clone(string repository, string directory, string? branch = null, int? depth = null);

    Task<List<IListRemoteResultItem>> ListRemote(string repository, bool refs = false, bool tags = false);
}

/// <summary>
/// Represents a user interaction interface.
/// </summary>
public interface IUserInteraction
{
    /// <summary>
    /// Displays a confirmation dialog and returns user's choice.
    /// </summary>
    /// <param name="format">The message to display</param>
    /// <returns>True if confirmed, false otherwise</returns>
    Task<bool> Confirm(string format, params object[] args);

    /// <summary>
    /// Prompts user for text input.
    /// </summary>
    /// <param name="defaultValue">Default value</param>
    /// <param name="format">The prompt message</param>
    /// <returns>User input as string</returns>
    Task<string> PromptForInput(string defaultValue, string format, params object[] args);

    /// <summary>
    /// Prompts user to select from multiple options.
    /// </summary>
    /// <param name="options">Available options</param>
    /// <param name="format">The prompt message</param>
    /// <returns>Selected option</returns>
    Task<string> PromptForSelection(IEnumerable<string> options, string format, params object[] args);

    /// <summary>
    /// Shows progress for long-running operations.
    /// </summary>
    /// <param name="id">Progress ID</param>
    /// <param name="progress">Progress value (0.0-1.0)</param>
    /// <param name="format">Progress message</param>
    Task UpdateProgress(string id, float progress, string format, params object[] args);
}
