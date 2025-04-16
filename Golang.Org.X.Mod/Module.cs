using System.Text;

namespace Golang.Org.X.Mod;

public static class Module
{
    public class InvalidPathError(string kind, string path, Exception innerException)
        : Exception($"malformed {kind} path {path}", innerException)
    {
        public string Kind { get; private init; } = kind;
        public string Path { get; private init; } = path;
    }

    public class InvalidVersionError(string version, bool pseudo, Exception innerException)
        : Exception($"{version} (pseudo={pseudo}) invalid", innerException)
    {
        public string Version { get; private init; } = version;
        public bool Pseudo { get; private init; } = pseudo;
    }

    public static string CanonicalVersion(string v)
    {
        var cv = Semver.Canonical(v);
        if (Semver.Build(v) == "+incompatible")
        {
            cv += "+incompatible";
        }
        return cv;
    }

    public static Exception Check(string path, string version)
    {
        throw new NotImplementedException();
    }

    public static Exception CheckFilePath(string path)
    {
        throw new NotImplementedException();
    }

    public static Exception CheckImportPath(string path)
    {
        throw new NotImplementedException();
    }

    public static Exception? CheckPath(string path)
    {
        InvalidPathError MakeError(Exception exception)
        {
            return new InvalidPathError("module", path, exception);
        }

        var err = ModuleInternal.CheckPath(path, ModuleInternal.PathKind.ModulePath);
        if (err != null)
        {
            return MakeError(err);
        }
        int i = path.IndexOf('/');
        if (i < 0)
        {
            i = path.Length;
        }
        if (i == 0)
        {
            return MakeError(new Exception("leading slash"));
        }
        if (!path[..i].Contains('.'))
        {
            return MakeError(new Exception("missing dot in first path element"));
        }
        if (path[0] == '-')
        {
            return MakeError(new Exception("leading dash in first path element"));
        }
        for (int j = 0; j < i; j++)
        {
            if (!ModuleInternal.FirstPathOK(path[j]))
            {
                return new InvalidPathError("module", path, new Exception($"invalid char '{path[j]}' in first path element"));
            }
        }
        var split = SplitPathVersion(path);
        if (!split.Item3)
        {
            return new InvalidPathError("module", path, new Exception("invalid version"));
        }
        return null;
    }

    public static Exception CheckPathMajor(string v, string pathMajor)
    {
        throw new NotImplementedException();
    }

    public static (string, Exception?) EscapePath(string path)
    {
        var err = CheckPath(path);
        if (err != null)
        {
            return (string.Empty, err);
        }

        return ModuleInternal.EscapeString(path);
    }

    public static (string, Exception?) EscapeVersion(string v)
    {
        var err = ModuleInternal.CheckElem(v, ModuleInternal.PathKind.FilePath);
        if (err != null || v.Contains('!'))
        {
            return (
                string.Empty,
                new InvalidVersionError(v, false, new Exception("disallowed version string"))
            );
        }
        return ModuleInternal.EscapeString(v);
    }

    public static bool IsPseudoVersion(string v)
    {
        throw new NotImplementedException();
    }

    public static bool IsZeroPseudoVersion(string v)
    {
        throw new NotImplementedException();
    }

    public static bool MatchPathMajor(string v, string pathMajor)
    {
        throw new NotImplementedException();
    }

    public static bool MatchPrefixPatterns(string globs, string target)
    {
        throw new NotImplementedException();
    }

    public static string PathMajorPrefix(string pathMajor)
    {
        throw new NotImplementedException();
    }

    public static string PseudoVersion(string major, string older, DateTime t, string rev)
    {
        throw new NotImplementedException();
    }

    public static (string, Exception) PseudoVersionBase(string v)
    {
        throw new NotImplementedException();
    }

    public static (string, Exception) PseudoVersionRev(string v)
    {
        throw new NotImplementedException();
    }

    public static (DateTime, Exception) PseudoVersionTime(string v)
    {
        throw new NotImplementedException();
    }

