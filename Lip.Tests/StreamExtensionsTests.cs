namespace Lip.Tests;

public class StreamExtensionsTests
{
    [Fact]
    public async Task ReadAsync_ReadsStream()
    {
        // Arrange.
        byte[] expected = [1, 2, 3, 4, 5];
        MemoryStream stream = new(expected);

        // Act.
        byte[] actual = await stream.ReadAsync();

        // Assert.
        Assert.Equal(expected, actual);
    }
}
