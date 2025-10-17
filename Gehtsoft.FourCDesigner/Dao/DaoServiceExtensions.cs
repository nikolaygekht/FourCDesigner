using Microsoft.Extensions.DependencyInjection;
using Gehtsoft.FourCDesigner.Utils;

namespace Gehtsoft.FourCDesigner.Dao;

/// <summary>
/// Extension methods for registering DAO services.
/// </summary>
public static class DaoServiceExtensions
{
    /// <summary>
    /// Registers all DAO-related services including database configuration, connection factory, and data access objects.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDaoServices(this IServiceCollection services)
    {
        // Register database configuration
        services.AddSingleton<IDbConfiguration, DbConfiguration>();

        // Register database connection factory
        services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

        // Register hash provider
        services.AddSingleton<IHashProvider, HashProvider>();

        // Register data access objects
        services.AddScoped<IUserDao, UserDao>();

        return services;
    }
}
