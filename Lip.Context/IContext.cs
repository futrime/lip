using System.IO.Abstractions;
using Microsoft.Extensions.Logging;

namespace Lip.Context;

public interface IContext
{
    IDownloader Downloader { get; }
    IFileSystem FileSystem { get; }
    IGit? Git { get; }
    ILogger Logger { get; }
    IUserInteraction UserInteraction { get; }
}
