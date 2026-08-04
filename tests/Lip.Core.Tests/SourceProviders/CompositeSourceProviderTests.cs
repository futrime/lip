using Lip.Core.Sources;
using Moq;

namespace Lip.Core.Tests.Sources;

public class CompositeSourceTests {
  [Fact]
  public void Keys_AggregatesAndDistinctsKeys() {
    // Arrange
    Mock<ISource> mockProvider1 = new();
    mockProvider1.Setup(p => p.Keys).Returns(["file1.txt", "common.txt"]);

    Mock<ISource> mockProvider2 = new();
    mockProvider2.Setup(p => p.Keys).Returns(["file2.txt", "common.txt"]);

    CompositeSource provider = new([mockProvider1.Object, mockProvider2.Object]);

    // Act
    List<string> keys = [.. provider.Keys];

    // Assert
    Assert.Equal(3, keys.Count);
    Assert.Contains("file1.txt", keys);
    Assert.Contains("file2.txt", keys);
    Assert.Contains("common.txt", keys);
  }

  [Fact]
  public async Task OpenRead_ReturnsFromFirstProviderWithKey() {
    // Arrange
    Mock<ISource> mockProvider1 = new();
    mockProvider1.Setup(p => p.Keys).Returns(["file1.txt"]);
    mockProvider1.Setup(p => p.OpenRead("file1.txt"))
        .ReturnsAsync(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("content1")));

    Mock<ISource> mockProvider2 = new();
    mockProvider2.Setup(p => p.Keys).Returns(["file2.txt"]);

    CompositeSource provider = new([mockProvider1.Object, mockProvider2.Object]);

    // Act
    using Stream stream = await provider.OpenRead("file1.txt");
    using StreamReader reader = new(stream);
    string content = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);

    // Assert
    Assert.Equal("content1", content);
  }

  [Fact]
  public async Task OpenRead_KeyNotFound_ThrowsArgumentException() {
    // Arrange
    CompositeSource provider = new([]);

    // Act & Assert
    await Assert.ThrowsAsync<ArgumentException>(() => provider.OpenRead("missing"));
  }
}
