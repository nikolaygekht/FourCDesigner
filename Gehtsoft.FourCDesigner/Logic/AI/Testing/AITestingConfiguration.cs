using Microsoft.Extensions.Configuration;

namespace Gehtsoft.FourCDesigner.Logic.AI.Testing;

/// <summary>
/// Configuration implementation for AI testing driver settings.
/// </summary>
public class AITestingConfiguration : IAITestingConfiguration
{
    private readonly IConfiguration mConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="AITestingConfiguration"/> class.
    /// </summary>
    /// <param name="configuration">The configuration provider.</param>
    /// <exception cref="ArgumentNullException">Thrown when configuration is null.</exception>
    public AITestingConfiguration(IConfiguration configuration)
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        mConfiguration = configuration;
    }

    /// <inheritdoc/>
    public string MockFilePath
    {
        get
        {
            string? value = mConfiguration["ai:mock:file"];
            if (string.IsNullOrEmpty(value))
                return "./data/ai-mock-responses.json"; // Default path

            return value;
        }
    }
}
