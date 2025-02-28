using Algorithms.Graphs;
using DataStructures.Graphs;
using DataStructures.Lists;
using Semver;
using SharpCompress;
using System.Runtime.InteropServices;

namespace Lip;

public interface IDependencySolver
{
    Task<List<PackageIdentifier>> GetUnnecessaryPackages();
    Task<List<PackageSpecifier>?> ResolveDependencies(
        List<PackageSpecifier> primaryPackageSpecifiers,
        List<PackageSpecifier> installedPackageSpecifiers);
}

public class DependencySolver(IPackageManager packageManager) : IDependencySolver
{
    private readonly IPackageManager _packageManager = packageManager;

    public async Task<List<PackageIdentifier>> GetUnnecessaryPackages()
    {
        PackageLock currentPackageLock = await _packageManager.GetCurrentPackageLock();

        List<LockTypeVertex> vertices = [.. currentPackageLock.Locks.Cast<LockTypeVertex>()];

        DirectedSparseGraph<LockTypeVertex> dependencyGraph = new();

        dependencyGraph.AddVertices(vertices);

        // Add edges.
        foreach (LockTypeVertex vertex in vertices)
        {
            vertex.Manifest.GetVariant(
                vertex.VariantLabel,
                RuntimeInformation.RuntimeIdentifier)?
                .Dependencies?
                .Select(kvp => kvp.Key)
                .Select(packageSpecifier => vertices.FirstOrDefault(v => v.Specifier.Identifier == packageSpecifier))
                .Where(dep => dep != null)
                .ForEach(dep => dependencyGraph.AddEdge(vertex, dep!));
        }

        // Find unnecessary packages.
        List<PackageIdentifier> unnecessaryPackages = [.. ConnectedComponents.Compute(dependencyGraph)
            .Where(component => !component.Any(v => v.Locked))
            .SelectMany(component => component)
            .Select(v => new PackageIdentifier{
                ToothPath = v.Manifest.ToothPath,
                VariantLabel = v.VariantLabel
            })];

        return unnecessaryPackages;
    }

    public async Task<List<PackageSpecifier>?> ResolveDependencies(
        List<PackageSpecifier> primaryPackageSpecifiers,
        List<PackageSpecifier> installedPackageSpecifiers)
    {
        await Task.Delay(0); // Suppress warning.

        PackageDependencyGraph.Vertex initialState = new()
        {
            Candidates = primaryPackageSpecifiers.ToDictionary(
                packageSpecifier => packageSpecifier.Identifier,
                packageSpecifier => new List<SemVersion> { packageSpecifier.Version }),
            Selected = []
        };

        Dictionary<PackageIdentifier, SemVersion> installedPackages = installedPackageSpecifiers
            .ToDictionary(
                packageSpecifier => packageSpecifier.Identifier,
                packageSpecifier => packageSpecifier.Version);

        PackageDependencyGraph graph = new(_packageManager, installedPackages);

        graph.AddVertex(initialState);

        try
        {
            PackageDependencyGraph.Vertex matchedState = DepthFirstSearcher.FindFirstMatch(
                graph,
                initialState,
                vertex => vertex.Candidates.Count == 0);

            return [.. matchedState.Selected.Select(kvp => PackageSpecifier.FromIdentifier(kvp.Key, kvp.Value))];

        }
        catch (Exception e) when (e.Message == "Item was not found!")
        {
            return null;
        }
    }
}

file record LockTypeVertex : PackageLock.Package, IComparable<LockTypeVertex>
{
    // C-Sharp-Algorithms requires this method to be implemented but we don't know why.
    public int CompareTo(LockTypeVertex? other) => throw new NotImplementedException();
}

