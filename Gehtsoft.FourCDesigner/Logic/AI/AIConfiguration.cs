using Microsoft.Extensions.Configuration;

namespace Gehtsoft.FourCDesigner.Logic.AI;

/// <summary>
/// Configuration implementation for AI driver settings.
/// </summary>
public class AIConfiguration : IAIConfiguration
{
    private readonly IConfiguration mConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="AIConfiguration"/> class.
    /// </summary>
    /// <param name="configuration">The configuration provider.</param>
    /// <exception cref="ArgumentNullException">Thrown when configuration is null.</exception>
    public AIConfiguration(IConfiguration configuration)
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        mConfiguration = configuration;
    }

    /// <inheritdoc/>
    public string Driver
    {
        get
        {
            string? value = mConfiguration["ai:driver"];
            if (string.IsNullOrEmpty(value))
                return "mock";

            return value;
        }
    }

    /// <inheritdoc/>
    public string Config
    {
        get
        {
            string? value = mConfiguration["ai:config"];
            if (string.IsNullOrEmpty(value))
            {
                string driver = Driver.ToLowerInvariant();
                return driver;
            }

            return value;
        }
    }
}
