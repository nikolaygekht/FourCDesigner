using Microsoft.Extensions.Configuration;

namespace Gehtsoft.FourCDesigner.Logic.AI.OpenAI;

/// <summary>
/// Configuration implementation for OpenAI driver settings.
/// </summary>
public class AIOpenAIConfiguration : IAIOpenAIConfiguration
{
    private readonly IConfiguration mConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="AIOpenAIConfiguration"/> class.
    /// </summary>
    /// <param name="configuration">The configuration provider.</param>
    /// <exception cref="ArgumentNullException">Thrown when configuration is null.</exception>
    public AIOpenAIConfiguration(IConfiguration configuration)
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        mConfiguration = configuration;
    }

    /// <inheritdoc/>
    public string ServiceUrl
    {
        get
        {
            string? value = mConfiguration["ai:openai:url"];
            if (string.IsNullOrEmpty(value))
                return "https://api.openai.com/v1"; // Default OpenAI URL

            return value;
        }
    }

    /// <inheritdoc/>
    public string ApiKey
    {
        get
        {
            string? value = mConfiguration["ai:openai:key"];
            if (string.IsNullOrEmpty(value))
                throw new InvalidOperationException(
                    "OpenAI API key not configured in ai:openai:key");

            return value;
        }
    }

    /// <inheritdoc/>
    public string Model
    {
        get
        {
            string? value = mConfiguration["ai:openai:model"];
            if (string.IsNullOrEmpty(value))
                return "gpt-3.5-turbo"; // Default model

            return value;
        }
    }

    /// <inheritdoc/>
    public int MaxTokens
    {
        get
        {
            string? value = mConfiguration["ai:openai:maxTokens"];
            if (string.IsNullOrEmpty(value) || !int.TryParse(value, out int result))
                return 1000; // Default max tokens

            return result;
        }
    }

    /// <inheritdoc/>
    public int TimeoutSeconds
    {
        get
        {
            string? value = mConfiguration["ai:openai:timeoutSeconds"];
            if (string.IsNullOrEmpty(value) || !int.TryParse(value, out int result))
                return 60; // Default timeout in seconds

            return result;
        }
    }
}
