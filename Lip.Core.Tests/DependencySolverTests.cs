using Moq;
using Semver;
using System.Runtime.InteropServices;

namespace Lip.Core.Tests;

public class DependencySolverTests
{
    [Fact]
    public async Task GetUnnecessaryPackages_ReturnsCorrectPackages()
    {
        // Arrange.
        PackageLock packageLock = new()
        {
            Packages = [
                MakePackage(
                    toothPath: "example.com/a",
                    dependencies: [],
                    locked: false
                ),
                MakePackage(
                    toothPath: "example.com/b",
                    dependencies: [],
                    locked: true
                ),
                MakePackage(
                    toothPath: "example.com/c",
                    dependencies: ["example.com/d"],
                    locked: false
                ),
                MakePackage(
                    toothPath: "example.com/d",
                    dependencies: ["example.com/e"],
                    locked: true
                ),
                MakePackage(
                    toothPath: "example.com/e",
                    dependencies: [],
                    locked: false
                ),
                MakePackage(
                    toothPath: "example.com/f",
                    dependencies: ["example.com/g"],
                    locked: false
                ),
                MakePackage(
                    toothPath: "example.com/g",
                    dependencies: [],
                    locked: false
                )
            ]
        };

        Mock<IPackageManager> packageManagerMock = new();
        packageManagerMock.Setup(m => m.GetCurrentPackageLock()).ReturnsAsync(packageLock);

        DependencySolver dependencySolver = new(packageManagerMock.Object);

        List<PackageIdentifier> expectedUnnecessaryPackages = [
            new()
            {
                ToothPath = "example.com/a",
                VariantLabel = string.Empty,
            },
            new()
            {
                ToothPath = "example.com/c",
                VariantLabel = string.Empty,
            },
            new()
            {
                ToothPath = "example.com/f",
                VariantLabel = string.Empty,
            },
            new()
            {
                ToothPath = "example.com/g",
                VariantLabel = string.Empty,
            }
        ];

        // Act.
        List<PackageIdentifier> unnecessaryPackages = await dependencySolver.GetUnnecessaryPackages();

        // Assert.
        Assert.Equal(expectedUnnecessaryPackages, unnecessaryPackages);
    }

    private static PackageLock.Package MakePackage(string toothPath, IEnumerable<string> dependencies, bool locked)
    {
        return new()
        {
            Files = [],
            Locked = locked,
            Manifest = new()
            {
                ToothPath = toothPath,
                Info = new()
                {
                    Name = string.Empty,
                    Description = string.Empty,
                    Tags = [],
                    AvatarUrl = new(),
                },
                Version = new(0),
                Variants = [
                    new()
                    {
                        Label = string.Empty,
                        Platform = RuntimeInformation.RuntimeIdentifier,
                        Dependencies = dependencies.ToDictionary(
                            dep => new PackageIdentifier
                            {
                                ToothPath = dep,
                                VariantLabel = string.Empty,
                            },
                            dep => SemVersionRange.Parse("0.0.0")
                        ),
                        Assets = [],
                        PreserveFiles = [],
                        RemoveFiles = [],
                        Scripts = new()
                        {
                            PreInstall = [],
                            Install = [],
                            PostInstall = [],
                            PrePack = [],
                            PostPack = [],
                            PreUninstall = [],
                            Uninstall = [],
                            PostUninstall = [],
                            AdditionalScripts = []
                        }
                    }
                ]
            },
            VariantLabel = string.Empty,
        };
    }
}