    public static void Sort(Version[] list)
    {
        throw new NotImplementedException();
    }

    public static (string, string, bool) SplitPathVersion(string path)
    {
        if (path.StartsWith("gopkg.in/"))
        {
            return ModuleInternal.SplitGopkgIn(path);
        }

        int i = path.Length;
        bool dot = false;
        while (i > 0 && ((path[i - 1] >= '0' && path[i - 1] <= '9') || path[i - 1] == '.'))
        {
            if (path[i - 1] == '.')
            {
                dot = true;
            }
            i--;
        }
        if (i <= 1 || i == path.Length || path[i - 1] != 'v' || path[i - 2] != '/')
        {
            return (path, "", true);
        }
        string prefix = path[..(i - 2)];
        string pathMajor = path[(i - 2)..];
        if (dot || pathMajor.Length <= 2 || pathMajor[2] == '0' || pathMajor == "/v1")
        {
            return (path, "", false);
        }
        return (prefix, pathMajor, true);
    }

    public static (string, Exception) UnescapePath(string escaped)
    {
        throw new NotImplementedException();
    }

    public static (string, Exception) UnescapeVersion(string escaped)
    {
        throw new NotImplementedException();
    }

    public static Exception VersionError(Version v, Exception err)
    {
        throw new NotImplementedException();
    }

    public static string ZeroPseudoVersion(string major)
    {
        throw new NotImplementedException();
    }
}

