using Flurl;
using Semver;
using System.Text;
using System.Text.Json;

namespace Lip.Core.Tests;

public class PackageManifestTests
{
    [Fact]
    public void Asset_Constructor_ValidValues_ReturnsCorrectInstance()
    {
        // Arrange & Act.
        PackageManifest.Asset asset = new()
        {
            Type = PackageManifest.Asset.TypeEnum.Self,
            Urls = [],
            Placements = []
        };

        PackageManifest.Asset newAsset = asset with { };

        // Assert.
        Assert.Equal(PackageManifest.Asset.TypeEnum.Self, newAsset.Type);
        Assert.Empty(newAsset.Urls);
        Assert.Empty(newAsset.Placements);
    }

    [Fact]
    public void InfoType_Constructor_ValidValues_ReturnsCorrectInstance()
    {
        // Arrange & Act.
        List<string> tags = ["tag1", "tag2"];
        Url avatarUrl = new();

        PackageManifest.InfoType info = new()
        {
            Name = string.Empty,
            Description = string.Empty,
            Tags = tags,
            AvatarUrl = avatarUrl,
        };

        PackageManifest.InfoType newInfo = info with { };

        // Assert.
        Assert.Empty(newInfo.Name);
        Assert.Empty(newInfo.Description);
        Assert.Equal(tags, newInfo.Tags);
        Assert.Equal(avatarUrl, newInfo.AvatarUrl);
    }

    [Fact]
    public void InfoType_Constructor_InvalidTags_ThrowsSchemaViolationException()
    {
        // Arrange & Act & Assert.
        SchemaViolationException exception = Assert.Throws<SchemaViolationException>(
            () => new PackageManifest.InfoType
            {
                Name = string.Empty,
                Description = string.Empty,
                Tags = ["invalid.tag"],
                AvatarUrl = new(),
            });

        Assert.Equal("info.tags[]", exception.Key);
    }

    [Fact]
    public void Placement_Constructor_ValidValues_ReturnsCorrectInstance()
    {
        // Arrange & Act.
        string dest = "path/to/dest";

        PackageManifest.Placement placement = new()
        {
            Type = PackageManifest.Placement.TypeEnum.File,
            Src = string.Empty,
            Dest = dest,
        };

        PackageManifest.Placement newPlacement = placement with { };

        // Assert.
        Assert.Equal(PackageManifest.Placement.TypeEnum.File, newPlacement.Type);
        Assert.Empty(newPlacement.Src);
        Assert.Equal(dest, newPlacement.Dest);
    }

    [Fact]
    public void Placement_Constructor_InvalidDest_ThrowsSchemaViolationException()
    {
        // Arrange & Act & Assert.
        SchemaViolationException exception = Assert.Throws<SchemaViolationException>(
            () => new PackageManifest.Placement
            {
                Type = PackageManifest.Placement.TypeEnum.File,
                Src = string.Empty,
                Dest = "/invalid/dest"
            });

        Assert.Equal("variants[].assets[].placements[].dest", exception.Key);
    }

    [Fact]
    public void ScriptsType_Constructor_ValidValues_ReturnsCorrectInstance()
    {
        // Arrange & Act.
        Dictionary<string, List<string>> additionalScripts = new()
        {
            ["key"] = [string.Empty]
        };

        PackageManifest.ScriptsType scripts = new()
        {
            PreInstall = [],
            Install = [],
            PostInstall = [],
            PrePack = [],
            PostPack = [],
            PreUninstall = [],
            Uninstall = [],
            PostUninstall = [],
            AdditionalScripts = additionalScripts,
        };

        PackageManifest.ScriptsType newScripts = scripts with { };

        // Assert.
        Assert.Empty(newScripts.PreInstall);
        Assert.Empty(newScripts.Install);
        Assert.Empty(newScripts.PostInstall);
        Assert.Empty(newScripts.PrePack);
        Assert.Empty(newScripts.PostPack);
        Assert.Empty(newScripts.PreUninstall);
        Assert.Empty(newScripts.Uninstall);
        Assert.Empty(newScripts.PostUninstall);
        Assert.Equal(additionalScripts, newScripts.AdditionalScripts);
    }

