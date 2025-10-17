using Microsoft.Extensions.DependencyInjection;
using Gehtsoft.FourCDesigner.Utils;
using Gehtsoft.FourCDesigner.Dao.Configuration;

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

        // Register hash provider configuration
        services.AddSingleton<IHashProviderConfiguration, HashProviderConfiguration>();

        // Register hash provider
        services.AddSingleton<IHashProvider, HashProvider>();

        // Register database initialization service
        services.AddSingleton<IDbInitializationService, DbInitializationService>();

        // Register database connection factory
        services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

        // Register data access objects as singleton (stateless, creates connections on the fly)
        services.AddSingleton<IUserDao, UserDao>();

        return services;
    }
}
