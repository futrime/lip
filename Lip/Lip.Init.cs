using System.Text;
using Microsoft.Extensions.Logging;

namespace Lip;

public partial class Lip
{
    public record InitArgs
    {
        public bool Force { get; init; } = false;
        public string? InitAuthor { get; init; }
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
                    Author = args.InitAuthor,
                    AvatarUrl = args.InitAvatarUrl
                }
            };
        }
        else
        {
            string tooth = args.InitTooth ?? await _userInteraction.PromptForInput("Enter the tooth path (e.g. {DefaultTooth}):", DefaultTooth) ?? DefaultTooth;
            string version = args.InitVersion ?? await _userInteraction.PromptForInput("Enter the package version (e.g. {DefaultVersion}):", DefaultVersion) ?? DefaultVersion;
            string? name = args.InitName ?? await _userInteraction.PromptForInput("Enter the package name:");
            string? description = args.InitDescription ?? await _userInteraction.PromptForInput("Enter the package description:");
            string? author = args.InitAuthor ?? await _userInteraction.PromptForInput("Enter the package author:");
            string? avatarUrl = args.InitAvatarUrl ?? await _userInteraction.PromptForInput("Enter the author's avatar URL:");

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
                    Author = author,
                    AvatarUrl = avatarUrl
                }
            };

            string jsonString = Encoding.UTF8.GetString(manifest.ToJsonBytes());
            if (!await _userInteraction.Confirm("Do you want to create the following package manifest file?\n{jsonString}", jsonString))
            {
                throw new OperationCanceledException("Operation canceled by the user.");
            }
        }

        // Create the manifest file path.
        string manifestPath = _pathManager.PackageManifestPath;

        // Check if the manifest file already exists.
        if (_fileSystem.File.Exists(manifestPath))
        {
            if (!args.Force)
            {
                throw new InvalidOperationException($"The file '{manifestPath}' already exists. Use the -f or --force option to overwrite it.");
            }

            _logger.LogWarning("The file '{ManifestPath}' already exists. Overwriting it.", manifestPath);
        }

        await _fileSystem.File.WriteAllBytesAsync(manifestPath, manifest.ToJsonBytes());

        _logger.LogInformation("Successfully initialized the package manifest file '{ManifestPath}'.", manifestPath);
    }
}
