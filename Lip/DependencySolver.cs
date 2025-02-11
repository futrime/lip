using System.Runtime.InteropServices;
using Algorithms.Graphs;
using DataStructures.Graphs;
using Flurl;
using Lip.Context;
using Microsoft.Extensions.Logging;
using Semver;

namespace Lip;

public class DependencySolver(
    IContext context,
    CacheManager cacheManager,
    PackageManager packageManager,
    List<Url> goModuleProxies)
{
    private readonly CacheManager _cacheManager = cacheManager;
    private readonly IContext _context = context;
    private readonly List<Url> _goModuleProxies = goModuleProxies;
    private readonly PackageManager _packageManager = packageManager;

    public async Task<List<PackageSpecifierWithoutVersion>> GetUnnecessaryPackages()
    {
        PackageLock packageLock = await _packageManager.GetCurrentPackageLock();

        List<ComparablePackageSpecifierWithoutVersion> necessaryPackages = [.. packageLock.Locks
            .Where(@lock => @lock.Locked)
            .Select(@lock => new ComparablePackageSpecifierWithoutVersion
            {
                ToothPath = @lock.Package.ToothPath,
                VariantLabel = @lock.VariantLabel
            })];

        DirectedSparseGraph<ComparablePackageSpecifierWithoutVersion> dependencyGraph = new();

        dependencyGraph.AddVertices([.. necessaryPackages.Cast<ComparablePackageSpecifierWithoutVersion>()]);

        // Add edges.
        foreach (ComparablePackageSpecifierWithoutVersion packageSpecifier in necessaryPackages)
        {
            PackageManifest packageManifest = (await _packageManager.GetInstalledPackageManifest(
                packageSpecifier.ToothPath,
                packageSpecifier.VariantLabel))!;

            IEnumerable<ComparablePackageSpecifierWithoutVersion> dependencies = packageManifest.GetSpecifiedVariant(
                packageSpecifier.VariantLabel,
                RuntimeInformation.RuntimeIdentifier)?
                .Dependencies?
                .Select(dep => PackageSpecifierWithoutVersion.Parse(dep.Key))
                .Cast<ComparablePackageSpecifierWithoutVersion>() ?? [];

            foreach (ComparablePackageSpecifierWithoutVersion dependency in dependencies)
            {
                dependencyGraph.AddEdge(packageSpecifier, dependency);
            }
        }

        // Find unnecessary packages.
        List<PackageSpecifierWithoutVersion> unnecessaryPackages = [.. ConnectedComponents.Compute(dependencyGraph)
            .Where(component => !component.Any(package => necessaryPackages.Contains(package)))
            .SelectMany(component => component)
            .Cast<PackageSpecifierWithoutVersion>()];

        return unnecessaryPackages;
    }

    private async Task<List<SemVersion>> GetRemoteVersions(PackageSpecifierWithoutVersion packageSpecifier)
    {
        // First, try to get remote versions from the Go module proxy.

        if (_goModuleProxies.Count != 0)
        {
            List<Url> goModuleVersionListUrls = _goModuleProxies.ConvertAll(proxy =>
                proxy.Clone()
                    .AppendPathSegments(
                        GoModule.EscapePath(packageSpecifier.ToothPath),
                        "@v",
                        "list")
            );

            foreach (Url url in _goModuleProxies)
            {
                Url goModuleVersionListUrl = url
                    .AppendPathSegments(
                        GoModule.EscapePath(packageSpecifier.ToothPath),
                        "@v",
                        "list");

                string tempFilePath = _context.FileSystem.Path.GetTempFileName();

                try
                {
                    await _context.Downloader.DownloadFile(goModuleVersionListUrl, tempFilePath);

                    string goModuleVersionListText = await _context.FileSystem.File.ReadAllTextAsync(tempFilePath);

                    return [.. goModuleVersionListText
                        .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Select(versionText => SemVersion.TryParse(versionText, out SemVersion? version) ? version : null)
                        .Where(version => version is not null)];
                }
                catch (Exception ex)
                {
                    _context.Logger.LogWarning(ex, "Failed to download {Url}. Attempting next URL.", url);
                }
            }

            _context.Logger.LogWarning(
                "Failed to download version list for {Package} from all Go module proxies.",
                packageSpecifier);
        }

        // Second, try to get remote versions from the Git repository.

        if (_context.Git is not null)
        {
            string repoUrl = Url.Parse($"https://{packageSpecifier.ToothPath}");
            return [.. (await _context.Git.ListRemote(repoUrl, refs: true, tags: true))
                .Where(item => item.Ref.StartsWith("refs/tags/v"))
                .Select(item => item.Ref)
                .Select(refName => refName.Substring("refs/tags/v".Length))
                .Select(version => SemVersion.Parse(version))];
        }

        // Otherwise, no remote source is available.

        throw new InvalidOperationException("No remote source is available.");
    }
}

file record ComparablePackageSpecifierWithoutVersion : PackageSpecifierWithoutVersion, IComparable<ComparablePackageSpecifierWithoutVersion>
{
    // C-Sharp-Algorithms requires this method to be implemented but we don't know why.
    public int CompareTo(ComparablePackageSpecifierWithoutVersion? other) => throw new NotImplementedException();
}
