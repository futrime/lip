using System.IO.Abstractions;
using Flurl;

namespace Lip;

public class PathManager(IFileSystem fileSystem, string? baseCacheDir = null)
{
    private const string DownloadedFileCacheDirName = "downloaded_files";
    private const string GitRepoCacheDirName = "git_repos";
    private const string PackageLockFileName = "tooth_lock.json";
    private const string PackageManifestCacheDirName = "package_manifests";

    private readonly IFileSystem _fileSystem = fileSystem;
    private readonly string? _baseCacheDir = baseCacheDir;

    public string BaseCacheDir => _fileSystem.Path.GetFullPath(_baseCacheDir ?? throw new InvalidOperationException("Base cache directory is not provided."));

    public string BaseDownloadedFileCacheDir => _fileSystem.Path.Join(BaseCacheDir, DownloadedFileCacheDirName);

    public string BaseGitRepoCacheDir => _fileSystem.Path.Join(BaseCacheDir, GitRepoCacheDirName);

    public string BasePackageManifestCacheDir => _fileSystem.Path.Join(BaseCacheDir, PackageManifestCacheDirName);

    public string CurrentPackageManifestPath => _fileSystem.Path.Join(WorkingDir, PackageManifestFileName);

    public string CurrentPackageLockPath => _fileSystem.Path.Join(WorkingDir, PackageLockFileName);

    public string PackageManifestFileName => "tooth.json";

    public string RuntimeConfigPath => _fileSystem.Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "lip", "liprc.json");

    public string WorkingDir => _fileSystem.Directory.GetCurrentDirectory();

    public string GetDownloadedFileCachePath(Url url)
    {
        string downloadedFileName = Url.Encode(url);
        return _fileSystem.Path.Join(BaseDownloadedFileCacheDir, downloadedFileName);
    }

    public string GetGitRepoDirCachePath(string repoUrl, string tag)
    {
        string repoDirName = Url.Encode(repoUrl);
        string tagDirName = Url.Encode(tag);
        return _fileSystem.Path.Join(BaseGitRepoCacheDir, repoDirName, tagDirName);
    }

    public string GetPackageManifestCachePath(string packageName)
    {
        string escapedPackageName = Url.Encode(packageName);
        string packageManifestFileName = $"{escapedPackageName}.json";
        return _fileSystem.Path.Join(BasePackageManifestCacheDir, packageManifestFileName);
    }

    public string GetPackageManifestPath(string baseDir)
    {
        return _fileSystem.Path.Join(baseDir, PackageManifestFileName);
    }
}
