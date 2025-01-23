namespace Lip.Context;

/// <summary>
/// Represents a user interaction interface.
/// </summary>
public interface IUserInteraction
{
    /// <summary>
    /// Displays a confirmation dialog and returns user's choice.
    /// </summary>
    /// <param name="format">The message to display</param>
    /// <returns>True if confirmed, false otherwise</returns>
    Task<bool> Confirm(string format, params object[] args);

    /// <summary>
    /// Prompts user for text input.
    /// </summary>
    /// <param name="format">The prompt message</param>
    /// <returns>User input as string</returns>
    Task<string?> PromptForInput(string format, params object[] args);

    /// <summary>
    /// Prompts user to select from multiple options.
    /// </summary>
    /// <param name="options">Available options</param>
    /// <param name="format">The prompt message</param>
    /// <returns>Selected option</returns>
    Task<string> PromptForSelection(IEnumerable<string> options, string format, params object[] args);

    /// <summary>
    /// Shows progress for long-running operations.
    /// </summary>
    /// <param name="id">Progress ID</param>
    /// <param name="progress">Progress value (0.0-1.0)</param>
    /// <param name="format">Progress message</param>
    Task UpdateProgress(string id, float progress, string format, params object[] args);
}