    [Fact]
    public void ScriptsType_Constructor_InvalidAdditionalScriptName_ThrowsSchemaViolationException()
    {
        // Arrange & Act & Assert.
        Dictionary<string, List<string>> additionalScripts = new()
        {
            ["invalid.script.name"] = [string.Empty]
        };

        SchemaViolationException exception = Assert.Throws<SchemaViolationException>(
            () => new PackageManifest.ScriptsType()
            {
                PreInstall = [],
                Install = [],
                PostInstall = [],
                PrePack = [],
                PostPack = [],
                PreUninstall = [],
                Uninstall = [],
                PostUninstall = [],
                AdditionalScripts = additionalScripts,
            });

        Assert.Equal("variants[].assets[].scripts.'invalid.script.name'", exception.Key);
    }

    [Fact]
    public void Variant_Constructor_ValidValues_ReturnsCorrectInstance()
    {
        // Arrange & Act.
        List<string> preserveFiles = ["path/to/preserve/file"];
        List<string> removeFiles = ["path/to/remove/file"];
        PackageManifest.ScriptsType scripts = new()
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
        };

        PackageManifest.Variant variant = new()
        {
            Label = string.Empty,
            Platform = string.Empty,
            Dependencies = [],
            Assets = [],
            PreserveFiles = preserveFiles,
            RemoveFiles = removeFiles,
            Scripts = scripts
        };

        PackageManifest.Variant newVariant = variant with { };

