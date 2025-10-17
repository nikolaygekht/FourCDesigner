using Microsoft.Extensions.Configuration;

namespace Gehtsoft.FourCDesigner.Logic.Session;

/// <summary>
/// Implementation of session settings that reads from IConfiguration.
/// </summary>
public class SessionSettings : ISessionSettings
{
    private readonly IConfiguration mConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionSettings"/> class.
    /// </summary>
    /// <param name="configuration">The configuration provider.</param>
    /// <exception cref="ArgumentNullException">Thrown when configuration is null.</exception>
    public SessionSettings(IConfiguration configuration)
    {
        mConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <inheritdoc/>
    public double SessionTimeoutInSeconds
    {
        get
        {
            string? value = mConfiguration["system:session:timeoutInSeconds"];
            if (string.IsNullOrEmpty(value))
                return 600.0; // Default: 10 minutes

            if (!double.TryParse(value, out double result) || result <= 0)
                return 600.0; // Default: 10 minutes

            return result;
        }
    }
}
