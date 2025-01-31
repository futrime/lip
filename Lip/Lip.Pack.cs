using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using SharpCompress.Common;
using SharpCompress.Writers;

namespace Lip;

public partial class Lip
{
    public record PackArgs
    {
        public enum ArchiveFormatType
        {
            Zip,
            Tar,
            TarGz,
        }

        public bool DryRun { get; init; } = false;
        public bool IgnoreScripts { get; init; } = false;
        public ArchiveFormatType ArchiveFormat { get; init; } = ArchiveFormatType.Zip;
    }

    public async Task Pack(string outputPath, PackArgs args)
    {
        PackageManifest packageManifest = await _packageManager.GetCurrentPackageManifestParsed()
            ?? throw new InvalidOperationException("No package manifest found.");

        // Run pre-pack scripts.

        if (!args.IgnoreScripts)
        {
            PackageManifest.VariantType? variant = packageManifest.GetSpecifiedVariant(
                string.Empty,
                RuntimeInformation.RuntimeIdentifier);
            PackageManifest.ScriptsType? script = variant?.Scripts;
            List<string>? prePackScripts = script?.PrePack;

            prePackScripts?.ForEach(script =>
            {
                _context.Logger.LogDebug("Running script: {script}", script);
                if (!args.DryRun)
                {
                    _context.CommandRunner.Run(
                        script,
                        _pathManager.WorkingDir);
                }
            });
        }

        // Pack files in the archive.

        DirectoryFileSource fileSource = new(
            _context.FileSystem,
            _pathManager.WorkingDir);

        List<PackageManifest.PlaceType> filePlacements = packageManifest.Variants?
            .SelectMany(v => v.Assets ?? [])
            .Where(a => a.Type == PackageManifest.AssetType.TypeEnum.Self)
            .SelectMany(a => a.Place ?? [])
            .ToList() ?? [];

        List<IFileSourceEntry> fileEntriesToPlace = [.. (await fileSource.GetAllEntries())
            .Where(entry => filePlacements.Any(placement => _pathManager.GetPlacementRelativePath(
                placement,
                entry.Key) is not null) || entry.Key == _pathManager.PackageManifestFileName)];

        using (Stream outputStream = !args.DryRun
            ? _context.FileSystem.File.Create(outputPath)
            : Stream.Null)
        using (IWriter writer = args.ArchiveFormat switch
        {
            PackArgs.ArchiveFormatType.Zip => WriterFactory.Open(
                outputStream,
                ArchiveType.Zip,
                new(CompressionType.Deflate)),
            PackArgs.ArchiveFormatType.Tar => WriterFactory.Open(
                outputStream,
                ArchiveType.Tar,
                new(CompressionType.None)),
            PackArgs.ArchiveFormatType.TarGz => WriterFactory.Open(
                outputStream,
                ArchiveType.Tar,
                new(CompressionType.GZip)),
            _ => throw new NotImplementedException(),
        })
        {
            foreach (IFileSourceEntry entry in fileEntriesToPlace)
            {
                using Stream entryStream = await entry.OpenRead();

                writer.Write(entry.Key, await entry.OpenRead());
            }
        }

        // Run post-pack scripts.

        if (!args.IgnoreScripts)
        {
            PackageManifest.VariantType? variant = packageManifest.GetSpecifiedVariant(
                string.Empty,
                RuntimeInformation.RuntimeIdentifier);
            PackageManifest.ScriptsType? script = variant?.Scripts;
            List<string>? postPackScripts = script?.PostPack;

            postPackScripts?.ForEach(script =>
            {
                _context.Logger.LogDebug("Running script: {script}", script);
                if (!args.DryRun)
                {
                    _context.CommandRunner.Run(
                        script,
                        _pathManager.WorkingDir);
                }
            });
        }
    }
}
