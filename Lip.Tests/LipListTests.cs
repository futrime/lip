using System.IO.Abstractions.TestingHelpers;
using System.Runtime.InteropServices;
using Lip.Context;
using Moq;

namespace Lip.Tests;

public class LipListTests
{
    [Fact]
    public void ListArgs_Constructor_TrivialValues_Passes()
    {
        // Arrange.
        Lip.ListArgs listArgs = new();

        // Act.
        listArgs = listArgs with { };
    }

    [Fact]
    public void ListResultItem_Constructor_TrivialValues_Passes()
    {
        // Arrange.
        Lip.ListResultItem listResultItem = new()
        {
            Manifest = new()
            {
                FormatVersion = PackageManifest.DefaultFormatVersion,
                FormatUuid = PackageManifest.DefaultFormatUuid,
                ToothPath = "example.com/pkg1",
                VersionText = "1.0.0"
            },
            Locked = true
        };

        // Act.
        listResultItem = listResultItem with { };

        // Assert.
        Assert.Equal(PackageManifest.DefaultFormatVersion, listResultItem.Manifest.FormatVersion);
        Assert.Equal(PackageManifest.DefaultFormatUuid, listResultItem.Manifest.FormatUuid);
        Assert.Equal("example.com/pkg1", listResultItem.Manifest.ToothPath);
        Assert.Equal("1.0.0", listResultItem.Manifest.VersionText);
        Assert.True(listResultItem.Locked);
    }

    [Fact]
    public async Task List_ReturnsListItems()
    {
        // Arrange.
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { "tooth_lock.json", new MockFileData($$"""
            {
                "format_version": {{PackageLock.DefaultFormatVersion}},
                "format_uuid": "{{PackageLock.DefaultFormatUuid}}",
                "locks": [
                    {
                        "locked": true,
                        "package": {
                            "format_version": {{PackageManifest.DefaultFormatVersion}},
                            "format_uuid": "{{PackageManifest.DefaultFormatUuid}}",
                            "tooth": "example.com/pkg1",
                            "version": "1.0.0",
                            "variants": [
                                {
                                    "label": "variant1",
                                    "platform": "{{RuntimeInformation.RuntimeIdentifier}}"
                                }
                            ]
                        },
                        "variant": "variant1"
                    },
                    {
                        "locked": false,
                        "package": {
                            "format_version": {{PackageManifest.DefaultFormatVersion}},
                            "format_uuid": "{{PackageManifest.DefaultFormatUuid}}",
                            "tooth": "example.com/pkg2",
                            "version": "1.0.1",
                            "variants": [
                                {
                                    "label": "variant2",
                                    "platform": "{{RuntimeInformation.RuntimeIdentifier}}"
                                }
                            ]
                        },
                        "variant": "variant1"
                    }
                ]
            }
            """) }
        });

        Mock<IContext> context = new();
        context.SetupGet(c => c.FileSystem).Returns(fileSystem);

        Lip lip = new(new(), context.Object);

        // Act.
        List<Lip.ListResultItem> listItems = await lip.List(new());

        // Assert.
        Assert.Equal(2, listItems.Count);

        Assert.Equal("example.com/pkg1", listItems[0].Manifest.ToothPath);
        Assert.Equal("1.0.0", listItems[0].Manifest.VersionText);
        Assert.Equal("variant1", listItems[0].Manifest.Variants![0].VariantLabel);
        Assert.True(listItems[0].Locked);

        Assert.Equal("example.com/pkg2", listItems[1].Manifest.ToothPath);
        Assert.Equal("1.0.1", listItems[1].Manifest.VersionText);
        Assert.Equal("variant2", listItems[1].Manifest.Variants![0].VariantLabel);
        Assert.False(listItems[1].Locked);
    }
}
