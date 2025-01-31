using System.Text;
using Moq;

namespace Lip.Tests;

public class IFileSourceTests
{
    [Fact]
    public async Task GetFileStream_ValidKey_ReturnsStream()
    {
        // Arrange.
        string key = "key";
        string content = "content";

        Mock<IFileSourceEntry> entry = new();
        entry.SetupGet(e => e.Key).Returns(key);
        entry.Setup(e => e.OpenRead())
             .ReturnsAsync(new MemoryStream(Encoding.UTF8.GetBytes(content)));

        Mock<IFileSource> fileSource = new();
        fileSource.Setup(f => f.GetEntry(key)).ReturnsAsync(entry.Object);

        // Act.
        Stream? stream = await fileSource.Object.GetFileStream(key);

        // Assert.
        Assert.NotNull(stream);
        Assert.Equal(content, new StreamReader(stream).ReadToEnd());
    }

    [Fact]
    public async Task GetFileStream_InvalidKey_ReturnsNull()
    {
        // Arrange.
        Mock<IFileSource> fileSource = new();
        fileSource.Setup(f => f.GetEntry(It.IsAny<string>())).ReturnsAsync((IFileSourceEntry?)null);

        // Act.
        Stream? stream = await fileSource.Object.GetFileStream(string.Empty);

        // Assert.
        Assert.Null(stream);
    }
}
