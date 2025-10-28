namespace Gehtsoft.FourCDesigner.Logic.Plan;

/// <summary>
/// Factory for retrieving AI prompts for lesson plan assistance operations.
/// </summary>
public interface IPromptFactory
{
    /// <summary>
    /// Gets the AI prompt for the specified request.
    /// </summary>
    /// <param name="requestId">The request identifier.</param>
    /// <returns>The prompt text for the AI operation.</returns>
    /// <exception cref="System.ArgumentException">Thrown when requestId is not recognized.</exception>
    string GetPrompt(RequestId requestId);
}
