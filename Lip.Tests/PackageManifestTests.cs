using System.Text;
using System.Text.Json;

namespace Lip.Tests;

public class PackageManifestTests
{
    [Fact]
    public void AssetType_Deserialize_MinimumJson_Passes()
    {
        // Arrange.
        string json = """
        {
            "type": "self"
        }
        """;

        // Act.
        PackageManifest.AssetType? asset = JsonSerializer.Deserialize<PackageManifest.AssetType>(json);

        // Assert.
        Assert.NotNull(asset);
        Assert.Equal(PackageManifest.AssetType.TypeEnum.Self, asset.Type);
        Assert.Null(asset.Urls);
        Assert.Null(asset.Place);
        Assert.Null(asset.Preserve);
        Assert.Null(asset.Remove);
    }

    [Fact]
    public void AssetType_Deserialize_MaximumJson_Passes()
    {
        // Arrange.
        string json = """
        {
            "type": "self",
            "urls": [],
            "place": [],
            "preserve": [],
            "remove": []
        }
        """;

        // Act.
        PackageManifest.AssetType? asset = JsonSerializer.Deserialize<PackageManifest.AssetType>(json);

        // Assert.
        Assert.NotNull(asset);
        Assert.Equal(PackageManifest.AssetType.TypeEnum.Self, asset.Type);
        Assert.Equal([], asset.Urls);
        Assert.Equal([], asset.Place);
        Assert.Equal([], asset.Preserve);
        Assert.Equal([], asset.Remove);
    }

    [Fact]
    public void InfoType_Deserialize_MinimumJson_Passes()
    {
        // Arrange.
        string json = "{}";

        // Act.
        PackageManifest.InfoType? info = JsonSerializer.Deserialize<PackageManifest.InfoType>(json);

        // Assert.
        Assert.NotNull(info);
        Assert.Null(info.Name);
        Assert.Null(info.Description);
        Assert.Null(info.Author);
        Assert.Null(info.Tags);
        Assert.Null(info.AvatarUrl);
    }

    [Fact]
    public void InfoType_Deserialize_MaximumJson_Passes()
    {
        // Arrange.
        string json = """
        {
            "name": "",
            "description": "",
            "author": "",
            "tags": ["tag", "tag:subtag"],
            "avatar_url": ""
        }
        """;

        // Act.
        PackageManifest.InfoType? info = JsonSerializer.Deserialize<PackageManifest.InfoType>(json);

        // Assert.
        Assert.NotNull(info);
        Assert.Equal("", info.Name);
        Assert.Equal("", info.Description);
        Assert.Equal("", info.Author);
        Assert.Equal(new[] { "tag", "tag:subtag" }, info.Tags);
        Assert.Equal("", info.AvatarUrl);
    }

    [Theory]
    [InlineData("invalid.tag")]
    [InlineData("tag:invalid.subtag")]
    [InlineData("invalid.tag:subtag")]
    [InlineData("invalid-tag:")]
    [InlineData(":invalid-subtag")]
    [InlineData(":")]
    [InlineData("")]
    public void InfoType_Deserialize_InvalidTag_ThrowsArgumentException(string tag)
    {
        // Arrange.
        string json = $$"""
        {
            "tags": ["{{tag}}"]
        }
        """;

        // Act.
        ArgumentException exception = Assert.Throws<ArgumentException>(
            "value", () => JsonSerializer.Deserialize<PackageManifest.InfoType>(json));

        // Assert.
        Assert.Equal($"Tag {tag} is invalid. (Parameter 'value')", exception.Message);
    }

    [Fact]
    public void PlaceType_Deserialize_CommonInput_Passes()
    {
        // Arrange.
        string json = """
        {
            "type": "file",
            "src": "",
            "dest": ""
        }
        """;

        // Act.
        PackageManifest.PlaceType? place = JsonSerializer.Deserialize<PackageManifest.PlaceType>(json);

        // Assert.
        Assert.NotNull(place);
        Assert.Equal(PackageManifest.PlaceType.TypeEnum.File, place.Type);
        Assert.Equal("", place.Src);
        Assert.Equal("", place.Dest);
    }

