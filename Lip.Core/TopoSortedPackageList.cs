using Semver;

namespace Lip.Core;

/// <summary>
/// Represents a list of packages that are topologically sorted, i.e. each package is guaranteed to be listed before
/// its dependencies.
/// </summary>
public class TopoSortedPackageList<T> : List<T> where T : TopoSortedPackageList<T>.IItem
{
    public interface IItem
    {
        public Dictionary<PackageIdentifier, SemVersionRange> Dependencies { get; }
        public PackageSpecifier Specifier { get; }
    }

    public new T this[int index]
    {
        get => base[index];
        set
        {
            base[index] = value;
            TopoSort();
        }
    }

    public TopoSortedPackageList() : base()
    {
    }

    public TopoSortedPackageList(IEnumerable<T> collection) : base(collection)
    {
        TopoSort();
    }

    public new void Add(T item)
    {
        base.Add(item);
        TopoSort();
    }

    public new void AddRange(IEnumerable<T> collection)
    {
        base.AddRange(collection);
        TopoSort();
    }

    public new void Insert(int index, T item) => throw new NotSupportedException();

    public new void InsertRange(int index, IEnumerable<T> collection) => throw new NotSupportedException();

    public new void Reverse() => throw new NotSupportedException();

    public new void Reverse(int index, int count) => throw new NotSupportedException();

    public new void Sort() => throw new NotSupportedException();

    public new void Sort(IComparer<T>? comparer) => throw new NotSupportedException();

    public new void Sort(int index, int count, IComparer<T>? comparer)
        => throw new NotSupportedException();

    public new void Sort(Comparison<T> comparison) => throw new NotSupportedException();

    private void TopoSort()
    {
        if (Count == 0) return;

        // Snapshot current items
        var items = new List<T>(this);

        // Create a lookup for quick access to items in the list
        // First, validate that there are no duplicate package identifiers to avoid an opaque ToDictionary crash.
        var duplicateGroups = items
            .GroupBy(i => i.Specifier.Identifier)
            .Where(g => g.Skip(1).Any())
            .ToList();
        if (duplicateGroups.Count > 0)
        {
            var duplicateIds = string.Join(", ", duplicateGroups.Select(g => g.Key.ToString()));
            throw new InvalidOperationException(
                $"TopoSortedPackageList contains multiple items with the same PackageIdentifier: {duplicateIds}.");
        }

        var itemMap = items.ToDictionary(i => i.Specifier.Identifier);

        var visited = new HashSet<PackageIdentifier>();
        var visiting = new HashSet<PackageIdentifier>();
        var sorted = new List<T>(Count);

        void Visit(T item)
        {
            var id = item.Specifier.Identifier;
            if (visited.Contains(id)) return;

            if (visiting.Contains(id))
            {
                // Cycle detected. 
                // We treat the cyclic dependency as "resolved" to break the infinite loop 
                // and proceed with partial ordering.
                return;
            }

            visiting.Add(id);

            // Visit dependencies first
            foreach (var dep in item.Dependencies)
            {
                if (itemMap.TryGetValue(dep.Key, out var depItem) &&
                    dep.Value.Contains(depItem.Specifier.Version))
                {
                    Visit(depItem);
                }
            }

            visiting.Remove(id);
            visited.Add(id);
            sorted.Add(item);
        }

        foreach (var item in items)
        {
            Visit(item);
        }

        // The DFS post-order traversal gives us [Leaf, ..., Root] (Dependency, ..., Dependent).
        // The requirement is "package... listed BEFORE its dependencies".
        // So we need [Root, ..., Leaf] -> Reverse the list.
        sorted.Reverse();

        Clear();
        base.AddRange(sorted);
    }
}