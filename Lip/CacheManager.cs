using System.IO.Abstractions;
using Flurl;
using Lip.Context;
using Microsoft.Extensions.Logging;
using Semver;

namespace Lip;

public class CacheManager(
    IContext context,
    PathManager pathManager,
    List<Url> gitHubProxies,
    List<Url> goModuleProxies)
{
    public record CacheSummary
    {
        public required Dictionary<Url, IFileInfo> DownloadedFiles { get; init; }
        public required Dictionary<PathManager.GitRepoInfo, IDirectoryInfo> GitRepos { get; init; }
    }

    private readonly IContext _context = context;
    private readonly List<Url> _githubProxies = gitHubProxies;
    private readonly List<Url> _goModuleProxies = goModuleProxies;
    private readonly PathManager _pathManager = pathManager;

    public async Task Clean()
    {
        await Task.Delay(0); // Suppress warning.

        string baseCacheDir = _pathManager.BaseCacheDir;

        if (_context.FileSystem.Directory.Exists(baseCacheDir))
        {
            _context.FileSystem.Directory.Delete(baseCacheDir, recursive: true);
        }
    }

    public async Task<IFileInfo> GetDownloadedFile(Url url) => await GetDownloadedFile([url]);

    public async Task<IFileInfo> GetDownloadedFile(List<Url> originalUrls)
    {
        // Apply GitHub proxy to GitHub URLs.
        List<Url> actualUrls = [.. originalUrls.SelectMany(url =>
        {
            // For typical URLs, just return the URL.
            if (url.Host == "github.com" && _githubProxies.Count != 0)
            {
                return _githubProxies.Select(proxy => proxy.AppendPathSegment(url.Path).SetQueryParams(url.QueryParams));
            }

            return [url];
        })];

        return await GetDownloadedFileWithActualUrls(actualUrls);
    }

    public async Task<IFileSource> GetPackageFileSource(PackageSpecifier packageSpecifier)
    {
        // First, try to get the package from the Go module proxy.
        if (_goModuleProxies.Count != 0)
        {
            IFileInfo goModuleArchive = await GetGoModuleArchive(packageSpecifier);

            return new GoModuleArchiveFileSource(
                _context.FileSystem,
                goModuleArchive.FullName,
                packageSpecifier.ToothPath,
                packageSpecifier.Version);
        }

        // Next, try to get the package from the Git repository.
        if (_context.Git is not null)
        {
            IDirectoryInfo repoDir = await GetGitRepoDir(packageSpecifier);

            return new DirectoryFileSource(_context.FileSystem, repoDir.FullName);
        }

        throw new InvalidOperationException("No remote source is available.");
    }

    public async Task<CacheSummary> List()
    {
        await Task.Delay(0); // Suppress warning.

        List<IFileInfo> downloadedFiles = [];

        string baseDownloadedFileCacheDir = _pathManager.BaseDownloadedFileCacheDir;

        if (_context.FileSystem.Directory.Exists(baseDownloadedFileCacheDir))
        {
            foreach (IFileInfo fileInfo in _context.FileSystem.DirectoryInfo.New(baseDownloadedFileCacheDir).EnumerateFiles())
            {
                downloadedFiles.Add(fileInfo);
            }
        }

        List<IDirectoryInfo> gitRepos = [];

        string baseGitRepoCacheDir = _pathManager.BaseGitRepoCacheDir;

        if (_context.FileSystem.Directory.Exists(baseGitRepoCacheDir))
        {
            foreach (IDirectoryInfo dirInfo in _context.FileSystem.DirectoryInfo.New(baseGitRepoCacheDir).EnumerateDirectories())
            {
                foreach (IDirectoryInfo subdirInfo in dirInfo.EnumerateDirectories())
                {
                    gitRepos.Add(subdirInfo);
                }
            }
        }

        return new CacheSummary()
        {
            DownloadedFiles = downloadedFiles.ToDictionary(file => _pathManager.ParseDownloadedFileCachePath(file.FullName)),
            GitRepos = gitRepos.ToDictionary(dir => _pathManager.ParseGitRepoDirCachePath(dir.FullName)),
        };
    }

    public async Task<IFileInfo> GetDownloadedFileWithActualUrls(List<Url> actualUrls)
    {
        foreach (Url url in actualUrls)
        {
            string filePath = _pathManager.GetDownloadedFileCachePath(url);

            if (_context.FileSystem.File.Exists(filePath))
            {
                return _context.FileSystem.FileInfo.New(filePath);
            }
        }

        foreach (Url url in actualUrls)
        {
            string filePath = _pathManager.GetDownloadedFileCachePath(url);

            _context.FileSystem.CreateParentDirectory(filePath);

            try
            {
                await _context.Downloader.DownloadFile(url, filePath);

                return _context.FileSystem.FileInfo.New(filePath);
            }
            catch (Exception ex)
            {
                _context.Logger.LogWarning(ex, "Failed to download {Url}. Attempting next URL.", url);
            }
        }

        throw new InvalidOperationException("All download attempts failed.");
    }

    private async Task<IDirectoryInfo> GetGitRepoDir(PackageSpecifier packageSpecifier)
    {
        string repoUrl = Url.Parse($"https://{packageSpecifier.ToothPath}");
        string tag = $"v{packageSpecifier.Version}";

        string repoDirPath = _pathManager.GetGitRepoDirCachePath(new()
        {
            Url = repoUrl,
            Tag = tag
        });

        if (!_context.FileSystem.Directory.Exists(repoDirPath))
        {
            _context.FileSystem.CreateParentDirectory(repoDirPath);

            // Here we assume that git availability is checked before calling this method.
            await _context.Git!.Clone(
                repoUrl,
                repoDirPath,
                branch: tag,
                depth: 1);
        }

        return _context.FileSystem.DirectoryInfo.New(repoDirPath);
    }

    private async Task<IFileInfo> GetGoModuleArchive(PackageSpecifier packageSpecifier)
    {
        SemVersion version = packageSpecifier.Version;

        List<Url> archiveFileUrls = [.. _goModuleProxies.Select(proxy =>
            proxy.Clone()
                .AppendPathSegments(
                    GoModule.EscapePath(packageSpecifier.ToothPath),
                    "@v",
                    GoModule.EscapeVersion(GoModule.CanonicalVersion(version.ToString())) + ".zip"
                )
        )];

        return await GetDownloadedFile(archiveFileUrls);
    }
}
