using Flurl;
using Lip.Context;
using SharpCompress.Archives;

namespace Lip;

/// <summary>
/// The main class of the Lip library.
/// </summary>
public partial class Lip
{
    private record PackageInstallDetail
    {
        public required IFileSource FileSource { get; init; }
        public required PackageManifest PackageManifest { get; init; }
        public required string VariantLabel { get; init; }
    }

    private readonly CacheManager _cacheManager;
    private readonly IContext _context;
    private readonly DependencySolver _dependencySolver;
    private readonly PackageManager _packageManager;
    private readonly PathManager _pathManager;
    private readonly RuntimeConfig _runtimeConfig;

    public Lip(RuntimeConfig runtimeConfig, IContext context)
    {
        _context = context;
        _runtimeConfig = runtimeConfig;

        _pathManager = new(context.FileSystem, baseCacheDir: runtimeConfig.Cache, workingDir: context.WorkingDir);

        List<Url> gitHubProxies = runtimeConfig.GitHubProxies.ConvertAll(url => new Url(url));
        List<Url> goModuleProxies = runtimeConfig.GoModuleProxies.ConvertAll(url => new Url(url));

        _cacheManager = new(_context, _pathManager, gitHubProxies, goModuleProxies);

        _packageManager = new(_context, _cacheManager, _pathManager);

        _dependencySolver = new(_context, _cacheManager, _packageManager, goModuleProxies);
    }

    private async Task<PackageInstallDetail> GetFileSourceFromUserInputPackageText(string userInputPackageText)
    {
        // First, check if package text refers to a local directory containing a tooth.json file.

        string possibleDirPath = _context.FileSystem.Path.Join(_pathManager.WorkingDir, userInputPackageText.Split('#')[0]);

        if (_context.FileSystem.Directory.Exists(possibleDirPath))
        {
            string toothJsonPath = _context.FileSystem.Path.Join(possibleDirPath, "tooth.json");

            if (_context.FileSystem.File.Exists(toothJsonPath))
            {
                DirectoryFileSource directoryFileSource = new(_context.FileSystem, possibleDirPath);

                var packageManifest = PackageManifest.FromJsonBytesParsed(
                    await _context.FileSystem.File.ReadAllBytesAsync(toothJsonPath));

                return new()
                {
                    FileSource = directoryFileSource,
                    PackageManifest = packageManifest,
                    VariantLabel = userInputPackageText.Split('#').ElementAtOrDefault(1) ?? string.Empty
                };
            }
        }

        // Second, check if package text refers to a local archive file.

        string possibleFilePath = _context.FileSystem.Path.Join(_pathManager.WorkingDir, userInputPackageText.Split('#')[0]);

        if (_context.FileSystem.File.Exists(possibleFilePath))
        {
            using Stream fileStream = _context.FileSystem.File.OpenRead(possibleFilePath);

            if (ArchiveFactory.IsArchive(fileStream, out _))
            {
                ArchiveFileSource archiveFileSource = new(_context.FileSystem, possibleFilePath);

                IFileSourceEntry? packageManifestEntry = await archiveFileSource.GetEntry(_pathManager.PackageManifestFileName);

                if (packageManifestEntry is not null)
                {
                    var packageManifest = PackageManifest.FromJsonBytesParsed(
                        await (await packageManifestEntry.OpenRead()).ReadAsync());

                    return new()
                    {
                        FileSource = archiveFileSource,
                        PackageManifest = packageManifest,
                        VariantLabel = userInputPackageText.Split('#').ElementAtOrDefault(1) ?? string.Empty
                    };
                }
            }
        }

        // Finally, assume package text is a package specifier.

        {
            var packageSpecifier = PackageSpecifier.Parse(userInputPackageText);

            IFileSource fileSource = await _cacheManager.GetPackageFileSource(packageSpecifier);

            IFileSourceEntry packageManifestEntry = await fileSource.GetEntry(_pathManager.PackageManifestFileName)
                ?? throw new InvalidOperationException($"Package manifest is not found in '{packageSpecifier}'.");

            var packageManifest = PackageManifest.FromJsonBytesParsed(
                await (await packageManifestEntry.OpenRead()).ReadAsync());

            return new()
            {
                FileSource = fileSource,
                PackageManifest = packageManifest,
                VariantLabel = packageSpecifier.VariantLabel
            };
        }
    }
}