        // Assert.
        Assert.Empty(newVariant.Label);
        Assert.Empty(newVariant.Platform);
        Assert.Empty(newVariant.Dependencies);
        Assert.Empty(newVariant.Assets);
        Assert.Equal(preserveFiles, newVariant.PreserveFiles);
        Assert.Equal(removeFiles, newVariant.RemoveFiles);
        Assert.Equal(scripts, newVariant.Scripts);
    }

    [Fact]
    public void Variant_Constructor_InvalidPreserveFiles_ThrowsSchemaViolationException()
    {
        // Arrange & Act & Assert.
        SchemaViolationException exception = Assert.Throws<SchemaViolationException>(
            () => new PackageManifest.Variant
            {
                Label = string.Empty,
                Platform = string.Empty,
                Dependencies = [],
                Assets = [],
                PreserveFiles = ["/invalid/file"],
                RemoveFiles = ["path/to/remove/file"],
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
            });

        Assert.Equal("variants[].preserve_files[]", exception.Key);
    }

    [Fact]
    public void Variant_Constructor_InvalidRemoveFiles_ThrowsSchemaViolationException()
    {
        // Arrange & Act & Assert.
        SchemaViolationException exception = Assert.Throws<SchemaViolationException>(
            () => new PackageManifest.Variant
            {
                Label = string.Empty,
                Platform = string.Empty,
                Dependencies = [],
                Assets = [],
                PreserveFiles = ["path/to/preserve/file"],
                RemoveFiles = ["/invalid/file"],
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
            });

        Assert.Equal("variants[].remove_file[]", exception.Key);
    }

    [Theory]
    [InlineData("label", "platform", "label", "platform", true)]
    [InlineData("*", "platform", "label", "platform", true)]
    [InlineData("", "platform", "label", "platform", false)]
    [InlineData("another_label", "platform", "label", "platform", false)]
    [InlineData("label", "*", "label", "platform", true)]
    [InlineData("label", "", "label", "platform", false)]
    [InlineData("label", "another_platform", "label", "platform", false)]
    public void Variant_Match_VariousValues_ReturnsCorrectAnswer(
        string label,
        string platform,
        string targetLabel,
        string targetPlatform,
        bool expectedAnswer)
    {
        // Arrange.
        PackageManifest.Variant variant = new()
        {
            Label = label,
            Platform = platform,
            Dependencies = [],
            Assets = [],
            PreserveFiles = ["path/to/preserve/file"],
            RemoveFiles = ["path/to/remove/file"],
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

        // Act.
        bool answer = variant.Match(targetLabel, targetPlatform);

        // Assert.
        Assert.Equal(expectedAnswer, answer);
    }

    [Fact]
    public void Constructor_ValidValues_ReturnsCorrectInstance()
    {
        // Arrange & Act.
        string toothPath = "example.com/pkg";
        SemVersion version = new(0);
        PackageManifest.InfoType info = new()
        {
            Name = string.Empty,
            Description = string.Empty,
            Tags = [],
            AvatarUrl = new(),
        };

        PackageManifest manifest = new()
        {
            ToothPath = toothPath,
            Version = version,
            Info = info,
            Variants = [],
        };

        PackageManifest newManifest = manifest with { };

        // Assert.
        Assert.Equal(toothPath, newManifest.ToothPath);
        Assert.Equal(version, newManifest.Version);
        Assert.Equal(info, newManifest.Info);
        Assert.Empty(newManifest.Variants);
    }

    [Fact]
    public void Constructor_InvalidToothPath_ThrowsShcemaViolationException()
    {
        // Arrange & Act & Assert.
        SchemaViolationException exception = Assert.Throws<SchemaViolationException>(
            () => new PackageManifest
            {
                ToothPath = "invalid*tooth*path",
                Version = new(0),
                Info = new()
                {
                    Name = string.Empty,
                    Description = string.Empty,
                    Tags = [],
                    AvatarUrl = new(),
                },
                Variants = [],
            });

        Assert.Equal("tooth", exception.Key);
    }

    private const string _minimumJson = """
        {
            "format_version": 3,
            "format_uuid": "289f771f-2c9a-4d73-9f3f-8492495a924d",
            "tooth": "example.com/pkg",
            "version": "0.0.0"
        }
        """;

    [Fact]
    public void FromJsonElement_MinimumJson_ReturnsCorrectInstance()
    {
        // Assert.
        JsonElement jsonElement = JsonDocument.Parse(_minimumJson).RootElement;

        // Act.
        PackageManifest manifest = PackageManifest.FromJsonElement(jsonElement);

        // Assert.
        Assert.Equal("example.com/pkg", manifest.ToothPath);
        Assert.Equal(new(0), manifest.Version);
    }

    [Fact]
    public void FromJsonElement_InvalidFormatVersion_ThrowsShcemaViolationException()
    {
        // Arrange.
        string manifestJson = """
            {
                "format_version": 0,
                "format_uuid": "289f771f-2c9a-4d73-9f3f-8492495a924d",
                "tooth": "example.com/pkg",
                "version": "0.0.0"
            }
            """;

        JsonElement jsonElement = JsonDocument.Parse(manifestJson).RootElement;

        // Act & Assert.
        SchemaViolationException exception = Assert.Throws<SchemaViolationException>(
            () => PackageManifest.FromJsonElement(jsonElement));

        Assert.Equal("format_version", exception.Key);
    }

    [Fact]
    public void FromJsonElement_InvalidFormatUuid_ThrowsShcemaViolationException()
    {
        // Arrange.
        string manifestJson = """
            {
                "format_version": 3,
                "format_uuid": "",
                "tooth": "example.com/pkg",
                "version": "0.0.0"
            }
            """;

        JsonElement jsonElement = JsonDocument.Parse(manifestJson).RootElement;

        // Act & Assert.
        SchemaViolationException exception = Assert.Throws<SchemaViolationException>(
            () => PackageManifest.FromJsonElement(jsonElement));

        Assert.Equal("format_uuid", exception.Key);
    }

    [Fact]
    public void FromJsonElement_InvalidAdditionalScriptFormat_ThrowsShcemaViolationException()
    {
        // Arrange.
        string manifestJson = """
            {
                "format_version": 3,
                "format_uuid": "289f771f-2c9a-4d73-9f3f-8492495a924d",
                "tooth": "example.com/pkg",
                "version": "0.0.0",
                "variants": [
                    {
                        "scripts": {
                            "custom": "invalid"
                        }
                    }
                ]
            }
            """;

        JsonElement jsonElement = JsonDocument.Parse(manifestJson).RootElement;

        // Act & Assert.
        SchemaViolationException exception = Assert.Throws<SchemaViolationException>(
            () => PackageManifest.FromJsonElement(jsonElement));

        Assert.Equal("variants[].assets[].scripts.'custom'", exception.Key);
    }

    [Fact]
    public async Task FromStream_ValidStream_ReturnsCorrectInstance()
    {
        // Assert.
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(_minimumJson));

        // Act.
        PackageManifest manifest = await PackageManifest.FromStream(stream);

        // Assert.
        Assert.Equal("example.com/pkg", manifest.ToothPath);
        Assert.Equal(new(0), manifest.Version);
    }

    [Fact]
    public void GetVariant_VariantMatched_ReturnsCorrectVariant()
    {
        // Assert.
        PackageManifest.Variant variant = new()
        {
            Label = "label",
            Platform = "platform",
            Dependencies = new()
            {
                [
                    new()
                    {
                        ToothPath = "exmaple.com/pkg",
                        VariantLabel = string.Empty
                    }
                ] = SemVersionRange.Parse("1.0.0")
            },
            Assets = [],
            PreserveFiles = ["path/to/preserve/file"],
            RemoveFiles = ["path/to/remove/file"],
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
                AdditionalScripts = new()
                {
                    ["script"] = [string.Empty]
                },
            }
        };

        PackageManifest packageManifest = new()
        {
            ToothPath = "example.com/pkg",
            Version = new(0),
            Info = new()
            {
                Name = string.Empty,
                Description = string.Empty,
                Tags = [],
                AvatarUrl = new(),
            },
            Variants = [
                variant
            ],
        };

        // Act.
        PackageManifest.Variant? variantGot = packageManifest.GetVariant("label", "platform");

        // Assert.
        Assert.NotNull(variantGot);
        Assert.Equal(variant.Label, variantGot.Label);
        Assert.Equal(variant.Platform, variantGot.Platform);
        Assert.Equal(variant.Dependencies, variantGot.Dependencies);
        Assert.Equal(variant.Assets, variantGot.Assets);
        Assert.Equal(variant.PreserveFiles, variantGot.PreserveFiles);
        Assert.Equal(variant.RemoveFiles, variantGot.RemoveFiles);
        Assert.Equal(variant.Scripts.PreInstall, variantGot.Scripts.PreInstall);
        Assert.Equal(variant.Scripts.Install, variantGot.Scripts.Install);
        Assert.Equal(variant.Scripts.PostInstall, variantGot.Scripts.PostInstall);
        Assert.Equal(variant.Scripts.PrePack, variantGot.Scripts.PrePack);
        Assert.Equal(variant.Scripts.PostPack, variantGot.Scripts.PostPack);
        Assert.Equal(variant.Scripts.PreUninstall, variantGot.Scripts.PreUninstall);
        Assert.Equal(variant.Scripts.Uninstall, variantGot.Scripts.Uninstall);
        Assert.Equal(variant.Scripts.PostUninstall, variantGot.Scripts.PostUninstall);
        Assert.Equal(variant.Scripts.AdditionalScripts, variantGot.Scripts.AdditionalScripts);
    }

    [Fact]
    public void GetVariant_NoVariantMatched_ReturnsNull()
    {
        // Assert.
        PackageManifest packageManifest = new()
        {
            ToothPath = "example.com/pkg",
            Version = new(0),
            Info = new()
            {
                Name = string.Empty,
                Description = string.Empty,
                Tags = [],
                AvatarUrl = new(),
            },
            Variants = [],
        };

        // Act.
        PackageManifest.Variant? variantGot = packageManifest.GetVariant("label", "platform");

        // Assert.
        Assert.Null(variantGot);
    }

    [Fact]
    public void GetVariant_NoVariantFullyMatched_ReturnsNull()
    {
        // Assert.
        PackageManifest.Variant variant = new()
        {
            Label = "*",
            Platform = "*",
            Dependencies = [],
            Assets = [],
            PreserveFiles = ["path/to/preserve/file"],
            RemoveFiles = ["path/to/remove/file"],
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

        PackageManifest packageManifest = new()
        {
            ToothPath = "example.com/pkg",
            Version = new(0),
            Info = new()
            {
                Name = string.Empty,
                Description = string.Empty,
                Tags = [],
                AvatarUrl = new(),
            },
            Variants = [
                variant
            ],
        };

        // Act.
        PackageManifest.Variant? variantGot = packageManifest.GetVariant("label", "platform");

        // Assert.
        Assert.Null(variantGot);
    }

    private readonly PackageManifest _outputManifest = new()
    {
        ToothPath = "example.com/pkg",
        Version = new(0),
        Info = new()
        {
            Name = string.Empty,
            Description = string.Empty,
            Tags = [],
            AvatarUrl = new(),
        },
        Variants = [
            new()
            {
                Label = string.Empty,
                Platform = string.Empty,
                Dependencies = new()
                {
                    [
                        new()
                        {
                            ToothPath = "example.com/pkg",
                            VariantLabel = string.Empty
                        }
                    ] = SemVersionRange.Parse("*")
                },
                Assets = [
                    new()
                    {
                        Type = PackageManifest.Asset.TypeEnum.Self,
                        Urls = [
                            new()
                        ],
                        Placements = [
                            new()
                            {
                                Type = PackageManifest.Placement.TypeEnum.File,
                                Src = string.Empty,
                                Dest = "path/to/file"
                            }
                        ]
                    }
                ],
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
                    AdditionalScripts = new()
                    {
                        [ "script" ] = [ string.Empty ]
                    }
                }
            }
        ],
    };

    private const string _outputJson = """
        {
            "format_version": 3,
            "format_uuid": "289f771f-2c9a-4d73-9f3f-8492495a924d",
            "tooth": "example.com/pkg",
            "version": "0.0.0",
            "info": {
                "name": "",
                "description": "",
                "tags": [],
                "avatar_url": ""
            },
            "variants": [
                {
                    "label": "",
                    "platform": "",
                    "dependencies": {
                        "example.com/pkg": "*"
                    },
                    "assets": [
                        {
                            "type": "self",
                            "urls": [
                                ""
                            ],
                            "placements": [
                                {
                                    "type": "file",
                                    "src": "",
                                    "dest": "path/to/file"
                                }
                            ]
                        }
                    ],
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
                        "post_uninstall": [],
                        "script": [
                            ""
                        ]
                    }
                }
            ]
        }
        """;

    [Fact]
    public void ToJsonElement_ReturnsCorrectJsonElement()
    {
        // Act.
        JsonElement jsonElement = _outputManifest.ToJsonElement();

        // Assert.
        Assert.Equal(_outputJson.ReplaceLineEndings(), jsonElement.ToString());
    }

    [Fact]
    public async Task ToStream_ReturnsCorrectStream()
    {
        // Assert.
        MemoryStream stream = new();

        // Act.
        await _outputManifest.ToStream(stream);

        // Assert.
        Assert.Equal(_outputJson.ReplaceLineEndings(), Encoding.UTF8.GetString(stream.ToArray()));
    }
}
