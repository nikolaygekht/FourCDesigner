using Microsoft.Extensions.DependencyInjection;

namespace Gehtsoft.FourCDesigner.Logic.Plan;

/// <summary>
/// Extension methods for registering plan-related services.
/// </summary>
public static class PlanServiceExtensions
{
    /// <summary>
    /// Registers all plan-related services including prompt factory, formatter, and AI controller.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPlanServices(this IServiceCollection services)
    {
        // Register prompt factory as singleton since prompts are static
        services.AddSingleton<IPromptFactory, PromptFactory>();

        // Register formatter as transient since it's stateless
        services.AddTransient<ILessonPlanFormatter, LessonPlanFormatter>();

        // Register AI controller as scoped (per-request lifecycle)
        services.AddScoped<IPlanAiController, PlanAiController>();

        return services;
    }
}