    [Theory]
    [InlineData("script")]
    [InlineData("additional_script")]
    public void ScriptsType_Constructor_AdditionalScriptsInitialized_Passes(string scriptName)
    {
        // Arrange.
        Dictionary<string, List<string>> additionalScripts = new()
        {
            [scriptName] = ["echo additional"]
        };

        // Act.
        var scripts = new PackageManifest.ScriptsType
        {
            AdditionalScripts = additionalScripts
        };

        // Assert.
        Assert.NotNull(scripts.AdditionalProperties);
        Assert.Single(scripts.AdditionalProperties);
        Assert.Single(scripts.AdditionalScripts);
        Assert.Equal(new[] { "echo additional" }, scripts.AdditionalScripts[scriptName]);
    }

    [Theory]
    [InlineData("invalid-script")]
    [InlineData("invalid.script")]
    [InlineData("invalid script")]
    [InlineData("invalidScript")]
    public void ScriptsType_Constructor_InvalidAdditionalScriptName_ThrowsArgumentException(string scriptName)
    {
        // Arrange.
        Dictionary<string, List<string>> additionalScripts = new()
        {
            [scriptName] = ["echo invalid"]
        };

        // Act.
        ArgumentException exception = Assert.Throws<ArgumentException>(
            "value", () => new PackageManifest.ScriptsType { AdditionalScripts = additionalScripts });

        // Assert.
        Assert.Equal($"Script name {scriptName} is invalid. (Parameter 'value')", exception.Message);
    }

    [Fact]
    public void ScriptsType_Deserialize_MinimumJson_Passes()
    {
        // Arrange.
        string json = "{}";

        // Act.
        PackageManifest.ScriptsType? scripts = JsonSerializer.Deserialize<PackageManifest.ScriptsType>(json);

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
        Assert.Null(scripts.AdditionalProperties);
        Assert.Equal([], scripts.AdditionalScripts);
    }

    [Fact]
    public void ScriptsType_Deserialize_MaximumJson_Passes()
    {
        // Arrange.
        string json = """
        {
            "pre_install": [],
            "install": [],
            "post_install": [],
            "pre_pack": [],
            "post_pack": [],
            "pre_uninstall": [],
            "uninstall": [],
            "post_uninstall": [],
            "additional_script": [
                "echo additional"
            ]
        }
        """;

        // Act.
        PackageManifest.ScriptsType? scripts = JsonSerializer.Deserialize<PackageManifest.ScriptsType>(json);

        // Assert.
        Assert.NotNull(scripts);
        Assert.Equal([], scripts.PreInstall);
        Assert.Equal([], scripts.Install);
        Assert.Equal([], scripts.PostInstall);
        Assert.Equal([], scripts.PrePack);
        Assert.Equal([], scripts.PostPack);
        Assert.Equal([], scripts.PreUninstall);
        Assert.Equal([], scripts.Uninstall);
        Assert.Equal([], scripts.PostUninstall);
        Assert.NotNull(scripts.AdditionalProperties);
        Assert.Single(scripts.AdditionalProperties);
        Assert.Contains("additional_script", scripts.AdditionalProperties);
        Assert.Single(scripts.AdditionalScripts);
        Assert.Contains("additional_script", scripts.AdditionalScripts);
        Assert.Equal(["echo additional"], scripts.AdditionalScripts["additional_script"]);
    }

    [Theory]
    [InlineData("invalid-script")]
    [InlineData("invalid.script")]
    [InlineData("invalid script")]
    [InlineData("invalidScript")]
    public void ScriptsType_Deserialize_InvalidPropertyKey_Passes(string scriptName)
    {
        // Arrange.
        string json = $$"""
        {
            "{{scriptName}}": []
        }
        """;

        // Act.
        PackageManifest.ScriptsType? scripts = JsonSerializer.Deserialize<PackageManifest.ScriptsType>(json);

        // Assert.
        Assert.NotNull(scripts);
        Assert.NotNull(scripts.AdditionalProperties);
        Assert.Single(scripts.AdditionalProperties);
        Assert.Equal([], scripts.AdditionalScripts);
    }

    [Fact]
    public void ScriptsType_Deserialize_InvalidPropertyValueKind_Passes()
    {
        // Arrange.
        string json = """
        {
            "additional_script_1": null
        }
        """;

        // Act.
        PackageManifest.ScriptsType? scripts = JsonSerializer.Deserialize<PackageManifest.ScriptsType>(json);

        // Assert.
        Assert.NotNull(scripts);
        Assert.NotNull(scripts.AdditionalProperties);
        Assert.Single(scripts.AdditionalProperties);
        Assert.Equal([], scripts.AdditionalScripts);
    }

