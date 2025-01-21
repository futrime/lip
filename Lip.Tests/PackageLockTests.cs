using System.Text;
using System.Text.Json;
using Semver;

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
                        "tooth": "example.com/pkg",
                        "version": "1.0.0"
                    }
                ],
                "locks": [
                    {
                        "tooth": "example.com/pkg",
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
        Assert.Equal("example.com/pkg", lockFile.Packages[0].ToothPath);
        Assert.Equal("1.0.0", lockFile.Packages[0].VersionText);
        Assert.Single(lockFile.Locks);
        Assert.Equal("example.com/pkg", lockFile.Locks[0].ToothPath);
        Assert.Equal("default", lockFile.Locks[0].VariantLabel);
        Assert.Equal("1.0.0", lockFile.Locks[0].VersionText);
        Assert.Equal(SemVersion.Parse("1.0.0"), lockFile.Locks[0].Version);
    }

    [Fact]
    public void FromBytes_NullJson_Throws()
    {
        // Arrange
        byte[] bytes = Encoding.UTF8.GetBytes("null");

        // Act & Assert
        JsonException exception = Assert.Throws<JsonException>(() => PackageLock.FromJsonBytes(bytes));
        Assert.Equal("Package lock bytes deserialization failed.", exception.Message);
        Assert.NotNull(exception.InnerException);
        Assert.IsType<JsonException>(exception.InnerException);
        Assert.Equal("JSON bytes deserialized to null.", exception.InnerException.Message);
    }

    [Fact]
    public void FromBytes_InvalidFormatVersion_Throws()
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
        JsonException exception = Assert.Throws<JsonException>(() => PackageLock.FromJsonBytes(bytes));
        Assert.Equal("Package lock bytes deserialization failed.", exception.Message);
        Assert.NotNull(exception.InnerException);
        Assert.IsType<SchemaViolationException>(exception.InnerException);
        Assert.Equal("Format version '0' is not equal to 3.", exception.InnerException.Message);
    }

    [Fact]
    public void FromBytes_InvalidFormatUuid_Throws()
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
        JsonException exception = Assert.Throws<JsonException>(() => PackageLock.FromJsonBytes(bytes));
        Assert.Equal("Package lock bytes deserialization failed.", exception.Message);
        Assert.NotNull(exception.InnerException);
        Assert.IsType<SchemaViolationException>(exception.InnerException);
        Assert.Equal("Format UUID 'invalid-uuid' is not equal to 289f771f-2c9a-4d73-9f3f-8492495a924d.", exception.InnerException.Message);
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
                    ToothPath = "example.com/pkg",
                    VersionText = "1.0.0"
                }
            ],
            Locks = [
                new() {
                    ToothPath = "example.com/pkg",
                    VariantLabel = "default",
                    VersionText = "1.0.0"
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
                        "tooth": "example.com/pkg",
                        "version": "1.0.0"
                    }
                ],
                "locks": [
                    {
                        "tooth": "example.com/pkg",
                        "variant": "default",
                        "version": "1.0.0"
                    }
                ]
            }
            """.ReplaceLineEndings(), Encoding.UTF8.GetString(bytes).ReplaceLineEndings());
    }

    [Fact]
    public void LockType_Constructor_ValidValues_Passes()
    {
        // Arrange & Act
        var lockType = new PackageLock.LockType
        {
            ToothPath = "example.com/package",
            VariantLabel = "default",
            VersionText = "1.0.0"
        };

        // Assert
        Assert.Equal("example.com/package", lockType.ToothPath);
        Assert.Equal("default", lockType.VariantLabel);
        Assert.Equal("1.0.0", lockType.VersionText);
    }

    [Fact]
    public void LockType_Constructor_InvalidToothPath_Throws()
    {
        // Arrange & Act & Assert
        SchemaViolationException exception = Assert.Throws<SchemaViolationException>(() => new PackageLock.LockType
        {
            ToothPath = "invalid/tooth",
            VariantLabel = "default",
            VersionText = "1.0.0"
        });
        Assert.Equal("Invalid tooth path 'invalid/tooth'.", exception.Message);
    }

    [Fact]
    public void LockType_Constructor_InvalidVariantLabel_Throws()
    {
        // Arrange & Act & Assert
        SchemaViolationException exception = Assert.Throws<SchemaViolationException>(() => new PackageLock.LockType
        {
            ToothPath = "example.com/package",
            VariantLabel = "invalid-variant",
            VersionText = "1.0.0"
        });
        Assert.Equal("Invalid variant label 'invalid-variant'.", exception.Message);
    }

    [Fact]
    public void LockType_Constructor_InvalidVersion_Throws()
    {
        // Arrange & Act & Assert
        SchemaViolationException exception = Assert.Throws<SchemaViolationException>(() => new PackageLock.LockType
        {
            ToothPath = "example.com/package",
            VariantLabel = "default",
            VersionText = "invalid-version"
        });
        Assert.Equal("Invalid version 'invalid-version'.", exception.Message);
    }
}
