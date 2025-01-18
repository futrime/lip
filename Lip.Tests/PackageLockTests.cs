using System.Text;
using System.Text.Json;

namespace Lip.Tests;

public class PackageLockTests
{
    [Fact]
    public void FromBytes_MinimumJson_Passes()
    {
        // Arrange
        byte[] bytes = Encoding.UTF8.GetBytes("""
            {
                "format_version": 3,
                "format_uuid": "289f771f-2c9a-4d73-9f3f-8492495a924d",
                "packages": [],
                "locks": []
            }
            """);

        // Act
        var lockFile = PackageLock.FromJsonBytes(bytes);

        // Assert
        Assert.Equal(3, lockFile.FormatVersion);
        Assert.Equal("289f771f-2c9a-4d73-9f3f-8492495a924d", lockFile.FormatUuid);
        Assert.NotNull(lockFile.Packages);
        Assert.Empty(lockFile.Packages);
        Assert.NotNull(lockFile.Locks);
        Assert.Empty(lockFile.Locks);
    }

    [Fact]
    public void FromBytes_MaximumJson_Passes()
    {
        // Arrange
        byte[] bytes = Encoding.UTF8.GetBytes("""
            {
                "format_version": 3,
                "format_uuid": "289f771f-2c9a-4d73-9f3f-8492495a924d",
                "packages": [
                    {
                        "format_version": 3,
                        "format_uuid": "289f771f-2c9a-4d73-9f3f-8492495a924d",
                        "tooth": "test/package",
                        "version": "1.0.0"
                    }
                ],
                "locks": [
                    {
                        "tooth": "test/package",
                        "variant": "default",
                        "version": "1.0.0"
                    }
                ]
            }
            """);

        // Act
        var lockFile = PackageLock.FromJsonBytes(bytes);

        // Assert
        Assert.Equal(3, lockFile.FormatVersion);
        Assert.Equal("289f771f-2c9a-4d73-9f3f-8492495a924d", lockFile.FormatUuid);
        Assert.Single(lockFile.Packages);
        Assert.Single(lockFile.Locks);
    }

    [Fact]
    public void FromBytes_NullJson_ThrowsArgumentException()
    {
        // Arrange
        byte[] bytes = Encoding.UTF8.GetBytes("null");

        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(
            "bytes", () => PackageLock.FromJsonBytes(bytes));
        Assert.Equal("Failed to deserialize package manifest. (Parameter 'bytes')", exception.Message);
    }

    [Fact]
    public void FromBytes_InvalidFormatVersion_ThrowsArgumentException()
    {
        // Arrange
        byte[] bytes = Encoding.UTF8.GetBytes("""
            {
                "format_version": 0,
                "format_uuid": "289f771f-2c9a-4d73-9f3f-8492495a924d",
                "packages": [],
                "locks": []
            }
            """);

        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(
            "value", () => PackageLock.FromJsonBytes(bytes));
        Assert.Equal("Format version '0' is not equal to 3. (Parameter 'value')", exception.Message);
    }

    [Fact]
    public void FromBytes_InvalidFormatUuid_ThrowsArgumentException()
    {
        // Arrange
        byte[] bytes = Encoding.UTF8.GetBytes("""
            {
                "format_version": 3,
                "format_uuid": "invalid-uuid",
                "packages": [],
                "locks": []
            }
            """);

        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(
            "value", () => PackageLock.FromJsonBytes(bytes));
        Assert.Equal(
            "Format UUID 'invalid-uuid' is not equal to 289f771f-2c9a-4d73-9f3f-8492495a924d. (Parameter 'value')",
            exception.Message);
    }

    [Fact]
    public void ToBytes_MinimumJson_Passes()
    {
        // Arrange
        var lockFile = new PackageLock
        {
            FormatVersion = 3,
            FormatUuid = "289f771f-2c9a-4d73-9f3f-8492495a924d",
            Packages = [],
            Locks = []
        };

        // Act
        byte[] bytes = lockFile.ToJsonBytes();

        // Assert
        Assert.Equal("""
            {
                "format_version": 3,
                "format_uuid": "289f771f-2c9a-4d73-9f3f-8492495a924d",
                "packages": [],
                "locks": []
            }
            """.ReplaceLineEndings(), Encoding.UTF8.GetString(bytes).ReplaceLineEndings());
    }

    [Fact]
    public void ToBytes_MaximumJson_Passes()
    {
        // Arrange
        var lockFile = new PackageLock
        {
            FormatVersion = 3,
            FormatUuid = "289f771f-2c9a-4d73-9f3f-8492495a924d",
            Packages = [
                new() {
                    FormatVersion = 3,
                    FormatUuid = "289f771f-2c9a-4d73-9f3f-8492495a924d",
                    ToothPath = "test/package",
                    Version = "1.0.0"
                }
            ],
            Locks = [
                new() {
                    ToothPath = "test/package",
                    VariantLabel = "default",
                    Version = "1.0.0"
                }
            ]
        };

        // Act
        byte[] bytes = lockFile.ToJsonBytes();

        // Assert
        Assert.Equal("""
            {
                "format_version": 3,
                "format_uuid": "289f771f-2c9a-4d73-9f3f-8492495a924d",
                "packages": [
                    {
                        "format_version": 3,
                        "format_uuid": "289f771f-2c9a-4d73-9f3f-8492495a924d",
                        "tooth": "test/package",
                        "version": "1.0.0"
                    }
                ],
                "locks": [
                    {
                        "tooth": "test/package",
                        "variant": "default",
                        "version": "1.0.0"
                    }
                ]
            }
            """.ReplaceLineEndings(), Encoding.UTF8.GetString(bytes).ReplaceLineEndings());
    }

    [Fact]
    public void LockType_Deserialize_MinimumJson_Passes()
    {
        // Arrange
        string json = """
            {
                "tooth": "test/package",
                "variant": "default",
                "version": "1.0.0"
            }
            """;

        // Act
        PackageLock.LockType? lockType = JsonSerializer.Deserialize<PackageLock.LockType>(json);

        // Assert
        Assert.NotNull(lockType);
        Assert.Equal("test/package", lockType.ToothPath);
        Assert.Equal("default", lockType.VariantLabel);
        Assert.Equal("1.0.0", lockType.Version);
    }
}
