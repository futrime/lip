namespace Lip;

public static class GoModule
{
    private static readonly string[] s_badWindowsNames =
    [
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
    ];

    public static string CanonicalVersion(string v)
    {
        // We do not adopt the original implementation of golang.org/x/mod but
        // instead reimplement it in C#.

        if (!v.StartsWith('v'))
        {
            v = "v" + v;
        }

        if (v.Contains('+'))
        {
            v = v[..v.IndexOf('+')];
        }

        if (!v.StartsWith("v0.") && !v.StartsWith("v1."))
        {
            v += "+incompatible";
        }

        return v;
    }

    public static bool CheckPath(string path)
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

        // if (first[0] == '-')
        //     return false;

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

    public static string EscapePath(string path)
    {
        if (!CheckPath(path))
        {
            throw new ArgumentException($"{path} is not a valid Go module path.", nameof(path));
        }

        return EscapeString(path);
    }

    public static string EscapeVersion(string v)
    {
        if (!CheckElem(v) || v.Contains('!'))
        {
            throw new ArgumentException($"{v} is not a valid Go module version.", nameof(v));
        }

        return EscapeString(v);
    }

    private static bool CheckElem(string elem)
    {
        // if (string.IsNullOrEmpty(elem))
        //     return false;

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

        if (s_badWindowsNames.Any(name => string.Equals(name, shortName, StringComparison.OrdinalIgnoreCase)))
            return false;

        // Windows short-name check
        int tildeIndex = shortName.LastIndexOf('~');
        if (tildeIndex >= 0 && tildeIndex < shortName.Length - 1)
        {
            string suffix = shortName[(tildeIndex + 1)..];
            if (suffix.All(char.IsDigit))
            {
                return false;
            }
        }

        return true;
    }

    private static string EscapeString(string s)
    {
        bool haveUpper = false;
        foreach (char c in s)
        {
            if (c >= 'A' && c <= 'Z')
            {
                haveUpper = true;
            }
        }

        if (!haveUpper)
        {
            return s;
        }

        var buf = new List<char>();
        foreach (char c in s)
        {
            if (c >= 'A' && c <= 'Z')
            {
                buf.Add('!');
                buf.Add((char)(c + 'a' - 'A'));
            }
            else
            {
                buf.Add(c);
            }
        }
        return new string([.. buf]);
    }

    private static bool IsFirstPathOk(char c)
    {
        if (c == '-') return true;
        if (c == '.') return true;
        if (c >= '0' && c <= '9') return true;
        if (c >= 'a' && c <= 'z') return true;
        return false;
    }

    private static bool IsModPathOk(char c)
    {
        if (c == '-' || c == '.' || c == '_' || c == '~')
            return true;
        if (c >= '0' && c <= '9')
            return true;
        if (c >= 'A' && c <= 'Z')
            return true;
        if (c >= 'a' && c <= 'z')
            return true;
        return false;
    }
}
