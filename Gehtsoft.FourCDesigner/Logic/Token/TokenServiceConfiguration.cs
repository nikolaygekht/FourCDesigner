using Microsoft.Extensions.Configuration;

namespace Gehtsoft.FourCDesigner.Logic.Token;

/// <summary>
/// Configuration implementation for token service.
/// </summary>
public class TokenServiceConfiguration : ITokenServiceConfiguration
{
    private readonly IConfiguration mConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenServiceConfiguration"/> class.
    /// </summary>
    /// <param name="configuration">The configuration provider.</param>
    /// <exception cref="ArgumentNullException">Thrown when configuration is null.</exception>
    public TokenServiceConfiguration(IConfiguration configuration)
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        mConfiguration = configuration;
    }

    /// <inheritdoc/>
    public double ExpirationInSeconds
    {
        get
        {
            string? value = mConfiguration["system:session:tokenExpirationInSeconds"];
            if (string.IsNullOrEmpty(value))
                return 300.0; // Default: 5 minutes

            if (!double.TryParse(value, out double result))
                throw new InvalidOperationException($"Invalid token expiration value: {value}");

            return result;
        }
    }
}
