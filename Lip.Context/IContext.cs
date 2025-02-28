using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

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
