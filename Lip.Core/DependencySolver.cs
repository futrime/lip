using Algorithms.Graphs;
using DataStructures.Graphs;
using Semver;
using SharpCompress;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Lip.Core;

public interface IDependencySolver
{
    Task<List<PackageIdentifier>> GetUnnecessaryPackages();
    Task<List<PackageSpecifier>?> ResolveDependencies(
        List<PackageSpecifier> primaryPackageSpecifiers,
        List<PackageSpecifier> installedPackageSpecifiers,
        List<PackageLock.Package> knownPackages);
}

public class DependencySolver(IPackageManager packageManager) : IDependencySolver
{
    private readonly IPackageManager _packageManager = packageManager;

    public async Task<List<PackageIdentifier>> GetUnnecessaryPackages()
    {
        PackageLock currentPackageLock = await _packageManager.GetCurrentPackageLock();

        List<VertexForGetUnnecessaryPackages> vertices = [.. currentPackageLock.Packages.Select(package => new VertexForGetUnnecessaryPackages
        {
            Package = package
        })];

        DirectedSparseGraph<VertexForGetUnnecessaryPackages> dependencyGraph = new();

        dependencyGraph.AddVertices(vertices);

        // Add edges.
        foreach (VertexForGetUnnecessaryPackages vertex in vertices)
        {
            vertex.Package.Variant.Dependencies
                .Select(kvp => kvp.Key)
                .Select(identifier => vertices.FirstOrDefault(
                    v => v.Package.Specifier.Identifier == identifier))
                .Where(dep => dep != null)
                .ForEach(dep => dependencyGraph.AddEdge(vertex, dep!));
        }

        // Find unnecessary packages.

        var sourceList = vertices.Where(v => v.Package.Locked).ToList();

        // If no locked packages, all packages are unnecessary.
        if (sourceList.Count == 0)
        {
            return [.. vertices.Select(v => v.Package.Specifier.Identifier)];
        }

        BreadthFirstShortestPaths<VertexForGetUnnecessaryPackages> bfs = new(
            dependencyGraph,
            Sources: sourceList);

        List<PackageIdentifier> unnecessaryPackages = [.. vertices
            .Where(v => !v.Package.Locked)
            .Where(v => !bfs.HasPathTo(v))
            .Select(v => v.Package.Specifier.Identifier)];

        return unnecessaryPackages;
    }

    public async Task<List<PackageSpecifier>?> ResolveDependencies(
        List<PackageSpecifier> primaryPackageSpecifiers,
        List<PackageSpecifier> installedPackageSpecifiers,
        List<PackageLock.Package> knownPackages)
    {
        await Task.Delay(0); // Suppress warning.

        StateForResolveDependencies initialState = new()
        {
            Candidates = primaryPackageSpecifiers.ToDictionary(
                packageSpecifier => packageSpecifier.Identifier,
                packageSpecifier => new List<SemVersion> { packageSpecifier.Version }),
            NextSelection = null,
            Selected = []
        };

        Dictionary<PackageIdentifier, SemVersion> installedPackages = installedPackageSpecifiers
            .ToDictionary(
                packageSpecifier => packageSpecifier.Identifier,
                packageSpecifier => packageSpecifier.Version);

        Stack<StateForResolveDependencies> stack = new();
        stack.Push(initialState);

        while (stack.Count > 0)
        {
            StateForResolveDependencies currentState = stack.Pop();

            if (!await currentState.ResolveSelection(_packageManager, knownPackages))
            {
                continue;
            }

            if (currentState.Candidates.Count == 0)
            {
                return [.. currentState.Selected.Select(kvp => PackageSpecifier.FromIdentifier(kvp.Key, kvp.Value))];
            }

            List<PackageSpecifier> preferredPackages = [.. installedPackages.Select(kvp => PackageSpecifier.FromIdentifier(kvp.Key, kvp.Value))];

            List<StateForResolveDependencies> neighbors = currentState.GetNeighbors(preferredPackages);

            // Reversely push the neighbors to the stack to explore the preferred packages first.
            neighbors.Reverse();

            neighbors.ForEach(stack.Push);
        }

        throw new InvalidOperationException("Failed to resolve dependencies.");
    }
}

