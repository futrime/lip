using System.Runtime.InteropServices;
using System.Text.Json;

namespace Lip.Migration.Tests;

public class MigratorFromV2Tests
{
    [Fact]
    public void IsMigratable_FormatVersionIsTwo_ReturnsTrue()
    {
        // Arrange
        string text = """
            {
                "format_version": 2
            }
            """;

        using JsonDocument doc = JsonDocument.Parse(text);

        // Act
        bool result = MigratorFromV2.IsMigratable(doc.RootElement);

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
        bool result = MigratorFromV2.IsMigratable(doc.RootElement);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsMigratable_FormatVersionIsNotInt_ReturnsFalse()
    {
        // Arrange
        string text = """
            {
                "format_version": "2"
            }
            """;

        using JsonDocument doc = JsonDocument.Parse(text);

        // Act
        bool result = MigratorFromV2.IsMigratable(doc.RootElement);

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
        bool result = MigratorFromV2.IsMigratable(doc.RootElement);

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
        // Arrange
        string textV2 = """
            {
                "format_version": 2,
                "tooth": "github.com/LiteLDev/LeviLamina",
                "version": "1.1.0",
                "info": {
                    "name": "LeviLamina",
                    "description": "A lightweight, modular and versatile mod loader for Minecraft Bedrock Edition.",
                    "author": "levimc",
                    "tags": []
                },
                "asset_url": "https://github.com/LiteLDev/LeviLamina/releases/download/v$(version)/levilamina-release-windows-x64.zip",
                "dependencies": {
                    "github.com/LiteLDev/bds": "1.21.60",
                    "github.com/LiteLDev/CrashLogger": "1.3.x",
                    "github.com/LiteLDev/levilamina-loc": "1.5.x",
                    "github.com/LiteLDev/PeEditor": "3.8.x",
                    "github.com/LiteLDev/PreLoader": "1.12.x",
                    "github.com/LiteLDev/bedrock-runtime-data": "1.21.6010-server"
                },
                "files": {
                    "place": [
                        {
                            "src": "LeviLamina/*",
                            "dest": "plugins/LeviLamina/"
                        }
                    ],
                    "remove": [
                        "bedrock_server_mod.exe"
                    ]
                },
                "platforms": [
                    {
                        "goos": "windows",
                        "goarch": "amd64",
                        "commands": {
                            "post_install": [
                                ".\\PeEditor.exe -mb"
                            ],
                            "post_uninstall": [
                                "IF EXIST bedrock_server.exe (DEL bedrock_server.exe.bak) ELSE (REN bedrock_server.exe.bak bedrock_server.exe)"
                            ]
                        }
                    }
                ]
            }
            """;

        string expectedTextV3 = $$"""
            {
                "format_version": 3,
                "format_uuid": "289f771f-2c9a-4d73-9f3f-8492495a924d",
                "tooth": "github.com/LiteLDev/LeviLamina",
                "version": "1.1.0",
                "info": {
                    "name": "LeviLamina",
                    "description": "A lightweight, modular and versatile mod loader for Minecraft Bedrock Edition.",
                    "tags": [],
                    "avatar_url": null
                },
                "variants": [
                    {
                        "label": null,
                        "platform": "win-x64",
                        "dependencies": {
                            "github.com/LiteLDev/bds": "1.21.60",
                            "github.com/LiteLDev/CrashLogger": "1.3.x",
                            "github.com/LiteLDev/levilamina-loc": "1.5.x",
                            "github.com/LiteLDev/PeEditor": "3.8.x",
                            "github.com/LiteLDev/PreLoader": "1.12.x",
                            "github.com/LiteLDev/bedrock-runtime-data": "1.21.6010-server"
                        },
                        "assets": [
                            {
                                "type": "zip",
                                "urls": [
                                    "https://github.com/LiteLDev/LeviLamina/releases/download/v{{"{{version}}"}}/levilamina-release-windows-x64.zip"
                                ],
                                "placements": [
                                    {
                                        "type": "dir",
                                        "src": "LeviLamina/",
                                        "dest": "plugins/LeviLamina/"
                                    }
                                ]
                            }
                        ],
                        "preserve_files": null,
                        "remove_files": [
                            "bedrock_server_mod.exe"
                        ],
                        "scripts": {
                            "pre_install": null,
                            "install": null,
                            "post_install": [
                                ".\\PeEditor.exe -mb"
                            ],
                            "pre_pack": null,
                            "post_pack": null,
                            "pre_uninstall": null,
                            "uninstall": null,
                            "post_uninstall": [
                                "IF EXIST bedrock_server.exe (DEL bedrock_server.exe.bak) ELSE (REN bedrock_server.exe.bak bedrock_server.exe)"
                            ]
                        }
                    }
                ]
            }
            """;

        using JsonDocument docV2 = JsonDocument.Parse(textV2);

        // Act
        JsonElement result = MigratorFromV2.Migrate(docV2.RootElement);

        string resultText = JsonSerializer.Serialize(result, _jsonSerializerOptions);

        // Assert
        Assert.Equal(expectedTextV3.ReplaceLineEndings(), resultText.ReplaceLineEndings());
    }
}
