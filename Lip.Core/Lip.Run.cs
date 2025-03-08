using System.Runtime.InteropServices;

namespace Lip.Core;

public partial class Lip
{
    public record RunArgs
    {
        public string VariantLabel { get; init; } = string.Empty;
    }

    public async Task Run(string scriptName, RunArgs args)
    {
        PackageManifest? packageManifest = await _packageManager.GetCurrentPackageManifest()
            ?? throw new InvalidOperationException("No package manifest found.");

        PackageManifest.Variant? variant = packageManifest.GetVariant(
            args.VariantLabel,
            RuntimeInformation.RuntimeIdentifier)
            ?? throw new InvalidOperationException($"Variant '{args.VariantLabel}' not found in package manifest");

        List<string> scripts = (variant.Scripts!.AdditionalScripts.GetValueOrDefault(scriptName))
            ?? throw new InvalidOperationException($"Script '{scriptName}' not found in package manifest with variant '{args.VariantLabel}'");

        foreach (string script in scripts)
        {
            await _context.CommandRunner.Run(script, _pathManager.WorkingDir);
        }
    }
}