file static class ModuleInternal
{
    public enum PathKind
    {
        ModulePath,
        ImportPath,
        FilePath,
    }

    private static readonly string[] _badWindowsNames = [
        "CON",
        "PRN",
        "AUX",
        "NUL",
        "COM1",
        "COM2",
        "COM3",
        "COM4",
        "COM5",
        "COM6",
        "COM7",
        "COM8",
        "COM9",
        "LPT1",
        "LPT2",
        "LPT3",
        "LPT4",
        "LPT5",
        "LPT6",
        "LPT7",
        "LPT8",
        "LPT9",
    ];

    public static Exception? CheckElem(string elem, PathKind kind)
    {
        if (string.IsNullOrEmpty(elem))
        {
            return new Exception("empty path element");
        }

        if (elem.Count(c => c == '.') == elem.Length)
        {
            return new Exception($"invalid path element \"{elem}\"");
        }

        if (elem[0] == '.' && kind == PathKind.ModulePath)
        {
            return new Exception("leading dot in path element");
        }

        if (elem[^1] == '.')
        {
            return new Exception("trailing dot in path element");
        }

        foreach (char r in elem)
        {
            bool ok = false;
            ok = kind switch
            {
                PathKind.ModulePath => (bool)ModPathOK(r),
                PathKind.ImportPath => (bool)ImportPathOK(r),
                PathKind.FilePath => (bool)FileNameOK(r),
                _ => throw new Exception($"internal error: invalid kind {kind}"),
            };
            if (!ok)
            {
                return new Exception($"invalid char \"{r}\"");
            }
        }

        string shortElem = elem;
        int dotIndex = shortElem.IndexOf('.');
        if (dotIndex >= 0)
        {
            shortElem = shortElem.Substring(0, dotIndex);
        }

        foreach (string bad in _badWindowsNames)
        {
            if (string.Equals(bad, shortElem, StringComparison.OrdinalIgnoreCase))
            {
                return new Exception($"\"{shortElem}\" disallowed as path element component on Windows");
            }
        }

        if (kind == PathKind.FilePath)
        {
            return null;
        }

        int tilde = shortElem.LastIndexOf('~');
        if (tilde >= 0 && tilde < shortElem.Length - 1)
        {
            string suffix = shortElem.Substring(tilde + 1);
            bool suffixIsDigits = true;
            foreach (char c in suffix)
            {
                if (c < '0' || c > '9')
                {
                    suffixIsDigits = false;
                    break;
                }
            }
            if (suffixIsDigits)
            {
                return new Exception("trailing tilde and digits in path element");
            }
        }
        return null;
    }

    public static Exception? CheckPath(string path, PathKind kind)
    {
        if (!IsValidUtf8(path))
        {
            return new Exception("invalid UTF-8");
        }
        if (path == "")
        {
            return new Exception("empty string");
        }
        if (path[0] == '-' && kind != PathKind.FilePath)
        {
            return new Exception("leading dash");
        }
        if (path.Contains("//"))
        {
            return new Exception("double slash");
        }
        if (path[^1] == '/')
        {
            return new Exception("trailing slash");
        }
        int elemStart = 0;
        for (int i = 0; i < path.Length; i++)
        {
            if (path[i] == '/')
            {
                Exception? err = CheckElem(path.Substring(elemStart, i - elemStart), kind);
                if (err != null)
                {
                    return err;
                }
                elemStart = i + 1;
            }
        }
        Exception? lastErr = CheckElem(path.Substring(elemStart), kind);
        if (lastErr != null)
        {
            return lastErr;
        }
        return null;

        static bool IsValidUtf8(string s)
        {
            // In .NET, strings are valid Unicode.
            // This stub simulates a UTF-8 validity check.
            try
            {
                Encoding.UTF8.GetBytes(s);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public static (string, Exception?) EscapeString(string s)
    {
        {
            bool haveUpper = false;
            foreach (char r in s)
            {
                if (r == '!' || r >= 0x80)
                {
                    return (string.Empty, new Exception("internal error: inconsistency in EscapePath"));
                }
                if (r >= 'A' && r <= 'Z')
                {
                    haveUpper = true;
                }
            }

            if (!haveUpper)
            {
                return (s, null);
            }

            var sb = new StringBuilder();
            foreach (char r in s)
            {
                if (r >= 'A' && r <= 'Z')
                {
                    sb.Append('!');
                    sb.Append(char.ToLowerInvariant(r));
                }
                else
                {
                    sb.Append(r);
                }
            }
            return (sb.ToString(), null);
        }
    }

    public static bool FileNameOK(char r)
    {
        if (r < 0x80)
        {
            const string allowed = "!#$%&()+,-.=@[]^_{}~ ";
            if ('0' <= r && r <= '9' || 'A' <= r && r <= 'Z' || 'a' <= r && r <= 'z')
            {
                return true;
            }
            return allowed.Contains(r);
        }
        return char.IsLetter(r);
    }

    public static bool FirstPathOK(char r)
    {
        return r == '-'
               || r == '.'
               || '0' <= r
               && r <= '9'
               || 'a' <= r
               && r <= 'z';
    }

    public static bool ImportPathOK(char r)
    {
        return ModPathOK(r) || r == '+';
    }

    public static bool ModPathOK(char r)
    {
        if (r < 0x80)
        {
            return r == '-'
                   || r == '.'
                   || r == '_'
                   || r == '~'
                   || '0' <= r
                   && r <= '9'
                   || 'A' <= r
                   && r <= 'Z'
                   || 'a' <= r
                   && r <= 'z';
        }
        return false;
    }

    public static (string, string, bool) SplitGopkgIn(string path)
    {
        if (!path.StartsWith("gopkg.in/"))
        {
            return (path, "", false);
        }
        int i = path.Length;
        string unstable = "-unstable";
        if (path.EndsWith(unstable))
        {
            i -= unstable.Length;
        }
        while (i > 0 && char.IsDigit(path[i - 1]))
        {
            i--;
        }
        if (i <= 1 || path[i - 1] != 'v' || path[i - 2] != '.')
        {
            return (path, "", false);
        }
        string prefix = path.Substring(0, i - 2);
        string pathMajor = path.Substring(i - 2);
        if (pathMajor.Length <= 2 || (pathMajor[2] == '0' && pathMajor != ".v0"))
        {
            return (path, "", false);
        }
        return (prefix, pathMajor, true);
    }
}