using System.Text.RegularExpressions;

namespace Lip;

/// <summary>
/// Provides utility methods for validating various strings.
/// </summary>
public static partial class StringValidator
{
    /// <summary>
    /// Checks if the path is safe to place files to.
    /// </summary>
    /// <param name="path">The path to validate.</param>
    /// <returns>True if the path is safe to place files to; otherwise, false.</returns>
    public static bool CheckSafePlacePath(string path)
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
        if (toothPathAndVariantLabel.Length != 2)
        {
            return false;
        }

        string toothPath = toothPathAndVariantLabel[0];
        if (!CheckToothPath(toothPath))
        {
            return false;
        }

        string variantLabel = toothPathAndVariantLabel[1];
        if (!CheckVariantLabel(variantLabel))
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
        return ScriptNameGeneratedRegex().IsMatch(scriptName);
    }

    /// <summary>
    /// Checks if the tag is valid.
    /// </summary>
    /// <param name="tag">The tag to validate.</param>
    /// <returns>True if the tag is valid; otherwise, false.</returns>
    public static bool CheckTag(string tag)
    {
        return TagGeneratedRegex().IsMatch(tag);
    }

    /// <summary>
    /// Checks if the tooth path is valid.
    /// </summary>
    /// <param name="toothPath">The tooth path to validate.</param>
    /// <returns>True if the tooth path is valid; otherwise, false.</returns>
    public static bool CheckToothPath(string toothPath)
    {
        return GoModule.CheckModPath(toothPath);
    }

    /// <summary>
    /// Checks if the URL is valid.
    /// </summary>
    /// <param name="url">The URL to validate.</param>
    /// <returns>True if the URL is valid; otherwise, false.</returns>
    public static bool CheckUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }

    /// <summary>
    /// Checks if the variant label is valid.
    /// </summary>
    /// <param name="variantLabel">The variant label to validate.</param>
    /// <returns>True if the variant label is valid; otherwise, false.</returns>
    public static bool CheckVariantLabel(string variantLabel)
    {
        return VariantLabelGeneratedRegex().IsMatch(variantLabel);
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

    /// <summary>
    /// Checks if the lock type is valid.
    /// </summary>
    /// <param name="lockType">The lock type to validate.</param>
    /// <returns>True if the lock type is valid; otherwise, false.</returns>
    public static bool CheckLockType(PackageLock.LockType lockType)
    {
        if (!CheckToothPath(lockType.ToothPath))
        {
            return false;
        }

        if (!CheckVariantLabel(lockType.VariantLabel))
        {
            return false;
        }

        if (!CheckVersion(lockType.Version))
        {
            return false;
        }

        return true;
    }

    [GeneratedRegex("^[a-z0-9]+(_[a-z0-9]+)*$")]
    private static partial Regex ScriptNameGeneratedRegex();

    [GeneratedRegex("^[a-z0-9-]+(:[a-z0-9-]+)?$")]
    private static partial Regex TagGeneratedRegex();

    [GeneratedRegex("^[a-z0-9]+(_[a-z0-9]+)*$")]
    private static partial Regex VariantLabelGeneratedRegex();
}

static file class GoModule
{
    private static readonly string[] BadWindowsNames = new[]
    {
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
    };

    public static bool CheckModPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        if (path[0] == '-')
            return false;

        if (path.Contains("//"))
            return false;

        if (path[^1] == '/')
            return false;

        string[] elements = path.Split('/');

        // First element special checks
        string first = elements[0];
        if (!first.Contains('.'))
            return false;

        if (first[0] == '-')
            return false;

        foreach (char c in first)
        {
            if (!IsFirstPathOk(c))
                return false;
        }

        // Check all elements
        foreach (string elem in elements)
        {
            if (!CheckElem(elem))
                return false;
        }

        return true;
    }

    private static bool IsFirstPathOk(char c)
        => c is '-' or '.' or >= '0' and <= '9' or >= 'a' and <= 'z';

    private static bool IsModPathOk(char c)
        => c is '-' or '.' or '_' or '~' or
           >= '0' and <= '9' or
           >= 'A' and <= 'Z' or
           >= 'a' and <= 'z';

    private static bool CheckElem(string elem)
    {
        if (string.IsNullOrEmpty(elem))
            return false;

        if (elem.All(c => c == '.'))
            return false;

        if (elem[0] == '.')
            return false;

        if (elem[^1] == '.')
            return false;

        foreach (char c in elem)
        {
            if (!IsModPathOk(c))
                return false;
        }

        // Windows name checks
        string shortName = elem;
        int dotIndex = elem.IndexOf('.');
        if (dotIndex >= 0)
            shortName = elem[..dotIndex];

        if (BadWindowsNames.Any(name => string.Equals(name, shortName, StringComparison.OrdinalIgnoreCase)))
            return false;

        // Windows short-name check
        int tildeIndex = shortName.LastIndexOf('~');
        if (tildeIndex >= 0 && tildeIndex < shortName.Length - 1)
        {
            string suffix = shortName[(tildeIndex + 1)..];
            if (suffix.All(char.IsDigit))
                return false;
        }

        return true;
    }
}
