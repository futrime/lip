using Flurl;
using Lip.Core;
using Microsoft.Extensions.Logging;
using Moq;
using Semver;
using System.IO.Abstractions.TestingHelpers;
using System.Runtime.InteropServices;
using Xunit;

namespace Lip.Core.Tests;

public class LipInstallTests
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

    public LipInstallTests()
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
    public async Task Install_WithUpgradeLockedPackagesTrue_UsesAtLeastRangeForLockedPackages()
    {
        // Arrange
        var userInput = new List<string> { "github.com/test/new-pkg" };
        var args = new Lip.InstallArgs
        {
            DryRun = false,
            IgnoreScripts = false,
            NoDependencies = false,
            OverwriteFiles = false,
            UpgradeLockedPackages = true
        };

        var lockedPackage = CreateLockedPackage("locked-pkg", "1.0.0");

        _packageManagerMock.Setup(pm => pm.GetCurrentPackageLock())
            .ReturnsAsync(new PackageLock { Packages = [lockedPackage] });
        _packageManagerMock.Setup(pm => pm.GetPackageFromLock(It.Is<PackageIdentifier>(id => id.ToString() == "github.com/test/locked-pkg")))
            .ReturnsAsync(lockedPackage);

        _packageManagerMock.Setup(pm => pm.GetPackageRemoteVersions(It.Is<PackageIdentifier>(id => id.ToString() == "github.com/test/new-pkg")))
            .ReturnsAsync(new List<SemVersion> { SemVersion.Parse("2.0.0") });

        _cacheManagerMock.Setup(cm => cm.GetPackageFileSource(It.IsAny<PackageSpecifier>()))
            .ReturnsAsync(new Mock<IFileSource>().Object);
        _packageManagerMock.Setup(pm => pm.GetPackageManifestFromFileSource(It.IsAny<IFileSource>()))
            .ReturnsAsync(CreateManifest("new-pkg", "2.0.0"));

        _dependencySolverMock.Setup(ds => ds.ResolveDependencies(It.IsAny<IEnumerable<(PackageIdentifier, SemVersionRange)>>(), It.IsAny<IEnumerable<PackageLock.Package>>()))
            .ReturnsAsync(new List<PackageSpecifier>());

        // Act
        await _lip.Install(userInput, args);

        // Assert
        _dependencySolverMock.Verify(ds => ds.ResolveDependencies(
            It.Is<IEnumerable<(PackageIdentifier, SemVersionRange)>>(reqs =>
                reqs.Any(r => r.Item1.ToString() == "github.com/test/locked-pkg" && r.Item2.ToString() == ">=1.0.0")
            ),
            It.IsAny<IEnumerable<PackageLock.Package>>()
        ), Times.Once);
    }

    [Fact]
    public async Task Install_WithUpgradeLockedPackagesFalse_UsesExactRangeForLockedPackages()
    {
        // Arrange
        var userInput = new List<string> { "github.com/test/new-pkg" };
        var args = new Lip.InstallArgs
        {
            DryRun = false,
            IgnoreScripts = false,
            NoDependencies = false,
            OverwriteFiles = false,
            UpgradeLockedPackages = false
        };

        var lockedPackage = CreateLockedPackage("locked-pkg", "1.0.0");

        _packageManagerMock.Setup(pm => pm.GetCurrentPackageLock())
            .ReturnsAsync(new PackageLock { Packages = [lockedPackage] });
        _packageManagerMock.Setup(pm => pm.GetPackageFromLock(It.Is<PackageIdentifier>(id => id.ToString() == "github.com/test/locked-pkg")))
            .ReturnsAsync(lockedPackage);

        _packageManagerMock.Setup(pm => pm.GetPackageRemoteVersions(It.Is<PackageIdentifier>(id => id.ToString() == "github.com/test/new-pkg")))
            .ReturnsAsync(new List<SemVersion> { SemVersion.Parse("2.0.0") });

        _cacheManagerMock.Setup(cm => cm.GetPackageFileSource(It.IsAny<PackageSpecifier>()))
            .ReturnsAsync(new Mock<IFileSource>().Object);
        _packageManagerMock.Setup(pm => pm.GetPackageManifestFromFileSource(It.IsAny<IFileSource>()))
            .ReturnsAsync(CreateManifest("new-pkg", "2.0.0"));

        _dependencySolverMock.Setup(ds => ds.ResolveDependencies(It.IsAny<IEnumerable<(PackageIdentifier, SemVersionRange)>>(), It.IsAny<IEnumerable<PackageLock.Package>>()))
            .ReturnsAsync(new List<PackageSpecifier>());

        // Act
        await _lip.Install(userInput, args);

        // Assert
        _dependencySolverMock.Verify(ds => ds.ResolveDependencies(
            It.Is<IEnumerable<(PackageIdentifier, SemVersionRange)>>(reqs =>
                reqs.Any(r => r.Item1.ToString() == "github.com/test/locked-pkg" && r.Item2.ToString() == "1.0.0")
            ),
            It.IsAny<IEnumerable<PackageLock.Package>>()
        ), Times.Once);
    }

    [Fact]
    public async Task Install_DoesNotThrow_WhenPackageIsAlreadyInstalled()
    {
        // Arrange
        var userInput = new List<string> { "github.com/test/existing-pkg" };
        var args = new Lip.InstallArgs
        {
            DryRun = false,
            IgnoreScripts = false,
            NoDependencies = false,
            OverwriteFiles = false,
            UpgradeLockedPackages = false
        };

        var existingPkgId = PackageIdentifier.Parse("github.com/test/existing-pkg");
        var existingPkg = CreateLockedPackage("existing-pkg", "1.0.0");


        _packageManagerMock.Setup(pm => pm.GetCurrentPackageLock())
             .ReturnsAsync(new PackageLock { Packages = [existingPkg] });
        _packageManagerMock.Setup(pm => pm.GetPackageFromLock(existingPkgId))
             .ReturnsAsync(existingPkg);

        _packageManagerMock.Setup(pm => pm.GetPackageRemoteVersions(existingPkgId))
            .ReturnsAsync(new List<SemVersion> { SemVersion.Parse("1.0.0") });

        _cacheManagerMock.Setup(cm => cm.GetPackageFileSource(It.IsAny<PackageSpecifier>()))
            .ReturnsAsync(new Mock<IFileSource>().Object);
        _packageManagerMock.Setup(pm => pm.GetPackageManifestFromFileSource(It.IsAny<IFileSource>()))
            .ReturnsAsync(CreateManifest("existing-pkg", "1.0.0"));

        _dependencySolverMock.Setup(ds => ds.ResolveDependencies(It.IsAny<IEnumerable<(PackageIdentifier, SemVersionRange)>>(), It.IsAny<IEnumerable<PackageLock.Package>>()))
            .ReturnsAsync(new List<PackageSpecifier>());

        // Act & Assert
        await _lip.Install(userInput, args);
    }

    [Fact]
    public async Task Install_FullFlow_InstallsAndUninstallsCorrectly()
    {
        // Arrange
        var userInput = new List<string> { "github.com/test/pkg-a" };
        var args = new Lip.InstallArgs
        {
            DryRun = false,
            IgnoreScripts = false,
            NoDependencies = false,
            OverwriteFiles = false,
            UpgradeLockedPackages = false
        };

        // Existing package that should be uninstalled (pkg-old)
        // because it will not be in the dependency resolution result
        var pkgOld = CreateLockedPackage("pkg-old", "1.0.0");

        // Locked package setup
        _packageManagerMock.Setup(pm => pm.GetCurrentPackageLock())
             .ReturnsAsync(new PackageLock { Packages = [pkgOld] });
        _packageManagerMock.Setup(pm => pm.GetPackageFromLock(pkgOld.Specifier.Identifier))
             .ReturnsAsync(pkgOld);

        // User Input Resolution Mocking
        _packageManagerMock.Setup(pm => pm.GetPackageRemoteVersions(It.Is<PackageIdentifier>(id => id.ToString() == "github.com/test/pkg-a")))
            .ReturnsAsync(new List<SemVersion> { SemVersion.Parse("1.0.0") });
        _cacheManagerMock.Setup(cm => cm.GetPackageFileSource(It.Is<PackageSpecifier>(s => s.Identifier.ToString() == "github.com/test/pkg-a")))
            .ReturnsAsync(new Mock<IFileSource>().Object);
        _packageManagerMock.Setup(pm => pm.GetPackageManifestFromFileSource(It.IsAny<IFileSource>()))
             .ReturnsAsync(CreateManifest("pkg-a", "1.0.0")); // Simplified: assumes same manifest for all file sources for now, or use specific setup

        // Dependency Resolution Result
        // Should return pkg-a and pkg-b (dependency of a)
        var pkgA = new PackageSpecifier { ToothPath = "github.com/test/pkg-a", Version = SemVersion.Parse("1.0.0"), VariantLabel = "" };
        var pkgB = new PackageSpecifier { ToothPath = "github.com/test/pkg-b", Version = SemVersion.Parse("1.0.0"), VariantLabel = "" };

        _dependencySolverMock.Setup(ds => ds.ResolveDependencies(
            It.IsAny<IEnumerable<(PackageIdentifier, SemVersionRange)>>(),
            It.IsAny<IEnumerable<PackageLock.Package>>()))
            .ReturnsAsync(new List<PackageSpecifier> { pkgA, pkgB });

        // Mock caching for dependency pkg-b (step 6 loop)
        _cacheManagerMock.Setup(cm => cm.GetPackageFileSource(It.Is<PackageSpecifier>(s => s.Identifier.ToString() == "github.com/test/pkg-b")))
            .ReturnsAsync(new Mock<IFileSource>().Object);
        // We need manifest for pkg-b to create install detail
        // IMPORTANT: The previous setup for GetPackageManifestFromFileSource was generic. 
        // We should refine it if we need distinct manifests. For this test, it's okay if they return "pkg-a" manifest 
        // as long as we don't assert specifically on the internal properties that might conflict, 
        // BUT Lip.Install uses manifest.ToothPath for specifier. 
        // So we MUST return correct manifest for correct file source.

        var sourceA = new Mock<IFileSource>();
        var sourceB = new Mock<IFileSource>();
        _cacheManagerMock.Setup(cm => cm.GetPackageFileSource(It.Is<PackageSpecifier>(s => s.Identifier.ToString() == "github.com/test/pkg-a")))
            .ReturnsAsync(sourceA.Object);
        _cacheManagerMock.Setup(cm => cm.GetPackageFileSource(It.Is<PackageSpecifier>(s => s.Identifier.ToString() == "github.com/test/pkg-b")))
             .ReturnsAsync(sourceB.Object);

        _packageManagerMock.Setup(pm => pm.GetPackageManifestFromFileSource(sourceA.Object))
             .ReturnsAsync(CreateManifest("pkg-a", "1.0.0"));
        _packageManagerMock.Setup(pm => pm.GetPackageManifestFromFileSource(sourceB.Object))
             .ReturnsAsync(CreateManifest("pkg-b", "1.0.0"));


        // Act
        await _lip.Install(userInput, args);

        // Assert

        // 1. Uninstallation of pkg-old
        _packageManagerMock.Verify(pm => pm.UninstallPackage(
            It.Is<PackageIdentifier>(id => id.ToString() == "github.com/test/pkg-old"),
            false,
            false
        ), Times.Once);

        // 2. Installation of pkg-a and pkg-b
        _packageManagerMock.Verify(pm => pm.InstallPackage(
            sourceA.Object,
            "",
            false,
            false,
            It.IsAny<bool>(), // Locked or not
            false
        ), Times.Once);

        _packageManagerMock.Verify(pm => pm.InstallPackage(
            sourceB.Object,
            "",
            false,
            false,
            It.IsAny<bool>(), // Locked or not
            false
        ), Times.Once);
    }

    [Fact]
    public async Task Install_WithLocalDirectory_InstallsCorrectly()
    {
        // Arrange
        var userInput = new List<string> { "./local-pkg" };
        var args = new Lip.InstallArgs
        {
            DryRun = false,
            IgnoreScripts = false,
            NoDependencies = false,
            OverwriteFiles = false,
            UpgradeLockedPackages = false
        };

        // Mock empty lock
        _packageManagerMock.Setup(pm => pm.GetCurrentPackageLock())
             .ReturnsAsync(new PackageLock { Packages = [] });

        // Mock FileSystem for directory check
        // The path will be combined with WorkingDir
        _fileSystem.AddDirectory(Path.Combine(_workingDir, "local-pkg"));

        // Mock Manifest for local directory
        // GetPackageManifestFromFileSource should be called with DirectoryFileSource
        _packageManagerMock.Setup(pm => pm.GetPackageManifestFromFileSource(It.IsAny<DirectoryFileSource>()))
             .ReturnsAsync(CreateManifest("local-pkg", "1.0.0"));

        // Identify dependency solver call
        _dependencySolverMock.Setup(ds => ds.ResolveDependencies(It.IsAny<IEnumerable<(PackageIdentifier, SemVersionRange)>>(), It.IsAny<IEnumerable<PackageLock.Package>>()))
             .ReturnsAsync(new List<PackageSpecifier> { new PackageSpecifier { ToothPath = "github.com/test/local-pkg", Version = SemVersion.Parse("1.0.0"), VariantLabel = "" } });

        // Mock caching (GetPackageFileSource needs to return something for the result of dependency resolution)
        _cacheManagerMock.Setup(cm => cm.GetPackageFileSource(It.IsAny<PackageSpecifier>()))
            .ReturnsAsync(new Mock<IFileSource>().Object);

        // Act
        await _lip.Install(userInput, args);

        // Assert
        // Verify GetPackageManifestFromFileSource was called with DirectoryFileSource
        _packageManagerMock.Verify(pm => pm.GetPackageManifestFromFileSource(It.IsAny<DirectoryFileSource>()), Times.Once);

        // Verify InstallPackage called
        _packageManagerMock.Verify(pm => pm.InstallPackage(It.IsAny<IFileSource>(), "", false, false, It.IsAny<bool>(), false), Times.Once);
    }

    [Fact]
    public async Task Install_WithDryRun_DoesNotUninstallOrInstall()
    {
        // Arrange
        var userInput = new List<string> { "github.com/test/pkg-a" };
        var args = new Lip.InstallArgs
        {
            DryRun = true,
            IgnoreScripts = false,
            NoDependencies = false,
            OverwriteFiles = false,
            UpgradeLockedPackages = false
        };

        // Mock generic setups for valid install flow
        _packageManagerMock.Setup(pm => pm.GetCurrentPackageLock()).ReturnsAsync(new PackageLock { Packages = [] });
        _packageManagerMock.Setup(pm => pm.GetPackageRemoteVersions(It.IsAny<PackageIdentifier>()))
             .ReturnsAsync(new List<SemVersion> { SemVersion.Parse("1.0.0") });
        _cacheManagerMock.Setup(cm => cm.GetPackageFileSource(It.IsAny<PackageSpecifier>()))
             .ReturnsAsync(new Mock<IFileSource>().Object);
        _packageManagerMock.Setup(pm => pm.GetPackageManifestFromFileSource(It.IsAny<IFileSource>()))
             .ReturnsAsync(CreateManifest("pkg-a", "1.0.0"));

        var pkgA = new PackageSpecifier { ToothPath = "github.com/test/pkg-a", Version = SemVersion.Parse("1.0.0"), VariantLabel = "" };
        _dependencySolverMock.Setup(ds => ds.ResolveDependencies(It.IsAny<IEnumerable<(PackageIdentifier, SemVersionRange)>>(), It.IsAny<IEnumerable<PackageLock.Package>>()))
             .ReturnsAsync(new List<PackageSpecifier> { pkgA });

        // Act
        await _lip.Install(userInput, args);

        // Assert
        // Verify InstallPackage called with dryRun=true
        _packageManagerMock.Verify(pm => pm.InstallPackage(It.IsAny<IFileSource>(), It.IsAny<string>(), true, It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task Install_WithNoDependencies_DoesNotResolveDependencies()
    {
        // Arrange
        var userInput = new List<string> { "github.com/test/pkg-a" };
        var args = new Lip.InstallArgs
        {
            DryRun = false,
            IgnoreScripts = false,
            NoDependencies = true,
            OverwriteFiles = false,
            UpgradeLockedPackages = false
        };

        // Mock setup
        _packageManagerMock.Setup(pm => pm.GetCurrentPackageLock()).ReturnsAsync(new PackageLock { Packages = [] });
        _packageManagerMock.Setup(pm => pm.GetPackageRemoteVersions(It.IsAny<PackageIdentifier>()))
             .ReturnsAsync(new List<SemVersion> { SemVersion.Parse("1.0.0") });
        _cacheManagerMock.Setup(cm => cm.GetPackageFileSource(It.IsAny<PackageSpecifier>()))
             .ReturnsAsync(new Mock<IFileSource>().Object);
        _packageManagerMock.Setup(pm => pm.GetPackageManifestFromFileSource(It.IsAny<IFileSource>()))
             .ReturnsAsync(CreateManifest("pkg-a", "1.0.0"));

        // Act
        await _lip.Install(userInput, args);

        // Assert
        // ResolveDependencies should NOT be called
        _dependencySolverMock.Verify(ds => ds.ResolveDependencies(It.IsAny<IEnumerable<(PackageIdentifier, SemVersionRange)>>(), It.IsAny<IEnumerable<PackageLock.Package>>()), Times.Never);

        // But install should still happen for the primary package
        _packageManagerMock.Verify(pm => pm.InstallPackage(It.IsAny<IFileSource>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task Install_WithPackageSpecifier_ParsesCorrectly()
    {
        // Arrange
        var userInput = new List<string> { "github.com/test/pkg-spec@1.2.3" };
        var args = new Lip.InstallArgs
        {
            DryRun = false,
            IgnoreScripts = false,
            NoDependencies = false,
            OverwriteFiles = false,
            UpgradeLockedPackages = false
        };

        // Mock setup
        _packageManagerMock.Setup(pm => pm.GetCurrentPackageLock()).ReturnsAsync(new PackageLock { Packages = [] });

        // Should NOT call GetPackageRemoteVersions because we have explicit version
        // Should call GetPackageFileSource with the specific version
        _cacheManagerMock.Setup(cm => cm.GetPackageFileSource(It.Is<PackageSpecifier>(s => s.Version.ToString() == "1.2.3")))
             .ReturnsAsync(new Mock<IFileSource>().Object);

        _packageManagerMock.Setup(pm => pm.GetPackageManifestFromFileSource(It.IsAny<IFileSource>()))
             .ReturnsAsync(CreateManifest("pkg-spec", "1.2.3"));

        _dependencySolverMock.Setup(ds => ds.ResolveDependencies(It.IsAny<IEnumerable<(PackageIdentifier, SemVersionRange)>>(), It.IsAny<IEnumerable<PackageLock.Package>>()))
             .ReturnsAsync(new List<PackageSpecifier> { new PackageSpecifier { ToothPath = "github.com/test/pkg-spec", Version = SemVersion.Parse("1.2.3"), VariantLabel = "" } });

        // Act
        await _lip.Install(userInput, args);

        // Assert
        _cacheManagerMock.Verify(cm => cm.GetPackageFileSource(It.Is<PackageSpecifier>(s => s.Version.ToString() == "1.2.3")), Times.Once);
    }

    [Fact]
    public async Task Install_ThrowsArgumentException_WhenPackageResolvesToNoVersions()
    {
        // Arrange
        var userInput = new List<string> { "github.com/test/unknown-pkg" };
        var args = new Lip.InstallArgs
        {
            DryRun = false,
            IgnoreScripts = false,
            NoDependencies = false,
            OverwriteFiles = false,
            UpgradeLockedPackages = false
        };

        // Mock setup
        _packageManagerMock.Setup(pm => pm.GetCurrentPackageLock()).ReturnsAsync(new PackageLock { Packages = [] });

        // Return empty list implies package not found remotely
        _packageManagerMock.Setup(pm => pm.GetPackageRemoteVersions(It.IsAny<PackageIdentifier>()))
             .ReturnsAsync(new List<SemVersion>());

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await _lip.Install(userInput, args));
    }

    [Fact]
    public async Task Install_WithLocalArchive_InstallsCorrectly()
    {
        // Arrange
        var userInput = new List<string> { "package.zip" }; // Use default variant to match CreateManifest
        var args = new Lip.InstallArgs
        {
            DryRun = false,
            IgnoreScripts = false,
            NoDependencies = false,
            OverwriteFiles = false,
            UpgradeLockedPackages = false
        };

        _packageManagerMock.Setup(pm => pm.GetCurrentPackageLock()).ReturnsAsync(new PackageLock { Packages = [] });

        // Create a dummy zip file with minimal valid structure if possible, or just magic headers
        // SharpCompress checks signatures. PK\03\04 is local file header. 
        // It might read more. Let's try standard empty zip signature if needed, 
        // but magic header usually suffices for IsArchive check.
        var zipBytes = new byte[] {
            0x50, 0x4B, 0x03, 0x04, // Local file header signature
            0x0A, 0x00,             // Version needed to extract
            0x00, 0x00,             // General purpose bit flag
            0x00, 0x00,             // Compression method
            0x00, 0x00, 0x00, 0x00, // Last mod file time/date
            0x00, 0x00, 0x00, 0x00, // CRC-32
            0x00, 0x00, 0x00, 0x00, // Compressed size
            0x00, 0x00, 0x00, 0x00, // Uncompressed size
            0x00, 0x00, 0x00, 0x00  // Filename length / Extra field length
        };
        _fileSystem.AddFile(Path.Combine(_workingDir, "package.zip"), new MockFileData(zipBytes));

        // Mock Manifest
        _packageManagerMock.Setup(pm => pm.GetPackageManifestFromFileSource(It.IsAny<ArchiveFileSource>()))
             .ReturnsAsync(CreateManifest("archive-pkg", "1.0.0"));

        // Expect default variant label.
        _dependencySolverMock.Setup(ds => ds.ResolveDependencies(It.IsAny<IEnumerable<(PackageIdentifier, SemVersionRange)>>(), It.IsAny<IEnumerable<PackageLock.Package>>()))
             .ReturnsAsync(new List<PackageSpecifier> { new PackageSpecifier { ToothPath = "github.com/test/archive-pkg", Version = SemVersion.Parse("1.0.0"), VariantLabel = "" } });

        _cacheManagerMock.Setup(cm => cm.GetPackageFileSource(It.IsAny<PackageSpecifier>()))
            .ReturnsAsync(new Mock<IFileSource>().Object);

        // Act
        await _lip.Install(userInput, args);

        // Assert
        _packageManagerMock.Verify(pm => pm.GetPackageManifestFromFileSource(It.IsAny<ArchiveFileSource>()), Times.Once);
        _packageManagerMock.Verify(pm => pm.InstallPackage(It.IsAny<IFileSource>(), "", false, false, It.IsAny<bool>(), false), Times.Once);
    }

    [Fact]
    public async Task Install_WithNoDependencies_FiltersInstalledPackagesCorrectly()
    {
        // Covers !userInputSpecifiers.Any(...) in Step 4
        // Logic: DP = UIP + (IP - UIP)

        // Arrange
        var userInput = new List<string> { "github.com/test/pkg-b" }; // User updates B
        var args = new Lip.InstallArgs
        {
            DryRun = false,
            IgnoreScripts = false,
            NoDependencies = true,
            OverwriteFiles = false,
            UpgradeLockedPackages = false
        };

        // Installed: A, B(old)
        var pkgA = CreateLockedPackage("pkg-a", "1.0.0");
        var pkgBOld = CreateLockedPackage("pkg-b", "1.0.0");

        _packageManagerMock.Setup(pm => pm.GetCurrentPackageLock())
             .ReturnsAsync(new PackageLock { Packages = [pkgA, pkgBOld] });
        _packageManagerMock.Setup(pm => pm.GetPackageFromLock(pkgA.Specifier.Identifier)).ReturnsAsync(pkgA);
        _packageManagerMock.Setup(pm => pm.GetPackageFromLock(pkgBOld.Specifier.Identifier)).ReturnsAsync(pkgBOld);

        // Remote B (new)
        _packageManagerMock.Setup(pm => pm.GetPackageRemoteVersions(It.Is<PackageIdentifier>(id => id.ToString() == "github.com/test/pkg-b")))
            .ReturnsAsync(new List<SemVersion> { SemVersion.Parse("2.0.0") });
        _cacheManagerMock.Setup(cm => cm.GetPackageFileSource(It.IsAny<PackageSpecifier>()))
             .ReturnsAsync(new Mock<IFileSource>().Object);
        _packageManagerMock.Setup(pm => pm.GetPackageManifestFromFileSource(It.IsAny<IFileSource>()))
             .ReturnsAsync(CreateManifest("pkg-b", "2.0.0")); // For B new

        // Act
        await _lip.Install(userInput, args);

        // Assert
        // A should remain installed (not uninstalled)
        _packageManagerMock.Verify(pm => pm.UninstallPackage(It.Is<PackageIdentifier>(id => id.ToString() == "github.com/test/pkg-a"), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);

        // B (old) should be uninstalled because it was overridden by UIP B(new) in the DP list?
        // Wait, Uninstalled = IP x (-DP + UIP).
        // DP (NoDeps) = UIP + IP.Where(!UIP).
        // DP = [B(new), A].
        // IP = [A, B(old)].
        // Is B(old) in DP? No, B(new) is in DP. Identifier matches.
        // check Step 5 logic:
        // if (!dependentSpecifiers.Any(d => d.Id == installed.Id) || userInputSpecifiers.Any(u => u.Id == installed.Id))
        //   Uninstall.

        // For A: Matches dependentSpecifier (A). UserInput? No. => Keep.
        // For B(old): Matches dependentSpecifier (B(new))? Yes (Identifier match). UserInput? Yes. => Uninstall.

        _packageManagerMock.Verify(pm => pm.UninstallPackage(It.Is<PackageIdentifier>(id => id.ToString() == "github.com/test/pkg-b"), false, false), Times.Once);
    }

    [Fact]
    public async Task Install_SkipsDependencies_IfAlreadyInstalled()
    {
        // Covers: if (installedSpecifiers.Any(...) continue;

        // Arrange
        var userInput = new List<string> { "github.com/test/pkg-a" };
        var args = new Lip.InstallArgs
        {
            DryRun = false,
            UpgradeLockedPackages = false,
            IgnoreScripts = false,
            NoDependencies = false,
            OverwriteFiles = false
        };

        // Installed: pkg-b (dependency of A)
        var pkgBLocked = CreateLockedPackage("pkg-b", "1.0.0");
        _packageManagerMock.Setup(pm => pm.GetCurrentPackageLock())
             .ReturnsAsync(new PackageLock { Packages = [pkgBLocked] });
        _packageManagerMock.Setup(pm => pm.GetPackageFromLock(pkgBLocked.Specifier.Identifier)).ReturnsAsync(pkgBLocked);

        // Setup A
        _packageManagerMock.Setup(pm => pm.GetPackageRemoteVersions(It.Is<PackageIdentifier>(id => id.ToString() == "github.com/test/pkg-a")))
            .ReturnsAsync(new List<SemVersion> { SemVersion.Parse("1.0.0") });
        _cacheManagerMock.Setup(cm => cm.GetPackageFileSource(It.Is<PackageSpecifier>(s => s.Identifier.ToString() == "github.com/test/pkg-a")))
             .ReturnsAsync(new Mock<IFileSource>().Object);
        _packageManagerMock.Setup(pm => pm.GetPackageManifestFromFileSource(It.IsAny<IFileSource>()))
             .ReturnsAsync(CreateManifest("pkg-a", "1.0.0"));

        // Resolution returns A and B
        var specA = new PackageSpecifier { ToothPath = "github.com/test/pkg-a", Version = SemVersion.Parse("1.0.0"), VariantLabel = "" };
        var specB = new PackageSpecifier { ToothPath = "github.com/test/pkg-b", Version = SemVersion.Parse("1.0.0"), VariantLabel = "" };

        _dependencySolverMock.Setup(ds => ds.ResolveDependencies(It.IsAny<IEnumerable<(PackageIdentifier, SemVersionRange)>>(), It.IsAny<IEnumerable<PackageLock.Package>>()))
             .ReturnsAsync(new List<PackageSpecifier> { specA, specB });

        // Act
        await _lip.Install(userInput, args);

        // Assert
        // Install pkg-a
        _packageManagerMock.Verify(pm => pm.InstallPackage(It.IsAny<IFileSource>(), "", false, false, It.IsAny<bool>(), false), Times.Once); // For A only

        // Should NOT install B (because it's installed)
        // We verify by checking that we didn't fetch manifest for B from file source inside the installation loop logic
        // But to be sure, we check that InstallPackage was called exactly ONCE (for A).
        _packageManagerMock.Verify(pm => pm.InstallPackage(It.IsAny<IFileSource>(), "", false, false, It.IsAny<bool>(), false), Times.Once);
    }

    [Fact]
    public async Task Install_SkipsDependencies_IfInUserInput()
    {
        // Covers: || userInputDetails.Any(...) continue;

        // Arrange
        var userInput = new List<string> { "github.com/test/pkg-a", "github.com/test/pkg-b" };
        var args = new Lip.InstallArgs
        {
            DryRun = false,
            UpgradeLockedPackages = false,
            IgnoreScripts = false,
            NoDependencies = false,
            OverwriteFiles = false
        };

        _packageManagerMock.Setup(pm => pm.GetCurrentPackageLock()).ReturnsAsync(new PackageLock { Packages = [] });

        // Setup A and B with distinct manifests
        var sourceA = new Mock<IFileSource>();
        var sourceB = new Mock<IFileSource>();

        _packageManagerMock.Setup(pm => pm.GetPackageRemoteVersions(It.IsAny<PackageIdentifier>()))
             .ReturnsAsync(new List<SemVersion> { SemVersion.Parse("1.0.0") });

        _cacheManagerMock.Setup(cm => cm.GetPackageFileSource(It.Is<PackageSpecifier>(s => s.Identifier.ToString() == "github.com/test/pkg-a")))
             .ReturnsAsync(sourceA.Object);
        _cacheManagerMock.Setup(cm => cm.GetPackageFileSource(It.Is<PackageSpecifier>(s => s.Identifier.ToString() == "github.com/test/pkg-b")))
             .ReturnsAsync(sourceB.Object);

        _packageManagerMock.Setup(pm => pm.GetPackageManifestFromFileSource(sourceA.Object))
             .ReturnsAsync(CreateManifest("pkg-a", "1.0.0"));
        _packageManagerMock.Setup(pm => pm.GetPackageManifestFromFileSource(sourceB.Object))
             .ReturnsAsync(CreateManifest("pkg-b", "1.0.0"));

        // Resolution returns A and B (B is dependency of A)
        var specA = new PackageSpecifier { ToothPath = "github.com/test/pkg-a", Version = SemVersion.Parse("1.0.0"), VariantLabel = "" };
        var specB = new PackageSpecifier { ToothPath = "github.com/test/pkg-b", Version = SemVersion.Parse("1.0.0"), VariantLabel = "" };

        _dependencySolverMock.Setup(ds => ds.ResolveDependencies(It.IsAny<IEnumerable<(PackageIdentifier, SemVersionRange)>>(), It.IsAny<IEnumerable<PackageLock.Package>>()))
             .ReturnsAsync(new List<PackageSpecifier> { specA, specB });

        // Act
        await _lip.Install(userInput, args);

        // Assert
        // Both installed exactly once (from user input list, not duplicated by dependency list)
        _packageManagerMock.Verify(pm => pm.InstallPackage(It.IsAny<IFileSource>(), "", false, false, It.IsAny<bool>(), false), Times.Exactly(2));
    }
}