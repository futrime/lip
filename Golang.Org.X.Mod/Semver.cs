namespace Golang.Org.X.Mod;

public static class Semver
{
    public static string Build(string v)
    {
        var (pv, ok) = SemverInternal.Parse(v);
        if (!ok)
        {
            return string.Empty;
        }
        return pv.Build;
    }

    public static string Canonical(string v)
    {
        var (p, ok) = SemverInternal.Parse(v);
        if (!ok)
        {
            return string.Empty;
        }
        if (!string.IsNullOrEmpty(p.Build))
        {
            return v[..^p.Build.Length];
        }
        if (!string.IsNullOrEmpty(p.Short))
        {
            return v + p.Short;
        }
        return v;
    }

    public static int Compare(string v, string w)
    {
        throw new NotImplementedException();
    }

    public static bool IsValid(string v)
    {
        throw new NotImplementedException();
    }

    public static string Major(string v)
    {
        throw new NotImplementedException();
    }

    public static string MajorMinor(string v)
    {
        throw new NotImplementedException();
    }

    [Obsolete("Deprecated")]
    public static string Max(string v, string w)
    {
        throw new NotImplementedException();
    }

    public static string Prerelease(string v)
    {
        throw new NotImplementedException();
    }

    public static void Sort(string[] list)
    {
        throw new NotImplementedException();
    }
}

file static class SemverInternal
{
    public static bool IsBadNum(string v)
    {
        int i = 0;
        while (i < v.Length && v[i] >= '0' && v[i] <= '9')
        {
            i++;
        }
        return i == v.Length && i > 1 && v[0] == '0';
    }

    public static bool IsIdentChar(char c)
    {
        return 'A' <= c
               && c <= 'Z'
               || 'a' <= c
               && c <= 'z'
               || '0' <= c
               && c <= '9'
               || c == '-';
    }

    public static (Parsed, bool) Parse(string v)
    {
        if (string.IsNullOrEmpty(v) || v[0] != 'v')
        {
            return (default, false);
        }

        var (major, rest, ok) = ParseInt(v[1..]);
        if (!ok)
        {
            return (default, false);
        }

        var p = new Parsed { Major = major };

        if (string.IsNullOrEmpty(rest))
        {
            p.Minor = "0";
            p.Patch = "0";
            p.Short = ".0.0";
            return (p, true);
        }

        if (rest[0] != '.')
        {
            return (default, false);
        }

        var (minor, rest2, ok2) = ParseInt(rest[1..]);
        if (!ok2)
        {
            return (default, false);
        }

        p.Minor = minor;

        if (string.IsNullOrEmpty(rest2))
        {
            p.Patch = "0";
            p.Short = ".0";
            return (p, true);
        }

        if (rest2[0] != '.')
        {
            return (default, false);
        }

        var (patch, rest3, ok3) = ParseInt(rest2[1..]);
        if (!ok3)
        {
            return (default, false);
        }

        p.Patch = patch;

        if (rest3.Length > 0 && rest3[0] == '-')
        {
            var (prerelease, rest4, ok4) = ParsePrerelease(rest3);
            if (!ok4)
            {
                return (default, false);
            }
            p.Prerelease = prerelease;
            rest3 = rest4;
        }

        if (rest3.Length > 0 && rest3[0] == '+')
        {
            var (build, rest5, ok5) = ParseBuild(rest3);
            if (!ok5)
            {
                return (default, false);
            }
            p.Build = build;
            rest3 = rest5;
        }

        if (rest3 != string.Empty)
        {
            return (default, false);
        }

        return (p, true);
    }

    public static (string t, string rest, bool ok) ParseBuild(string v)
    {
        if (string.IsNullOrEmpty(v) || v[0] != '+')
        {
            return (string.Empty, string.Empty, false);
        }

        int i = 1;
        int start = 1;
        while (i < v.Length)
        {
            if (!IsIdentChar(v[i]) && v[i] != '.')
            {
                return (string.Empty, string.Empty, false);
            }
            if (v[i] == '.')
            {
                if (start == i)
                {
                    return (string.Empty, string.Empty, false);
                }
                start = i + 1;
            }
            i++;
        }

        if (start == i)
        {
            return (string.Empty, string.Empty, false);
        }

        return (v[..i], v[i..], true);
    }

    public static (string t, string rest, bool ok) ParseInt(string v)
    {
        if (string.IsNullOrEmpty(v))
            return (string.Empty, string.Empty, false);

        if (v[0] < '0' || v[0] > '9')
            return (string.Empty, string.Empty, false);

        int i = 1;
        while (i < v.Length && v[i] >= '0' && v[i] <= '9')
        {
            i++;
        }

        if (v[0] == '0' && i != 1)
            return (string.Empty, string.Empty, false);

        return (v[..i], v[i..], true);
    }

    public static (string t, string rest, bool ok) ParsePrerelease(string v)
    {
        if (string.IsNullOrEmpty(v) || v[0] != '-')
        {
            return (string.Empty, string.Empty, false);
        }

        int i = 1;
        int start = 1;
        while (i < v.Length && v[i] != '+')
        {
            if (!IsIdentChar(v[i]) && v[i] != '.')
            {
                return (string.Empty, string.Empty, false);
            }
            if (v[i] == '.')
            {
                if (start == i || IsBadNum(v[start..i]))
                {
                    return (string.Empty, string.Empty, false);
                }
                start = i + 1;
            }
            i++;
        }

        if (start == i || IsBadNum(v[start..i]))
        {
            return (string.Empty, string.Empty, false);
        }

        return (v[..i], v[i..], true);
    }
}

file struct Parsed()
{
    public string Major = string.Empty;
    public string Minor = string.Empty;
    public string Patch = string.Empty;
    public string Short = string.Empty;
    public string Prerelease = string.Empty;
    public string Build = string.Empty;
}