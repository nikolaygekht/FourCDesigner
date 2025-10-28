using Microsoft.Extensions.Configuration;

namespace Gehtsoft.FourCDesigner.Logic.AI.Testing;

/// <summary>
/// Configuration implementation for AI testing driver settings.
/// </summary>
public class AITestingConfiguration : IAITestingConfiguration
{
    private readonly IConfiguration mConfiguration;
    private readonly string mConfigName;

    /// <summary>
    /// Initializes a new instance of the <see cref="AITestingConfiguration"/> class.
    /// </summary>
    /// <param name="configuration">The configuration provider.</param>
    /// <param name="configName">The configuration name to use.</param>
    /// <exception cref="ArgumentNullException">Thrown when configuration or configName is null.</exception>
    public AITestingConfiguration(IConfiguration configuration, string configName)
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        if (configName == null)
            throw new ArgumentNullException(nameof(configName));

        mConfiguration = configuration;
        mConfigName = configName;
    }

    /// <inheritdoc/>
    public string MockFilePath
    {
        get
        {
            string? value = mConfiguration[$"ai:{mConfigName}:file"];
            if (string.IsNullOrEmpty(value))
                return "./data/ai-mock-responses.json";

            return value;
        }
    }
}
