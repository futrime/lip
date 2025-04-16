using Moq;
using Semver;

namespace Lip.Core.Tests;

public class TopoSortedPackageListTest
{
    public interface IItem : TopoSortedPackageList<IItem>.IItem
    {
    }

    [Fact]
    public void Constructor_NoParameters_CreatesEmptyList()
    {
        // Arrange & act.
        var list = new TopoSortedPackageList<IItem>();

        // Assert.
        Assert.Empty(list);
    }

    [Fact]
    public void Constructor_CollectionWithDependencies_CreatesListWithTopologicalOrder()
    {
        // Arrange.
        var item1 = new Mock<IItem>();
        item1.SetupGet(i => i.Dependencies).Returns(
            new Dictionary<PackageIdentifier, SemVersionRange>()
            {
                [PackageIdentifier.Parse("example.com/pkg2")] = SemVersionRange.Parse("2.0.0"),
            });
        item1.SetupGet(i => i.Specifier).Returns(PackageSpecifier.Parse("example.com/pkg1@1.0.0"));

        var item2 = new Mock<IItem>();
        item2.SetupGet(i => i.Dependencies).Returns(
            new Dictionary<PackageIdentifier, SemVersionRange>()
            {
                [PackageIdentifier.Parse("example.com/pkg3")] = SemVersionRange.Parse("3.0.0"),
            });
        item2.SetupGet(i => i.Specifier).Returns(PackageSpecifier.Parse("example.com/pkg2@2.0.0"));

        var item3 = new Mock<IItem>();
        item3.SetupGet(i => i.Dependencies).Returns([]);
        item3.SetupGet(i => i.Specifier).Returns(PackageSpecifier.Parse("example.com/pkg3@3.0.0"));

        List<IItem> collection = [item1.Object, item2.Object, item3.Object];

        // Act.
        var list = new TopoSortedPackageList<IItem>(collection);

        // Assert.
        Assert.Equal([item3.Object, item2.Object, item1.Object], list);
    }

    [Fact]
    public void IndexerGet_ValidIndex_ReturnsItem()
    {
        // Arrange.
        var item1 = new Mock<IItem>();
        item1.SetupGet(i => i.Dependencies).Returns(
            new Dictionary<PackageIdentifier, SemVersionRange>()
            {
                [PackageIdentifier.Parse("example.com/pkg2")] = SemVersionRange.Parse("2.0.0"),
            });
        item1.SetupGet(i => i.Specifier).Returns(PackageSpecifier.Parse("example.com/pkg1@1.0.0"));

        var item2 = new Mock<IItem>();
        item2.SetupGet(i => i.Dependencies).Returns(
            new Dictionary<PackageIdentifier, SemVersionRange>()
            {
                [PackageIdentifier.Parse("example.com/pkg3")] = SemVersionRange.Parse("3.0.0"),
            });
        item2.SetupGet(i => i.Specifier).Returns(PackageSpecifier.Parse("example.com/pkg2@2.0.0"));

        var item3 = new Mock<IItem>();
        item3.SetupGet(i => i.Dependencies).Returns([]);
        item3.SetupGet(i => i.Specifier).Returns(PackageSpecifier.Parse("example.com/pkg3@3.0.0"));

        var list = new TopoSortedPackageList<IItem>([item1.Object, item2.Object, item3.Object]);

        // Act.
        IItem item = list[0];

        // Assert.
        Assert.Equal(item1.Object, item);
    }

    [Fact]
    public void IndexerSet_ValidIndex_SetsItemAndTopoSorts()
    {
        // Arrange.
        var item1 = new Mock<IItem>();
        item1.SetupGet(i => i.Dependencies).Returns(
            new Dictionary<PackageIdentifier, SemVersionRange>()
            {
                [PackageIdentifier.Parse("example.com/pkg2")] = SemVersionRange.Parse("2.0.0"),
            });
        item1.SetupGet(i => i.Specifier).Returns(PackageSpecifier.Parse("example.com/pkg1@1.0.0"));

        var item2 = new Mock<IItem>();
        item2.SetupGet(i => i.Dependencies).Returns(
            new Dictionary<PackageIdentifier, SemVersionRange>()
            {
                [PackageIdentifier.Parse("example.com/pkg3")] = SemVersionRange.Parse("3.0.0"),
            });
        item2.SetupGet(i => i.Specifier).Returns(PackageSpecifier.Parse("example.com/pkg2@2.0.0"));

        var item3 = new Mock<IItem>();
        item3.SetupGet(i => i.Dependencies).Returns([]);
        item3.SetupGet(i => i.Specifier).Returns(PackageSpecifier.Parse("example.com/pkg3@3.0.0"));

        var list = new TopoSortedPackageList<IItem>([item1.Object, item2.Object, item3.Object]);

        var newItem = new Mock<IItem>();
        newItem.SetupGet(i => i.Dependencies).Returns(new Dictionary<PackageIdentifier, SemVersionRange>()
        {
            [PackageIdentifier.Parse("example.com/pkg1")] = SemVersionRange.Parse("1.0.0"),
        });
        newItem.SetupGet(i => i.Specifier).Returns(PackageSpecifier.Parse("example.com/pkg4@4.0.0"));

        // Act.
        list[2] = newItem.Object;

        // Assert.
        Assert.Equal([newItem.Object, item1.Object, item2.Object], list);
    }

