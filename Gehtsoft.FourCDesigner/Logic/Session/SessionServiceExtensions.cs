using Microsoft.Extensions.DependencyInjection;

namespace Gehtsoft.FourCDesigner.Logic.Session;

/// <summary>
/// Extension methods for registering session-related services.
/// </summary>
public static class SessionServiceExtensions
{
    /// <summary>
    /// Registers all session-related services including configuration and controllers.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSessionServices(this IServiceCollection services)
    {
        // Register session configuration
        services.AddSingleton<ISessionSettings, SessionSettings>();

        // Register session controller as singleton to share MemoryCache across all requests
        services.AddSingleton<ISessionController, SessionController>();

        return services;
    }
}
