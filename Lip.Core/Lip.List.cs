namespace Lip.Core;

public partial class Lip
{
    public record ListArgs { }

    public record ListResultItem
    {
        public required bool Locked { get; init; }
        public required PackageSpecifier Specifier { get; init; }
        public required PackageManifest.Variant Variant { get; init; }
    }

    public async Task<List<ListResultItem>> List(ListArgs args)
    {
        PackageLock packageLock = await _packageManager.GetCurrentPackageLock();

        List<ListResultItem> listItems = packageLock.Packages
            .ConvertAll(@lock => new ListResultItem
            {
                Locked = @lock.Locked,
                Specifier = @lock.Specifier,
                Variant = @lock.Variant
            });

        return listItems;
    }
}
