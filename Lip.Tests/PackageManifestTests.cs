using System.Text;
using System.Text.Json;
using Semver;

namespace Lip.Tests;

public class PackageManifestTests
{
    [Fact]
    public void AssetType_Constructor_TrivialValues_Passes()
    {
        // Arrange.
        PackageManifest.AssetType assetType = new()
        {
            Type = PackageManifest.AssetType.TypeEnum.Self,
        };

        // Act.
        assetType = assetType with { };
    }

    [Theory]
    [InlineData(null, null, null)]
    [InlineData(new string[] { "https://example.com" }, new string[] { "preserve" }, new string[] { "remove" })]
    public void AssetType_Constructor_ValidValues_Passes(
        string[]? urls,
        string[]? preserve,
        string[]? remove)
    {
        // Arrange & Act.
        var asset = new PackageManifest.AssetType
        {
            Type = PackageManifest.AssetType.TypeEnum.Self,
            Urls = urls?.ToList(),
            Place = null,
            Preserve = preserve?.ToList(),
            Remove = remove?.ToList()
        };

        // Assert.
        Assert.Equal(PackageManifest.AssetType.TypeEnum.Self, asset.Type);
        Assert.Equal(urls, asset.Urls);
        Assert.Null(asset.Place);
        Assert.Equal(preserve, asset.Preserve);
        Assert.Equal(remove, asset.Remove);
    }

    [Fact]
    public void AssetType_Constructor_InvalidUrl_Throws()
    {
        // Arrange & Act.
        SchemaViolationException exception = Assert.Throws<SchemaViolationException>(
            () => new PackageManifest.AssetType
            {
                Type = PackageManifest.AssetType.TypeEnum.Self,
                Urls = ["invalid"]
            });

        // Assert.
        Assert.Equal("urls", exception.Key);
        Assert.Equal("URL 'invalid' is invalid.", exception.Message);
    }

    [Fact]
    public void AssetType_Constructor_UnsafePreserve_Throws()
    {
        // Arrange & Act.
        SchemaViolationException exception = Assert.Throws<SchemaViolationException>(
            () => new PackageManifest.AssetType
            {
                Type = PackageManifest.AssetType.TypeEnum.Self,
                Preserve = ["/invalid"]
            });

        // Assert.
        Assert.Equal("preserve", exception.Key);
        Assert.Equal("Path '/invalid' is unsafe to preserve.", exception.Message);
    }

    [Fact]
    public void AssetType_Constructor_UnsafeRemove_Throws()
    {
        // Arrange & Act.
        SchemaViolationException exception = Assert.Throws<SchemaViolationException>(
            () => new PackageManifest.AssetType
            {
                Type = PackageManifest.AssetType.TypeEnum.Self,
                Remove = ["/invalid"]
            });

        // Assert.
        Assert.Equal("remove", exception.Key);
        Assert.Equal("Path '/invalid' is unsafe to remove.", exception.Message);
    }

    [Fact]
    public void InfoType_Constructor_TrivialValues_Passes()
    {
        // Arrange.
        PackageManifest.InfoType infoType = new();

        // Act.
        infoType = infoType with { };
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(new string[] { "tag" }, "https://example.com")]
    public void InfoType_Constructor_ValidValues_Passes(string[]? tags, string? avatarUrl)
    {
        // Arrange & Act.
        var info = new PackageManifest.InfoType
        {
            Name = "name",
            Description = "description",
            Tags = tags?.ToList(),
            AvatarUrl = avatarUrl
        };

        // Assert.
        Assert.Equal("name", info.Name);
        Assert.Equal("description", info.Description);
        Assert.Equal(tags, info.Tags);
        Assert.Equal(avatarUrl, info.AvatarUrl);
    }

    [Fact]
    public void InfoType_Constructor_InvalidTag_Throws()
    {
        // Arrange & Act.
        SchemaViolationException exception = Assert.Throws<SchemaViolationException>(
            () => new PackageManifest.InfoType { Tags = ["invalid.tag"] });

        // Assert.
        Assert.Equal("Tag 'invalid.tag' is invalid.", exception.Message);
    }

