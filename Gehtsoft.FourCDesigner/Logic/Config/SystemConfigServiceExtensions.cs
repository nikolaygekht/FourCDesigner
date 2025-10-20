using Microsoft.Extensions.DependencyInjection;

namespace Gehtsoft.FourCDesigner.Logic.Config;

/// <summary>
/// Extension methods for registering system configuration services.
/// </summary>
public static class SystemConfigServiceExtensions
{
    /// <summary>
    /// Registers system configuration services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSystemConfig(this IServiceCollection services)
    {
        // Register system configuration as singleton (stateless, reads from IConfiguration)
        services.AddSingleton<ISystemConfig, SystemConfig>();

        // Register URL builder as singleton (stateless, uses ISystemConfig)
        services.AddSingleton<IUrlBuilder, UrlBuilder>();

        return services;
    }
}
