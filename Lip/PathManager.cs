using System.IO.Abstractions;
using System.Text.RegularExpressions;
using DotNet.Globbing;
using Flurl;
using Semver;

namespace Lip;

public class PathManager(IFileSystem fileSystem, string? baseCacheDir = null, string? workingDir = null)
{
    public record GitRepoInfo
    {
        public required string Url { get; init; }
        public required string Tag { get; init; }
    }

    private const string DownloadedFileCacheDirName = "downloaded_files";
    private const string GitRepoCacheDirName = "git_repos";
    private const string PackageLockFileName = "tooth_lock.json";

    private readonly IFileSystem _fileSystem = fileSystem;
    private readonly string? _baseCacheDir = baseCacheDir;
    private readonly string? _workingDir = workingDir;

    public string BaseCacheDir => _fileSystem.Path.GetFullPath(_baseCacheDir ?? throw new InvalidOperationException("Base cache directory is not provided."));

    public string BaseDownloadedFileCacheDir => _fileSystem.Path.Join(BaseCacheDir, DownloadedFileCacheDirName);

    public string BaseGitRepoCacheDir => _fileSystem.Path.Join(BaseCacheDir, GitRepoCacheDirName);

    public string CurrentPackageManifestPath => _fileSystem.Path.Join(WorkingDir, PackageManifestFileName);

    public string CurrentPackageLockPath => _fileSystem.Path.Join(WorkingDir, PackageLockFileName);

    public string PackageManifestFileName => "tooth.json";

    public string RuntimeConfigPath => _fileSystem.Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "lip", "liprc.json");

    public string WorkingDir => _fileSystem.Path.GetFullPath(_workingDir ?? _fileSystem.Directory.GetCurrentDirectory());

    public string GetDownloadedFileCachePath(Url url)
    {
        string downloadedFileName = Url.Encode(url);
        return _fileSystem.Path.Join(BaseDownloadedFileCacheDir, downloadedFileName);
    }

    public string GetGitRepoDirCachePath(GitRepoInfo repo)
    {
        string repoDirName = Url.Encode(repo.Url);
        string tagDirName = Url.Encode(repo.Tag);
        return _fileSystem.Path.Join(BaseGitRepoCacheDir, repoDirName, tagDirName);
    }

    public string GetPackageManifestPath(string baseDir)
    {
        return _fileSystem.Path.Join(baseDir, PackageManifestFileName);
    }

    public string? GetPlacementRelativePath(PackageManifest.PlaceType placement, string fileSourceEntryKey)
    {
        if (placement.Type == PackageManifest.PlaceType.TypeEnum.File)
        {
            string fileName = _fileSystem.Path.GetFileName(fileSourceEntryKey);

            if (fileSourceEntryKey == placement.Src)
            {
                // The destination is the file path, so leave empty.
                return string.Empty;
            }
            else if (placement.Src != string.Empty)
            {
                Glob glob = Glob.Parse(placement.Src);

                if (glob.IsMatch(fileSourceEntryKey))
                {
                    // The destination is the directory path.
                    return fileName;
                }
            }

            return null;
        }
        else if (placement.Type == PackageManifest.PlaceType.TypeEnum.Dir)
        {
            string placementSrc = placement.Src;

            if (placementSrc != string.Empty && !placementSrc.EndsWith('/'))
            {
                placementSrc += '/';
            }

            if (fileSourceEntryKey.StartsWith(placementSrc))
            {
                // The destination is the directory root.
                return fileSourceEntryKey[placementSrc.Length..];
            }

            return null;
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    public Url ParseDownloadedFileCachePath(string downloadedFileCachePath)
    {
        // Dynamically construct a regex pattern to match the base cache directory path.
        Regex pattern = new($"{Regex.Escape(BaseDownloadedFileCacheDir)}{Regex.Escape(_fileSystem.Path.DirectorySeparatorChar.ToString())}(.*)");
        Match match = pattern.Match(downloadedFileCachePath);
        if (!match.Success)
        {
            throw new InvalidOperationException($"Invalid downloaded file cache path: {downloadedFileCachePath}");
        }
        return Url.Parse(Url.Decode(match.Groups[1].Value, true));
    }

    public GitRepoInfo ParseGitRepoDirCachePath(string repoDirCachePath)
    {
        Regex pattern = new($"{Regex.Escape(BaseGitRepoCacheDir)}{Regex.Escape(_fileSystem.Path.DirectorySeparatorChar.ToString())}(.*){Regex.Escape(_fileSystem.Path.DirectorySeparatorChar.ToString())}(.*)");
        Match match = pattern.Match(repoDirCachePath);
        if (!match.Success)
        {
            throw new InvalidOperationException($"Invalid Git repo directory cache path: {repoDirCachePath}");
        }
        return new GitRepoInfo
        {
            Url = Url.Decode(match.Groups[1].Value, true),
            Tag = Url.Decode(match.Groups[2].Value, true)
        };
    }
}
