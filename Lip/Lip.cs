using Flurl;
using Lip.Context;

namespace Lip;

/// <summary>
/// The main class of the Lip library.
/// </summary>
public partial class Lip
{
    private readonly CacheManager _cacheManager;
    private readonly IContext _context;
    private readonly PathManager _pathManager;
    private readonly RuntimeConfig _runtimeConfig;

    public Lip(RuntimeConfig runtimeConfig, IContext context)
    {
        _context = context;
        _runtimeConfig = runtimeConfig;

        _pathManager = new(context.FileSystem, baseCacheDir: runtimeConfig.Cache, workingDir: context.WorkingDir);

        Url? githubProxyUrl = runtimeConfig.GitHubProxy.Length > 0 ? Url.Parse(runtimeConfig.GitHubProxy) : null;
        Url? goModuleProxyUrl = runtimeConfig.GoModuleProxy.Length > 0 ? Url.Parse(runtimeConfig.GoModuleProxy) : null;
        _cacheManager = new(context, _pathManager, githubProxyUrl, goModuleProxyUrl);
    }
}
