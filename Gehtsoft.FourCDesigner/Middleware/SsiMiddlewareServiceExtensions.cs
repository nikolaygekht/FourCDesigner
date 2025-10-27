using Microsoft.Extensions.DependencyInjection;

namespace Gehtsoft.FourCDesigner.Middleware;

/// <summary>
/// Extension methods for registering SSI middleware services.
/// </summary>
public static class SsiMiddlewareServiceExtensions
{
    /// <summary>
    /// Adds SSI middleware services to the dependency injection container.
    /// Registers configuration for SSI middleware.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
    public static IServiceCollection AddSsiMiddleware(this IServiceCollection services)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        // Register SSI middleware configuration as singleton (stateless, reads from IConfiguration)
        services.AddSingleton<ISsiMiddlewareConfig, SsiMiddlewareConfig>();

        return services;
    }
}
