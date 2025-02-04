using System.IO.Abstractions.TestingHelpers;
using System.Runtime.InteropServices;
using Lip.Context;
using Moq;

namespace Lip.Tests;

public class LipRunTests
{
    private static readonly string s_workDir = OperatingSystem.IsWindows()
        ? Path.Join("C:", "path", "to", "work")
        : Path.Join("/", "path", "to", "work");

    [Fact]
    public void RunArgs_Constructor_TrivialValues_Passes()
    {
        // Arrange.
        Lip.RunArgs runArgs = new();

        // Act.
        runArgs = runArgs with { };
    }

    [Fact]
    public async Task Run_ValidScript_Passes()
    {
        // Arrange.
        string packageManifestData = $$"""
            {
                "format_version": 3,
                "format_uuid": "289f771f-2c9a-4d73-9f3f-8492495a924d",
                "tooth": "example.com/repo",
                "version": "1.0.0",
                "variants": [
                    {
                        "label": "test_variant",
                        "platform": "{{RuntimeInformation.RuntimeIdentifier}}",
                        "scripts": {
                            "test": [
                                "run_test"
                            ]
                        }
                    }
                ]
            }
            """;

        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { "/path/to/work/tooth.json", new MockFileData(packageManifestData) },
        }, currentDirectory: s_workDir);

        Mock<ICommandRunner> commandRunner = new();
        commandRunner.Setup(c => c.Run("run_test", s_workDir)).ReturnsAsync(0);

        Mock<IContext> context = new();
        context.SetupGet(c => c.CommandRunner).Returns(commandRunner.Object);
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        Lip lip = new(new(), context.Object);

        // Act.
        int code = await lip.Run("test", new()
        {
            VariantLabel = "test_variant"
        });

        // Arrange.
        Assert.Equal(0, code);
    }

    [Fact]
    public async Task Run_PackageManifestNotExists_ThrowsFileNotFoundException()
    {
        // Arrange.
        MockFileSystem fileSystem = new();

        Mock<IContext> context = new();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        Lip lip = new(new(), context.Object);

        // Act & Assert.
        await Assert.ThrowsAsync<InvalidOperationException>(() => lip.Run("test", new()));
    }

    [Fact]
    public async Task Run_VariantNotFound_ThrowsInvalidOperationException()
    {
        // Arrange.
        string packageManifestData = $$"""
            {
                "format_version": 3,
                "format_uuid": "289f771f-2c9a-4d73-9f3f-8492495a924d",
                "tooth": "example.com/repo",
                "version": "1.0.0",
                "variants": [
                    {
                        "label": "test_variant",
                        "platform": "{{RuntimeInformation.RuntimeIdentifier}}",
                        "scripts": {
                            "test": [
                                "run_test"
                            ]
                        }
                    }
                ]
            }
            """;

        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { "/path/to/work/tooth.json", new MockFileData(packageManifestData) },
        }, currentDirectory: s_workDir);

        Mock<IContext> context = new();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        Lip lip = new(new(), context.Object);

        // Act & Assert.
        await Assert.ThrowsAsync<InvalidOperationException>(() => lip.Run("test", new()
        {
            VariantLabel = "unknown_variant"
        }));
    }

    [Fact]
    public async Task Run_ScriptNotFound_ThrowsInvalidOperationException()
    {
        // Arrange.
        string packageManifestData = $$"""
            {
                "format_version": 3,
                "format_uuid": "289f771f-2c9a-4d73-9f3f-8492495a924d",
                "tooth": "example.com/repo",
                "version": "1.0.0",
                "variants": [
                    {
                        "platform": "{{RuntimeInformation.RuntimeIdentifier}}",
                        "scripts": {
                            "test": [
                                "run_test"
                            ]
                        }
                    }
                ]
            }
            """;

        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { "/path/to/work/tooth.json", new MockFileData(packageManifestData) },
        }, currentDirectory: s_workDir);

        Mock<IContext> context = new();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        Lip lip = new(new(), context.Object);

        // Act & Assert.
        await Assert.ThrowsAsync<InvalidOperationException>(() => lip.Run("unknown_script", new()));
    }

    [Fact]
    public async Task Run_CommandFails_ReturnsErrorCode()
    {
        // Arrange.
        string packageManifestData = $$"""
            {
                "format_version": 3,
                "format_uuid": "289f771f-2c9a-4d73-9f3f-8492495a924d",
                "tooth": "example.com/repo",
                "version": "1.0.0",
                "variants": [
                    {
                        "platform": "{{RuntimeInformation.RuntimeIdentifier}}",
                        "scripts": {
                            "test": [
                                "run_test"
                            ]
                        }
                    }
                ]
            }
            """;

        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>
        {
            { "/path/to/work/tooth.json", new MockFileData(packageManifestData) },
        }, currentDirectory: s_workDir);

        Mock<ICommandRunner> commandRunner = new();
        commandRunner.Setup(c => c.Run("run_test", s_workDir)).ReturnsAsync(1);

        Mock<IContext> context = new();
        context.SetupGet(c => c.CommandRunner).Returns(commandRunner.Object);
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        Lip lip = new(new(), context.Object);

        // Act.
        int code = await lip.Run("test", new());

        // Arrange.
        Assert.Equal(1, code);
    }
}
