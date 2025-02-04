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
    private readonly PackageManager _packageManager;
    private readonly PathManager _pathManager;
    private readonly RuntimeConfig _runtimeConfig;

    public Lip(RuntimeConfig runtimeConfig, IContext context)
    {
        _context = context;
        _runtimeConfig = runtimeConfig;

        _pathManager = new(context.FileSystem, baseCacheDir: runtimeConfig.Cache, workingDir: context.WorkingDir);

        List<Url> gitHubProxies = [.. runtimeConfig.GitHubProxies.Select(url => new Url(url))];
        List<Url> goModuleProxies = [.. runtimeConfig.GoModuleProxies.Select(url => new Url(url))];

        _cacheManager = new(_context, _pathManager, gitHubProxies, goModuleProxies);

        _packageManager = new(_context, _cacheManager, _pathManager);
    }
}
