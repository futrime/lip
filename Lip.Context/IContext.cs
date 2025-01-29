using System.IO.Abstractions;
using Microsoft.Extensions.Logging;

namespace Lip.Context;

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
