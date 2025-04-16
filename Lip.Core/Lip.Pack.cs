using Microsoft.Extensions.Logging;
using SharpCompress.Common;
using SharpCompress.Writers;
using System.Runtime.InteropServices;

namespace Lip.Core;

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
        PackageManifest packageManifest = await _packageManager.GetCurrentPackageManifest()
            ?? throw new InvalidOperationException("No package manifest found.");

        // Run pre-pack scripts.

        if (!args.IgnoreScripts)
        {
            var prePackScripts = packageManifest.GetVariant(
                string.Empty,
                RuntimeInformation.RuntimeIdentifier)?
                .Scripts
                .PrePack;

            if (prePackScripts != null)
            {
                foreach (var script in prePackScripts)
                {
                    _context.Logger.LogDebug("Running script: {script}", script);
                    if (!args.DryRun)
                    {
                        await _context.CommandRunner.Run(script, _pathManager.WorkingDir);
                    }
                }
            }
        }

        // Pack files in the archive.

        DirectoryFileSource fileSource = new(
            _context.FileSystem,
            _pathManager.WorkingDir);

        List<PackageManifest.Placement> filePlacements = [.. packageManifest.Variants
            .SelectMany(v => v.Assets)
            .Where(a => a.Type == PackageManifest.Asset.TypeEnum.Self)
            .SelectMany(a => a.Placements)];

        var allEntries = new List<IFileSourceEntry>();
        await foreach (var entry in fileSource.GetAllEntries())
        {
            allEntries.Add(entry);
        }
        List<IFileSourceEntry> fileEntriesToPlace = [.. allEntries
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

        foreach (var entry in allEntries)
        {
            await entry.DisposeAsync();
        }

        // Run post-pack scripts.

        if (!args.IgnoreScripts)
        {
            var postPackScripts = packageManifest.GetVariant(
                string.Empty,
                RuntimeInformation.RuntimeIdentifier)?
                .Scripts
                .PostPack;

            if (postPackScripts != null)
            {
                foreach (var script in postPackScripts)
                {
                    _context.Logger.LogDebug("Running script: {script}", script);
                    if (!args.DryRun)
                    {
                        await _context.CommandRunner.Run(script, _pathManager.WorkingDir);
                    }
                }
            }
        }
    }
}