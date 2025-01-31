using System.IO.Abstractions.TestingHelpers;
using System.Runtime.InteropServices;
using System.Text;
using Lip.Context;
using Microsoft.Extensions.Logging;
using Moq;
using SharpCompress.Readers;

namespace Lip.Tests;

public class LipPackTests
{
    private static readonly string s_workspacePath = OperatingSystem.IsWindows()
        ? Path.Join("C:", "path", "to", "workspace")
        : Path.Join("/", "path", "to", "workspace");

    [Fact]
    public void PackArgs_Constructor_TrivialValues_Passes()
    {
        // Arrange.
        Lip.PackArgs packArgs = new();

        // Act.
        packArgs = packArgs with { };
    }

    [Fact]
    public async Task Pack_NoPackageManifest_ThrowsInvalidOperationException()
    {
        // Arrange.
        MockFileSystem fileSystem = new();

        Mock<IContext> context = new();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        Lip lip = new(new(), context.Object);

        // Act & assert.
        await Assert.ThrowsAsync<InvalidOperationException>(() => lip.Pack("output", new()));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public async Task Pack_DefaultArguments_CreatesArchive(int variantListIndex)
    {
        // Arrange.
        List<PackageManifest.VariantType> variants = variantListIndex switch
        {
            0 => [],
            1 => [
                new(){
                    Platform = RuntimeInformation.RuntimeIdentifier,
                }],
            2 => [new(){
                Platform = RuntimeInformation.RuntimeIdentifier,
                Assets = [
                    new(){
                        Type = PackageManifest.AssetType.TypeEnum.Self,
                    },
                    new(){
                        Type = PackageManifest.AssetType.TypeEnum.Self,
                        Place = [
                            new(){
                                Type = PackageManifest.PlaceType.TypeEnum.File,
                                Src = "file1",
                                Dest = "file1"
                            },
                            new(){
                                Type = PackageManifest.PlaceType.TypeEnum.Dir,
                                Src = "dir",
                                Dest = "dir"
                            }
                        ]
                    }
                ]
            }],
            3 => [new(){
                Platform = RuntimeInformation.RuntimeIdentifier,
                Scripts = new()
            }],
            4 => [new(){
                Platform = RuntimeInformation.RuntimeIdentifier,
                Scripts = new()
                {
                    PrePack = [
                        "echo 'pre-pack script'"
                    ],
                    PostPack = [
                        "echo 'post-pack script'"
                    ]
                }
            }],
            _ => throw new NotImplementedException()
        };

        PackageManifest packageManifest = new()
        {
            FormatVersion = PackageManifest.DefaultFormatVersion,
            FormatUuid = PackageManifest.DefaultFormatUuid,
            ToothPath = "example.com/pkg",
            VersionText = "1.0.0",
            Variants = variants
        };

        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
        {
                Path.Join(s_workspacePath, "tooth.json"),
                new MockFileData(packageManifest.ToJsonBytes())
        },
        {
                Path.Join(s_workspacePath, "file1"),
                new MockFileData("file1")
        },
        {
                Path.Join(s_workspacePath, "dir", "file2"),
                new MockFileData("file2")
        },
        {
                Path.Join(s_workspacePath, "dir", "dir", "file3"),
                new MockFileData("file3")
        }
        }, s_workspacePath);

        Mock<ICommandRunner> commandRunner = new();
        commandRunner.Setup(c => c.Run(It.IsAny<string>(), It.IsAny<string>())).Verifiable();

        Mock<ILogger> logger = new();

