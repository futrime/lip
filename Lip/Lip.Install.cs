namespace Lip;

public partial class Lip
{
    public record InstallArgs
    {
        public required bool DryRun { get; init; }
        public required bool Force { get; init; }
        public required bool IgnoreScripts { get; init; }
        public required bool NoDependencies { get; init; }
        public required bool Save { get; init; }
    }

    public async Task Install(List<string> packageTexts, InstallArgs args)
    {
        List<IFileSource> fileSources = [.. await Task.WhenAll(packageTexts.Select(GetFileSourceFromPackageText))];
    }

    private async Task<IFileSource> GetFileSourceFromPackageText(string packageText)
    {
        // First, check if package text refers to a local directory containing a tooth.json file.
        string possibleDirPath = Path.Join(_pathManager.WorkingDir, packageText);

        if (_context.FileSystem.Directory.Exists(possibleDirPath))
        {
            string toothJsonPath = Path.Join(possibleDirPath, "tooth.json");

            if (_context.FileSystem.File.Exists(toothJsonPath))
            {
                return new DirectoryFileSource(_context.FileSystem, possibleDirPath);
            }
        }

        // Next, check if package text refers to a local file.
        string possibleFilePath = Path.Join(_pathManager.WorkingDir, packageText);

        if (_context.FileSystem.File.Exists(possibleFilePath))
        {
            return new ArchiveFileSource(_context.FileSystem, possibleFilePath);
        }

        // Finally, assume package text is a package specifier.
        var packageSpecifier = PackageSpecifier.Parse(packageText);

        return await _cacheManager.GetPackageFileSource(packageSpecifier);
    }
}
