using Gehtsoft.FourCDesigner.Logic.Config;
using Microsoft.Extensions.Configuration;

namespace Gehtsoft.FourCDesigner.Middleware;

/// <summary>
/// Implementation of SSI middleware configuration that reads from IConfiguration.
/// </summary>
public class SsiMiddlewareConfig : ISsiMiddlewareConfig
{
    private readonly IConfiguration mConfiguration;
    private readonly ISystemConfig mSystemConfig;

    /// <summary>
    /// Initializes a new instance of the <see cref="SsiMiddlewareConfig"/> class.
    /// </summary>
    /// <param name="configuration">The configuration provider.</param>
    /// <param name="systemConfig">The system configuration.</param>
    /// <exception cref="ArgumentNullException">Thrown when configuration or systemConfig is null.</exception>
    public SsiMiddlewareConfig(IConfiguration configuration, ISystemConfig systemConfig)
    {
        mConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        mSystemConfig = systemConfig ?? throw new ArgumentNullException(nameof(systemConfig));
    }

    /// <inheritdoc/>
    public string AppVersion
    {
        get
        {
            string? value = mConfiguration["system:version"];
            if (string.IsNullOrEmpty(value))
                return "1.0.0"; // Default version

            return value;
        }
    }

    /// <inheritdoc/>
    public string ExternalPrefix
    {
        get
        {
            return mSystemConfig.ExternalPrefix;
        }
    }
}
