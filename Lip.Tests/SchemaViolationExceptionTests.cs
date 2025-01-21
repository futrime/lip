namespace Lip.Tests;

public class SchemaViolationExceptionTests
{
    [Fact]
    public void Constructor_WithKeyOnly_SetsKeyProperty()
    {
        // Arrange.
        const string key = "testKey";

        // Act.
        var exception = new SchemaViolationException(key);

        // Assert.
        Assert.Equal(key, exception.Key);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public void Constructor_WithKeyAndMessage_SetsProperties()
    {
        // Arrange.
        const string key = "testKey";
        const string message = "Test error message";

        // Act.
        var exception = new SchemaViolationException(key, message);

        // Assert.
        Assert.Equal(key, exception.Key);
        Assert.Equal(message, exception.Message);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public void Constructor_WithKeyMessageAndInnerException_SetsProperties()
    {
        // Arrange.
        const string key = "testKey";
        const string message = "Test error message";
        var innerException = new InvalidOperationException();

        // Act.
        var exception = new SchemaViolationException(key, message, innerException);

        // Assert.
        Assert.Equal(key, exception.Key);
        Assert.Equal(message, exception.Message);
        Assert.Same(innerException, exception.InnerException);
    }

    [Fact]
    public void Key_IsAccessible_AfterConstruction()
    {
        // Arrange.
        const string key = "testKey";
        var exception = new SchemaViolationException(key);

        // Act.
        string actualKey = exception.Key;

        // Assert.
        Assert.Equal(key, actualKey);
    }
}
