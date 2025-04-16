using Microsoft.Extensions.Logging;
using Semver;
using System.Runtime.InteropServices;
using System.Text;

namespace Lip.Core;

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
                ToothPath = args.InitTooth ?? DefaultTooth,
                Version = SemVersion.Parse(args.InitVersion ?? DefaultVersion),
                Info = new()
                {
                    Name = args.InitName ?? "",
                    Description = args.InitDescription ?? "",
                    Tags = [],
                    AvatarUrl = args.InitAvatarUrl
                },
                Variants = [
                    new PackageManifest.Variant
                    {
                        Label = "",
                        Platform = RuntimeInformation.RuntimeIdentifier,
                        Dependencies = [],
                        Assets = [],
                        PreserveFiles = [],
                        RemoveFiles = [],
                        Scripts = new PackageManifest.ScriptsType
                        {
                            PreInstall = [],
                            Install = [],
                            PostInstall  = [],
                            PrePack  = [],
                            PostPack  = [],
                            PreUninstall  = [],
                            Uninstall  = [],
                            PostUninstall  = [],
                            AdditionalScripts  = [],
                        }
                    }
                ]
            };
        }
        else
        {
            string tooth = args.InitTooth ?? await _context.UserInteraction.PromptForInput(DefaultTooth, "Enter the tooth path");
            string version = args.InitVersion ?? await _context.UserInteraction.PromptForInput(DefaultVersion, "Enter the package version");
            string name = args.InitName ?? await _context.UserInteraction.PromptForInput(string.Empty, "Enter the package name");
            string description = args.InitDescription ?? await _context.UserInteraction.PromptForInput(string.Empty, "Enter the package description");
            string avatarUrl = args.InitAvatarUrl ?? await _context.UserInteraction.PromptForInput(string.Empty, "Enter the package's avatar URL");

            manifest = new()
            {
                ToothPath = tooth,
                Version = SemVersion.Parse(version),
                Info = new()
                {
                    Name = name,
                    Description = description,
                    Tags = [],
                    AvatarUrl = avatarUrl
                },
                Variants = [
                    new PackageManifest.Variant
                    {
                        Label = "",
                        Platform = RuntimeInformation.RuntimeIdentifier,
                        Dependencies = [],
                        Assets = [],
                        PreserveFiles = [],
                        RemoveFiles = [],
                        Scripts = new PackageManifest.ScriptsType
                        {
                            PreInstall = [],
                            Install = [],
                            PostInstall  = [],
                            PrePack  = [],
                            PostPack  = [],
                            PreUninstall  = [],
                            Uninstall  = [],
                            PostUninstall  = [],
                            AdditionalScripts  = [],
                        }
                    }
                ]
            };

            if (!await _context.UserInteraction.Confirm(
                "Do you want to create the following package manifest file?\n{0}",
                manifest.ToJsonElement().GetRawText()))
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