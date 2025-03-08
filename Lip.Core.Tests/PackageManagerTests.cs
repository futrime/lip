// using Flurl;
// // using Microsoft.Extensions.Logging;
// using Moq;
// using Semver;
// using System.IO.Abstractions.TestingHelpers;
// using System.Runtime.InteropServices;
// using static Lip.Context.IGit;

// namespace Lip.Core.Tests;

// public class PackageManagerTests
// {
//     private static readonly string s_cacheDir = OperatingSystem.IsWindows()
//         ? Path.Join("C:", "path", "to", "cache")
//         : Path.Join("/", "path", "to", "cache");

//     private static readonly string s_workingDir = OperatingSystem.IsWindows()
//         ? Path.Join("C:", "path", "to", "working")
//         : Path.Join("/", "path", "to", "working");

//     private static readonly PackageManifest s_examplePackage_1 = new()
//     {
//         FormatVersion = PackageManifest.DefaultFormatVersion,
//         FormatUuid = PackageManifest.DefaultFormatUuid,
//         ToothPath = "example.com/pkg",
//         VersionText = "1.0.0",
//     };

//     private static readonly PackageLock s_examplePackageLock = new()
//     {
//         Locks = [
//                 new PackageLock.Package()
//                 {
//                     Locked = true,
//                     Manifest = s_examplePackage_1,
//                     VariantLabel = "variant",
//                     Files = []
//                 }
//             ]
//     };

//     private static PackageManager PackageManagerFromFiles(IDictionary<string, MockFileData> files)
//     {
//         var fileSystem = new MockFileSystem(files, s_workingDir);

//         var context = new Mock<IContext>();
//         context.SetupGet(c => c.FileSystem).Returns(fileSystem);

//         return PackageManagerFromCxtAndFs(context, fileSystem);
//     }

//     private static PackageManager PackageManagerFromCxtAndFs(Mock<IContext> context, MockFileSystem fileSystem)
//     {
//         var pathManager = new PathManager(fileSystem, baseCacheDir: s_cacheDir, workingDir: s_workingDir);

//         var cacheManager = new CacheManager(context.Object, pathManager, [], []);

//         return new PackageManager(context.Object, cacheManager, pathManager, []);
//     }

//     [Fact]
//     public async Task GetCurrentPackageLock_NotFound_ReturnsDefault()
//     {
//         // Arrange.
//         var expectedPackageLock = new PackageLock()
//         {
//             Locks = []
//         };

//         var packageManager = PackageManagerFromFiles(new Dictionary<string, MockFileData>());

//         // Act.
//         var result = await packageManager.GetCurrentPackageLock();

//         // Assert.
//         Assert.Equal(expectedPackageLock.ToJsonBytes(), result.ToJsonBytes());
//     }

//     [Fact]
//     public async Task GetCurrentPackageLock_Found_ReturnsPackageLock()
//     {
//         // Arrange.
//         var expectedPackageLock = s_examplePackageLock;

//         var packageManager = PackageManagerFromFiles(new Dictionary<string, MockFileData>
//         {
//             { Path.Join(s_workingDir, "tooth_lock.json"), new MockFileData(expectedPackageLock.ToJsonBytes()) }
//         });

//         // Act.
//         var result = await packageManager.GetCurrentPackageLock();

//         // Assert.
//         Assert.Equal(expectedPackageLock.ToJsonBytes(), result.ToJsonBytes());
//     }

//     [Fact]
//     public async Task GetCurrentPackageManifestParsed_NotExits()
//     {
//         // Arrange.
//         var packageManager = PackageManagerFromFiles(new Dictionary<string, MockFileData>());

//         // Act.
//         var result = await packageManager.GetCurrentPackageManifestWithTemplate();

//         // Assert.
//         Assert.Null(result);
//     }

//     [Fact]
//     public async Task GetCurrentPackageManifestParsed_Exits()
//     {
//         // Arrange.
//         var expectedPackage = s_examplePackage_1;

//         var packageManager = PackageManagerFromFiles(new Dictionary<string, MockFileData>() {
//             { Path.Join(s_workingDir, "tooth.json"), new MockFileData(expectedPackage.ToJsonBytes()) }
//         });

//         // Act.
//         var result = await packageManager.GetCurrentPackageManifestWithTemplate();

//         // Assert.
//         Assert.Equal(expectedPackage, result);
//     }

//     [Fact]
//     public async Task GetPackageManifestFromInstalledPackages_Found()
//     {
//         // Arrange.
//         var expectedPackage = s_examplePackage_1;

//         var packageManager = PackageManagerFromFiles(new Dictionary<string, MockFileData> {
//             { Path.Join(s_workingDir, "tooth_lock.json"), new MockFileData(s_examplePackageLock.ToJsonBytes()) }
//         });

//         var specifier = new PackageSpecifierWithoutVersion { ToothPath = expectedPackage.ToothPath, VariantLabel = "variant" };

//         // Act.
//         var result = await packageManager.GetPackageManifestFromInstalledPackages(specifier);

//         // Assert.
//         Assert.Equal(expectedPackage, result);
//     }

//     [Fact]
//     public async Task GetPackageManifestFromInstalledPackages_NotFound()
//     {
//         // Arrange.
//         var packageManager = PackageManagerFromFiles(new Dictionary<string, MockFileData> {
//             { Path.Join(s_workingDir, "tooth_lock.json"), new MockFileData(s_examplePackageLock.ToJsonBytes()) }
//         });

