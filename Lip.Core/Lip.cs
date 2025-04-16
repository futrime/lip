using Flurl;

namespace Lip.Core;

public partial class Lip(
    RuntimeConfig runtimeConfig,
    IContext context,
    ICacheManager cacheManager,
    IDependencySolver dependencySolver,
    IPackageManager packageManager,
    IPathManager pathManager)
{
    private readonly ICacheManager _cacheManager = cacheManager;
    private readonly IContext _context = context;
    private readonly IDependencySolver _dependencySolver = dependencySolver;
    private readonly IPackageManager _packageManager = packageManager;
    private readonly IPathManager _pathManager = pathManager;
    private readonly RuntimeConfig _runtimeConfig = runtimeConfig;

    public static Lip Create(RuntimeConfig runtimeConfig, IContext context)
    {
        List<Url> gitHubProxies = runtimeConfig.GitHubProxies.ConvertAll(url => new Url(url));
        List<Url> goModuleProxies = runtimeConfig.GoModuleProxies.ConvertAll(url => new Url(url));

        PathManager pathManager = new(context.FileSystem, runtimeConfig.Cache, context.WorkingDir);
        CacheManager cacheManager = new(context, pathManager, gitHubProxies, goModuleProxies);
        PackageManager packageManager = new(context, cacheManager, pathManager, gitHubProxies, goModuleProxies);
        DependencySolver dependencySolver = new(context, packageManager);

        return new Lip(runtimeConfig, context, cacheManager, dependencySolver, packageManager, pathManager);
    }
}