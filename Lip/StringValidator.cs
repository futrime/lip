using System.Text.RegularExpressions;

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

        return CheckPackageIdentifier(parts[0]);
    }

    /// <summary>
    /// Checks if the package identifier is valid.
    /// </summary>
    /// <param name="packageIdentifier">The package identifier to validate.</param>
    /// <returns>True if the package identifier is valid; otherwise, false.</returns>
    public static bool CheckPackageIdentifier(string packageIdentifier)
    {
        // Split the package specifier into tooth path and variant label.
        string[] toothPathAndVariantLabel = packageIdentifier.Split('#');

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
    /// Checks if the variant label is valid.
    /// </summary>
    /// <param name="variantLabel">The variant label to validate.</param>
    /// <returns>True if the variant label is valid; otherwise, false.</returns>
    public static bool CheckVariantLabel(string variantLabel)
    {
        return new Regex("^([a-z0-9]+(_[a-z0-9]+)*)?$").IsMatch(variantLabel);
    }

    private static bool CheckVersion(string version)
    {
        return Semver.SemVersion.TryParse(version, out _);
    }
}