//         var specifier = new PackageSpecifierWithoutVersion { ToothPath = "example.com/pkg1", VariantLabel = "variant" };

//         // Act.
//         var result = await packageManager.GetPackageManifestFromInstalledPackages(specifier);

//         // Assert.
//         Assert.Null(result);
//     }

//     [Fact]
//     public async Task GetPackageManifestFromSpecifier_Found()
//     {
//         // Arrange.
//         var expectedPackage = s_examplePackage_1;

//         var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>{
//             {
//                 Path.Join(s_cacheDir, "git_repos", "https%3A%2F%2Fexample.com%2Fpkg", "v1.0.0", "tooth.json"),
//                 new MockFileData(expectedPackage.ToJsonBytes())
//             },
//         });

//         Mock<IContext> context = new();
//         context.SetupGet(c => c.FileSystem).Returns(fileSystem);
//         context.SetupGet(c => c.Git).Returns(new Mock<IGit>().Object);

//         var packageManager = PackageManagerFromCxtAndFs(context, fileSystem);

//         // Act.
//         var result = await packageManager.GetPackageManifestFromSpecifier(new PackageSpecifier
//         {
//             ToothPath = expectedPackage.ToothPath,
//             VariantLabel = "",
//             Version = expectedPackage.Version
//         });

//         // Assert.
//         Assert.Equal(expectedPackage, result);
//     }

//     [Fact]
//     public async Task GetPackageManifestFromSpecifier_NotFound()
//     {
//         // Arrange.
//         var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());

//         Mock<IContext> context = new();
//         context.SetupGet(c => c.FileSystem).Returns(fileSystem);
//         context.SetupGet(c => c.Git).Returns(new Mock<IGit>().Object);

//         var packageManager = PackageManagerFromCxtAndFs(context, fileSystem);

//         // Act.
//         var result = await packageManager.GetPackageManifestFromSpecifier(new PackageSpecifier
//         {
//             ToothPath = s_examplePackage_1.ToothPath,
//             VariantLabel = "",
//             Version = s_examplePackage_1.Version
//         });

//         // Assert.
//         Assert.Null(result);
//     }

//     [Fact]
//     public async Task GetPackageManifestFromFileSource_Found()
//     {
//         // Arrange.
//         var expectedPackage = s_examplePackage_1;

//         var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>{
//             { Path.Join(s_workingDir, "tooth.json"),new MockFileData(expectedPackage.ToJsonBytes())}
//         }, s_workingDir);

//         var context = new Mock<IContext>();
//         context.SetupGet(c => c.FileSystem).Returns(fileSystem);

//         var fileSource = new DirectoryFileSource(fileSystem, s_workingDir);

//         var packageManager = PackageManagerFromCxtAndFs(context, fileSystem);

//         // Act.
//         var result = await packageManager.GetPackageManifestFromFileSource(fileSource);

//         // Assert.
//         Assert.Equal(expectedPackage, result);
//     }

//     [Fact]
//     public async Task GetPackageManifestFromFileSource_NotFound()
//     {
//         // Arrange.
//         var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>(), s_workingDir);

//         var context = new Mock<IContext>();
//         context.SetupGet(c => c.FileSystem).Returns(fileSystem);

//         var fileSource = new DirectoryFileSource(fileSystem, s_workingDir);

//         var packageManager = PackageManagerFromCxtAndFs(context, fileSystem);

//         // Act.
//         var result = await packageManager.GetPackageManifestFromFileSource(fileSource);

//         // Assert.
//         Assert.Null(result);
//     }

//     [Fact]
//     public async Task GetPackageRemoteVersions_WithGoModuleProxy()
//     {
//         // Arrange.
//         var expectedVersions = new List<SemVersion> { new(0, 1, 0), new(0, 2, 0), new(0, 3, 0) };
//         var versionFile = string.Join("\n", expectedVersions.Select((ver) => "v" + ver.ToString())) + "\n0.4.0\n15.0.0\nv114514";

//         var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>(), s_workingDir);

//         var downloader = new Mock<IDownloader>();
//         downloader.Setup(d => d.DownloadFile(Url.Parse("https://example.com/example.com/user/repo/@v/list"), It.IsAny<string>()))
//         .Callback<Url, string>((_, destinationPath) =>
//         {
//             fileSystem.AddFile(destinationPath, new MockFileData(versionFile));
//         });

//         var context = new Mock<IContext>();
//         context.SetupGet(c => c.FileSystem).Returns(fileSystem);
//         context.SetupGet(c => c.Downloader).Returns(downloader.Object);

//         var pathManager = new PathManager(fileSystem, baseCacheDir: s_cacheDir, workingDir: s_workingDir);

//         var cacheManager = new CacheManager(context.Object, pathManager, [], []);

//         var packageManager = new PackageManager(context.Object, cacheManager, pathManager, [Url.Parse("https://example.com")]);

//         // Act.
//         var result = await packageManager.GetPackageRemoteVersions(new PackageSpecifierWithoutVersion
//         {
//             ToothPath = "example.com/user/repo",
//             VariantLabel = ""
//         });

//         // Assert.
//         Assert.Equal(expectedVersions, result);
//     }