        Mock<IContext> context = new();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);
        context.SetupGet(c => c.CommandRunner).Returns(commandRunner.Object);
        context.SetupGet(c => c.Logger).Returns(logger.Object);

        Lip lip = new(new(), context.Object);

        // Act.
        await lip.Pack("output", new());

        // Assert.
        Assert.True(fileSystem.File.Exists("output"));
        Assert.Equal(
            Encoding.UTF8.GetString(packageManifest.ToJsonBytes()),
            ReadArchiveEntryContent(fileSystem, "output", "tooth.json"));

        if (variantListIndex == 2)
        {
            Assert.Equal("file1", ReadArchiveEntryContent(fileSystem, "output", "file1"));
            Assert.Equal("file2", ReadArchiveEntryContent(fileSystem, "output", "dir/file2"));
            Assert.Equal("file3", ReadArchiveEntryContent(fileSystem, "output", "dir/dir/file3"));
        }

        if (variantListIndex == 4)
        {
            commandRunner.Verify(c => c.Run("echo 'pre-pack script'", s_workspacePath), Times.Once);
            commandRunner.Verify(c => c.Run("echo 'post-pack script'", s_workspacePath), Times.Once);
        }
    }

    [Fact]
    public async Task Pack_GlobFileMatches_CreatesArchive()
    {
        // Arrange.
        PackageManifest packageManifest = new()
        {
            FormatVersion = PackageManifest.DefaultFormatVersion,
            FormatUuid = PackageManifest.DefaultFormatUuid,
            ToothPath = "example.com/pkg",
            VersionText = "1.0.0",
            Variants =
            [
                new()
                {
                    Platform = RuntimeInformation.RuntimeIdentifier,
                    Assets =
                    [
                        new()
                        {
                            Type = PackageManifest.AssetType.TypeEnum.Self,
                            Place =
                            [
                                new()
                                {
                                    Type = PackageManifest.PlaceType.TypeEnum.File,
                                    Src = "*.txt",
                                    Dest = "files"
                                }
                            ]
                        }
                    ]
                }
            ]
        };

        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
        {
                Path.Join(s_workspacePath, "tooth.json"),
                new MockFileData(packageManifest.ToJsonBytes())
        },
        {
                Path.Join(s_workspacePath, "file1.txt"),
                new MockFileData("file1")
        },
        {
                Path.Join(s_workspacePath, "file2.txt"),
                new MockFileData("file2")
        },
        {
                Path.Join(s_workspacePath, "file3.md"),
                new MockFileData("file3")
        }
        }, s_workspacePath);

        Mock<ICommandRunner> commandRunner = new();
        commandRunner.Setup(c => c.Run(It.IsAny<string>(), It.IsAny<string>())).Verifiable();

        Mock<ILogger> logger = new();

        Mock<IContext> context = new();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);
        context.SetupGet(c => c.CommandRunner).Returns(commandRunner.Object);
        context.SetupGet(c => c.Logger).Returns(logger.Object);

        Lip lip = new(new(), context.Object);

        // Act.
        await lip.Pack("output", new());

        // Assert.
        Assert.True(fileSystem.File.Exists("output"));
        Assert.Equal(
            Encoding.UTF8.GetString(packageManifest.ToJsonBytes()),
            ReadArchiveEntryContent(fileSystem, "output", "tooth.json"));
        Assert.Equal("file1", ReadArchiveEntryContent(fileSystem, "output", "file1.txt"));
        Assert.Equal("file2", ReadArchiveEntryContent(fileSystem, "output", "file2.txt"));
        Assert.Null(ReadArchiveEntryContent(fileSystem, "output", "file3.md"));
    }

    [Fact]
    public async Task Pack_InvalidPlaceType_ThrowsNotImplementedException()
    {
        // Arrange.
        PackageManifest packageManifest = new()
        {
            FormatVersion = PackageManifest.DefaultFormatVersion,
            FormatUuid = PackageManifest.DefaultFormatUuid,
            ToothPath = "example.com/pkg",
            VersionText = "1.0.0",
            Variants =
            [
                new()
                {
                    Platform = RuntimeInformation.RuntimeIdentifier,
                    Assets =
                    [
                        new()
                        {
                            Type = PackageManifest.AssetType.TypeEnum.Self,
                            Place =
                            [
                                new()
                                {
                                    Type = (PackageManifest.PlaceType.TypeEnum)int.MaxValue,
                                    Src = "file1",
                                    Dest = "file1"
                                }
                            ]
                        }
                    ]
                }
            ]
        };

        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
        {
                Path.Join(s_workspacePath, "tooth.json"),
                new MockFileData(packageManifest.ToJsonBytes())
        }
        }, s_workspacePath);

        Mock<ILogger> logger = new();

        Mock<IContext> context = new();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);
        context.SetupGet(c => c.Logger).Returns(logger.Object);

        Lip lip = new(new(), context.Object);

        // Act & assert.
        await Assert.ThrowsAsync<NotImplementedException>(() => lip.Pack("output", new()));
    }

    [Fact]
    public async Task Pack_DryRun_NotRunsCommandsNorCreatesFile()
    {
        // Arrange.
        PackageManifest packageManifest = new()
        {
            FormatVersion = PackageManifest.DefaultFormatVersion,
            FormatUuid = PackageManifest.DefaultFormatUuid,
            ToothPath = "example.com/pkg",
            VersionText = "1.0.0",
            Variants =
            [
                new()
                {
                    Platform = RuntimeInformation.RuntimeIdentifier,
                    Scripts = new()
                    {
                        PrePack =
                        [
                            "echo 'pre-pack script'"
                        ]
                    }
                }
            ]
        };

        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
        {
                Path.Join(s_workspacePath, "tooth.json"),
                new MockFileData(packageManifest.ToJsonBytes())
        },
        }, s_workspacePath);

        Mock<ICommandRunner> commandRunner = new();
        commandRunner.Setup(c => c.Run(It.IsAny<string>(), It.IsAny<string>())).Verifiable();

        Mock<ILogger> logger = new();

        Mock<IContext> context = new();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);
        context.SetupGet(c => c.CommandRunner).Returns(commandRunner.Object);
        context.SetupGet(c => c.Logger).Returns(logger.Object);

        Lip lip = new(new(), context.Object);

        // Act.
        await lip.Pack("output", new()
        {
            DryRun = true
        });

        // Assert.
        Assert.False(fileSystem.File.Exists("output"));
        commandRunner.Verify(c => c.Run(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Pack_IgnoreScripts_NotRunsCommands()
    {
        // Arrange.
        PackageManifest packageManifest = new()
        {
            FormatVersion = PackageManifest.DefaultFormatVersion,
            FormatUuid = PackageManifest.DefaultFormatUuid,
            ToothPath = "example.com/pkg",
            VersionText = "1.0.0",
            Variants =
            [
                new()
                {
                    Platform = RuntimeInformation.RuntimeIdentifier,
                    Scripts = new()
                    {
                        PrePack =
                        [
                            "echo 'pre-pack script'"
                        ]
                    }
                }
            ]
        };

        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
        {
                Path.Join(s_workspacePath, "tooth.json"),
                new MockFileData(packageManifest.ToJsonBytes())
        },
        }, s_workspacePath);

        Mock<ICommandRunner> commandRunner = new();
        commandRunner.Setup(c => c.Run(It.IsAny<string>(), It.IsAny<string>())).Verifiable();

        Mock<ILogger> logger = new();

        Mock<IContext> context = new();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);
        context.SetupGet(c => c.CommandRunner).Returns(commandRunner.Object);
        context.SetupGet(c => c.Logger).Returns(logger.Object);

        Lip lip = new(new(), context.Object);

        // Act.
        await lip.Pack("output", new()
        {
            IgnoreScripts = true
        });

        // Assert.
        Assert.True(fileSystem.File.Exists("output"));
        Assert.Equal(
            Encoding.UTF8.GetString(packageManifest.ToJsonBytes()),
            ReadArchiveEntryContent(fileSystem, "output", "tooth.json"));
        commandRunner.Verify(c => c.Run(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData(Lip.PackArgs.ArchiveFormatType.Zip)]
    [InlineData(Lip.PackArgs.ArchiveFormatType.Tar)]
    [InlineData(Lip.PackArgs.ArchiveFormatType.TarGz)]
    public async Task Pack_DifferentArchiveFormats_CreatesArchive(Lip.PackArgs.ArchiveFormatType archiveFormatType)
    {
        // Arrange.
        PackageManifest packageManifest = new()
        {
            FormatVersion = PackageManifest.DefaultFormatVersion,
            FormatUuid = PackageManifest.DefaultFormatUuid,
            ToothPath = "example.com/pkg",
            VersionText = "1.0.0",
        };

        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
        {
                Path.Join(s_workspacePath, "tooth.json"),
                new MockFileData(packageManifest.ToJsonBytes())
        },
        }, s_workspacePath);

        Mock<IContext> context = new();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        Lip lip = new(new(), context.Object);

        // Act.
        await lip.Pack("output", new()
        {
            ArchiveFormat = archiveFormatType
        });

        // Assert.
        Assert.True(fileSystem.File.Exists("output"));
        Assert.Equal(
            Encoding.UTF8.GetString(packageManifest.ToJsonBytes()),
            ReadArchiveEntryContent(fileSystem, "output", "tooth.json"));
    }

    [Fact]
    public async Task Pack_InvalidArchiveFormat_ThrowsNotImplementedException()
    {
        // Arrange.
        PackageManifest packageManifest = new()
        {
            FormatVersion = PackageManifest.DefaultFormatVersion,
            FormatUuid = PackageManifest.DefaultFormatUuid,
            ToothPath = "example.com/pkg",
            VersionText = "1.0.0",
        };

        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
        {
                Path.Join(s_workspacePath, "tooth.json"),
                new MockFileData(packageManifest.ToJsonBytes())
        },
        }, s_workspacePath);

        Mock<IContext> context = new();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        Lip lip = new(new(), context.Object);

        // Act & assert.
        await Assert.ThrowsAsync<NotImplementedException>(() => lip.Pack("output", new()
        {
            ArchiveFormat = (Lip.PackArgs.ArchiveFormatType)int.MaxValue
        }));
    }

    private static string? ReadArchiveEntryContent(
        MockFileSystem fileSystem,
        string archiveFileRelativePath,
        string entryName)
    {
        using Stream archiveFileStream = fileSystem.File.OpenRead(archiveFileRelativePath);

        using IReader reader = ReaderFactory.Open(archiveFileStream);

        while (reader.MoveToNextEntry())
        {
            if (reader.Entry.Key == entryName)
            {
                using MemoryStream memoryStream = new();

                reader.WriteEntryTo(memoryStream);

                memoryStream.Position = 0;

                using StreamReader streamReader = new(memoryStream);

                return streamReader.ReadToEnd();
            }
        }

        return null;
    }
}
