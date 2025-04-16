using Algorithms.Graphs;
using DataStructures.Graphs;
using Microsoft.Extensions.Logging;
using Semver;
using SharpCompress;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Lip.Core;

public interface IDependencySolver
{
    Task<List<PackageIdentifier>> GetUnnecessaryPackages();
    Task<List<PackageSpecifier>?> ResolveDependencies(
        IEnumerable<PackageSpecifier> primaryPackageSpecifiers,
        IEnumerable<PackageSpecifier> installedPackageSpecifiers,
        IEnumerable<PackageLock.Package> knownPackages);
}

public class DependencySolver(IContext context, IPackageManager packageManager) : IDependencySolver
{
    private readonly IContext _context = context;
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
        IEnumerable<PackageSpecifier> primaryPackageSpecifiers,
        IEnumerable<PackageSpecifier> installedPackageSpecifiers,
        IEnumerable<PackageLock.Package> knownPackages)
    {
        await Task.CompletedTask; // Suppress warning.

        _context.Logger.LogDebug("Resolving dependencies...");

        StateForResolveDependencies initialState = new()
        {
            Candidates = primaryPackageSpecifiers.ToDictionary(
                packageSpecifier => packageSpecifier.Identifier,
                packageSpecifier => new HashSet<SemVersion> { packageSpecifier.Version }),
            NextSelection = null,
            Selected = []
        };

        _context.Logger.LogDebug("Initial candidates:");
        foreach (var kvp in initialState.Candidates)
        {
            _context.Logger.LogDebug("  {Package}@{Versions}", kvp.Key, kvp.Value);
        }

        Stack<StateForResolveDependencies> stack = new();
        stack.Push(initialState);

        while (stack.Count > 0)
        {
            StateForResolveDependencies currentState = stack.Pop();

            if (!await currentState.ResolveSelection(_packageManager, knownPackages))
            {
                continue;
            }

            // If all dependencies are resolved, return the selected packages.
            if (currentState.Candidates.Count == 0)
            {
                return [.. currentState.Selected.Select(kvp => PackageSpecifier.FromIdentifier(kvp.Key, kvp.Value))];
            }

            // Get the neighbors of the current state and push them to the stack.

            List<StateForResolveDependencies> neighbors = currentState.GetNeighbors(preferredPackages: installedPackageSpecifiers);

            _context.Logger.LogDebug("Exploring {NeighborsCount} neighbors:", neighbors.Count);
            foreach (StateForResolveDependencies neighbor in neighbors)
            {
                _context.Logger.LogDebug("  {Neighbor}@{Version}", neighbor.NextSelection?.Item1, neighbor.NextSelection?.Item2);
            }

            // Reversely push the neighbors to the stack to explore the preferred packages first.
            neighbors.Reverse();

            neighbors.ForEach(stack.Push);
        }

        throw new InvalidOperationException("Cannot find a valid state to satisfy all dependencies.");
    }
}