//     [Fact]
//     public async Task GetPackageRemoteVersions_WithGit()
//     {
//         // Arrange.
//         var expectedVersions = new List<SemVersion> { new(0, 1, 0), new(0, 2, 0), new(0, 3, 0) };

//         var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>(), s_workingDir);

//         var git = new Mock<IGit>();
//         git.Setup(g => g.ListRemote("https://example.com/user/repo", true, true))
//         .Returns(
//             Task<List<ListRemoteResultItem>>.Factory.StartNew(() => [
//                 new (){Sha ="175394eb04c96bd99dc095bbbd337008a9cbffa1" ,Ref = "refs/tags/v0.1.0"},
//                 new (){Sha ="ef73ef6d1aadb96355f13cba845a79727cc52ddd" ,Ref = "refs/tags/v0.2.0"},
//                 new (){Sha ="278d385619bbc5191eb326fee5f89fe6af2b1031" ,Ref = "refs/tags/v0.3.0"},
//                 new (){Sha ="a9e0f95779dcaa218d763a4278813f2298305f07" ,Ref = "refs/pull/101/head"},
//                 new (){Sha ="66fdb3a16edbcc48e7de49f9b786f38680116477" ,Ref = "refs/heads/feat/schema-v3"}
//             ])
//         );

//         var context = new Mock<IContext>();
//         context.SetupGet(c => c.FileSystem).Returns(fileSystem);
//         context.SetupGet(c => c.Git).Returns(git.Object);

//         var pathManager = new PathManager(fileSystem, baseCacheDir: s_cacheDir, workingDir: s_workingDir);

//         var cacheManager = new CacheManager(context.Object, pathManager, [], []);

//         var packageManager = new PackageManager(context.Object, cacheManager, pathManager, []);

//         // Act.
//         var result = await packageManager.GetPackageRemoteVersions(new PackageSpecifierWithoutVersion
//         {
//             ToothPath = "example.com/user/repo",
//             VariantLabel = ""
//         });

//         // Assert.
//         Assert.Equal(expectedVersions, result);
//     }

//     [Fact]
//     public async Task GetPackageRemoteVersions_WithGoModuleProxyFailed_And_NoGit()
//     {
//         // Arrange.
//         var packageSpecifier = new PackageSpecifierWithoutVersion
//         {
//             ToothPath = "example.com/user/repo",
//             VariantLabel = ""
//         };

//         var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>(), s_workingDir);

//         var downloader = new Mock<IDownloader>();
//         downloader.Setup(d => d.DownloadFile(Url.Parse("https://example.com/example.com/user/repo/@v/list"), It.IsAny<string>()))
//         .Callback<Url, string>((_, _) =>
//         {
//             throw new Exception("DownLoad FAIL");
//         });

//         var logger = new Mock<ILogger>();

//         var context = new Mock<IContext>();
//         context.SetupGet(c => c.FileSystem).Returns(fileSystem);
//         context.SetupGet(c => c.Downloader).Returns(downloader.Object);
//         context.SetupGet(c => c.Logger).Returns(logger.Object);

//         var pathManager = new PathManager(fileSystem, baseCacheDir: s_cacheDir, workingDir: s_workingDir);

//         var cacheManager = new CacheManager(context.Object, pathManager, [], []);

//         var packageManager = new PackageManager(context.Object, cacheManager, pathManager, [Url.Parse("https://example.com")]);

//         // Act. & Assert.
//         await Assert.ThrowsAnyAsync<InvalidOperationException>(async () =>
//         {
//             var result = await packageManager.GetPackageRemoteVersions(packageSpecifier);
//         });
//     }

//     [Fact]
//     public async Task Install_ManifestNotFound()
//     {
//         // Arrange.
//         var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>(), s_workingDir);

//         var context = new Mock<IContext>();
//         context.SetupGet(c => c.FileSystem).Returns(fileSystem);

//         var fileSource = new DirectoryFileSource(fileSystem, s_workingDir);

//         var packageManager = PackageManagerFromCxtAndFs(context, fileSystem);

//         // Act.
//         var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
//         {
//             await packageManager.InstallPackage(fileSource, "", false, false, false);
//         });

//         // Assert.
//         Assert.Equal("Package manifest not found.", exception.Message);
//     }

//     [Fact]
//     public async Task Install_NoVariant()
//     {
//         // Arrange.
//         var emptyPack = new PackageManifest()
//         {
//             FormatVersion = PackageManifest.DefaultFormatVersion,
//             FormatUuid = PackageManifest.DefaultFormatUuid,
//             ToothPath = "example.com/pkg",
//             VersionText = "1.0.0",
//         };

//         var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>{
//             {Path.Join(s_cacheDir,"package","tooth.json"),new MockFileData(emptyPack.ToJsonBytes())}
//         }, s_workingDir);

//         var logger = new Mock<ILogger>();

//         var context = new Mock<IContext>();
//         context.SetupGet(c => c.FileSystem).Returns(fileSystem);
//         context.SetupGet(c => c.Logger).Returns(logger.Object);

//         var fileSource = new DirectoryFileSource(fileSystem, Path.Join(s_cacheDir, "package"));

//         var packageManager = PackageManagerFromCxtAndFs(context, fileSystem);

//         // Act.
//         var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
//         {
//             await packageManager.InstallPackage(fileSource, "", false, false, false);
//         });

//         // Assert.
//         Assert.Equal("The package does not contain variant .", exception.Message);
//     }

