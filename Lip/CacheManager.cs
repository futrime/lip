using System.IO.Abstractions;
using Flurl;
using Lip.Context;
using Semver;

namespace Lip;

public class CacheManager(
    IContext context,
    PathManager pathManager,
    Url? githubProxy = null,
    Url? goModuleProxy = null)
{
    public record ListResult
    {
        public required Dictionary<Url, IFileInfo> DownloadedFiles { get; init; }
        public required Dictionary<PathManager.GitRepoInfo, IDirectoryInfo> GitRepos { get; init; }
        public required Dictionary<PackageSpecifier, IFileInfo> PackageManifestFiles { get; init; }
    }

    private readonly IContext _context = context;
    private readonly Url? _githubProxy = githubProxy;
    private readonly Url? _goModuleProxy = goModuleProxy;
    private readonly PathManager _pathManager = pathManager;

    public async Task Clean()
    {
        if (await _context.FileSystem.Directory.ExistsAsync(_pathManager.BaseCacheDir))
        {
            await _context.FileSystem.Directory.DeleteAsync(_pathManager.BaseCacheDir, recursive: true);
        }
    }

    public async Task<Stream> GetDownloadedFile(Url url)
    {
        if (url.Host == "github.com" && _githubProxy is not null)
        {
            url = _githubProxy.AppendPathSegment(url.Path).SetQueryParams(url.QueryParams);
        }

        string filePath = _pathManager.GetDownloadedFileCachePath(url);

        if (await _context.FileSystem.Directory.ExistsAsync(filePath))
        {
            throw new InvalidOperationException($"Attempt to get downloaded file at '{filePath}' where is a directory.");
        }

        if (!await _context.FileSystem.File.ExistsAsync(filePath))
        {
            await _context.FileSystem.CreateParentDirectoryAsync(filePath);

            await _context.Downloader.DownloadFile(url, filePath);
        }

        return await _context.FileSystem.File.OpenReadAsync(filePath);
    }

    public async Task<IDirectoryInfo> GetGitRepoDir(PackageSpecifier packageSpecifier)
    {
        if (_context.Git is null)
        {
            throw new InvalidOperationException("Git client is not available.");
        }

        string repoUrl = Url.Parse($"https://{packageSpecifier.ToothPath}");
        string tag = $"v{packageSpecifier.Version}";

        string repoDirPath = _pathManager.GetGitRepoDirCachePath(new()
        {
            Url = repoUrl,
            Tag = tag
        });

        if (await _context.FileSystem.File.ExistsAsync(repoDirPath))
        {
            throw new InvalidOperationException($"Attempt to get Git repo directory at '{repoDirPath}' where is a file.");
        }

        if (!await _context.FileSystem.Directory.ExistsAsync(repoDirPath))
        {
            await _context.FileSystem.CreateParentDirectoryAsync(repoDirPath);

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

        if (await _context.FileSystem.Directory.ExistsAsync(filePath))
        {
            throw new InvalidOperationException($"Attempt to get package manifest file at '{filePath}' where is a directory.");
        }

        // If already cached, return the file stream.
        if (await _context.FileSystem.File.ExistsAsync(filePath))
        {
            return await _context.FileSystem.File.OpenReadAsync(filePath);
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

    public async Task<ListResult> List()
    {
        List<IFileInfo> downloadedFiles = [];
        if (await _context.FileSystem.Directory.ExistsAsync(_pathManager.BaseDownloadedFileCacheDir))
        {
            foreach (IFileInfo fileInfo in await _context.FileSystem.DirectoryInfo.New(_pathManager.BaseDownloadedFileCacheDir).EnumerateFilesAsync())
            {
                downloadedFiles.Add(fileInfo);
            }
        }

        List<IDirectoryInfo> gitRepos = [];
        if (await _context.FileSystem.Directory.ExistsAsync(_pathManager.BaseGitRepoCacheDir))
        {
            foreach (IDirectoryInfo dirInfo in await _context.FileSystem.DirectoryInfo.New(_pathManager.BaseGitRepoCacheDir).EnumerateDirectoriesAsync())
            {
                foreach (IDirectoryInfo subdirInfo in await dirInfo.EnumerateDirectoriesAsync())
                {
                    gitRepos.Add(subdirInfo);
                }
            }
        }

        List<IFileInfo> packageManifestFiles = [];
        if (await _context.FileSystem.Directory.ExistsAsync(_pathManager.BasePackageManifestCacheDir))
        {
            foreach (IFileInfo fileInfo in await _context.FileSystem.DirectoryInfo.New(_pathManager.BasePackageManifestCacheDir).EnumerateFilesAsync())
            {
                packageManifestFiles.Add(fileInfo);
            }
        }

        return new ListResult()
        {
            DownloadedFiles = downloadedFiles.ToDictionary(file => _pathManager.ParseDownloadedFileCachePath(file.FullName)),
            GitRepos = gitRepos.ToDictionary(dir => _pathManager.ParseGitRepoDirCachePath(dir.FullName)),
            PackageManifestFiles = packageManifestFiles.ToDictionary(file => PackageSpecifier.Parse(_pathManager.ParsePackageManifestCachePath(file.FullName)))
        };
    }

    private async Task<Stream> GetPackageManifestFileFromGitRepo(PackageSpecifier packageSpecifier)
    {
        // Before calling this method, we assume that:
        // 1. The git client is not null.
        // 2. The package manifest cache path does not exist.

        IDirectoryInfo repoDir = await GetGitRepoDir(packageSpecifier);
        string srcPath = _pathManager.GetPackageManifestPath(repoDir.FullName);

        if (!await _context.FileSystem.File.ExistsAsync(srcPath))
        {
            throw new InvalidOperationException($"Package manifest file not found for package '{packageSpecifier}' at '{srcPath}'.");
        }

        string destPath = _pathManager.GetPackageManifestCachePath(packageSpecifier.SpecifierWithoutVariant);

        await _context.FileSystem.CreateParentDirectoryAsync(destPath);

        await _context.FileSystem.File.CopyAsync(srcPath, destPath);

        return await _context.FileSystem.File.OpenReadAsync(destPath);
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
        using Stream _ = await GetDownloadedFile(archiveFileUrl);

        string archiveFilePath = _pathManager.GetDownloadedFileCachePath(archiveFileUrl);

        ArchiveFileSource archive = new(_context.FileSystem, archiveFilePath);

        string archivePackageManifestKey = $"{packageSpecifier.ToothPath}@v{version}{(version.Major >= 2 ? "+incompatible" : "")}/{_pathManager.PackageManifestFileName}";

        IFileSourceEntry archivePackageManifestEntry = (await archive.GetFile(archivePackageManifestKey))
            ?? throw new InvalidOperationException($"Package manifest file not found for package '{packageSpecifier}' at {archivePackageManifestKey}.");

        using Stream manifestStream = await archivePackageManifestEntry.OpenRead();

        // Save the package manifest file to the cache.
        string manifestFilePath = _pathManager.GetPackageManifestCachePath(packageSpecifier.SpecifierWithoutVariant);

        await _context.FileSystem.CreateParentDirectoryAsync(manifestFilePath);

        using (Stream fileStream = await _context.FileSystem.File.CreateAsync(manifestFilePath))
        {
            await manifestStream.CopyToAsync(fileStream);
        }

        return await _context.FileSystem.File.OpenReadAsync(manifestFilePath);
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
