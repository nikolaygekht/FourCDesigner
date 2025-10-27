using Microsoft.Extensions.Configuration;

namespace Gehtsoft.FourCDesigner.Middleware;

/// <summary>
/// Implementation of SSI middleware configuration that reads from IConfiguration.
/// </summary>
public class SsiMiddlewareConfig : ISsiMiddlewareConfig
{
    private readonly IConfiguration mConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="SsiMiddlewareConfig"/> class.
    /// </summary>
    /// <param name="configuration">The configuration provider.</param>
    /// <exception cref="ArgumentNullException">Thrown when configuration is null.</exception>
    public SsiMiddlewareConfig(IConfiguration configuration)
    {
        mConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration));
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
}
