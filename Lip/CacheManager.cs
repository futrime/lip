using System.IO.Abstractions;
using Flurl;
using Lip.Context;
using Semver;

namespace Lip;

public class CacheManager(IContext context, PathManager pathManager, Url? githubProxy, Url? goModuleProxy)
{
    private readonly IContext _context = context;
    private readonly Url? _githubProxy = githubProxy;
    private readonly Url? _goModuleProxy = goModuleProxy;
    private readonly PathManager _pathManager = pathManager;

    public async Task<Stream> GetDownloadedFile(Url url)
    {
        if (url.Host == "github.com" && _githubProxy is not null)
        {
            url = Url.Parse($"{_githubProxy}/{url.Path}?{url.Query}");
        }

        string filePath = _pathManager.GetDownloadedFileCachePath(url);

        if (!_context.FileSystem.Path.Exists(filePath))
        {
            await _context.Downloader.DownloadFile(url, filePath);
        }

        return _context.FileSystem.File.OpenRead(filePath);
    }

    public async Task<IDirectoryInfo> GetGitRepoDir(string repoUrl, string tag)
    {
        string repoDirPath = _pathManager.GetGitRepoDirCachePath(repoUrl, tag);

        if (_context.FileSystem.File.Exists(repoDirPath))
        {
            throw new InvalidOperationException($"Attempt to get Git repo directory at '{repoDirPath}' where is a file.");
        }

        if (!_context.FileSystem.Directory.Exists(repoDirPath))
        {
            _pathManager.CreateParentDirectory(repoDirPath);

            // Since we have already checked that the git client is not null, we can safely use the '!' operator.
            await _context.Git!.Clone(repoUrl, repoDirPath);
        }

        return _context.FileSystem.DirectoryInfo.New(repoDirPath);
    }

    public async Task<Stream> GetPackageManifestFile(PackageSpecifier packageSpecifier)
    {
        string filePath = _pathManager.GetPackageManifestCachePath(packageSpecifier.Specifier);

        // If already cached, return the file stream.
        if (_context.FileSystem.Path.Exists(filePath))
        {
            return _context.FileSystem.File.OpenRead(filePath);
        }

        // Otherwise, fetch the file from the source.
        if (_context.Git is not null)
        {
            return await GetPackageManifestFileFromGitRepo(packageSpecifier);
        }
        else if (_goModuleProxy is not null)
        {
            return await GetPackageManifestFileFromGoModuleProxy(packageSpecifier);
        }
        else
        {
            throw new InvalidOperationException("Neither git client nor Go module proxy is available.");
        }
    }

    private async Task<Stream> GetPackageManifestFileFromGitRepo(PackageSpecifier packageSpecifier)
    {
        // Before calling this method, we assume that:
        // 1. The git client is not null.
        // 2. The package manifest cache path does not exist.

        string repoUrl = GetRepoUrlFromToothPath(packageSpecifier.ToothPath);
        string tag = GetTagFromVersion(packageSpecifier.Version);

        IDirectoryInfo repoDir = await GetGitRepoDir(repoUrl, tag);
        string srcPath = _pathManager.GetPackageManifestPath(repoDir.FullName);

        if (!_context.FileSystem.Path.Exists(srcPath))
        {
            throw new InvalidOperationException($"Package manifest file not found in the git repo '{repoUrl}' at '{srcPath}'.");
        }

        string destPath = _pathManager.GetPackageManifestCachePath(packageSpecifier.Specifier);
        _context.FileSystem.File.Copy(srcPath, destPath);

        return _context.FileSystem.File.OpenRead(destPath);
    }

    private async Task<Stream> GetPackageManifestFileFromGoModuleProxy(PackageSpecifier packageSpecifier)
    {
        // Before calling this method, we assume that:
        // 1. The Go module proxy is not null.
        // 2. The package manifest cache path does not exist.

        string goModulePath = packageSpecifier.ToothPath;
        SemVersion version = packageSpecifier.Version;

        Url fileUrl = GetGoModuleFileUrlFromModulePathAndVersion(goModulePath, version);
        string filePath = _pathManager.GetPackageManifestCachePath(packageSpecifier.Specifier);

        await _context.Downloader.DownloadFile(fileUrl, filePath);

        return _context.FileSystem.File.OpenRead(filePath);
    }

    private static string GetGoModuleFileNameFromVersion(SemVersion version)
    {
        // Reference: https://go.dev/ref/mod#glos-canonical-version
        if (version.Metadata.Length > 0)
        {
            throw new ArgumentException("Go module proxy does not accept version with build metadata.");
        }

        // Reference: https://go.dev/ref/mod#non-module-compat
        return $"v{version}{(version.Major >= 2 ? "+incompatible" : "")}.zip";
    }

    private Url GetGoModuleFileUrlFromModulePathAndVersion(string goModulePath, SemVersion version)
    {
        string fileName = GetGoModuleFileNameFromVersion(version);
        string escapedPath = GoModule.EscapePath(goModulePath);

        // We have already checked that the go module proxy is not null, so we can safely use the '!' operator.
        return Url.Parse($"{_goModuleProxy!}/{escapedPath}/@v/{fileName}");
    }

    private static string GetRepoUrlFromToothPath(string toothPath)
    {
        return Url.Parse($"https://{toothPath}");
    }

    private static string GetTagFromVersion(SemVersion version)
    {
        return $"v{version}";
    }
}