    [Fact]
    public void InfoType_Constructor_InvalidAvatarUrl_Throws()
    {
        // Arrange & Act.
        SchemaViolationException exception = Assert.Throws<SchemaViolationException>(
            () => new PackageManifest.InfoType { AvatarUrl = "invalid" });

        // Assert.
        Assert.Equal("avatar_url", exception.Key);
        Assert.Equal("Avatar URL 'invalid' is invalid.", exception.Message);
    }

    [Fact]
    public void PlaceType_Constructor_TrivialValues_Passes()
    {
        // Arrange.
        PackageManifest.PlaceType place = new()
        {
            Type = PackageManifest.PlaceType.TypeEnum.File,
            Src = "path/to/src",
            Dest = "path/to/dest"
        };

        // Act.
        place = place with { };
    }

    [Fact]
    public void PlaceType_Constructor_ValidValues_Passes()
    {
        // Arrange & Act.
        var place = new PackageManifest.PlaceType
        {
            Type = PackageManifest.PlaceType.TypeEnum.File,
            Src = "/path/to/src",
            Dest = "path/to/dest"
        };

        // Assert.
        Assert.Equal(PackageManifest.PlaceType.TypeEnum.File, place.Type);
        Assert.Equal("/path/to/src", place.Src);
        Assert.Equal("path/to/dest", place.Dest);
    }

    [Fact]
    public void PlaceType_Constructor_UnsafeDest_Throws()
    {
        // Arrange & Act.
        SchemaViolationException exception = Assert.Throws<SchemaViolationException>(
            () => new PackageManifest.PlaceType
            {
                Type = PackageManifest.PlaceType.TypeEnum.File,
                Src = "/path/to/src",
                Dest = "/path/to/dest"
            });

        // Assert.
        Assert.Equal("dest", exception.Key);
        Assert.Equal("Path '/path/to/dest' is unsafe to place.", exception.Message);
    }

    [Fact]
    public void ScriptsType_Constructor_TrivialValues_Passes()
    {
        // Arrange.
        PackageManifest.ScriptsType scripts = new();

        // Act.
        scripts = scripts with { };
    }

    [Fact]
    public void ScriptsType_Constructor_Trivial_Passes()
    {
        // Arrange & Act.
        var scripts = new PackageManifest.ScriptsType()
        {
            PreInstall = [],
            Install = [],
            PostInstall = [],
            PrePack = [],
            PostPack = [],
            PreUninstall = [],
            Uninstall = [],
            PostUninstall = []
        };

        // Assert.
        Assert.Empty(scripts.PreInstall);
        Assert.Empty(scripts.Install);
        Assert.Empty(scripts.PostInstall);
        Assert.Empty(scripts.PrePack);
        Assert.Empty(scripts.PostPack);
        Assert.Empty(scripts.PreUninstall);
        Assert.Empty(scripts.Uninstall);
        Assert.Empty(scripts.PostUninstall);
        Assert.Empty(scripts.AdditionalScripts);
    }

    [Fact]
    public void ScriptsType_Constructor_WithAdditionalScripts_Passes()
    {
        // Arrange.
        Dictionary<string, List<string>> additionalScripts = new()
        {
            ["additional_script"] = ["echo additional"]
        };

        // Act.
        var scripts = new PackageManifest.ScriptsType
        {
            AdditionalScripts = additionalScripts
        };

        // Assert.
        Assert.Single(scripts.AdditionalScripts);
        Assert.Equal(["echo additional"], scripts.AdditionalScripts["additional_script"]);
    }

    [Fact]
    public void ScriptsType_Constructor_InvalidAdditionalScriptName_Throws()
    {
        // Arrange.
        Dictionary<string, List<string>> additionalScripts = new()
        {
            ["invalid.script"] = ["echo invalid"]
        };

        // Act.
        SchemaViolationException exception = Assert.Throws<SchemaViolationException>(
            () => new PackageManifest.ScriptsType { AdditionalScripts = additionalScripts });

        // Assert.
        Assert.Equal("invalid.script", exception.Key);
        Assert.Equal("Script name 'invalid.script' is invalid.", exception.Message);
    }

