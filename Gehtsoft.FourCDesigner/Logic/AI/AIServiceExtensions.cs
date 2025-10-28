using Gehtsoft.FourCDesigner.Logic.AI.Testing;
using Gehtsoft.FourCDesigner.Logic.AI.Ollama;
using Gehtsoft.FourCDesigner.Logic.AI.OpenAI;
using Microsoft.Extensions.DependencyInjection;

namespace Gehtsoft.FourCDesigner.Logic.AI;

/// <summary>
/// Extension methods for registering AI-related services.
/// </summary>
public static class AIServiceExtensions
{
    /// <summary>
    /// Registers all AI-related services including configurations, drivers, and factory.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAIServices(this IServiceCollection services)
    {
        // Register main AI configuration
        services.AddSingleton<IAIConfiguration, AIConfiguration>();

        // Register testing/mock driver configuration
        services.AddSingleton<IAITestingConfiguration, AITestingConfiguration>();

        // Register Ollama driver configuration
        services.AddSingleton<IAIOllamaConfiguration, AIOllamaConfiguration>();

        // Register OpenAI driver configuration
        services.AddSingleton<IAIOpenAIConfiguration, AIOpenAIConfiguration>();

        // Register the factory as singleton
        services.AddSingleton<AIDriverFactory>();

        // Register IAIDriver using the factory (scoped so each request gets same driver)
        services.AddScoped<IAIDriver>(serviceProvider =>
        {
            var factory = serviceProvider.GetRequiredService<AIDriverFactory>();
            return factory.CreateDriver();
        });

        return services;
    }
}