    [Fact]
    public void ScriptsType_Deserialize_InvalidPropertyItemValueKind_Passes()
    {
        // Arrange.
        string json = """
        {
            "additional_script": [
                null
            ]
        }
        """;

        // Act.
        PackageManifest.ScriptsType? scripts = JsonSerializer.Deserialize<PackageManifest.ScriptsType>(json);

        // Assert.
        Assert.NotNull(scripts);
        Assert.NotNull(scripts.AdditionalProperties);
        Assert.Single(scripts.AdditionalProperties);
        Assert.Equal([], scripts.AdditionalScripts);
    }

    [Fact]
    public void VariantType_Deserialize_MinimumJson_Passes()
    {
        // Arrange.
        string json = "{}";

        // Act.
        PackageManifest.VariantType? variant = JsonSerializer.Deserialize<PackageManifest.VariantType>(json);

        // Assert.
        Assert.NotNull(variant);
        Assert.Null(variant.Platform);
        Assert.Null(variant.Dependencies);
        Assert.Null(variant.Assets);
        Assert.Null(variant.Scripts);
    }

    [Fact]
    public void VariantType_Deserialize_MaximumJson_Passes()
    {
        // Arrange.
        string json = """
        {
            "label": "",
            "platform": "",
            "dependencies": {},
            "assets": [],
            "scripts": {}
        }
        """;

        // Act.
        PackageManifest.VariantType? variant = JsonSerializer.Deserialize<PackageManifest.VariantType>(json);

        // Assert.
        Assert.NotNull(variant);
        Assert.Equal("", variant.VariantLabel);
        Assert.Equal("", variant.Platform);
        Assert.Equal([], variant.Dependencies);
        Assert.Equal([], variant.Assets);
        Assert.NotNull(variant.Scripts);
    }

    [Fact]
    public void FromBytes_MinimumJson_Passes()
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
        var manifest = PackageManifest.FromJsonBytes(bytes);