    [Theory]
    [InlineData("""
        {
            "invalid.script.name": [""]
        }
        """)]
    [InlineData("""
        {
            "script_name": "not array"
        }
        """)]
    [InlineData("""
        {
            "script_name": [[]]
        }
        """)]
    public void ScriptsType_Deserialize_InvalidAdditionalProperties_Ignores(string json)
    {
        // Arrange.
        byte[] bytes = Encoding.UTF8.GetBytes(json);

        // Act.
        PackageManifest.ScriptsType? scripts = JsonSerializer.Deserialize<PackageManifest.ScriptsType>(bytes);

        // Assert.
        Assert.NotNull(scripts);
        Assert.Null(scripts.PreInstall);
        Assert.Null(scripts.Install);
        Assert.Null(scripts.PostInstall);
        Assert.Null(scripts.PrePack);
        Assert.Null(scripts.PostPack);
        Assert.Null(scripts.PreUninstall);
        Assert.Null(scripts.Uninstall);
        Assert.Null(scripts.PostUninstall);
        Assert.Empty(scripts.AdditionalScripts);
    }

    [Fact]
    public void VariantType_Constructor_TrivialValues_Passes()
    {
        // Arrange.
        PackageManifest.VariantType variant = new();

        // Act.
        variant = variant with { };
    }

    private static readonly (string, string)[] s_testDependencies = [("example.com/pkg", "1.0.x")];

    [Theory]
    [InlineData(null)]
    [InlineData(0)] // Use index to reference TestDependencies
    public void VariantType_Constructor_ValidValues_Passes(int? dependencyIndex)
    {
        // Arrange & Act.
        Dictionary<string, string>? dependencies = dependencyIndex.HasValue ?
            s_testDependencies.Skip(dependencyIndex.Value).Take(1).ToDictionary(x => x.Item1, x => x.Item2) :
            null;

        var variant = new PackageManifest.VariantType
        {
            VariantLabelRaw = "variant",
            Platform = "platform",
            Dependencies = dependencies,
            Assets = [],
            Scripts = new()
        };

        // Assert. 
        Assert.Equal("variant", variant.VariantLabel);
        Assert.Equal("variant", variant.VariantLabelRaw);
        Assert.Equal("platform", variant.Platform);
        Assert.Equal(dependencies, variant.Dependencies);
        Assert.NotNull(variant.Assets);
        Assert.Empty(variant.Assets);
        Assert.NotNull(variant.Scripts);
    }

    [Fact]
    public void VariantType_Constructor_NullVariantLabel_Passes()
    {
        // Arrange & Act.
        var variant = new PackageManifest.VariantType
        {
            VariantLabelRaw = null,
            Platform = "platform",
            Dependencies = null,
            Assets = [],
            Scripts = new()
        };

        // Assert.
        Assert.Equal("", variant.VariantLabel);
        Assert.Null(variant.VariantLabelRaw);
        Assert.Equal("platform", variant.Platform);
        Assert.Null(variant.Dependencies);
        Assert.NotNull(variant.Assets);
        Assert.Empty(variant.Assets);
        Assert.NotNull(variant.Scripts);
    }

    [Fact]
    public void VariantType_Constructor_InvalidDependencyKey_Throws()
    {
        // Arrange & Act.
        SchemaViolationException exception = Assert.Throws<SchemaViolationException>(
            () => new PackageManifest.VariantType
            {
                Dependencies = new Dictionary<string, string> { { "invalid*key", "1.0.x" } }
            });

        // Assert.
        Assert.Equal("dependencies", exception.Key);
        Assert.Equal("Package specifier 'invalid*key' is invalid.", exception.Message);
    }

    [Fact]
    public void VariantType_Constructor_InvalidDependencyValue_Throws()
    {
        // Arrange & Act.
        SchemaViolationException exception = Assert.Throws<SchemaViolationException>(
            () => new PackageManifest.VariantType
            {
                Dependencies = new Dictionary<string, string> { { "example.com/pkg", "invalid.version.range" } }
            });

        // Assert.
        Assert.Equal("dependencies", exception.Key);
        Assert.Equal("Version range 'invalid.version.range' is invalid.", exception.Message);
    }

    [Fact]
    public void PackageManifest_Constructor_TrivialValues_Passes()
    {
        // Arrange.
        PackageManifest manifest = new()
        {
            FormatVersion = 3,
            FormatUuid = "289f771f-2c9a-4d73-9f3f-8492495a924d",
            ToothPath = "",
            VersionText = "1.0.0"
        };

        // Act.
        manifest = manifest with { };
    }

