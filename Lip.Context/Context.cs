using Lip.Core;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace Lip.Context;

public class Context : IContext
{
    public required ICommandRunner CommandRunner { get; init; }
    public required IDownloader Downloader { get; init; }
    public required IFileSystem FileSystem { get; init; }
    public required IGit? Git { get; init; }
    public required ILogger Logger { get; init; }
    public required IUserInteraction UserInteraction { get; init; }
    public required string? WorkingDir { get; init; }
}