file record StateForResolveDependencies
{
    public required Dictionary<PackageIdentifier, HashSet<SemVersion>> Candidates { get; set; }

    public required Tuple<PackageIdentifier, SemVersion>? NextSelection { get; set; }

    public required Dictionary<PackageIdentifier, SemVersion> Selected { get; set; }

    /// <summary>
    /// Gets the neighbors of the state by selecting the candidate with least versions to
    /// explore. The versions are sorted by the preferred packages and then by the version
    /// precedence.
    /// </summary>
    /// <remarks>
    /// After calling this method, ResolveSelection() must be called.
    /// </remarks>
    /// <param name="preferredPackages">The preferred packages to explore first.</param>
    /// <returns>The neighbors of the state which are not normalized.</returns>
    public List<StateForResolveDependencies> GetNeighbors(IEnumerable<PackageSpecifier> preferredPackages)
    {
        if (Candidates.Count == 0)
        {
            return [];
        }

        KeyValuePair<PackageIdentifier, HashSet<SemVersion>> candidateToExplore = Candidates.MinBy(kvp => kvp.Value.Count);
        IOrderedEnumerable<SemVersion> candidateVersionsToExplorer = candidateToExplore.Value
            .OrderByDescending(v => preferredPackages.Contains(PackageSpecifier.FromIdentifier(
                candidateToExplore.Key,
                v)))
            .ThenByDescending(v => v, SemVersion.PrecedenceComparer);

        return [.. candidateVersionsToExplorer.Select(version =>
        {
            return this with
            {
                Candidates = Candidates.ToDictionary(),
                NextSelection = new Tuple<PackageIdentifier, SemVersion>(candidateToExplore.Key, version),
                Selected = Selected.ToDictionary(), // Clone the selected dictionary.
            };
        })];
    }

    /// <summary>
    /// Resolve the selection. This will pop next selection from candidates, and add it to selected.
    /// </summary>
    /// <param name="packageManager"></param>
    /// <param name="knownPackages"></param>
    /// <returns>True if the state is valid. False otherwise.</returns>
    public async Task<bool> ResolveSelection(IPackageManager packageManager, IEnumerable<PackageLock.Package> knownPackages)
    {
        // Before resolving the selection, normalize the state.
        // Selected packages must neither be in candidates nor be selected again.
        foreach (var kvp in Selected)
        {
            if (Candidates.ContainsKey(kvp.Key))
            {
                if (!Candidates[kvp.Key].Contains(kvp.Value))
                {
                    throw new InvalidOperationException($"The selected package {kvp.Key} has version {kvp.Value} which does not satisfy the candidates: {Candidates[kvp.Key]}");
                }

                Candidates.Remove(kvp.Key);
            }

            if (NextSelection?.Item1 == kvp.Key)
            {
                if (NextSelection.Item2 != kvp.Value)
                {
                    throw new InvalidOperationException($"The selected package {kvp.Key} has version {kvp.Value} which is different from the next selection: {NextSelection.Item2}");
                }

                NextSelection = null;
            }
        }

        // If there is no next selection, it does not need to resolve.
        if (NextSelection == null)
        {
            return true;
        }

        PackageSpecifier nextSelectionSpecifier = PackageSpecifier.FromIdentifier(NextSelection.Item1, NextSelection.Item2);

        Dictionary<PackageIdentifier, SemVersionRange> dependenciesInRange = await GetDependencies(nextSelectionSpecifier, packageManager, knownPackages);

        Dictionary<PackageIdentifier, HashSet<SemVersion>> dependencyCandidates = [];
        foreach (var dep in dependenciesInRange)
        {
            IEnumerable<SemVersion> localVersions = knownPackages
                .Where(package => package.Specifier.Identifier == dep.Key)
                .Select(package => package.Specifier.Version);
            List<SemVersion> remoteVersions = [];
            try
            {
                remoteVersions = await packageManager.GetPackageRemoteVersions(dep.Key);
            }
            catch (Exception) { }
            HashSet<SemVersion> validVersions = [
                ..localVersions.Where(dep.Value.Contains),
                ..remoteVersions.Where(dep.Value.Contains),
            ];
            dependencyCandidates.Add(dep.Key, validVersions);
        }

        Dictionary<PackageIdentifier, HashSet<SemVersion>> newCandidates = Candidates
            // Pop the next selection from candidates.
            .Where(kvp => kvp.Key != NextSelection.Item1)
            // Intersect the candidates with the new dependency candidates.
            .Concat(dependencyCandidates)
            .GroupBy(kvp => kvp.Key)
            .ToDictionary(
                g => g.Key,
                g => g.Select(kvp => kvp.Value)
                      .Aggregate((current, next) => [.. current.Intersect(next)])
                      .ToHashSet()
            );

        Candidates = newCandidates;

        Selected.Add(NextSelection.Item1, NextSelection.Item2);

        NextSelection = null;

        // After resolving the selection, normalize the state.
        // Selected packages must not be in candidates.
        foreach (var kvp in Selected)
        {
            if (Candidates.ContainsKey(kvp.Key))
            {
                if (!Candidates[kvp.Key].Contains(kvp.Value))
                {
                    throw new InvalidOperationException($"The selected package {kvp.Key} has version {kvp.Value} which does not satisfy the candidates: {Candidates[kvp.Key]}");
                }

                Candidates.Remove(kvp.Key);
            }
        }

        return true;
    }

    private static async Task<Dictionary<PackageIdentifier, SemVersionRange>> GetDependencies(
        PackageSpecifier packageSpecifier,
        IPackageManager packageManager,
        IEnumerable<PackageLock.Package> knownPackages
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