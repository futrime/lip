using System.Text;
using Microsoft.Extensions.Logging;

namespace Lip;

public partial class Lip
{
    public record InitArgs
    {
        public bool Force { get; init; } = false;
        public string? InitAvatarUrl { get; init; }
        public string? InitDescription { get; init; }
        public string? InitName { get; init; }
        public string? InitTooth { get; init; }
        public string? InitVersion { get; init; }
        public bool Yes { get; init; } = false;
    }

    private const string DefaultTooth = "example.com/org/package";
    private const string DefaultVersion = "0.1.0";

    public async Task Init(InitArgs args)
    {
        PackageManifest manifest;

        if (args.Yes)
        {
            manifest = new()
            {
                FormatVersion = PackageManifest.DefaultFormatVersion,
                FormatUuid = PackageManifest.DefaultFormatUuid,
                ToothPath = args.InitTooth ?? DefaultTooth,
                VersionText = args.InitVersion ?? DefaultVersion,
                Info = new()
                {
                    Name = args.InitName,
                    Description = args.InitDescription,
                    AvatarUrl = args.InitAvatarUrl
                }
            };
        }
        else
        {
            string tooth = args.InitTooth ?? await _context.UserInteraction.PromptForInput("Enter the tooth path (e.g. {DefaultTooth}):", DefaultTooth) ?? DefaultTooth;
            string version = args.InitVersion ?? await _context.UserInteraction.PromptForInput("Enter the package version (e.g. {DefaultVersion}):", DefaultVersion) ?? DefaultVersion;
            string? name = args.InitName ?? await _context.UserInteraction.PromptForInput("Enter the package name:");
            string? description = args.InitDescription ?? await _context.UserInteraction.PromptForInput("Enter the package description:");
            string? avatarUrl = args.InitAvatarUrl ?? await _context.UserInteraction.PromptForInput("Enter the package's avatar URL:");

            manifest = new()
            {
                FormatVersion = PackageManifest.DefaultFormatVersion,
                FormatUuid = PackageManifest.DefaultFormatUuid,
                ToothPath = tooth,
                VersionText = version,
                Info = new()
                {
                    Name = name,
                    Description = description,
                    AvatarUrl = avatarUrl
                }
            };

            string jsonString = Encoding.UTF8.GetString(manifest.ToJsonBytes());
            if (!await _context.UserInteraction.Confirm("Do you want to create the following package manifest file?\n{jsonString}", jsonString))
            {
                throw new OperationCanceledException("Operation canceled by the user.");
            }
        }

        // Create the manifest file path.
        string manifestPath = _pathManager.CurrentPackageManifestPath;

        // Check if the manifest file already exists.
        if (_context.FileSystem.File.Exists(manifestPath))
        {
            if (!args.Force)
            {
                throw new InvalidOperationException($"The file '{manifestPath}' already exists. Use the -f or --force option to overwrite it.");
            }

            _context.Logger.LogWarning("The file '{ManifestPath}' already exists. Overwriting it.", manifestPath);
        }

        await _packageManager.SaveCurrentPackageManifest(manifest);
    }
}
