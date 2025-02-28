using Algorithms.Graphs;
using DataStructures.Graphs;
using Semver;
using System.Diagnostics.CodeAnalysis;

namespace Lip;

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

    private class ItemWrapper : IComparable<ItemWrapper>
    {
        public required T Item { get; init; }

        [ExcludeFromCodeCoverage]
        public int CompareTo(ItemWrapper? other) => throw new NotImplementedException();
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
        DirectedSparseGraph<ItemWrapper> dependencyGraph = new();

        dependencyGraph.AddVertices([.. this.Select(item => new ItemWrapper { Item = item })]);

        // Add edges.
        foreach (ItemWrapper vertex in dependencyGraph.Vertices)
        {
            IEnumerable<ItemWrapper> dependencies = vertex.Item.Dependencies
                .Select(dep => dependencyGraph.Vertices.FirstOrDefault(
                    v => dep.Key == v.Item.Specifier.Identifier
                         && dep.Value.Contains(v.Item.Specifier.Version)))
                .Where(dep => dep is not null)!;

            foreach (ItemWrapper dependency in dependencies)
            {
                dependencyGraph.AddEdge(vertex, dependency);
            }
        }

        // Topologically sort the graph.
        IEnumerable<T> sortedElements = TopologicalSorter.Sort(dependencyGraph)
            .Select(wrapper => wrapper.Item);

        // Update the list. Note that the order is reversed.
        Clear();
        base.AddRange(sortedElements);
    }
}
