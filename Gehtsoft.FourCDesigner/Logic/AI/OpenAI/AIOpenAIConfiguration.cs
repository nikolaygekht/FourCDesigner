using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Gehtsoft.FourCDesigner.Logic.AI.OpenAI;

/// <summary>
/// Configuration implementation for OpenAI driver settings.
/// </summary>
public class AIOpenAIConfiguration : IAIOpenAIConfiguration
{
    private readonly IConfiguration mConfiguration;
    private readonly string mConfigName;
    private IReadOnlyDictionary<string, object>? mParameters;

    /// <summary>
    /// Initializes a new instance of the <see cref="AIOpenAIConfiguration"/> class.
    /// </summary>
    /// <param name="configuration">The configuration provider.</param>
    /// <param name="configName">The configuration name to use.</param>
    /// <exception cref="ArgumentNullException">Thrown when configuration or configName is null.</exception>
    public AIOpenAIConfiguration(IConfiguration configuration, string configName)
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
                return "https://api.openai.com/v1";

            return value;
        }
    }

    /// <inheritdoc/>
    public string ApiKey
    {
        get
        {
            string? value = mConfiguration[$"ai:{mConfigName}:key"];
            if (string.IsNullOrEmpty(value))
                throw new InvalidOperationException(
                    $"OpenAI API key not configured in ai:{mConfigName}:key");

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
                return "gpt-3.5-turbo";

            return value;
        }
    }

    /// <inheritdoc/>
    public int TimeoutSeconds
    {
        get
        {
            string? value = mConfiguration[$"ai:{mConfigName}:timeoutSeconds"];
            if (string.IsNullOrEmpty(value) || !int.TryParse(value, out int result))
                return 60;

            return result;
        }
    }

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, object> Parameters
    {
        get
        {
            if (mParameters != null)
                return mParameters;

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            IConfigurationSection? parametersSection = mConfiguration.GetSection($"ai:{mConfigName}:parameters");

            if (parametersSection.Exists())
            {
                foreach (IConfigurationSection child in parametersSection.GetChildren())
                {
                    string key = child.Key;
                    object value = ParseConfigurationValue(child);
                    parameters[key] = value;
                }
            }

            mParameters = parameters;
            return mParameters;
        }
    }

    /// <summary>
    /// Recursively parses a configuration section into a typed value or nested dictionary.
    /// </summary>
    /// <param name="section">The configuration section to parse.</param>
    /// <returns>A parsed value (int, double, bool, string, or Dictionary for nested objects).</returns>
    private static object ParseConfigurationValue(IConfigurationSection section)
    {
        // Check if this section has children (nested object)
        IEnumerable<IConfigurationSection> children = section.GetChildren();
        if (children.Any())
        {
            // This is a nested object - recursively parse children
            Dictionary<string, object> nestedDict = new Dictionary<string, object>();
            foreach (IConfigurationSection child in children)
            {
                nestedDict[child.Key] = ParseConfigurationValue(child);
            }
            return nestedDict;
        }

        // This is a leaf value - parse as primitive type
        string? value = section.Value;

        if (string.IsNullOrEmpty(value))
            return string.Empty;

        // Try to parse as specific types
        if (int.TryParse(value, out int intValue))
            return intValue;

        if (double.TryParse(value, out double doubleValue))
            return doubleValue;

        if (bool.TryParse(value, out bool boolValue))
            return boolValue;

        return value;
    }
}
