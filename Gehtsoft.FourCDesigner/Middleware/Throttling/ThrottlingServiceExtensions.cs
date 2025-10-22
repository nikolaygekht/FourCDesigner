namespace Gehtsoft.FourCDesigner.Middleware.Throttling;

/// <summary>
/// Extension methods for registering throttling services.
/// Uses MemoryCache-based throttling with action filters.
/// </summary>
public static class ThrottlingServiceExtensions
{
    /// <summary>
    /// Adds throttling services to the dependency injection container.
    /// Registers configuration and cache for throttling.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
    public static IServiceCollection AddThrottling(this IServiceCollection services)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        // Register configuration as singleton
        services.AddSingleton<IThrottleConfiguration, ThrottleConfiguration>();

        // Register throttle cache as singleton
        services.AddSingleton<IThrottleCache, ThrottleCache>();

        return services;
    }
}