//     [Theory]
//     [InlineData(true)]
//     [InlineData(false)] // test lock
//     [InlineData(true, "veriants")] // test veriants
//     public async Task Install_EmptyVariant(bool locked, string veriants = "")
//     {
//         // Arrange
//         var emptyPack = new PackageManifest()
//         {
//             FormatVersion = PackageManifest.DefaultFormatVersion,
//             FormatUuid = PackageManifest.DefaultFormatUuid,
//             ToothPath = "example.com/pkg",
//             VersionText = "1.0.0",
//             Variants = [
//                 new() {
//                     VariantLabelRaw = veriants,
//                     Platform = RuntimeInformation.RuntimeIdentifier
//                 }
//             ]
//         };

//         var expectedPackageLock = new PackageLock
//         {
//             Locks = [
//                 new PackageLock.Package()
//                 {
//                     Locked = locked,
//                     Manifest = emptyPack,
//                     VariantLabel = "",
//                     Files = []
//                 }
//             ]
//         };

//         var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>{
//             {Path.Join(s_cacheDir,"package","tooth.json"),new MockFileData(emptyPack.ToJsonBytes())}
//         }, s_workingDir);

//         var logger = new Mock<ILogger>();

//         var context = new Mock<IContext>();
//         context.SetupGet(c => c.FileSystem).Returns(fileSystem);
//         context.SetupGet(c => c.Logger).Returns(logger.Object);

//         var fileSource = new DirectoryFileSource(fileSystem, Path.Join(s_cacheDir, "package"));

//         var packageManager = PackageManagerFromCxtAndFs(context, fileSystem);

//         // Act.
//         await packageManager.InstallPackage(fileSource, veriants, false, false, locked);

//         // Assert.
//         var resultPackageLock = fileSystem.GetFile(Path.Join(s_workingDir, "tooth_lock.json")).Contents;

//         Assert.Equal(expectedPackageLock.ToJsonBytes().ToString(), resultPackageLock.ToString()); // ! I don't know why but the bytes didn't equal
//     }

//     [Fact]
//     public async Task Install_AlreadyInstalled_SameVersion()
//     {
//         // Arrange
//         var emptyPack = new PackageManifest()
//         {
//             FormatVersion = PackageManifest.DefaultFormatVersion,
//             FormatUuid = PackageManifest.DefaultFormatUuid,
//             ToothPath = "example.com/pkg",
//             VersionText = "1.0.0",
//             Variants = [
//                 new() {
//                     VariantLabelRaw = "",
//                     Platform = RuntimeInformation.RuntimeIdentifier
//                 }
//             ]
//         };

//         var expectedPackageLock = new PackageLock
//         {
//             Locks = [
//                 new PackageLock.Package()
//                 {
//                     Locked = true,
//                     Manifest = emptyPack,
//                     VariantLabel = "",
//                     Files = []
//                 }
//             ]
//         };

//         var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>{
//             {Path.Join(s_cacheDir,"package","tooth.json"),new MockFileData(emptyPack.ToJsonBytes())},
//             {Path.Join(s_workingDir,"tooth_lock.json"),new MockFileData(expectedPackageLock.ToJsonBytes())}
//         }, s_workingDir);

//         var logger = new Mock<ILogger>();

//         var context = new Mock<IContext>();
//         context.SetupGet(c => c.FileSystem).Returns(fileSystem);
//         context.SetupGet(c => c.Logger).Returns(logger.Object);

//         var fileSource = new DirectoryFileSource(fileSystem, Path.Join(s_cacheDir, "package"));

//         var packageManager = PackageManagerFromCxtAndFs(context, fileSystem);

//         // Act.
//         await packageManager.InstallPackage(fileSource, "", false, false, true);

//         // Assert.
//         // ?
//     }

//     [Fact]
//     public async Task Install_AlreadyInstalled_OtherVersion()
//     {
//         // Arrange.
//         var emptyPack = new PackageManifest()
//         {
//             FormatVersion = PackageManifest.DefaultFormatVersion,
//             FormatUuid = PackageManifest.DefaultFormatUuid,
//             ToothPath = "example.com/pkg",
//             VersionText = "1.0.0",
//             Variants = [
//                 new() {
//                     VariantLabelRaw = "",
//                     Platform = RuntimeInformation.RuntimeIdentifier
//                 }
//             ]
//         };

//         var packageLock = new PackageLock
//         {
//             Locks = [
//                 new PackageLock.Package()
//                 {
//                     Locked = true,
//                     Manifest = emptyPack with {VersionText = "1.1.0"},
//                     VariantLabel = "",
//                     Files = []
//                 }
//             ]
//         };

//         var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>{
//             {Path.Join(s_cacheDir,"package","tooth.json"),new MockFileData(emptyPack.ToJsonBytes())},
//             {Path.Join(s_workingDir,"tooth_lock.json"),new MockFileData(packageLock.ToJsonBytes())}
//         }, s_workingDir);

//         var logger = new Mock<ILogger>();

//         var commandRunner = new Mock<ICommandRunner>();

//         var context = new Mock<IContext>();
//         context.SetupGet(c => c.FileSystem).Returns(fileSystem);
//         context.SetupGet(c => c.Logger).Returns(logger.Object);

//         var fileSource = new DirectoryFileSource(fileSystem, Path.Join(s_cacheDir, "package"));