[ExcludeFromCodeCoverage]
file record StateForResolveDependencies
{
    public required Dictionary<PackageIdentifier, List<SemVersion>> Candidates { get; set; }

    public required Tuple<PackageIdentifier, SemVersion>? NextSelection { get; set; }

    public required Dictionary<PackageIdentifier, SemVersion> Selected { get; set; }

    /// <summary>
    /// Gets the neighbors of the state by selecting the candidate with least versions to
    /// explore. The versions are sorted by the preferred packages and then by the version
    /// precedence.
    /// </summary>
    /// <param name="preferredPackages">The preferred packages to explore first.</param>
    /// <returns>The neighbors of the state which are not normalized.</returns>
    public List<StateForResolveDependencies> GetNeighbors(List<PackageSpecifier> preferredPackages)
    {
        if (Candidates.Count == 0)
        {
            return [];
        }

        if (Candidates.Any(kvp => kvp.Value.Count == 0))
        {
            throw new InvalidOperationException("There are candidates with no versions to explore.");
        }

        KeyValuePair<PackageIdentifier, List<SemVersion>> candidateToExplore = Candidates.MinBy(kvp => kvp.Value.Count);
        IOrderedEnumerable<SemVersion> candidateVersionsToExplorer = candidateToExplore.Value
            .OrderByDescending(v => preferredPackages.Contains(PackageSpecifier.FromIdentifier(
                candidateToExplore.Key,
                v)))
            .ThenByDescending(v => v, SemVersion.PrecedenceComparer);

        return [.. candidateVersionsToExplorer.Select(version =>
        {
            Dictionary<PackageIdentifier, List<SemVersion>> newCandidates = Candidates
                .Where(kvp => kvp.Key != candidateToExplore.Key)
                .ToDictionary();

            return this with
            {
                Candidates = newCandidates,
                NextSelection = new Tuple<PackageIdentifier, SemVersion>(candidateToExplore.Key, version),
                Selected = Selected.ToDictionary(), // Clone the selected dictionary.
            };
        })];
    }

    /// <summary>
    /// Resolve the selection.
    /// </summary>
    /// <param name="packageManager"></param>
    /// <param name="knownPackages"></param>
    /// <returns>True if the state is valid. False otherwise.</returns>
    public async Task<bool> ResolveSelection(IPackageManager packageManager, List<PackageLock.Package> knownPackages)
    {
        if (Candidates.Any(kvp => Selected.ContainsKey(kvp.Key)))
        {
            return false;
        }

        if (NextSelection == null)
        {
            return true;
        }

        if (Candidates.ContainsKey(NextSelection.Item1)
            || Selected.ContainsKey(NextSelection.Item1))
        {
            return false;
        }

        PackageSpecifier specifier = PackageSpecifier.FromIdentifier(NextSelection.Item1, NextSelection.Item2);

        Dictionary<PackageIdentifier, SemVersionRange> dependenciesInRange = await GetDependencies(specifier, packageManager, knownPackages);

        Dictionary<PackageIdentifier, List<SemVersion>> dependencyCandidates = (await Task.WhenAll(
            dependenciesInRange.Select(async dep =>
                {
                    List<SemVersion> remoteVersions = await packageManager.GetPackageRemoteVersions(dep.Key);
                    List<SemVersion> validVersions = [.. remoteVersions.Where(v => dep.Value.Contains(v))];
                    return new KeyValuePair<PackageIdentifier, List<SemVersion>>(dep.Key, validVersions);
                })
        )).ToDictionary();

        Dictionary<PackageIdentifier, List<SemVersion>> newCandidates = Candidates
            .Where(kvp => kvp.Key != NextSelection.Item1)
            .Concat(dependencyCandidates)
            .GroupBy(kvp => kvp.Key)
            .ToDictionary(
                g => g.Key,
                g => g.Count() > 1
                    ? [.. g.First().Value.Intersect(g.Last().Value)]
                    : g.First().Value // Get the intersection of the versions.
            );

        if (newCandidates.Any(kvp => kvp.Value.Count == 0))
        {
            return false;
        }

        Candidates = newCandidates;

        Selected.Add(NextSelection.Item1, NextSelection.Item2);

        NextSelection = null;

        return true;
    }

    private static async Task<Dictionary<PackageIdentifier, SemVersionRange>> GetDependencies(
        PackageSpecifier packageSpecifier,
        IPackageManager packageManager,
        List<PackageLock.Package> knownPackages
    )
    {
        // To support installing packages not in cache.
        if (knownPackages.Any(p => p.Specifier == packageSpecifier))
        {
            return knownPackages
                .First(p => p.Specifier == packageSpecifier)
                .Variant.Dependencies;
        }

        PackageManifest manifest = await packageManager.GetPackageManifestFromCache(packageSpecifier)
            ?? throw new InvalidOperationException($"Failed to get package manifest for {packageSpecifier}.");

        PackageManifest.Variant variant = manifest.GetVariant(packageSpecifier.VariantLabel, RuntimeInformation.RuntimeIdentifier)
            ?? throw new InvalidOperationException($"Variant {packageSpecifier.VariantLabel} not found for {packageSpecifier}.");

        return variant.Dependencies;
    }
}

[ExcludeFromCodeCoverage]
file record VertexForGetUnnecessaryPackages : IComparable<VertexForGetUnnecessaryPackages>
{
    public required PackageLock.Package Package { get; init; }

    // C-Sharp-Algorithms requires this method to be implemented but we don't know why.
    public int CompareTo(VertexForGetUnnecessaryPackages? other) => throw new NotImplementedException();
}
