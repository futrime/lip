using System.Text.RegularExpressions;
using Flurl;

namespace Lip;

/// <summary>
/// Provides utility methods for validating various strings.
/// </summary>
public static class StringValidator
{
    /// <summary>
    /// Checks if the package specifier is valid.
    /// </summary>
    /// <param name="packageSpecifier">The package specifier to validate.</param>
    /// <returns>True if the package specifier is valid; otherwise, false.</returns>
    public static bool CheckPackageSpecifier(string packageSpecifier)
    {
        // Split the package specifier, whose first part is tooth path + variant label and the second part is version.
        string[] parts = packageSpecifier.Split('@');
        if (parts.Length != 2)
        {
            return false;
        }

        string version = parts[1];
        if (!CheckVersion(version))
        {
            return false;
        }

        return CheckPackageSpecifierWithoutVersion(parts[0]);
    }

    /// <summary>
    /// Checks if the package specifier is valid without version.
    /// </summary>
    /// <param name="packageSpecifier">The package specifier to validate.</param>
    /// <returns>True if the package specifier is valid without version; otherwise, false.</returns>
    public static bool CheckPackageSpecifierWithoutVersion(string packageSpecifier)
    {
        // Split the package specifier into tooth path and variant label.
        string[] toothPathAndVariantLabel = packageSpecifier.Split('#');

        if (toothPathAndVariantLabel.Length > 2)
        {
            return false;
        }

        string toothPath = toothPathAndVariantLabel[0];
        if (!CheckToothPath(toothPath))
        {
            return false;
        }

        if (toothPathAndVariantLabel.Length == 2)
        {
            string variantLabel = toothPathAndVariantLabel[1];
            if (!CheckVariantLabel(variantLabel))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Checks if the path is safe to place files to.
    /// </summary>
    /// <param name="path">The path to validate.</param>
    /// <returns>True if the path is safe to place files to; otherwise, false.</returns>
    public static bool CheckPlaceDestPath(string path)
    {
        if (Path.IsPathFullyQualified(path) || Path.IsPathRooted(path))
        {
            return false;
        }

        if (path.Contains(".."))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if the script name is valid.
    /// </summary>
    /// <param name="scriptName">The script name to validate.</param>
    /// <returns>True if the script name is valid; otherwise, false.</returns>
    public static bool CheckScriptName(string scriptName)
    {
        return new Regex("^[a-z0-9]+(_[a-z0-9]+)*$").IsMatch(scriptName);
    }

    /// <summary>
    /// Checks if the tag is valid.
    /// </summary>
    /// <param name="tag">The tag to validate.</param>
    /// <returns>True if the tag is valid; otherwise, false.</returns>
    public static bool CheckTag(string tag)
    {
        return new Regex("^[a-z0-9-]+(:[a-z0-9-]+)?$").IsMatch(tag);
    }

    /// <summary>
    /// Checks if the tooth path is valid.
    /// </summary>
    /// <param name="toothPath">The tooth path to validate.</param>
    /// <returns>True if the tooth path is valid; otherwise, false.</returns>
    public static bool CheckToothPath(string toothPath)
    {
        return GoModule.CheckPath(toothPath);
    }

    /// <summary>
    /// Checks if the URL is valid.
    /// </summary>
    /// <param name="url">The URL to validate.</param>
    /// <returns>True if the URL is valid; otherwise, false.</returns>
    public static bool CheckUrl(string url)
    {
        return Url.IsValid(url);
    }

    /// <summary>
    /// Checks if the variant label is valid.
    /// </summary>
    /// <param name="variantLabel">The variant label to validate.</param>
    /// <returns>True if the variant label is valid; otherwise, false.</returns>
    public static bool CheckVariantLabel(string variantLabel)
    {
        return new Regex("^([a-z0-9]+(_[a-z0-9]+)*)?$").IsMatch(variantLabel);
    }

    /// <summary>
    /// Checks if the version is valid.
    /// </summary>
    /// <param name="version">The version to validate.</param>
    /// <returns>True if the version is valid; otherwise, false.</returns>
    public static bool CheckVersion(string version)
    {
        return Semver.SemVersion.TryParse(version, out _);
    }

    /// <summary>
    /// Checks if the version range is valid.
    /// </summary>
    /// <param name="versionRange">The version range to validate.</param>
    /// <returns>True if the version range is valid; otherwise, false.</returns>
    public static bool CheckVersionRange(string versionRange)
    {
        return Semver.SemVersionRange.TryParseNpm(versionRange, out _);
    }
}