    [Fact]
    public void FromJsonBytesParsed_NeedsParsing_Passes()
    {
        // Arrange.
        byte[] bytes = Encoding.UTF8.GetBytes("""
            {
                "format_version": 3,
                "format_uuid": "289f771f-2c9a-4d73-9f3f-8492495a924d",
                "tooth": "",
                "version": "1.0.0",
                "info": {
                    "name": "name-{{version}}",
                },
            }
            """);

        // Act.
        var manifest = PackageManifest.FromJsonBytesParsed(bytes);

        // Assert.
        Assert.Equal(3, manifest.FormatVersion);
        Assert.Equal("289f771f-2c9a-4d73-9f3f-8492495a924d", manifest.FormatUuid);
        Assert.Equal("", manifest.ToothPath);
        Assert.Equal("1.0.0", manifest.VersionText);
        Assert.Equal(SemVersion.Parse("1.0.0"), manifest.Version);
        Assert.NotNull(manifest.Info);
        Assert.Equal("name-1.0.0", manifest.Info.Name);
    }

    [Fact]
    public void FromJsonBytesRaw_MinimumJson_Passes()
    {
        // Arrange.
        byte[] bytes = Encoding.UTF8.GetBytes("""
            {
                "format_version": 3,
                "format_uuid": "289f771f-2c9a-4d73-9f3f-8492495a924d",
                "tooth": "",
                "version": "1.0.0"
            }
            """);

        // Act.
        var manifest = PackageManifest.FromJsonBytesWithTemplate(bytes);

        // Assert.
        Assert.Equal(3, manifest.FormatVersion);
        Assert.Equal("289f771f-2c9a-4d73-9f3f-8492495a924d", manifest.FormatUuid);
        Assert.Equal("", manifest.ToothPath);
        Assert.Equal("1.0.0", manifest.VersionText);
        Assert.Equal(SemVersion.Parse("1.0.0"), manifest.Version);
    }

    [Fact]
    public void FromJsonBytesRaw_NullJson_Throws()
    {
        // Arrange.
        byte[] bytes = Encoding.UTF8.GetBytes("null");

        // Act.
        JsonException exception = Assert.Throws<JsonException>(() => PackageManifest.FromJsonBytesWithTemplate(bytes));

        // Assert.
        Assert.Equal("Package manifest bytes deserialization failed.", exception.Message);
        Assert.NotNull(exception.InnerException);

    }

    [Fact]
    public void FromJsonBytesRaw_InvalidFormatVersion_Throws()
    {
        // Arrange.
        byte[] bytes = Encoding.UTF8.GetBytes("""
            {
                "format_version": 0,
                "format_uuid": "289f771f-2c9a-4d73-9f3f-8492495a924d",
                "tooth": "",
                "version": "1.0.0"
            }
            """);

        // Act & assert.
        Assert.Throws<SchemaViolationException>(() => PackageManifest.FromJsonBytesWithTemplate(bytes));
    }

    [Fact]
    public void FromJsonBytesRaw_InvalidFormatUuid_Throws()
    {
        // Arrange.
        byte[] bytes = Encoding.UTF8.GetBytes("""
            {
                "format_version": 3,
                "format_uuid": "invalid-uuid",
                "tooth": "",
                "version": "1.0.0"
            }
            """);

        // Act & assert.
        Assert.Throws<SchemaViolationException>(() => PackageManifest.FromJsonBytesWithTemplate(bytes));
    }

    [Fact]
    public void FromJsonBytesRaw_InvalidVersion_Throws()
    {
        // Arrange.
        byte[] bytes = Encoding.UTF8.GetBytes("""
            {
                "format_version": 3,
                "format_uuid": "289f771f-2c9a-4d73-9f3f-8492495a924d",
                "tooth": "",
                "version": "0.0.0.0"
            }
            """);

        // Act & Assert.
        Assert.Throws<SchemaViolationException>(() => PackageManifest.FromJsonBytesWithTemplate(bytes));
    }

