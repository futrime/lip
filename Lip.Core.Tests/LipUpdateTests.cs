using Flurl;
using Lip.Core;
using Microsoft.Extensions.Logging;
using Moq;
using Semver;
using System.IO.Abstractions.TestingHelpers;
using System.Runtime.InteropServices;
using Xunit;

namespace Lip.Core.Tests;

public class LipUpdateTests
{
    private readonly Mock<ICacheManager> _cacheManagerMock = new();
    private readonly Mock<IContext> _contextMock = new();
    private readonly Mock<IDependencySolver> _dependencySolverMock = new();
    private readonly Mock<IPackageManager> _packageManagerMock = new();
    private readonly Mock<IPathManager> _pathManagerMock = new();
    private readonly RuntimeConfig _runtimeConfig = new();
    private readonly MockFileSystem _fileSystem = new();
    private readonly Mock<ILogger> _loggerMock = new();

    private readonly string _workingDir = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"C:\app" : "/app";
    private readonly Lip _lip;

    public LipUpdateTests()
    {
        _contextMock.Setup(c => c.FileSystem).Returns(_fileSystem);
        _contextMock.Setup(c => c.Logger).Returns(_loggerMock.Object);
        _pathManagerMock.Setup(p => p.WorkingDir).Returns(_workingDir);

        _lip = new Lip(
            _runtimeConfig,
            _contextMock.Object,
            _cacheManagerMock.Object,
            _dependencySolverMock.Object,
            _packageManagerMock.Object,
            _pathManagerMock.Object);
    }

    private PackageManifest CreateManifest(string name, string version)
    {
        return new PackageManifest
        {
            ToothPath = $"github.com/test/{name}",
            Version = SemVersion.Parse(version),
            Info = new PackageManifest.InfoType
            {
                Name = name,
                Description = "Test package",
                Tags = [],
                AvatarUrl = new Url("http://example.com/avatar")
            },
            Variants =
            [
                new PackageManifest.Variant
                {
                    Label = "",
                    Platform = RuntimeInformation.RuntimeIdentifier,
                    Dependencies = [],
                    Assets = [],
                    PreserveFiles = [],
                    RemoveFiles = [],
                    Scripts = new PackageManifest.ScriptsType
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
        };
    }

    private PackageLock.Package CreateLockedPackage(string name, string version)
    {
        var manifest = CreateManifest(name, version);
        return new PackageLock.Package
        {
            Locked = true,
            Manifest = manifest,
            VariantLabel = "",
            Files = []
        };
    }

    [Fact]
    public async Task Update_CallsInstallWithUpgradeLockedPackagesTrue()
    {
        // Arrange
        var userInput = new List<string> { "github.com/test/pkg-update" };
        var args = new Lip.UpdateArgs
        {
            DryRun = false,
            IgnoreScripts = false,
            NoDependencies = false
        };

        // We use a locked package to verify it requests AtLeast version (which implies UpgradeLockedPackages=true)
        var lockedPackage = CreateLockedPackage("locked-pkg", "1.0.0");
        var pkgUpdateLocked = CreateLockedPackage("pkg-update", "1.0.0"); // Update package is also installed

        _packageManagerMock.Setup(pm => pm.GetCurrentPackageLock())
            .ReturnsAsync(new PackageLock { Packages = [lockedPackage, pkgUpdateLocked] });
        _packageManagerMock.Setup(pm => pm.GetPackageFromLock(It.Is<PackageIdentifier>(id => id.ToString() == "github.com/test/locked-pkg")))
             .ReturnsAsync(lockedPackage);
        _packageManagerMock.Setup(pm => pm.GetPackageFromLock(It.Is<PackageIdentifier>(id => id.ToString() == "github.com/test/pkg-update")))
             .ReturnsAsync(pkgUpdateLocked);

        // Mock user input resolution
        _packageManagerMock.Setup(pm => pm.GetPackageRemoteVersions(It.IsAny<PackageIdentifier>()))
            .ReturnsAsync(new List<SemVersion> { SemVersion.Parse("2.0.0") });
        _cacheManagerMock.Setup(cm => cm.GetPackageFileSource(It.IsAny<PackageSpecifier>()))
            .ReturnsAsync(new Mock<IFileSource>().Object);
        _packageManagerMock.Setup(pm => pm.GetPackageManifestFromFileSource(It.IsAny<IFileSource>()))
            .ReturnsAsync(CreateManifest("pkg-update", "2.0.0"));

        // Mock dependency resolution success (return empty list)
        _dependencySolverMock.Setup(ds => ds.ResolveDependencies(It.IsAny<IEnumerable<(PackageIdentifier, SemVersionRange)>>(), It.IsAny<IEnumerable<PackageLock.Package>>()))
            .ReturnsAsync(new List<PackageSpecifier>());

        // Act
        await _lip.Update(userInput, args);

        // Assert
        // Verify dependency solver was called with AtLeast range for the locked package
        // This confirms that UpgradeLockedPackages was true inside Install
        _dependencySolverMock.Verify(ds => ds.ResolveDependencies(
            It.Is<IEnumerable<(PackageIdentifier, SemVersionRange)>>(reqs =>
                reqs.Any(r => r.Item1.ToString() == "github.com/test/locked-pkg" && r.Item2.ToString() == ">=1.0.0")
            ),
            It.IsAny<IEnumerable<PackageLock.Package>>()
        ), Times.Once);
    }
    [Fact]
    public async Task Update_WithUninstalledPackage_LogsWarningAndDoesNotInstall()
    {
        // Arrange
        var userInput = new List<string> { "github.com/test/uninstalled-pkg" };
        var args = new Lip.UpdateArgs
        {
            DryRun = false,
            IgnoreScripts = false,
            NoDependencies = false
        };

        // Mock empty package lock (package is not installed)
        _packageManagerMock.Setup(pm => pm.GetCurrentPackageLock())
            .ReturnsAsync(new PackageLock { Packages = [] });

        // Act
        await _lip.Update(userInput, args);

        // Assert
        // Verify ResolveDependencies was NEVER called (since Install should be skipped)
        _dependencySolverMock.Verify(ds => ds.ResolveDependencies(
            It.IsAny<IEnumerable<(PackageIdentifier, SemVersionRange)>>(),
            It.IsAny<IEnumerable<PackageLock.Package>>()
        ), Times.Never);

        // Verify warning was logged
        _loggerMock.Verify(static l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>(static (v, t) => v.ToString()!.Contains("is not installed. Skipping.")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }
}