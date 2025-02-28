using System.Runtime.InteropServices;

namespace Lip;

public partial class Lip
{
    public record CacheAddArgs { }
    public record CacheCleanArgs { }
    public record CacheListArgs { }
    public record CacheListResult
    {
        public required List<string> DownloadedFiles { get; init; }
        public required List<string> GitRepos { get; init; }
    }

    public async Task CacheAdd(string packageSpecifierText, CacheAddArgs _)
    {
        var packageSpecifier = PackageSpecifier.Parse(packageSpecifierText);

        PackageManifest packageManifest = await _packageManager.GetPackageManifestFromCache(packageSpecifier)
            ?? throw new InvalidOperationException($"Cannot get package manifest from package '{packageSpecifier}'.");

        if (packageManifest.ToothPath != packageSpecifier.ToothPath)
        {
            throw new InvalidOperationException($"Tooth path in package manifest '{packageManifest.ToothPath}' does not match package specifier '{packageSpecifier.ToothPath}'.");
        }

        if (packageManifest.Version != packageSpecifier.Version)
        {
            throw new InvalidOperationException($"Version in package manifest '{packageManifest.Version}' does not match package specifier '{packageSpecifier.Version}'.");
        }

        PackageManifest.Variant? variant = packageManifest.GetVariant(
            packageSpecifier.VariantLabel, RuntimeInformation.RuntimeIdentifier);

        foreach (PackageManifest.Asset asset in variant?.Assets ?? [])
        {
            if (asset.Type != PackageManifest.Asset.TypeEnum.Self)
            {
                foreach (string url in asset.Urls ?? [])
                {
                    await _cacheManager.GetFileFromUrl(url);
                }
            }
        }
    }

    public async Task CacheClean(CacheCleanArgs _)
    {
        await _cacheManager.Clean();
    }

    public async Task<CacheListResult> CacheList(CacheListArgs _)
    {
        ICacheManager.ICacheSummary cacheSummary = await _cacheManager.List();
        return new CacheListResult
        {
            DownloadedFiles = [.. cacheSummary.DownloadedFiles.Keys],
            GitRepos = [.. cacheSummary.GitRepos.Keys.Select(repo => $"{repo.Url} {repo.Tag}")],
        };
    }
}
