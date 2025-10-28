namespace Gehtsoft.FourCDesigner.Logic.AI;

/// <summary>
/// Configuration interface for AI driver settings.
/// </summary>
public interface IAIConfiguration
{
    /// <summary>
    /// Gets the AI driver type to use (mock, ollama, or openai).
    /// </summary>
    string Driver { get; }

    /// <summary>
    /// Gets the configuration name to use for the selected driver.
    /// </summary>
    /// <remarks>
    /// This allows multiple named configurations for the same driver type.
    /// For example: "openai-gpt-5", "openai-gpt-4", "ollama-llama3".
    /// </remarks>
    string Config { get; }
}
