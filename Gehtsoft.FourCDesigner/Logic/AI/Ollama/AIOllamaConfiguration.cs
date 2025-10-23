using Microsoft.Extensions.Configuration;

namespace Gehtsoft.FourCDesigner.Logic.AI.Ollama;

/// <summary>
/// Configuration implementation for Ollama AI driver settings.
/// </summary>
public class AIOllamaConfiguration : IAIOllamaConfiguration
{
    private readonly IConfiguration mConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="AIOllamaConfiguration"/> class.
    /// </summary>
    /// <param name="configuration">The configuration provider.</param>
    /// <exception cref="ArgumentNullException">Thrown when configuration is null.</exception>
    public AIOllamaConfiguration(IConfiguration configuration)
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
            string? value = mConfiguration["ai:ollama:url"];
            if (string.IsNullOrEmpty(value))
                return "http://localhost:11434"; // Default Ollama URL

            return value;
        }
    }

    /// <inheritdoc/>
    public string Model
    {
        get
        {
            string? value = mConfiguration["ai:ollama:model"];
            if (string.IsNullOrEmpty(value))
                throw new InvalidOperationException(
                    "Ollama model not configured in ai:ollama:model");

            return value;
        }
    }

    /// <inheritdoc/>
    public int MaxTokens
    {
        get
        {
            string? value = mConfiguration["ai:ollama:maxTokens"];
            if (string.IsNullOrEmpty(value) || !int.TryParse(value, out int result))
                return 500; // Default max tokens

            return result;
        }
    }

    /// <inheritdoc/>
    public int TimeoutSeconds
    {
        get
        {
            string? value = mConfiguration["ai:ollama:timeoutSeconds"];
            if (string.IsNullOrEmpty(value) || !int.TryParse(value, out int result))
                return 120; // Default timeout in seconds

            return result;
        }
    }
}
