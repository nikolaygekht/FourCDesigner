using Microsoft.Extensions.DependencyInjection;

namespace Gehtsoft.FourCDesigner.Logic.Token;

/// <summary>
/// Extension methods for registering token-related services.
/// </summary>
public static class TokenServiceExtensions
{
    /// <summary>
    /// Registers all token-related services including configuration and token service.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddTokenServices(this IServiceCollection services)
    {
        // Register token configuration
        services.AddSingleton<ITokenServiceConfiguration, TokenServiceConfiguration>();

        // Register token service as singleton to share MemoryCache across all requests
        services.AddSingleton<ITokenService, TokenService>();

        return services;
    }
}
