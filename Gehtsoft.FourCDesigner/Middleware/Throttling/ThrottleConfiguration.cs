namespace Gehtsoft.FourCDesigner.Middleware.Throttling;

/// <summary>
/// Implementation of throttle configuration that reads from IConfiguration.
/// </summary>
internal class ThrottleConfiguration : IThrottleConfiguration
{
    private readonly IConfiguration mConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThrottleConfiguration"/> class.
    /// </summary>
    /// <param name="configuration">The configuration instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when configuration is null.</exception>
    public ThrottleConfiguration(IConfiguration configuration)
    {
        mConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <inheritdoc/>
    public bool ThrottlingEnabled =>
        mConfiguration.GetValue<bool>("system:throttle:enabled", true);
}
