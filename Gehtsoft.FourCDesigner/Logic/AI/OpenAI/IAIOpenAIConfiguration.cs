namespace Gehtsoft.FourCDesigner.Logic.AI.OpenAI;

/// <summary>
/// Configuration interface for OpenAI driver settings.
/// </summary>
public interface IAIOpenAIConfiguration
{
    /// <summary>
    /// Gets the OpenAI service URL.
    /// </summary>
    string ServiceUrl { get; }

    /// <summary>
    /// Gets the OpenAI API key.
    /// </summary>
    string ApiKey { get; }

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
