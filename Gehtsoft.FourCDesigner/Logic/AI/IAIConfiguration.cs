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
}
