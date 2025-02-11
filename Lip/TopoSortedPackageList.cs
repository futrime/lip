using System.Runtime.InteropServices;
using Algorithms.Graphs;
using DataStructures.Graphs;

namespace Lip;

/// <summary>
/// Represents a list of packages that are topologically sorted, where dependencies are placed before dependents.
/// </summary>
public class TopoSortedPackageList : List<TopoSortedPackageList.ItemType>
{
    public record ItemType
    {
        public required PackageManifest PackageManifest { get; init; }
        public required string VariantLabel { get; init; }
    }

    public new ItemType this[int index]
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

    public TopoSortedPackageList(IEnumerable<ItemType> collection) : base(collection)
    {
        TopoSort();
    }

    public new void Add(ItemType item)
    {
        base.Add(item);
        TopoSort();
    }

    public new void AddRange(IEnumerable<ItemType> collection)
    {
        base.AddRange(collection);
        TopoSort();
    }

    public new void Insert(int index, ItemType item)
    {
        base.Insert(index, item);
        TopoSort();
    }

    public new void InsertRange(int index, IEnumerable<ItemType> collection)
    {
        base.InsertRange(index, collection);
        TopoSort();
    }

    public new void Reverse() => throw new NotSupportedException();

    public new void Reverse(int index, int count) => throw new NotSupportedException();

    public new void Sort() => throw new NotSupportedException();

    public new void Sort(IComparer<ItemType>? comparer) => throw new NotSupportedException();

    public new void Sort(int index, int count, IComparer<ItemType>? comparer)
        => throw new NotSupportedException();

    public new void Sort(Comparison<ItemType> comparison) => throw new NotSupportedException();

    private void TopoSort()
    {
        DirectedSparseGraph<ComparableItemType> dependencyGraph = new();

        dependencyGraph.AddVertices([.. this.Cast<ComparableItemType>()]);

        // Add edges.
        foreach (ComparableItemType item in dependencyGraph.Vertices)
        {
            IEnumerable<ComparableItemType> dependencies = item.PackageManifest.GetSpecifiedVariant(
                item.VariantLabel,
                RuntimeInformation.RuntimeIdentifier)?
                .Dependencies?
                .Select(dep => PackageSpecifierWithoutVersion.Parse(dep.Key))
                .Select(dep => dependencyGraph.Vertices.FirstOrDefault(v => v.PackageManifest.ToothPath == dep.ToothPath
                                                                            && v.VariantLabel == dep.VariantLabel))
                .Where(dep => dep is not null)
                .Cast<ComparableItemType>() ?? [];

            foreach (ComparableItemType dependency in dependencies)
            {
                dependencyGraph.AddEdge(item, dependency);
            }
        }

        // Topologically sort the graph.
        IEnumerable<ComparableItemType> sortedElements = TopologicalSorter.Sort(dependencyGraph);

        // Update the list. Note that the order is reversed.
        Clear();
        base.AddRange(sortedElements.Reverse());
    }
}

file record ComparableItemType : TopoSortedPackageList.ItemType, IComparable<ComparableItemType>
{
    // C-Sharp-Algorithms requires this method to be implemented but we don't know why.
    public int CompareTo(ComparableItemType? other) => throw new NotImplementedException();
}
