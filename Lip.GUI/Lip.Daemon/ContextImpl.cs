using System.IO.Abstractions;
using Lip.Context;
using Microsoft.Extensions.Logging;

namespace Lip.Daemon;
internal class ContextImpl : IContext
{
    public ICommandRunner CommandRunner => throw new NotImplementedException();

    public IDownloader Downloader => throw new NotImplementedException();

    public IFileSystem FileSystem => throw new NotImplementedException();

    public IGit? Git => throw new NotImplementedException();

    public ILogger Logger => throw new NotImplementedException();

    public IUserInteraction UserInteraction => throw new NotImplementedException();

    public string? WorkingDir => throw new NotImplementedException();
}