//         var packageManager = PackageManagerFromCxtAndFs(context, fileSystem);

//         // Act. & Assert.
//         await Assert.ThrowsAsync<InvalidOperationException>(async () =>
//         {
//             await packageManager.InstallPackage(fileSource, "", false, false, false);
//         });
//     }

//     [Theory]
//     [InlineData(false, false)]
//     [InlineData(true, true)]
//     public async Task InstallPackage_Assets_And_Scripts(bool dryRun, bool ignoreScripts)
//     {
//         // Arrange.
//         var manifest = new PackageManifest
//         {
//             FormatVersion = 3,
//             FormatUuid = "289f771f-2c9a-4d73-9f3f-8492495a924d",
//             ToothPath = "example.com/pkg",
//             VersionText = "1.0.0",
//             Variants =
//             [
//             new()
//             {
//                 Platform = RuntimeInformation.RuntimeIdentifier,
//                 Assets =
//                 [
//                 new()
//                 {
//                     Type = PackageManifest.AssetType.TypeEnum.Self,
//                     Place = [new(){
//                         Type = PackageManifest.PlaceType.TypeEnum.File,
//                         Src = "a.txt",Dest = "a.txt"}
//                         ],
//                 },
//                 new()
//                 {
//                     Type = PackageManifest.AssetType.TypeEnum.Self,
//                     Place = [new(){
//                         Type = PackageManifest.PlaceType.TypeEnum.File,
//                         Src = "a/a.txt",Dest = "a/a.txt"}
//                         ]
//                 },
//                 new()
//                 {
//                     Type = PackageManifest.AssetType.TypeEnum.Self,
//                     Place = [new(){
//                         Type = PackageManifest.PlaceType.TypeEnum.Dir,
//                         Src = "b",Dest = "b"}
//                         ]
//                 },
//                 new()
//                 {
//                     Type = PackageManifest.AssetType.TypeEnum.Uncompressed,
//                     Urls = ["https://example.com/c"],
//                     Place = [new(){
//                         Type = PackageManifest.PlaceType.TypeEnum.File,
//                         Src = "",
//                         Dest = "c"
//                     }]
//                 },
//                 new()
//                 {
//                     Type = PackageManifest.AssetType.TypeEnum.Zip,
//                     Urls = ["https://example.com/d.zip"],
//                     Place = [new(){
//                         Type = PackageManifest.PlaceType.TypeEnum.Dir,
//                         Src = "",
//                         Dest = "d"
//                     }],
//                 },
//                 ],
//                 Preserve = ["d/*"],
//                 Remove = ["d/file2.txt"],
//                 Scripts = new PackageManifest.ScriptsType
//                 {
//                 PreInstall = ["echo pre-install"],
//                 Install = ["echo install"],
//                 PostInstall = ["echo post-install"],
//                 PrePack = ["echo pre-pack"],
//                 PostPack = ["echo post-pack"],
//                 PreUninstall = ["echo pre-uninstall"],
//                 Uninstall = ["echo uninstall"],
//                 PostUninstall = ["echo post-uninstall"],
//                 AdditionalScripts = new Dictionary<string, List<string>>
//                 {
//                     { "same_script", new List<string> { "echo same" } },
//                     { "custom_script1", new List<string> { "echo custom1" } }
//                 }
//                 }
//             }
//             ]
//         };

//         var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>{
//             {Path.Join(s_cacheDir,"package","tooth.json"),new MockFileData(manifest.ToJsonBytes())},
//             {Path.Join(s_cacheDir,"package","a.txt"),new MockFileData("self file")},
//             {Path.Join(s_cacheDir,"package","a","a.txt"),new MockFileData("self file")},
//             {Path.Join(s_cacheDir,"package","b","b.txt"),new MockFileData("self file")},
//             {Path.Join("for-compress","file1.txt"),new MockFileData("from compress")},
//             {Path.Join("for-compress","file2.txt"),new MockFileData("from compress")},
//         }, s_workingDir);

//         var logger = new Mock<ILogger>();

//         var commandRunner = new Mock<ICommandRunner>();

//         var downloader = new Mock<IDownloader>();
//         downloader.Setup(d => d.DownloadFile(Url.Parse("https://example.com/c"), It.IsAny<string>()))
//         .Callback<Url, string>((_, destinationPath) =>
//         {
//             fileSystem.AddFile(destinationPath, new MockFileData("c"));
//         });
//         downloader.Setup(d => d.DownloadFile(Url.Parse("https://example.com/d.zip"), It.IsAny<string>()))
//         .Callback<Url, string>((_, destinationPath) =>
//         {
//             fileSystem.AddFile(destinationPath, new MockFileData([]));

//             using var archive = SharpCompress.Archives.Zip.ZipArchive.Create();
//             archive.AddEntry("file1.txt", fileSystem.File.OpenRead(Path.Join("for-compress", "file1.txt")));
//             archive.AddEntry("file2.txt", fileSystem.File.OpenRead(Path.Join("for-compress", "file2.txt")));
//             archive.SaveTo(fileSystem.File.OpenWrite(destinationPath));
//         });

//         var context = new Mock<IContext>();
//         context.SetupGet(c => c.FileSystem).Returns(fileSystem);
//         context.SetupGet(c => c.Logger).Returns(logger.Object);
//         context.SetupGet(c => c.CommandRunner).Returns(commandRunner.Object);
//         context.SetupGet(c => c.Downloader).Returns(downloader.Object);

