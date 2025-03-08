using System.Runtime.InteropServices;
using System.Text.Json;

namespace Lip.Migration.Tests;

public class MigrationFromV1Tests
{
    [Fact]
    public void IsMigratable_FormatVersionIsOne_ReturnsTrue()
    {
        // Arrange
        string text = """
            {
                "format_version": 1
            }
            """;

        using JsonDocument doc = JsonDocument.Parse(text);

        // Act
        bool result = MigratorFromV1.IsMigratable(doc.RootElement);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsMigratable_FormatVersionIsNotTwo_ReturnsFalse()
    {
        // Arrange
        string text = """
            {
                "format_version": 0
            }
            """;

        using JsonDocument doc = JsonDocument.Parse(text);

        // Act
        bool result = MigratorFromV1.IsMigratable(doc.RootElement);

        // Assert
        Assert.False(result);
    }


    [Fact]
    public void IsMigratable_FormatVersionIsNotInt_ReturnsFalse()
    {
        // Arrange
        string text = """
            {
                "format_version": "0"
            }
            """;

        using JsonDocument doc = JsonDocument.Parse(text);

        // Act
        bool result = MigratorFromV1.IsMigratable(doc.RootElement);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsMigratable_FormatVersionIsMissing_ReturnsFalse()
    {
        // Arrange
        string text = """
            {
                "foo": "bar"
            }
            """;

        using JsonDocument doc = JsonDocument.Parse(text);

        // Act
        bool result = MigratorFromV1.IsMigratable(doc.RootElement);

        // Assert
        Assert.False(result);
    }

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        IndentSize = 4,
        WriteIndented = true,
    };

    [Fact]
    public void Migrate_ValidJson_ReturnsMigratedJson()
    {
        // Arrange.
        string jsonTextV1 = """
            {
                "format_version": 1,
                "tooth": "github.com/LiteLScript-Dev/HelperLib",
                "version": "2.14.1",
                "dependencies": {},
                "information": {
                    "name": "HelperLib",
                    "description": "TypeScript.d.ts file with auto-completion and code hints for LiteLScript developers",
                    "homepage": "https://github.com/LiteLScript-Dev/HelperLib"
                },
                "placement": [
                    {
                        "source": "src/*",
                        "destination": "declaration/llse/*"
                    }
                ]
            }
            """;

        using JsonDocument jsonDocumentV1 = JsonDocument.Parse(jsonTextV1);

        string expectedJsonTextV3 = $$"""
            {
                "format_version": 3,
                "format_uuid": "289f771f-2c9a-4d73-9f3f-8492495a924d",
                "tooth": "github.com/LiteLScript-Dev/HelperLib",
                "version": "2.14.1",
                "info": {
                    "name": "HelperLib",
                    "description": "TypeScript.d.ts file with auto-completion and code hints for LiteLScript developers",
                    "tags": [],
                    "avatar_url": null
                },
                "variants": [
                    {
                        "label": null,
                        "platform": null,
                        "dependencies": {},
                        "assets": [
                            {
                                "type": "self",
                                "urls": null,
                                "placements": [
                                    {
                                        "type": "dir",
                                        "src": "src/",
                                        "dest": "declaration/llse/"
                                    }
                                ]
                            }
                        ],
                        "preserve_files": null,
                        "remove_files": null,
                        "scripts": null
                    }
                ]
            }
            """;

        // Act.
        JsonElement migratedJson = MigratorFromV1.Migrate(jsonDocumentV1.RootElement);

        string migratedJsonText = JsonSerializer.Serialize(migratedJson, _jsonSerializerOptions);

        // Assert.
        Assert.Equal(expectedJsonTextV3.ReplaceLineEndings(), migratedJsonText.ReplaceLineEndings());
    }
}
