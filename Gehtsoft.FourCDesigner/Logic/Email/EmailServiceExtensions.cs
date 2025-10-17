using Gehtsoft.FourCDesigner.Logic.Email.Configuration;
using Gehtsoft.FourCDesigner.Logic.Email.Queue;
using Gehtsoft.FourCDesigner.Logic.Email.Sender;
using Gehtsoft.FourCDesigner.Logic.Email.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Gehtsoft.FourCDesigner.Logic.Email;

/// <summary>
/// Extension methods for registering email services.
/// </summary>
public static class EmailServiceExtensions
{
    /// <summary>
    /// Registers all email-related services including configuration, storage, queue, sender, and background service.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddEmailServices(this IServiceCollection services)
    {
        // Register configuration as singleton
        services.AddSingleton<IEmailConfiguration, EmailConfiguration>();

        // Register storage as singleton (stateless file operations)
        services.AddSingleton<IEmailStorage, FileEmailStorage>();

        // Register queue as singleton (shared queue)
        services.AddSingleton<IEmailQueue, EmailQueue>();

        // Register sender state as singleton (shared state between services)
        services.AddSingleton<EmailSenderState>();

        // Register SMTP sender as scoped (per-operation connection)
        services.AddScoped<ISmtpSender, SmtpSender>();

        // Register email sender service as singleton (processes queue with mutex protection)
        services.AddSingleton<IEmailSenderService, EmailSenderService>();

        // Register email service as singleton (business logic, uses singleton queue)
        services.AddSingleton<IEmailService, EmailService>();

        // Register background service as hosted service
        services.AddHostedService<EmailBackgroundService>();

        return services;
    }
}