    [Fact]
    public void GetSpecifiedVariant_NullVariants_ReturnsNull()
    {
        // Arrange.
        var manifest = new PackageManifest
        {
            FormatVersion = 3,
            FormatUuid = "289f771f-2c9a-4d73-9f3f-8492495a924d",
            ToothPath = "",
            VersionText = "1.0.0"
        };
        string variantLabel = "";
        string platform = "platform";

        // Act.
        PackageManifest.VariantType? variant = manifest.GetSpecifiedVariant(variantLabel, platform);

        // Assert.
        Assert.Null(variant);
    }

    [Fact]
    public void GetSpecifiedVariant_EmptyVariants_ReturnsNull()
    {
        // Arrange.
        var manifest = new PackageManifest
        {
            FormatVersion = 3,
            FormatUuid = "289f771f-2c9a-4d73-9f3f-8492495a924d",
            ToothPath = "",
            VersionText = "1.0.0",
            Variants = []
        };
        string variantLabel = "";
        string platform = "platform";

        // Act.
        PackageManifest.VariantType? variant = manifest.GetSpecifiedVariant(variantLabel, platform);

        // Assert.
        Assert.Null(variant);
    }

    [Theory]
    [InlineData("", "platform", null, "platform")]
    [InlineData("", "platform", "", "platform")]
    [InlineData("variant", "platform", "variant", "platform")]
    public void GetSpecifiedVariant_SingleVariant_ReturnsVariant(
        string variantLabel, string platform, string? manifestVariantLabel, string manifestPlatform)
    {
        // Arrange.
        var manifest = new PackageManifest
        {
            FormatVersion = 3,
            FormatUuid = "289f771f-2c9a-4d73-9f3f-8492495a924d",
            ToothPath = "",
            VersionText = "1.0.0",
            Variants =
            [
                new PackageManifest.VariantType
                {
                    VariantLabelRaw = manifestVariantLabel,
                    Platform = manifestPlatform,
                }
            ]
        };

        // Act.
        PackageManifest.VariantType? variant = manifest.GetSpecifiedVariant(variantLabel, platform);

        // Assert.
        Assert.NotNull(variant);
        Assert.Equal(variantLabel, variant.VariantLabelRaw);
        Assert.Equal(variantLabel, variant.VariantLabel);
        Assert.Equal(platform, variant.Platform);
        Assert.NotNull(variant.Dependencies);
        Assert.Empty(variant.Dependencies);
        Assert.NotNull(variant.Assets);
        Assert.Empty(variant.Assets);
        Assert.NotNull(variant.Scripts);
        Assert.Null(variant.Scripts.PreInstall);
        Assert.Null(variant.Scripts.Install);
        Assert.Null(variant.Scripts.PostInstall);
        Assert.Null(variant.Scripts.PrePack);
        Assert.Null(variant.Scripts.PostPack);
        Assert.Null(variant.Scripts.PreUninstall);
        Assert.Null(variant.Scripts.Uninstall);
        Assert.Null(variant.Scripts.PostUninstall);
        Assert.Equal([], variant.Scripts.AdditionalScripts);
    }

    [Fact]
    public void GetSpecifiedVariant_SingleFullVariant_ReturnsVariant()
    {
        // Arrange.
        var manifest = new PackageManifest
        {
            FormatVersion = 3,
            FormatUuid = "289f771f-2c9a-4d73-9f3f-8492495a924d",
            ToothPath = "",
            VersionText = "1.0.0",
            Variants =
            [
                new PackageManifest.VariantType
                {
                    Platform = "platform",
                    Dependencies = {},
                    Assets = [],
                    Scripts = new()
                }
            ]
        };

        // Act.
        PackageManifest.VariantType? variant = manifest.GetSpecifiedVariant("", "platform");

        // Assert.
        Assert.NotNull(variant);
        Assert.Equal("", variant.VariantLabelRaw);
        Assert.Equal("", variant.VariantLabel);
        Assert.Equal("platform", variant.Platform);
        Assert.NotNull(variant.Dependencies);
        Assert.Empty(variant.Dependencies);
        Assert.NotNull(variant.Assets);
        Assert.Empty(variant.Assets);
        Assert.NotNull(variant.Scripts);
        Assert.Null(variant.Scripts.PreInstall);
        Assert.Null(variant.Scripts.Install);
        Assert.Null(variant.Scripts.PostInstall);
        Assert.Null(variant.Scripts.PrePack);
        Assert.Null(variant.Scripts.PostPack);
        Assert.Null(variant.Scripts.PreUninstall);
        Assert.Null(variant.Scripts.Uninstall);
        Assert.Null(variant.Scripts.PostUninstall);
        Assert.Equal([], variant.Scripts.AdditionalScripts);
    }