//         var fileSource = new DirectoryFileSource(fileSystem, Path.Join(s_cacheDir, "package"));

//         var pathManager = new PathManager(fileSystem, baseCacheDir: s_cacheDir, workingDir: s_workingDir);

//         var cacheManager = new CacheManager(context.Object, pathManager, [], []);

//         var packageManager = new PackageManager(context.Object, cacheManager, pathManager, []);

//         // Act.
//         await packageManager.InstallPackage(fileSource, "", dryRun, ignoreScripts, false);

//         // Assert.
//         // == Check Filse ==
//         var fileFormSelf1 = fileSystem.GetFile(Path.Join(s_workingDir, "a.txt"));
//         var fileFormSelf2 = fileSystem.GetFile(Path.Join(s_workingDir, "a", "a.txt"));
//         var fileFormSelf3 = fileSystem.GetFile(Path.Join(s_workingDir, "b", "b.txt"));
//         var fileFormDownLoadUncompressed = fileSystem.GetFile(Path.Join(s_workingDir, "c"));
//         var fileFormDownLoadZip1 = fileSystem.GetFile(Path.Join(s_workingDir, "d", "file1.txt"));
//         var fileFormDownLoadZip2 = fileSystem.GetFile(Path.Join(s_workingDir, "d", "file2.txt"));

//         var locks = await packageManager.GetCurrentPackageLock();

//         if (!dryRun)
//         {
//             Assert.Equal(new MockFileData("self file").Contents, fileFormSelf1.Contents);
//             Assert.NotNull(fileFormSelf2);
//             Assert.Equal(new MockFileData("self file").Contents, fileFormSelf3.Contents);
//             Assert.NotNull(fileFormDownLoadUncompressed);
//             Assert.Equal(new MockFileData("from compress").Contents, fileFormDownLoadZip1.Contents);
//             Assert.NotNull(fileFormDownLoadZip2);

//             var installedFilesRecod = locks.Locks
//                 .First().Files
//                 .Select(p => Path.Join(s_workingDir, p))
//                 .Select(Path.GetFullPath);

//             Assert.Equal(new List<string>(){
//                 Path.Join(s_workingDir, "a.txt"),
//                 Path.Join(s_workingDir, "a", "a.txt"),
//                 Path.Join(s_workingDir, "b", "b.txt"),
//                 Path.Join(s_workingDir, "c"),
//                 Path.Join(s_workingDir, "d", "file1.txt"),
//                 Path.Join(s_workingDir, "d", "file2.txt"),
//             }.Select(Path.GetFullPath)
//             , installedFilesRecod);

//         }
//         else
//         {
//             Assert.Null(fileFormSelf1);
//         }

//         //  == Check Commands ==
//         if (!dryRun)
//         {
//             commandRunner.Verify(c => c.Run("echo pre-install", s_workingDir), Times.Once);
//             commandRunner.Verify(c => c.Run("echo install", s_workingDir), Times.Once);
//             commandRunner.Verify(c => c.Run("echo post-install", s_workingDir), Times.Once);
//             commandRunner.Verify(c => c.Run(It.IsNotIn("echo pre-install", "echo install", "echo post-install"), s_workingDir), Times.Never);
//         }
//         else
//         {
//             commandRunner.Verify(c => c.Run(It.IsAny<string>(), s_workingDir), Times.Never);
//         }
//         commandRunner.Verify(c => c.Run(It.IsAny<string>(), It.IsNotIn(s_workingDir)), Times.Never);

//     }

//     [Fact]
//     public async Task InstallPackage_File_Already_Exists()
//     {
//         // Arrange.
//         var manifest = new PackageManifest
//         {
//             FormatVersion = 3,
//             FormatUuid = "289f771f-2c9a-4d73-9f3f-8492495a924d",
//             ToothPath = "example.com/pkg",
//             VersionText = "1.0.0",
//             Variants =
//             [
//             new()
//             {
//                 Platform = RuntimeInformation.RuntimeIdentifier,
//                 Assets =
//                 [
//                 new()
//                 {
//                     Type = PackageManifest.AssetType.TypeEnum.Self,
//                     Place = [new(){
//                         Type = PackageManifest.PlaceType.TypeEnum.File,
//                         Src = "a.txt",Dest = "a.txt"}
//                         ]
//                 },
//                 ],
//                 Scripts = new PackageManifest.ScriptsType
//                 {
//                 PreInstall = ["echo pre-install"],
//                 Install = ["echo install"],
//                 PostInstall = ["echo post-install"],
//                 PrePack = ["echo pre-pack"],
//                 PostPack = ["echo post-pack"],
//                 PreUninstall = ["echo pre-uninstall"],
//                 Uninstall = ["echo uninstall"],
//                 PostUninstall = ["echo post-uninstall"],
//                 AdditionalScripts = new Dictionary<string, List<string>>
//                 {
//                     { "same_script", new List<string> { "echo same" } },
//                     { "custom_script1", new List<string> { "echo custom1" } }
//                 }
//                 }
//             }
//             ]
//         };

