namespace Golang.Org.X.Mod.Tests;

public class SemverTests
{
    public static TheoryData<string, string> Tests => new()
    {
        { "bad", "" },
        { "v1-alpha.beta.gamma", "" },
        { "v1-pre", "" },
        { "v1+meta", "" },
        { "v1-pre+meta", "" },
        { "v1.2-pre", "" },
        { "v1.2+meta", "" },
        { "v1.2-pre+meta", "" },
        { "v1.0.0-alpha", "v1.0.0-alpha" },
        { "v1.0.0-alpha.1", "v1.0.0-alpha.1" },
        { "v1.0.0-alpha.beta", "v1.0.0-alpha.beta" },
        { "v1.0.0-beta", "v1.0.0-beta" },
        { "v1.0.0-beta.2", "v1.0.0-beta.2" },
        { "v1.0.0-beta.11", "v1.0.0-beta.11" },
        { "v1.0.0-rc.1", "v1.0.0-rc.1" },
        { "v1", "v1.0.0" },
        { "v1.0", "v1.0.0" },
        { "v1.0.0", "v1.0.0" },
        { "v1.2", "v1.2.0" },
        { "v1.2.0", "v1.2.0" },
        { "v1.2.3-456", "v1.2.3-456" },
        { "v1.2.3-456.789", "v1.2.3-456.789" },
        { "v1.2.3-456-789", "v1.2.3-456-789" },
        { "v1.2.3-456a", "v1.2.3-456a" },
        { "v1.2.3-pre", "v1.2.3-pre" },
        { "v1.2.3-pre+meta", "v1.2.3-pre" },
        { "v1.2.3-pre.1", "v1.2.3-pre.1" },
        { "v1.2.3-zzz", "v1.2.3-zzz" },
        { "v1.2.3", "v1.2.3" },
        { "v1.2.3+meta", "v1.2.3" },
        { "v1.2.3+meta-pre", "v1.2.3" },
        { "v1.2.3+meta-pre.sha.256a", "v1.2.3" }
    };

    [Theory]
    [MemberData(nameof(Tests))]
    public void Build_ValidInput_ReturnsExpectedOutput(string input, string output)
    {
        // Assert.
        string want = string.Empty;
        if (!string.IsNullOrEmpty(output))
        {
            int index = input.IndexOf('+');
            if (index >= 0)
            {
                want = input[index..];
            }
        }

        // Act.
        string build = Semver.Build(input);

        // Assert.
        Assert.Equal(want, build);
    }

    [Theory]
    [MemberData(nameof(Tests))]
    public void Canonical_ValidInput_ReturnsExpectedOutput(string input, string output)
    {
        // Act.
        string canonical = Semver.Canonical(input);

        // Assert.
        Assert.Equal(output, canonical);
    }
}