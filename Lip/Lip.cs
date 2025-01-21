using System.IO.Abstractions;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace Lip;

/// <summary>
/// The main class of the Lip library.
/// </summary>
/// <param name="runtimeConfig">The runtime configuration.</param>
/// <param name="fileSystem">The file system wrapper.</param>
/// <param name="logger">The logger.</param>
/// <param name="userInteraction">The user interaction wrapper.</param>
public partial class Lip(
    RuntimeConfig runtimeConfig,
    IFileSystem fileSystem,
    ILogger logger,
    IUserInteraction userInteraction)
{
    private readonly IFileSystem _fileSystem = fileSystem;
    private readonly ILogger _logger = logger;
    private readonly IPathManager _pathManager = new PathManager(fileSystem, runtimeConfig.Cache);
    private readonly RuntimeConfig _runtimeConfig = runtimeConfig;
    private readonly IUserInteraction _userInteraction = userInteraction;
}
