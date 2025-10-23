namespace Gehtsoft.FourCDesigner.Logic.AI.Ollama;

/// <summary>
/// Configuration interface for Ollama AI driver settings.
/// </summary>
public interface IAIOllamaConfiguration
{
    /// <summary>
    /// Gets the Ollama service URL.
    /// </summary>
    string ServiceUrl { get; }

    /// <summary>
    /// Gets the model to use.
    /// </summary>
    string Model { get; }

    /// <summary>
    /// Gets the maximum number of tokens to generate in responses.
    /// </summary>
    int MaxTokens { get; }

    /// <summary>
    /// Gets the request timeout in seconds.
    /// </summary>
    int TimeoutSeconds { get; }
}
