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
    /// Gets the request timeout in seconds.
    /// </summary>
    int TimeoutSeconds { get; }

    /// <summary>
    /// Gets the custom parameters to send to the OpenAI API.
    /// </summary>
    /// <remarks>
    /// This dictionary contains model-specific parameters such as:
    /// - "max_tokens" (for GPT-3.5, GPT-4)
    /// - "max_completion_tokens" (for GPT-5)
    /// - "temperature"
    /// - any other model-specific parameters
    /// </remarks>
    IReadOnlyDictionary<string, object> Parameters { get; }
}
