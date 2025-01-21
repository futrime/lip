namespace Lip;

/// <summary>
/// Represents an exception that is thrown when a schema violation is detected.
/// </summary>
public class SchemaViolationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaViolationException"/> class.
    /// </summary>
    /// <param name="key">The key where the value violates the schema.</param>
    public SchemaViolationException(string key)
    {
        Key = key;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaViolationException"/> class with a specified error message.
    /// </summary>
    /// <param name="key">The key where the value violates the schema.</param>
    /// <param name="message">The message that describes the error.</param>
    public SchemaViolationException(string key, string? message)
        : base(message)
    {
        Key = key;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaViolationException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="key">The key where the value violates the schema.</param>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public SchemaViolationException(string key, string? message, Exception? innerException)
        : base(message, innerException)
    {
        Key = key;
    }

    public string Key { get; }
}
