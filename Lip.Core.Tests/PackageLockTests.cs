using System.Runtime.InteropServices;
using System.Text;

namespace Lip.Core.Tests;

public class PackageLockTests
{
    private static readonly List<string> _defaultFiles = [
        "file1.txt",
        "file2.txt"
    ];

    private static readonly PackageManifest.Variant _defaultVariant = new()
    {
        Label = _defaultVariantLabel,
        Platform = RuntimeInformation.RuntimeIdentifier,
        Assets = [],
        PreserveFiles = [],
        RemoveFiles = [],
        Dependencies = [],
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
            AdditionalScripts = [],
        }
    };

    private static readonly PackageManifest _defaultManifest = new()
    {
        ToothPath = "example.com/pkg",
        Version = new(1, 0, 0),
        Info = new()
        {
            Name = string.Empty,
            Description = string.Empty,
            Tags = [],
            AvatarUrl = new(),
        },
        Variants = [
            _defaultVariant
        ]
    };

    private static readonly List<PackageLock.Package> _defaultPackages =
    [
        new PackageLock.Package
        {
            Files = _defaultFiles,
            Locked = false,
            Manifest = _defaultManifest,
            VariantLabel = _defaultVariantLabel
        }
    ];

    private readonly string _defaultJson = $$"""
        {
            "format_version": 3,
            "format_uuid": "289f771f-2c9a-4d73-9f3f-8492495a924d",
            "packages": [
                {
                    "files": [
                        "file1.txt",
                        "file2.txt"
                    ],
                    "locked": false,
                    "manifest": {
                        "format_version": 3,
                        "format_uuid": "289f771f-2c9a-4d73-9f3f-8492495a924d",
                        "tooth": "example.com/pkg",
                        "version": "1.0.0",
                        "info": {
                            "name": "",
                            "description": "",
                            "tags": [],
                            "avatar_url": ""
                        },
                        "variants": [
                            {
                                "label": "variant",
                                "platform": "{{RuntimeInformation.RuntimeIdentifier}}",
                                "dependencies": {},
                                "assets": [],
                                "preserve_files": [],
                                "remove_files": [],
                                "scripts": {
                                    "pre_install": [],
                                    "install": [],
                                    "post_install": [],
                                    "pre_pack": [],
                                    "post_pack": [],
                                    "pre_uninstall": [],
                                    "uninstall": [],
                                    "post_uninstall": []
                                }
                            }
                        ]
                    },
                    "variant": "variant"
                }
            ]
        }
        """;

    private static readonly PackageSpecifier _defaultSpecifier = new()
    {
        ToothPath = "example.com/pkg",
        VariantLabel = _defaultVariantLabel,
        Version = new(1, 0, 0)
    };

    private const string _defaultVariantLabel = "variant";

    [Fact]
    public void Package_Constructor_ValidValues_ReturnsCorrectInstance()
    {
        // Arrange & Act.
        PackageLock.Package package = new()
        {
            Files = _defaultFiles,
            Locked = false,
            Manifest = _defaultManifest,
            VariantLabel = _defaultVariantLabel
        };

        PackageLock.Package newPackage = package with { };

        // Assert.
        Assert.Equal(_defaultFiles, newPackage.Files);
        Assert.False(newPackage.Locked);
    }

    [Fact]
    public void Package_Constructor_InvalidVariantLabel_ThrowsSchemaViolationException()
    {
        // Arrange.
        string invalidVariantLabel = "invalid variant label";

        // Act & Assert.
        Assert.Throws<SchemaViolationException>(() => new PackageLock.Package
        {
            Files = _defaultFiles,
            Locked = false,
            Manifest = _defaultManifest,
            VariantLabel = invalidVariantLabel
        });
    }

    [Fact]
    public void Package_Specifier_ReturnsCorrectSpecifier()
    {
        // Arrange.
        PackageLock.Package package = new()
        {
            Files = _defaultFiles,
            Locked = false,
            Manifest = _defaultManifest,
            VariantLabel = _defaultVariantLabel
        };

        // Act.
        PackageSpecifier specifier = package.Specifier;

        // Assert.
        Assert.Equal(_defaultSpecifier, specifier);
    }

    [Fact]
    public void Package_Variant_ReturnsCorrectVariant()
    {
        // Arrange.
        PackageLock.Package package = new()
        {
            Files = _defaultFiles,
            Locked = false,
            Manifest = _defaultManifest,
            VariantLabel = _defaultVariantLabel
        };

        // Act.
        PackageManifest.Variant variant = package.Variant;

        // Assert.
        Assert.Equal(_defaultVariant.Label, variant.Label);
        Assert.Equal(_defaultVariant.Platform, variant.Platform);
    }

    [Fact]
    public void Constructor_ValidValues_ReturnsCorrectInstance()
    {
        // Arrange & Act.
        PackageLock packageLock = new()
        {
            Packages = _defaultPackages
        };

        PackageLock newPackageLock = packageLock with { };

        // Assert.
        Assert.Equal(_defaultPackages, newPackageLock.Packages);
    }

    [Fact]
    public async Task FromStream_ValidJson_ReturnsCorrectInstance()
    {
        // Arrange.
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(_defaultJson));

        // Act.
        PackageLock packageLock = await PackageLock.FromStream(stream);

        // Assert.
        Assert.Single(packageLock.Packages);
        PackageLock.Package package = packageLock.Packages[0];
        Assert.Equal(_defaultFiles, package.Files);
        Assert.False(package.Locked);
    }

    [Fact]
    public async Task FromStream_NullJson_ThrowsSchemaViolationException()
    {
        // Arrange
        string json = "null";
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(json));

        // Act & Assert
        await Assert.ThrowsAsync<SchemaViolationException>(
            async () => await PackageLock.FromStream(stream));
    }

    [Fact]
    public async Task FromStream_IncorrectFormatVersion_ThrowsSchemaViolationException()
    {
        // Arrange
        string json = _defaultJson.Replace(
            @"""format_version"": 3",
            @"""format_version"": 2");
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(json));

        // Act & Assert
        await Assert.ThrowsAsync<SchemaViolationException>(
            async () => await PackageLock.FromStream(stream));
    }

    [Fact]
    public async Task FromStream_IncorrectFormatUuid_ThrowsSchemaViolationException()
    {
        // Arrange
        string json = _defaultJson.Replace(
            @"""format_uuid"": ""289f771f-2c9a-4d73-9f3f-8492495a924d""",
            @"""format_uuid"": ""incorrect-uuid""");
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(json));

        // Act & Assert
        await Assert.ThrowsAsync<SchemaViolationException>(
            async () => await PackageLock.FromStream(stream));
    }

    [Fact]
    public async Task ToStream_ValidValues_Passes()
    {
        // Arrange
        PackageLock packageLock = new()
        {
            Packages = _defaultPackages
        };
        using MemoryStream stream = new();

        // Act
        await packageLock.ToStream(stream);
        stream.Position = 0;
        string json = Encoding.UTF8.GetString(stream.ToArray());

        // Assert
        Assert.Equal(_defaultJson.ReplaceLineEndings(), json.ReplaceLineEndings());
    }
}