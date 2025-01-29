using System.Text;
using Scriban;
using Scriban.Parsing;

namespace Lip;

public partial class Lip
{
    public record ViewArgs { }

    public async Task<string> View(string packageSpecifierText, string path, ViewArgs args)
    {
        var packageSpecifier = PackageSpecifier.Parse(packageSpecifierText);

        using Stream packageManifestFileStream = await _cacheManager.GetPackageManifestFile(packageSpecifier);
        PackageManifest packageManifest = PackageManifest.FromJsonBytesParsed(await packageManifestFileStream.ReadAsync());

        if (packageManifest.ToothPath != packageSpecifier.ToothPath)
        {
            throw new InvalidOperationException($"Tooth path in package manifest '{packageManifest.ToothPath}' does not match package specifier '{packageSpecifier.ToothPath}'.");
        }

        if (packageManifest.Version != packageSpecifier.Version)
        {
            throw new InvalidOperationException($"Version in package manifest '{packageManifest.Version}' does not match package specifier '{packageSpecifier.Version}'.");
        }

        if (path == string.Empty)
        {
            byte[] jsonBytes = packageManifest.ToJsonBytes();
            return Encoding.UTF8.GetString(jsonBytes);
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
