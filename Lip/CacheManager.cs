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
    public record CacheSummary
    {
        public required Dictionary<Url, IFileInfo> DownloadedFiles { get; init; }
        public required Dictionary<PathManager.GitRepoInfo, IDirectoryInfo> GitRepos { get; init; }
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

    public async Task<IFileInfo> GetDownloadedFile(Url url)
    {
        if (url.Host == "github.com" && _githubProxy is not null)
        {
            url = _githubProxy.AppendPathSegment(url.Path).SetQueryParams(url.QueryParams);
        }

        string filePath = _pathManager.GetDownloadedFileCachePath(url);

        if (!await _context.FileSystem.File.ExistsAsync(filePath))
        {
            await _context.FileSystem.CreateParentDirectoryAsync(filePath);

            await _context.Downloader.DownloadFile(url, filePath);
        }

        return _context.FileSystem.FileInfo.New(filePath);
    }

    public async Task<IFileSource> GetPackageFileSource(PackageSpecifier packageSpecifier)
    {
        if (_context.Git is not null)
        {
            IDirectoryInfo repoDir = await GetGitRepoDir(packageSpecifier);

            return new DirectoryFileSource(_context.FileSystem, repoDir.FullName);
        }
        else if (_goModuleProxy is not null)
        {
            IFileInfo goModuleArchive = await GetGoModuleArchive(packageSpecifier);

            return new ArchiveFileSource(_context.FileSystem, goModuleArchive.FullName);
        }
        else
        {
            throw new InvalidOperationException("No remote source is available.");
        }
    }

    public async Task<CacheSummary> List()
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

        return new CacheSummary()
        {
            DownloadedFiles = downloadedFiles.ToDictionary(file => _pathManager.ParseDownloadedFileCachePath(file.FullName)),
            GitRepos = gitRepos.ToDictionary(dir => _pathManager.ParseGitRepoDirCachePath(dir.FullName)),
        };
    }

    private async Task<IDirectoryInfo> GetGitRepoDir(PackageSpecifier packageSpecifier)
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

    private async Task<IFileInfo> GetGoModuleArchive(PackageSpecifier packageSpecifier)
    {
        if (_goModuleProxy is null)
        {
            throw new InvalidOperationException("Go module proxy is not available.");
        }

        SemVersion version = packageSpecifier.Version;

        // Reference: https://go.dev/ref/mod#glos-canonical-version
        if (version.Metadata != string.Empty)
        {
            throw new ArgumentException("Go module proxy does not accept version with build metadata.", nameof(packageSpecifier));
        }

        // Reference: https://go.dev/ref/mod#non-module-compat
        string archiveFileNameInUrl = $"v{version}{(version.Major >= 2 ? "+incompatible" : string.Empty)}.zip";

        string escapedGoModulePath = GoModule.EscapePath(packageSpecifier.ToothPath);
        Url archiveFileUrl = _goModuleProxy!.Clone().AppendPathSegments(escapedGoModulePath, "@v", archiveFileNameInUrl);

        return await GetDownloadedFile(archiveFileUrl);
    }
}
