using Microsoft.Extensions.DependencyInjection;

namespace Gehtsoft.FourCDesigner.Utils;

/// <summary>
/// Extension methods for registering message services.
/// </summary>
public static class MessagesServiceExtensions
{
    /// <summary>
    /// Registers message services with the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMessages(this IServiceCollection services)
    {
        // Register factory as singleton
        services.AddSingleton<IMessagesFactory, MessagesFactory>();

        // Register default IMessages as scoped (can be overridden per request based on user preferences)
        services.AddScoped<IMessages>(provider =>
        {
            var factory = provider.GetRequiredService<IMessagesFactory>();
            return factory.GetDefaultMessages();
        });

        return services;
    }
}
