using Microsoft.Extensions.DependencyInjection;

namespace Gehtsoft.FourCDesigner.Logic.User;

/// <summary>
/// Extension methods for registering user-related services.
/// </summary>
public static class UserServiceExtensions
{
    /// <summary>
    /// Registers all user-related services including configuration, validators, and controllers.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddUserServices(this IServiceCollection services)
    {
        // Register user configuration
        services.AddSingleton<IUserConfiguration, UserConfiguration>();

        // Register password validator
        services.AddScoped<IPasswordValidator, PasswordValidator>();

        // Register user controller
        services.AddScoped<IUserController, UserController>();

        return services;
    }
}