    [Fact]
    public void Add_NewItem_AddsItemAndTopoSorts()
    {
        // Arrange.
        var item1 = new Mock<IItem>();
        item1.SetupGet(i => i.Dependencies).Returns(
            new Dictionary<PackageIdentifier, SemVersionRange>()
            {
                [PackageIdentifier.Parse("example.com/pkg2")] = SemVersionRange.Parse("2.0.0"),
            });
        item1.SetupGet(i => i.Specifier).Returns(PackageSpecifier.Parse("example.com/pkg1@1.0.0"));

        var item2 = new Mock<IItem>();
        item2.SetupGet(i => i.Dependencies).Returns(
            new Dictionary<PackageIdentifier, SemVersionRange>()
            {
                [PackageIdentifier.Parse("example.com/pkg3")] = SemVersionRange.Parse("3.0.0"),
            });
        item2.SetupGet(i => i.Specifier).Returns(PackageSpecifier.Parse("example.com/pkg2@2.0.0"));

        var item3 = new Mock<IItem>();
        item3.SetupGet(i => i.Dependencies).Returns([]);
        item3.SetupGet(i => i.Specifier).Returns(PackageSpecifier.Parse("example.com/pkg3@3.0.0"));

#pragma warning disable IDE0028 // Simplify collection initialization
        var list = new TopoSortedPackageList<IItem>([item1.Object, item3.Object]);

        // Act.
        list.Add(item2.Object);
#pragma warning restore IDE0028 // Simplify collection initialization

        // Assert.
        Assert.Equal([item3.Object, item2.Object, item1.Object], list);
    }

    [Fact]
    public void AddRange_MultipleItems_AddsItemsAndTopoSorts()
    {
        // Arrange.
        var item1 = new Mock<IItem>();
        item1.SetupGet(i => i.Dependencies).Returns(
            new Dictionary<PackageIdentifier, SemVersionRange>()
            {
                [PackageIdentifier.Parse("example.com/pkg2")] = SemVersionRange.Parse("2.0.0"),
            });
        item1.SetupGet(i => i.Specifier).Returns(PackageSpecifier.Parse("example.com/pkg1@1.0.0"));

        var item2 = new Mock<IItem>();
        item2.SetupGet(i => i.Dependencies).Returns(
            new Dictionary<PackageIdentifier, SemVersionRange>()
            {
                [PackageIdentifier.Parse("example.com/pkg3")] = SemVersionRange.Parse("3.0.0"),
            });
        item2.SetupGet(i => i.Specifier).Returns(PackageSpecifier.Parse("example.com/pkg2@2.0.0"));

        var item3 = new Mock<IItem>();
        item3.SetupGet(i => i.Dependencies).Returns([]);
        item3.SetupGet(i => i.Specifier).Returns(PackageSpecifier.Parse("example.com/pkg3@3.0.0"));

        var list = new TopoSortedPackageList<IItem>([item1.Object]);

        // Act.
        list.AddRange([item2.Object, item3.Object]);

        // Assert.
        Assert.Equal([item3.Object, item2.Object, item1.Object], list);
    }

    [Fact]
    public void Insert_ThrowsNotSupportedException()
    {
        // Arrange.
        var list = new TopoSortedPackageList<IItem>();

        // Act & Assert.
        Assert.Throws<NotSupportedException>(() => list.Insert(0, new Mock<IItem>().Object));
    }

    [Fact]
    public void InsertRange_ThrowsNotSupportedException()
    {
        // Arrange.
        var list = new TopoSortedPackageList<IItem>();

        // Act & Assert.
        Assert.Throws<NotSupportedException>(() => list.InsertRange(0, new List<IItem>()));
    }

    [Fact]
    public void Reverse_ThrowsNotSupportedException()
    {
        // Arrange.
        var list = new TopoSortedPackageList<IItem>();

        // Act & Assert.
        Assert.Throws<NotSupportedException>(() => list.Reverse());
    }

    [Fact]
    public void ReverseRange_ThrowsNotSupportedException()
    {
        // Arrange.
        var list = new TopoSortedPackageList<IItem>();

        // Act & Assert.
        Assert.Throws<NotSupportedException>(() => list.Reverse(0, 1));
    }

    [Fact]
    public void Sort_ThrowsNotSupportedException()
    {
        // Arrange.
        var list = new TopoSortedPackageList<IItem>();

        // Act & Assert.
        Assert.Throws<NotSupportedException>(() => list.Sort());
    }

    [Fact]
    public void SortWithComparer_ThrowsNotSupportedException()
    {
        // Arrange.
        var list = new TopoSortedPackageList<IItem>();

        // Act & Assert.
        Assert.Throws<NotSupportedException>(() => list.Sort((IComparer<IItem>?)null));
    }

    [Fact]
    public void SortRange_ThrowsNotSupportedException()
    {
        // Arrange.
        var list = new TopoSortedPackageList<IItem>();

        // Act & Assert.
        Assert.Throws<NotSupportedException>(() => list.Sort(0, 1, null));
    }

    [Fact]
    public void SortWithComparison_ThrowsNotSupportedException()
    {
        // Arrange.
        var list = new TopoSortedPackageList<IItem>();

        // Act & Assert.
        Assert.Throws<NotSupportedException>(() => list.Sort((x, y) => 0));
    }
}