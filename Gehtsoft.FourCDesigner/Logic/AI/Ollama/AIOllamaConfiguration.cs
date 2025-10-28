using Microsoft.Extensions.Configuration;

namespace Gehtsoft.FourCDesigner.Logic.AI.Ollama;

/// <summary>
/// Configuration implementation for Ollama AI driver settings.
/// </summary>
public class AIOllamaConfiguration : IAIOllamaConfiguration
{
    private readonly IConfiguration mConfiguration;
    private readonly string mConfigName;

    /// <summary>
    /// Initializes a new instance of the <see cref="AIOllamaConfiguration"/> class.
    /// </summary>
    /// <param name="configuration">The configuration provider.</param>
    /// <param name="configName">The configuration name to use.</param>
    /// <exception cref="ArgumentNullException">Thrown when configuration or configName is null.</exception>
    public AIOllamaConfiguration(IConfiguration configuration, string configName)
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        if (configName == null)
            throw new ArgumentNullException(nameof(configName));

        mConfiguration = configuration;
        mConfigName = configName;
    }

    /// <inheritdoc/>
    public string ServiceUrl
    {
        get
        {
            string? value = mConfiguration[$"ai:{mConfigName}:url"];
            if (string.IsNullOrEmpty(value))
                return "http://localhost:11434";

            return value;
        }
    }

    /// <inheritdoc/>
    public string Model
    {
        get
        {
            string? value = mConfiguration[$"ai:{mConfigName}:model"];
            if (string.IsNullOrEmpty(value))
                throw new InvalidOperationException(
                    $"Ollama model not configured in ai:{mConfigName}:model");

            return value;
        }
    }

    /// <inheritdoc/>
    public int MaxTokens
    {
        get
        {
            string? value = mConfiguration[$"ai:{mConfigName}:maxTokens"];
            if (string.IsNullOrEmpty(value) || !int.TryParse(value, out int result))
                return 500;

            return result;
        }
    }

    /// <inheritdoc/>
    public int TimeoutSeconds
    {
        get
        {
            string? value = mConfiguration[$"ai:{mConfigName}:timeoutSeconds"];
            if (string.IsNullOrEmpty(value) || !int.TryParse(value, out int result))
                return 120;

            return result;
        }
    }
}
