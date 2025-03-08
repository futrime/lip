using Scriban;
using Scriban.Parsing;
using System.Text;

namespace Lip.Core;

public partial class Lip
{
    public record ViewArgs { }

    public async Task<string> View(string packageSpecifierText, string? path, ViewArgs args)
    {
        var packageSpecifier = PackageSpecifier.Parse(packageSpecifierText);

        PackageManifest packageManifest = await _packageManager.GetPackageManifestFromCache(packageSpecifier)
            ?? throw new InvalidOperationException($"Cannot get package manifest from package '{packageSpecifier}'.");

        if (packageManifest.ToothPath != packageSpecifier.ToothPath)
        {
            throw new InvalidOperationException($"Tooth path in package manifest '{packageManifest.ToothPath}' does not match package specifier '{packageSpecifier.ToothPath}'.");
        }

        if (packageManifest.Version != packageSpecifier.Version)
        {
            throw new InvalidOperationException($"Version in package manifest '{packageManifest.Version}' does not match package specifier '{packageSpecifier.Version}'.");
        }

        if (path is null)
        {
            return packageManifest.ToJsonElement().GetRawText();
        }

        Template template = Template.Parse(path, lexerOptions: new()
        {
            Mode = ScriptMode.ScriptOnly
        });

        if (template.HasErrors)
        {
            StringBuilder sb = new();
            foreach (LogMessage message in template.Messages)
            {
                sb.Append(message.ToString());
            }
            throw new FormatException($"Failed to parse template '{path}': {sb}");
        }

        return template.Render(packageManifest.ToJsonElement());
    }
}