    [Fact]
    public void GetSpecifiedVariant_SingleVariantWithScripts_ReturnsVariant()
    {
        // Arrange.
        var manifest = new PackageManifest
        {
            FormatVersion = 3,
            FormatUuid = "289f771f-2c9a-4d73-9f3f-8492495a924d",
            ToothPath = "",
            VersionText = "1.0.0",
            Variants =
            [
                new PackageManifest.VariantType
                {
                    Platform = "platform",
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
                    }
                }
            ]
        };

        // Act.
        PackageManifest.VariantType? variant = manifest.GetSpecifiedVariant("", "platform");

        // Assert.
        Assert.NotNull(variant);
        Assert.Equal("", variant.VariantLabelRaw);
        Assert.Equal("", variant.VariantLabel);
        Assert.Equal("platform", variant.Platform);
        Assert.NotNull(variant.Dependencies);
        Assert.Empty(variant.Dependencies);
        Assert.NotNull(variant.Assets);
        Assert.Empty(variant.Assets);
        Assert.NotNull(variant.Scripts);
        Assert.Equal([], variant.Scripts.PreInstall);
        Assert.Equal([], variant.Scripts.Install);
        Assert.Equal([], variant.Scripts.PostInstall);
        Assert.Equal([], variant.Scripts.PrePack);
        Assert.Equal([], variant.Scripts.PostPack);
        Assert.Equal([], variant.Scripts.PreUninstall);
        Assert.Equal([], variant.Scripts.Uninstall);
        Assert.Equal([], variant.Scripts.PostUninstall);
        Assert.NotNull(variant.Scripts.AdditionalScripts);
        Assert.Empty(variant.Scripts.AdditionalScripts);
    }

    [Theory]
    [InlineData("variant1", "platform", "variant*", "platform")]
    [InlineData("", "platform", null, null)]
    [InlineData("", "platform", null, "platform*")]
    public void GetSpecifiedVariant_WildcardOnlySingleVariant_ReturnsNull(
        string variantLabel, string platform, string? manifestVariantLabel, string? manifestPlatform)
    {
        // Arrange.
        var manifest = new PackageManifest
        {
            FormatVersion = 3,
            FormatUuid = "289f771f-2c9a-4d73-9f3f-8492495a924d",
            ToothPath = "",
            VersionText = "1.0.0",
            Variants =
            [
                new PackageManifest.VariantType
                {
                    VariantLabelRaw = manifestVariantLabel,
                    Platform = manifestPlatform,
                }
            ]
        };

        // Act.
        PackageManifest.VariantType? variant = manifest.GetSpecifiedVariant(variantLabel, platform);

        // Assert.
        Assert.Null(variant);
    }

    [Theory]
    [InlineData("variant", "platform", null, "platform")]
    [InlineData("variant", "platform", "mismatch*", "platform")]
    [InlineData("", "platform", null, "mismatch*")]
    public void GetSpecifiedVariant_MismatchedSingleVariant_ReturnsNull(
            string variantLabel, string platform, string? manifestVariantLabel, string? manifestPlatform)
    {
        // Arrange.
        var manifest = new PackageManifest
        {
            FormatVersion = 3,
            FormatUuid = "289f771f-2c9a-4d73-9f3f-8492495a924d",
            ToothPath = "",
            VersionText = "1.0.0",
            Variants =
            [
                new PackageManifest.VariantType
                {
                    VariantLabelRaw = manifestVariantLabel,
                    Platform = manifestPlatform,
                }
            ]
        };

        // Act.
        PackageManifest.VariantType? variant = manifest.GetSpecifiedVariant(variantLabel, platform);

        // Assert.
        Assert.Null(variant);
    }

    [Fact]
    public void GetSpecifiedVariant_MultipleVariants_ReturnsMergedVariant()
    {
        // Arrange.
        var manifest = new PackageManifest
        {
            FormatVersion = 3,
            FormatUuid = "289f771f-2c9a-4d73-9f3f-8492495a924d",
            ToothPath = "",
            VersionText = "1.0.0",
            Variants =
            [
            new()
            {
                Platform = "platform",
                Dependencies = new Dictionary<string, string>
                {
                { "github.com/futrime/tooth", "1.0.0" }
                },
                Assets =
                [
                new()
                {
                    Type = PackageManifest.AssetType.TypeEnum.Self,
                    Urls = ["https://example.com/1"]
                }
                ],
                Scripts = new PackageManifest.ScriptsType
                {
                PreInstall = ["echo pre-install"],
                Install = ["echo install"],
                PostInstall = ["echo post-install"],
                PrePack = ["echo pre-pack"],
                PostPack = ["echo post-pack"],
                PreUninstall = ["echo pre-uninstall"],
                Uninstall = ["echo uninstall"],
                PostUninstall = ["echo post-uninstall"],
                AdditionalScripts = new Dictionary<string, List<string>>
                {
                    { "same_script", new List<string> { "echo same" } },
                    { "custom_script1", new List<string> { "echo custom1" } }
                }
                }
            },
            new()
            {
                Platform = "platform",
                Dependencies = new Dictionary<string, string>
                {
                { "github.com/futrime/tooth2", "2.0.0" }
                },
                Assets =
                [
                new()
                {
                    Type = PackageManifest.AssetType.TypeEnum.Self,
                    Urls = ["https://example.com/2"]
                }
                ],
                Scripts = new PackageManifest.ScriptsType
                {
                PreInstall = ["echo pre-install-2"],
                Install = ["echo install-2"],
                PostInstall = ["echo post-install-2"],
                PrePack = ["echo pre-pack-2"],
                PostPack = ["echo post-pack-2"],
                PreUninstall = ["echo pre-uninstall-2"],
                Uninstall = ["echo uninstall-2"],
                PostUninstall = ["echo post-uninstall-2"],
                AdditionalScripts = new Dictionary<string, List<string>>
                {
                    { "same_script", new List<string> { "echo same2" } },
                    { "custom_script2", new List<string> { "echo custom2" } }
                }
                }
            }
            ]
        };

        // Act.
        PackageManifest.VariantType? variant = manifest.GetSpecifiedVariant("", "platform");

        // Assert.
        Assert.NotNull(variant);
        Assert.Equal("", variant.VariantLabelRaw);
        Assert.Equal("", variant.VariantLabel);
        Assert.Equal("platform", variant.Platform);
        Assert.NotNull(variant.Dependencies);
        Assert.Equal(2, variant.Dependencies.Count);
        Assert.Equal("1.0.0", variant.Dependencies["github.com/futrime/tooth"]);
        Assert.Equal("2.0.0", variant.Dependencies["github.com/futrime/tooth2"]);
        Assert.NotNull(variant.Assets);
        Assert.Equal(2, variant.Assets.Count);
        Assert.Equal(PackageManifest.AssetType.TypeEnum.Self, variant.Assets[0].Type);
        Assert.Equal(["https://example.com/1"], variant.Assets[0].Urls);
        Assert.Equal(PackageManifest.AssetType.TypeEnum.Self, variant.Assets[1].Type);
        Assert.Equal(["https://example.com/2"], variant.Assets[1].Urls);
        Assert.NotNull(variant.Scripts);
        Assert.Equal(["echo pre-install-2"], variant.Scripts.PreInstall);
        Assert.Equal(["echo install-2"], variant.Scripts.Install);
        Assert.Equal(["echo post-install-2"], variant.Scripts.PostInstall);
        Assert.Equal(["echo pre-pack-2"], variant.Scripts.PrePack);
        Assert.Equal(["echo post-pack-2"], variant.Scripts.PostPack);
        Assert.Equal(["echo pre-uninstall-2"], variant.Scripts.PreUninstall);
        Assert.Equal(["echo uninstall-2"], variant.Scripts.Uninstall);
        Assert.Equal(["echo post-uninstall-2"], variant.Scripts.PostUninstall);
        Assert.NotNull(variant.Scripts.AdditionalScripts);
        Assert.Equal(3, variant.Scripts.AdditionalScripts.Count);
        Assert.Equal(["echo same2"], variant.Scripts.AdditionalScripts["same_script"]);
        Assert.Equal(["echo custom1"], variant.Scripts.AdditionalScripts["custom_script1"]);
        Assert.Equal(["echo custom2"], variant.Scripts.AdditionalScripts["custom_script2"]);
    }

    [Fact]
    public void ToJsonBytes_MinimumJson_Passes()
    {
        // Arrange.
        var manifest = new PackageManifest
        {
            FormatVersion = 3,
            FormatUuid = "289f771f-2c9a-4d73-9f3f-8492495a924d",
            ToothPath = "",
            VersionText = "1.0.0"
        };

        // Act.
        byte[] bytes = manifest.ToJsonBytes();

        // Assert.
        Assert.Equal("""
            {
                "format_version": 3,
                "format_uuid": "289f771f-2c9a-4d73-9f3f-8492495a924d",
                "tooth": "",
                "version": "1.0.0"
            }
            """.ReplaceLineEndings(), Encoding.UTF8.GetString(bytes).ReplaceLineEndings());
    }

    [Fact]
    public void ToJsonElement_MinimumJson_Passes()
    {
        // Arrange.
        var manifest = new PackageManifest
        {
            FormatVersion = 3,
            FormatUuid = "289f771f-2c9a-4d73-9f3f-8492495a924d",
            ToothPath = "",
            VersionText = "1.0.0"
        };

        // Act.
        JsonElement element = manifest.ToJsonElement();

        // Assert.
        Assert.Equal(3, element.GetProperty("format_version").GetInt32());
        Assert.Equal("289f771f-2c9a-4d73-9f3f-8492495a924d", element.GetProperty("format_uuid").GetString());
        Assert.Equal("", element.GetProperty("tooth").GetString());
        Assert.Equal("1.0.0", element.GetProperty("version").GetString());
    }

    [Fact]
    public void WithTemplateParsed_CommonInput_Passes()
    {
        // Arrange.
        var manifest = new PackageManifest
        {
            FormatVersion = 3,
            FormatUuid = "289f771f-2c9a-4d73-9f3f-8492495a924d",
            ToothPath = "",
            VersionText = "1.0.0",
            Variants = [
                new(){
                    Assets = [
                        new(){
                            Type = PackageManifest.AssetType.TypeEnum.Zip,
                            Urls = ["https://example.com/{{version}}.zip"]
                        }
                    ]
                }
            ]
        };

        // Act.
        PackageManifest result = manifest.WithTemplateParsed();

        // Assert.
        Assert.Equal(3, result.FormatVersion);
        Assert.Equal("289f771f-2c9a-4d73-9f3f-8492495a924d", result.FormatUuid);
        Assert.Equal("", result.ToothPath);
        Assert.Equal("1.0.0", result.VersionText);
        Assert.NotNull(result.Variants);
        Assert.Single(result.Variants);
        Assert.NotNull(result.Variants[0].Assets);
        Assert.Single(result.Variants[0].Assets!);
        Assert.Equal(new[] { "https://example.com/1.0.0.zip" }, result.Variants[0].Assets![0].Urls);
    }

    [Fact]
    public void WithTemplateParsed_InvalidTemplate_Throws()
    {
        // Arrange.
        var manifest = new PackageManifest
        {
            FormatVersion = 3,
            FormatUuid = "289f771f-2c9a-4d73-9f3f-8492495a924d",
            ToothPath = "",
            VersionText = "1.0.0",
            Variants = [
                new(){
                    Assets = [
                        new(){
                            Type = PackageManifest.AssetType.TypeEnum.Zip,
                            Urls = ["https://example.com/{{{invalid}}.zip"]
                        }
                    ]
                }
            ]
        };

        // Act.
        FormatException exception = Assert.Throws<FormatException>(() => manifest.WithTemplateParsed());

        // Assert.
        Assert.Equal(
            "Failed to parse template: <input>(12,56) : error : Unexpected token `}` Expecting a colon : after identifier `invalid` for object initializer member name<input>(12,56) : error : Invalid token found `}`. Expecting <EOL>/end of line.",
            exception.Message);
    }
}
