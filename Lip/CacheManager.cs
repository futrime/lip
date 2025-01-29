using System.IO.Abstractions;
using Flurl;
using Lip.Context;
using Semver;
using SharpCompress.Archives;

namespace Lip;

public class CacheManager(
    IContext context,
    PathManager pathManager,
    Url? githubProxy = null,
    Url? goModuleProxy = null)
{
    private readonly IContext _context = context;
    private readonly Url? _githubProxy = githubProxy;
    private readonly Url? _goModuleProxy = goModuleProxy;
    private readonly PathManager _pathManager = pathManager;

    public async Task<Stream> GetDownloadedFile(Url url)
    {
        if (url.Host == "github.com" && _githubProxy is not null)
        {
            url = _githubProxy.AppendPathSegment(url.Path).SetQueryParams(url.QueryParams);
        }

        string filePath = _pathManager.GetDownloadedFileCachePath(url);

        if (_context.FileSystem.Directory.Exists(filePath))
        {
            throw new InvalidOperationException($"Attempt to get downloaded file at '{filePath}' where is a directory.");
        }

        if (!_context.FileSystem.File.Exists(filePath))
        {
            _context.FileSystem.CreateParentDirectory(filePath);

            await _context.Downloader.DownloadFile(url, filePath);
        }

        return _context.FileSystem.File.OpenRead(filePath);
    }

    public async Task<IDirectoryInfo> GetGitRepoDir(PackageSpecifier packageSpecifier)
    {
        if (_context.Git is null)
        {
            throw new InvalidOperationException("Git client is not available.");
        }

        string repoUrl = Url.Parse($"https://{packageSpecifier.ToothPath}");
        string tag = $"v{packageSpecifier.Version}";

        string repoDirPath = _pathManager.GetGitRepoDirCachePath(repoUrl, tag);

        if (_context.FileSystem.File.Exists(repoDirPath))
        {
            throw new InvalidOperationException($"Attempt to get Git repo directory at '{repoDirPath}' where is a file.");
        }

        if (!_context.FileSystem.Directory.Exists(repoDirPath))
        {
            _context.FileSystem.CreateParentDirectory(repoDirPath);

            await _context.Git.Clone(
                repoUrl,
                repoDirPath,
                branch: tag,
                depth: 1);
        }

        return _context.FileSystem.DirectoryInfo.New(repoDirPath);
    }

    public async Task<Stream> GetPackageManifestFile(PackageSpecifier packageSpecifier)
    {
        string filePath = _pathManager.GetPackageManifestCachePath(packageSpecifier.SpecifierWithoutVariant);

        if (_context.FileSystem.Directory.Exists(filePath))
        {
            throw new InvalidOperationException($"Attempt to get package manifest file at '{filePath}' where is a directory.");
        }

        // If already cached, return the file stream.
        if (_context.FileSystem.File.Exists(filePath))
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
            throw new InvalidOperationException("No remote source is available.");
        }
    }

    private async Task<Stream> GetPackageManifestFileFromGitRepo(PackageSpecifier packageSpecifier)
    {
        // Before calling this method, we assume that:
        // 1. The git client is not null.
        // 2. The package manifest cache path does not exist.

        IDirectoryInfo repoDir = await GetGitRepoDir(packageSpecifier);
        string srcPath = _pathManager.GetPackageManifestPath(repoDir.FullName);

        if (!_context.FileSystem.File.Exists(srcPath))
        {
            throw new InvalidOperationException($"Package manifest file not found for package '{packageSpecifier}' at '{srcPath}'.");
        }

        string destPath = _pathManager.GetPackageManifestCachePath(packageSpecifier.SpecifierWithoutVariant);

        _context.FileSystem.CreateParentDirectory(destPath);

        await Task.Run(() => _context.FileSystem.File.Copy(srcPath, destPath));

        return _context.FileSystem.File.OpenRead(destPath);
    }

    private async Task<Stream> GetPackageManifestFileFromGoModuleProxy(PackageSpecifier packageSpecifier)
    {
        // Before calling this method, we assume that:
        // 1. The Go module proxy is not null.
        // 2. The package manifest cache path does not exist.

        SemVersion version = packageSpecifier.Version;

        // Build the archive download URL.
        string archiveFileNameInUrl = GetGoModuleFileNameFromVersion(version);
        string escapedGoModulePath = GoModule.EscapePath(packageSpecifier.ToothPath);
        Url archiveFileUrl = _goModuleProxy!.Clone().AppendPathSegments(escapedGoModulePath, "@v", archiveFileNameInUrl);

        // Download and open the archive.
        using Stream archiveStream = await GetDownloadedFile(archiveFileUrl);

        IArchive archive = ArchiveFactory.Open(archiveStream);

        string archivePackageManifestPath = $"{packageSpecifier.ToothPath}@v{version}{(version.Major >= 2 ? "+incompatible" : "")}/{_pathManager.PackageManifestFileName}";

        IArchiveEntry? entry = archive.Entries.FirstOrDefault(
            entry => entry.Key == archivePackageManifestPath
        ) ?? throw new InvalidOperationException($"Package manifest file not found for package '{packageSpecifier}' at {archivePackageManifestPath}.");

        using Stream manifestStream = entry.OpenEntryStream();

        // Save the package manifest file to the cache.
        string manifestFilePath = _pathManager.GetPackageManifestCachePath(packageSpecifier.SpecifierWithoutVariant);

        _context.FileSystem.CreateParentDirectory(manifestFilePath);

        using (Stream fileStream = _context.FileSystem.File.Create(manifestFilePath))
        {
            await manifestStream.CopyToAsync(fileStream);
        }

        return _context.FileSystem.File.OpenRead(manifestFilePath);
    }

    private static string GetGoModuleFileNameFromVersion(SemVersion version)
    {
        // Reference: https://go.dev/ref/mod#glos-canonical-version
        if (version.Metadata.Length > 0)
        {
            throw new ArgumentException("Go module proxy does not accept version with build metadata.", nameof(version));
        }

        // Reference: https://go.dev/ref/mod#non-module-compat
        return $"v{version}{(version.Major >= 2 ? "+incompatible" : "")}.zip";
    }
}