//         var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>{
//             {Path.Join(s_cacheDir,"package","tooth.json"),new MockFileData(manifest.ToJsonBytes())},
//             {Path.Join(s_cacheDir,"package","a.txt"),new MockFileData("self file")},
//             {Path.Join(s_cacheDir,"package","a","a.txt"),new MockFileData("self file")},
//             {Path.Join(s_cacheDir,"package","b","b.txt"),new MockFileData("self file")},
//             {Path.Join(s_workingDir,"a.txt"),new MockFileData("already exists")}
//         }, s_workingDir);

//         var logger = new Mock<ILogger>();

//         var commandRunner = new Mock<ICommandRunner>();

//         var context = new Mock<IContext>();
//         context.SetupGet(c => c.FileSystem).Returns(fileSystem);
//         context.SetupGet(c => c.Logger).Returns(logger.Object);
//         context.SetupGet(c => c.CommandRunner).Returns(commandRunner.Object);

//         var fileSource = new DirectoryFileSource(fileSystem, Path.Join(s_cacheDir, "package"));

//         var pathManager = new PathManager(fileSystem, baseCacheDir: s_cacheDir, workingDir: s_workingDir);

//         var cacheManager = new CacheManager(context.Object, pathManager, [], []);

//         var packageManager = new PackageManager(context.Object, cacheManager, pathManager, []);

//         // Act.
//         await Assert.ThrowsAsync<InvalidOperationException>(
//             async () => await packageManager.InstallPackage(fileSource, "", false, false, false)
//         );
//         // Assert.

//         var fileNotChange = fileSystem.GetFile(Path.Join(s_workingDir, "a.txt"));

//         Assert.Equal(new MockFileData("already exists").Contents, fileNotChange.Contents);
//     }

//     [Theory]
//     [InlineData(false, false)]
//     [InlineData(true, true)]
//     public async Task Uninstall(bool dryRun, bool ignoreScripts)
//     {
//         // Arrange.
//         var manifest = new PackageManifest
//         {
//             FormatVersion = 3,
//             FormatUuid = "289f771f-2c9a-4d73-9f3f-8492495a924d",
//             ToothPath = "example.com/pkg",
//             VersionText = "1.0.0",
//             Variants =
//             [
//             new()
//             {
//                 Platform = RuntimeInformation.RuntimeIdentifier,
//                 Assets =
//                 [
//                 new()
//                 {
//                     Type = PackageManifest.AssetType.TypeEnum.Self,
//                     Place = [new(){
//                         Type = PackageManifest.PlaceType.TypeEnum.File,
//                         Src = "a.txt",Dest = "a.txt"}
//                         ],
//                 },
//                 new()
//                 {
//                     Type = PackageManifest.AssetType.TypeEnum.Self,
//                     Place = [new(){
//                         Type = PackageManifest.PlaceType.TypeEnum.File,
//                         Src = "a/a.txt",Dest = "a/a.txt"}
//                         ]
//                 },
//                 new()
//                 {
//                     Type = PackageManifest.AssetType.TypeEnum.Self,
//                     Place = [new(){
//                         Type = PackageManifest.PlaceType.TypeEnum.Dir,
//                         Src = "b",Dest = "b"}
//                         ]
//                 },
//                 new()
//                 {
//                     Type = PackageManifest.AssetType.TypeEnum.Uncompressed,
//                     Urls = ["https://example.com/c"],
//                     Place = [new(){
//                         Type = PackageManifest.PlaceType.TypeEnum.File,
//                         Src = "",
//                         Dest = "c"
//                     }]
//                 },
//                 new()
//                 {
//                     Type = PackageManifest.AssetType.TypeEnum.Zip,
//                     Urls = ["https://example.com/d.zip"],
//                     Place = [new(){
//                         Type = PackageManifest.PlaceType.TypeEnum.Dir,
//                         Src = "",
//                         Dest = "d"
//                     }],

//                 },
//                 ],
//                 Preserve = ["d/file1.txt"],
//                 Remove = ["d/file2.txt"],
//                 Scripts = new PackageManifest.ScriptsType
//                 {
//                 PreInstall = ["echo pre-install"],
//                 Install = ["echo install"],
//                 PostInstall = ["echo post-install"],
//                 PrePack = ["echo pre-pack"],
//                 PostPack = ["echo post-pack"],
//                 PreUninstall = ["echo pre-uninstall"],
//                 Uninstall = ["echo uninstall"],
//                 PostUninstall = ["echo post-uninstall"],
//                 AdditionalScripts = new Dictionary<string, List<string>>
//                 {
//                     { "same_script", new List<string> { "echo same" } },
//                     { "custom_script1", new List<string> { "echo custom1" } }
//                 }
//                 }
//             }
//             ]
//         };

//         var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>{
//             {Path.Join(s_cacheDir,"package","tooth.json"),new MockFileData(manifest.ToJsonBytes())},
//             {Path.Join(s_cacheDir,"package","a.txt"),new MockFileData("self file")},
//             {Path.Join(s_cacheDir,"package","a","a.txt"),new MockFileData("self file")},
//             {Path.Join(s_cacheDir,"package","b","b.txt"),new MockFileData("self file")},
//             {Path.Join("for-compress","file1.txt"),new MockFileData("from compress")},
//             {Path.Join("for-compress","file2.txt"),new MockFileData("from compress")},
//             {Path.Join(s_workingDir,"userfile"),new MockFileData("user file") }
//         }, s_workingDir);

//         var logger = new Mock<ILogger>();

//         var commandRunner = new Mock<ICommandRunner>();

