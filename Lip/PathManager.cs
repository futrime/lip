using System.IO.Abstractions;

namespace Lip;

public interface IPathManager
{
    string BaseAssetCacheDir { get; }
    string BaseCacheDir { get; }
    string BasePackageCacheDir { get; }
    string PackageManifestPath { get; }
    string PackageRecordPath { get; }
    string RuntimeConfigPath { get; }
    string WorkingDir { get; }

    string GetAssetCacheDir(string assetUrl);
    string GetPackageCacheDir(string packageName);
}

public class PathManager(IFileSystem fileSystem, string? baseCacheDir = null) : IPathManager
{
    private const string AssetCacheDirName = "assets";
    private const string PackageCacheDirName = "packages";
    private const string PackageManifestFileName = "tooth.json";
    private const string PackageRecordFileName = "tooth.lock";

    private readonly IFileSystem _fileSystem = fileSystem;
    private readonly string? _baseCacheDir = baseCacheDir;

    public string BaseAssetCacheDir => _fileSystem.Path.Join(BaseCacheDir, AssetCacheDirName);

    public string BaseCacheDir => _fileSystem.Path.GetFullPath(_baseCacheDir ?? throw new InvalidOperationException("Runtime configuration is not set."));

    public string BasePackageCacheDir => _fileSystem.Path.Join(BaseCacheDir, PackageCacheDirName);

    public string PackageManifestPath => _fileSystem.Path.Join(WorkingDir, PackageManifestFileName);

    public string PackageRecordPath => _fileSystem.Path.Join(WorkingDir, PackageRecordFileName);

    public string RuntimeConfigPath => _fileSystem.Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "lip", "runtime_config.json");

    public string WorkingDir => _fileSystem.Directory.GetCurrentDirectory();

    public string GetAssetCacheDir(string assetUrl)
    {
        string assetDirName = Uri.EscapeDataString(assetUrl);
        return _fileSystem.Path.Join(BaseAssetCacheDir, assetDirName);
    }

    public string GetPackageCacheDir(string packageName)
    {
        string packageDirName = Uri.EscapeDataString(packageName);
        return _fileSystem.Path.Join(BasePackageCacheDir, packageDirName);
    }
}
