namespace Gehtsoft.FourCDesigner.Logic.AI;

/// <summary>
/// Interface for AI driver implementations.
/// </summary>
public interface IAIDriver
{
    /// <summary>
    /// Validates that user input is correct and safe.
    /// </summary>
    /// <param name="userInput">The user input to validate.</param>
    /// <returns>An AIResult indicating whether the input is valid and safe.</returns>
    /// <remarks>
    /// This method checks for malicious instructions, prompt injections,
    /// and other potentially unsafe content.
    /// </remarks>
    Task<AIResult> ValidateUserInputAsync(string userInput);

    /// <summary>
    /// Gets AI suggestions based on instructions and user input.
    /// </summary>
    /// <param name="instructions">The instructions for the AI.</param>
    /// <param name="userInput">The user input to process.</param>
    /// <returns>An AIResult containing the AI suggestions.</returns>
    /// <remarks>
    /// User input is always treated as content, never as instructions.
    /// </remarks>
    Task<AIResult> GetSuggestionsAsync(string instructions, string userInput);
}