        // Assert.
        Assert.Equal(3, manifest.FormatVersion);
        Assert.Equal("289f771f-2c9a-4d73-9f3f-8492495a924d", manifest.FormatUuid);
        Assert.Equal("", manifest.ToothPath);
        Assert.Equal("1.0.0", manifest.Version);
    }

    [Fact]
    public void FromBytes_MaximumJson_Passes()
    {
        // Arrange.
        byte[] bytes = Encoding.UTF8.GetBytes("""
            {
                "format_version": 3,
                "format_uuid": "289f771f-2c9a-4d73-9f3f-8492495a924d",
                "tooth": "",
                "version": "1.0.0",
                "info": {},
                "variants": []
            }
            """);

        // Act.
        var manifest = PackageManifest.FromJsonBytes(bytes);

        // Assert.
        Assert.Equal(3, manifest.FormatVersion);
        Assert.Equal("289f771f-2c9a-4d73-9f3f-8492495a924d", manifest.FormatUuid);
        Assert.Equal("", manifest.ToothPath);
        Assert.Equal("1.0.0", manifest.Version);
        Assert.NotNull(manifest.Info);
        Assert.NotNull(manifest.Variants);
        Assert.Equal([], manifest.Variants);
    }

    [Fact]
    public void FromBytes_NullJson_ThrowsArgumentException()
    {
        // Arrange.
        byte[] bytes = Encoding.UTF8.GetBytes("null");

        // Act.
        ArgumentException exception = Assert.Throws<ArgumentException>("bytes", () => PackageManifest.FromJsonBytes(bytes));

        // Assert.
        Assert.Equal("Failed to deserialize package manifest. (Parameter 'bytes')", exception.Message);
    }

    [Fact]
    public void FromBytes_InvalidFormatVersion_ThrowsArgumentException()
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

        // Act.
        ArgumentException exception = Assert.Throws<ArgumentException>("value", () => PackageManifest.FromJsonBytes(bytes));

        // Assert.
        Assert.Equal("Format version '0' is not equal to 3. (Parameter 'value')", exception.Message);
    }

    [Fact]
    public void FromBytes_InvalidFormatUuid_ThrowsArgumentException()
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

        // Act.
        ArgumentException exception = Assert.Throws<ArgumentException>("value", () => PackageManifest.FromJsonBytes(bytes));

        // Assert.
        Assert.Equal("Format UUID 'invalid-uuid' is not equal to 289f771f-2c9a-4d73-9f3f-8492495a924d. (Parameter 'value')", exception.Message);
    }

    [Fact]
    public void FromBytes_InvalidVersion_ThrowsArgumentException()
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

        // Act.
        ArgumentException exception = Assert.Throws<ArgumentException>("value", () => PackageManifest.FromJsonBytes(bytes));

        // Assert.
        Assert.Equal("Version '0.0.0.0' is invalid. (Parameter 'value')", exception.Message);
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
            Version = "1.0.0"
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
            Version = "1.0.0",
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
            Version = "1.0.0",
            Variants =
            [
                new PackageManifest.VariantType
                {
                    VariantLabel = manifestVariantLabel,
                    Platform = manifestPlatform,
                }
            ]
        };

        // Act.
        PackageManifest.VariantType? variant = manifest.GetSpecifiedVariant(variantLabel, platform);

        // Assert.
        Assert.NotNull(variant);
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
        Assert.Equal([], variant.Scripts.AdditionalProperties);
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
            Version = "1.0.0",
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
        Assert.Equal([], variant.Scripts.AdditionalProperties);
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
            Version = "1.0.0",
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
                        AdditionalProperties = {}
                    }
                }
            ]
        };

        // Act.
        PackageManifest.VariantType? variant = manifest.GetSpecifiedVariant("", "platform");

        // Assert.
        Assert.NotNull(variant);
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
        Assert.NotNull(variant.Scripts.AdditionalProperties);
        Assert.Empty(variant.Scripts.AdditionalProperties);
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
            Version = "1.0.0",
            Variants =
            [
                new PackageManifest.VariantType
                {
                    VariantLabel = manifestVariantLabel,
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
            Version = "1.0.0",
            Variants =
            [
                new PackageManifest.VariantType
                {
                    VariantLabel = manifestVariantLabel,
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
            Version = "1.0.0",
            Variants =
            [
            new()
            {
                Platform = "platform",
                Dependencies = new Dictionary<string, string>
                {
                { "dependency1", "1.0.0" }
                },
                Assets =
                [
                new()
                {
                    Type = PackageManifest.AssetType.TypeEnum.Self,
                    Urls = ["url1"]
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
                { "dependency2", "2.0.0" }
                },
                Assets =
                [
                new()
                {
                    Type = PackageManifest.AssetType.TypeEnum.Self,
                    Urls = ["url2"]
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
        Assert.Equal("", variant.VariantLabel);
        Assert.Equal("platform", variant.Platform);
        Assert.NotNull(variant.Dependencies);
        Assert.Equal(2, variant.Dependencies.Count);
        Assert.Equal("1.0.0", variant.Dependencies["dependency1"]);
        Assert.Equal("2.0.0", variant.Dependencies["dependency2"]);
        Assert.NotNull(variant.Assets);
        Assert.Equal(2, variant.Assets.Count);
        Assert.Equal(PackageManifest.AssetType.TypeEnum.Self, variant.Assets[0].Type);
        Assert.Equal(["url1"], variant.Assets[0].Urls);
        Assert.Equal(PackageManifest.AssetType.TypeEnum.Self, variant.Assets[1].Type);
        Assert.Equal(["url2"], variant.Assets[1].Urls);
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
    public void ToBytes_MinimumJson_Passes()
    {
        // Arrange.
        var manifest = new PackageManifest
        {
            FormatVersion = 3,
            FormatUuid = "289f771f-2c9a-4d73-9f3f-8492495a924d",
            ToothPath = "",
            Version = "1.0.0"
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
    public void WithTemplateParsed_CommonInput_Passes()
    {
        // Arrange.
        var manifest = new PackageManifest
        {
            FormatVersion = 3,
            FormatUuid = "289f771f-2c9a-4d73-9f3f-8492495a924d",
            ToothPath = "",
            Version = "1.0.0",
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
        Assert.Equal("1.0.0", result.Version);
        Assert.NotNull(result.Variants);
        Assert.Single(result.Variants);
        Assert.NotNull(result.Variants[0].Assets);
        Assert.Single(result.Variants[0].Assets!);
        Assert.Equal(new[] { "https://example.com/1.0.0.zip" }, result.Variants[0].Assets![0].Urls);
    }

    [Fact]
    public void WithTemplateParsed_InvalidTemplate_ThrowsArgumentException()
    {
        // Arrange.
        var manifest = new PackageManifest
        {
            FormatVersion = 3,
            FormatUuid = "289f771f-2c9a-4d73-9f3f-8492495a924d",
            ToothPath = "",
            Version = "1.0.0",
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