//         var downloader = new Mock<IDownloader>();
//         downloader.Setup(d => d.DownloadFile(Url.Parse("https://example.com/c"), It.IsAny<string>()))
//         .Callback<Url, string>((_, destinationPath) =>
//         {
//             fileSystem.AddFile(destinationPath, new MockFileData("c"));
//         });
//         downloader.Setup(d => d.DownloadFile(Url.Parse("https://example.com/d.zip"), It.IsAny<string>()))
//         .Callback<Url, string>((_, destinationPath) =>
//         {
//             fileSystem.AddFile(destinationPath, new MockFileData([]));

//             using var archive = SharpCompress.Archives.Zip.ZipArchive.Create();
//             archive.AddEntry("file1.txt", fileSystem.File.OpenRead(Path.Join("for-compress", "file1.txt")));
//             archive.AddEntry("file2.txt", fileSystem.File.OpenRead(Path.Join("for-compress", "file2.txt")));
//             archive.SaveTo(fileSystem.File.OpenWrite(destinationPath));
//         });

//         var context = new Mock<IContext>();
//         context.SetupGet(c => c.FileSystem).Returns(fileSystem);
//         context.SetupGet(c => c.Logger).Returns(logger.Object);
//         context.SetupGet(c => c.CommandRunner).Returns(commandRunner.Object);
//         context.SetupGet(c => c.Downloader).Returns(downloader.Object);

//         var fileSource = new DirectoryFileSource(fileSystem, Path.Join(s_cacheDir, "package"));

//         var pathManager = new PathManager(fileSystem, baseCacheDir: s_cacheDir, workingDir: s_workingDir);

//         var cacheManager = new CacheManager(context.Object, pathManager, [], []);

//         var packageManager = new PackageManager(context.Object, cacheManager, pathManager, []);

//         await packageManager.InstallPackage(fileSource, "", false, true, false);

//         // Act.
//         await packageManager.UninstallPackage(new PackageSpecifierWithoutVersion()
//         {
//             VariantLabel = manifest.Variants.First().VariantLabel,
//             ToothPath = manifest.ToothPath,
//         }, dryRun, ignoreScripts);

//         // Assert.
//         // == Check Filse ==
//         var fileFormSelf1 = fileSystem.GetFile(Path.Join(s_workingDir, "a.txt"));
//         var fileFormSelf2 = fileSystem.GetFile(Path.Join(s_workingDir, "a", "a.txt"));
//         var dirA = fileSystem.GetFile(Path.Join(s_workingDir, "a"));
//         var fileFormSelf3 = fileSystem.GetFile(Path.Join(s_workingDir, "b", "b.txt"));
//         var fileFormDownLoadUncompressed = fileSystem.GetFile(Path.Join(s_workingDir, "c"));
//         var fileFormDownLoadZip1 = fileSystem.GetFile(Path.Join(s_workingDir, "d", "file1.txt"));
//         var fileFormDownLoadZip2 = fileSystem.GetFile(Path.Join(s_workingDir, "d", "file2.txt"));
//         var userFile = fileSystem.GetFile(Path.Join(s_workingDir, "userfile"));

//         var locks = await packageManager.GetCurrentPackageLock();

//         if (!dryRun)
//         {
//             Assert.Null(fileFormSelf1);
//             Assert.Null(fileFormSelf2);
//             Assert.Null(dirA);
//             Assert.Null(fileFormSelf3);
//             Assert.Null(fileFormDownLoadUncompressed);
//             Assert.NotNull(fileFormDownLoadZip1);
//             Assert.Null(fileFormDownLoadZip2);
//             Assert.NotNull(userFile);
//         }
//         else
//         {
//             Assert.NotNull(fileFormSelf1);
//         }

//         //  == Check Commands ==
//         if (!dryRun)
//         {
//             commandRunner.Verify(c => c.Run("echo pre-uninstall", s_workingDir), Times.Once);
//             commandRunner.Verify(c => c.Run("echo uninstall", s_workingDir), Times.Once);
//             commandRunner.Verify(c => c.Run("echo post-uninstall", s_workingDir), Times.Once);
//             commandRunner.Verify(c => c.Run(It.IsNotIn("echo pre-uninstall", "echo uninstall", "echo post-uninstall"), s_workingDir), Times.Never);
//         }
//         else
//         {
//             commandRunner.Verify(c => c.Run(It.IsAny<string>(), s_workingDir), Times.Never);
//         }
//         commandRunner.Verify(c => c.Run(It.IsAny<string>(), It.IsNotIn(s_workingDir)), Times.Never);

//     }

//     [Fact]
//     public async Task Uninstall_PackNotFound()
//     {
//         // Arrange.
//         var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>(), s_workingDir);

//         var logger = new Mock<ILogger>();

//         var context = new Mock<IContext>();
//         context.SetupGet(c => c.FileSystem).Returns(fileSystem);
//         context.SetupGet(c => c.Logger).Returns(logger.Object);

//         var fileSource = new DirectoryFileSource(fileSystem, Path.Join(s_cacheDir, "package"));

//         var packageManager = PackageManagerFromCxtAndFs(context, fileSystem);

//         // Act.
//         await packageManager.UninstallPackage(new PackageSpecifierWithoutVersion()
//         {
//             VariantLabel = "",
//             ToothPath = "exampel.com/pkg",
//         }, true, true);

//         // Assert.
//         // ?
//     }

// }
