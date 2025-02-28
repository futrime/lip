using System.Diagnostics.CodeAnalysis;

namespace Lip.Context;

public interface IGit
{
    [ExcludeFromCodeCoverage]
    record ListRemoteResultItem
    {
        public required string Sha { get; init; }
        public required string Ref { get; init; }
    }

    Task Clone(string repository, string directory, string? branch = null, int? depth = null);

    Task<List<ListRemoteResultItem>> ListRemote(string repository, bool refs = false, bool tags = false);
}
