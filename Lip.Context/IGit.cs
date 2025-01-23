namespace Lip.Context;

public interface IGit
{
    Task Clone(string repository, string directory, string? branch = null, int? depth = null);
}