file class PackageDependencyGraph(
    IPackageManager packageManager,
    Dictionary<PackageIdentifier, SemVersion> installedPackages)
    : DirectedSparseGraph<PackageDependencyGraph.Vertex>
{
    public class Vertex : IComparable<Vertex>
    {
        public required Dictionary<PackageIdentifier, List<SemVersion>> Candidates { get; init; }
        public required Dictionary<PackageIdentifier, SemVersion> Selected { get; init; }

        public int CompareTo(Vertex? other) => throw new NotImplementedException();

        public override bool Equals(object? obj)
        {
            return obj is Vertex other
                && Candidates.Count == other.Candidates.Count
                && Selected.Count == other.Selected.Count
                && Candidates.All(c => other.Candidates.TryGetValue(c.Key, out var val)
                    && c.Value.OrderBy(v => v).SequenceEqual(val.OrderBy(v => v)))
                && Selected.All(s => other.Selected.TryGetValue(s.Key, out var val)
                    && s.Value == val);
        }

        public override int GetHashCode()
        {
            HashCode hash = new();

            foreach (var candidate in Candidates.OrderBy(c => c.Key))
            {
                hash.Add(candidate.Key);
                foreach (var version in candidate.Value.OrderBy(v => v))
                {
                    hash.Add(version);
                }
            }

            foreach (var selected in Selected.OrderBy(s => s.Key))
            {
                hash.Add(selected.Key);
                hash.Add(selected.Value);
            }

            return hash.ToHashCode();
        }

        /// <summary>
        /// Normalize the vertex by removing the candidates that are already selected. If the
        /// vertex is invalid, i.e. there are candidates that have no versions to explore, or
        /// there are candidates that does not contain the selected version, return null.
        /// </summary>
        /// <returns>The normalized vertex or null if the vertex is invalid.</returns>
        public Vertex? Normalized()
        {
            // If there are candidates that have no versions to explore, return null.
            if (Candidates.Any(kvp => kvp.Value.Count == 0))
            {
                return null;
            }

            // If there are candidates selected but not containing the selected version, return
            // null.
            if (Candidates.Any(kvp => Selected.ContainsKey(kvp.Key) && !kvp.Value.Contains(Selected[kvp.Key])))
            {
                return null;
            }

            // Remove the selected versions from the candidates.
            return new Vertex
            {
                Candidates = Candidates
                    .Where(kvp => !Selected.ContainsKey(kvp.Key))
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value
                    ),
                Selected = Selected
            };
        }
    }

    private readonly Dictionary<PackageIdentifier, SemVersion> _installedPackages = installedPackages;
    private readonly IPackageManager _packageManager = packageManager;

    public override DLinkedList<Vertex> Neighbours(Vertex vertex)
    {
        if (!HasVertex(vertex) || vertex.Candidates.Count == 0)
        {
            return base.Neighbours(vertex);
        }

        // Select the candidate with least versions and get sorted versions. The installed
        // version is always preferred.
        KeyValuePair<PackageIdentifier, List<SemVersion>> packageCandidate = vertex.Candidates.MinBy(kvp => kvp.Value.Count);
        IOrderedEnumerable<SemVersion> versionCandidates = packageCandidate.Value
            .OrderByDescending(v => _installedPackages.TryGetValue(packageCandidate.Key, out var installedVersion) && v == installedVersion)
            .ThenBy(v => v, SemVersion.PrecedenceComparer);

        // Process each version candidate
        List<Vertex> neighbors = [.. Task.WhenAll(versionCandidates.Select(async version =>
        {
            PackageSpecifier packageSpecifier = PackageSpecifier.FromIdentifier(packageCandidate.Key, version);
            PackageManifest? packageManifest = await _packageManager.GetPackageManifestFromCache(packageSpecifier);

            Dictionary<PackageIdentifier, SemVersionRange> dependencies = packageManifest?
                .GetVariant(packageCandidate.Key.VariantLabel, RuntimeInformation.RuntimeIdentifier)?
                .Dependencies
                ?? [];

            KeyValuePair<PackageIdentifier, List<SemVersion>>[] dependencyCandidates = await Task.WhenAll(dependencies.Select(async dep =>
            {
                List<SemVersion> remoteVersions = await _packageManager.GetPackageRemoteVersions(dep.Key);
                List<SemVersion> validVersions = [.. remoteVersions.Where(v => dep.Value.Contains(v))];
                return new KeyValuePair<PackageIdentifier, List<SemVersion>>(dep.Key, validVersions);
            }));

            Dictionary<PackageIdentifier, List<SemVersion>> newCandidates = vertex.Candidates
                .Where(kvp => kvp.Key != packageCandidate.Key)
                .Concat(dependencyCandidates)
                .GroupBy(kvp => kvp.Key)
                .ToDictionary(
                    g => g.Key,
                    g => g.Count() > 1
                        ? [.. g.First().Value.Intersect(g.Last().Value)]
                        : g.First().Value
                );

            Dictionary<PackageIdentifier, SemVersion> newSelected = vertex.Selected
                .Concat([new KeyValuePair<PackageIdentifier, SemVersion>(packageCandidate.Key, version)])
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            Vertex? newVertex = new Vertex
            {
                Candidates = newCandidates,
                Selected = newSelected
            }.Normalized();

            return newVertex;
        })).Result.Where(v => v != null)];

        foreach (Vertex neighbor in neighbors)
        {
            AddVertex(neighbor);
            AddEdge(vertex, neighbor);
        }

        return base.Neighbours(vertex);
    }
}
