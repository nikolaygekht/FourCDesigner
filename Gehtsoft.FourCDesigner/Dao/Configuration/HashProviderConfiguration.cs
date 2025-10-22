using Microsoft.Extensions.Configuration;

namespace Gehtsoft.FourCDesigner.Dao.Configuration;

/// <summary>
/// Configuration implementation for hash provider.
/// </summary>
public class HashProviderConfiguration : IHashProviderConfiguration
{
    private readonly IConfiguration mConfiguration;
    private const string DefaultSalt = "FourCDesigner-DefaultSalt-2025";

    /// <summary>
    /// Initializes a new instance of the <see cref="HashProviderConfiguration"/> class.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <exception cref="ArgumentNullException">Thrown when configuration is null.</exception>
    public HashProviderConfiguration(IConfiguration configuration)
    {
        mConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <inheritdoc/>
    public string Salt
    {
        get
        {
            string? salt = mConfiguration["hash:salt"];
            return string.IsNullOrEmpty(salt) ? DefaultSalt : salt;
        }
    }
}
