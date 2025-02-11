namespace Lip;

public partial class Lip
{
    public record ListArgs { }

    public record ListResultItem
    {
        public required PackageManifest Manifest { get; init; }
        public required bool Locked { get; init; }
    }

    public async Task<List<ListResultItem>> List(ListArgs args)
    {
        PackageLock packageLock = await _packageManager.GetCurrentPackageLock();

        List<ListResultItem> listItems = packageLock.Locks
            .ConvertAll(@lock => new ListResultItem
            {
                Manifest = @lock.Package,
                Locked = @lock.Locked
            });

        return listItems;
    }
}
