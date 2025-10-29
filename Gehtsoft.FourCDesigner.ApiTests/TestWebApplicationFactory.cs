// Suppress nullability warnings for intentional null tests
#pragma warning disable CS8625, CS8602, CS8600, CS8620
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Gehtsoft.FourCDesigner.Dao;
using Gehtsoft.FourCDesigner.Logic.Email.Sender;

namespace Gehtsoft.FourCDesigner.ApiTests;

/// <summary>
/// Custom web application factory for API tests with in-memory SQLite database and mock SMTP sender.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string mDatabaseName;
    private readonly MockSmtpSender mMockSmtpSender;
    private readonly bool mEnableAITesting;

    /// <summary>
    /// Gets the mock SMTP sender that captures all emails sent during tests.
    /// Thread-safe and can be accessed from tests to retrieve sent emails.
    /// </summary>
    public MockSmtpSender MockSmtpSender => mMockSmtpSender;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestWebApplicationFactory"/> class.
    /// </summary>
    /// <param name="databaseName">The name of the in-memory database (must be unique per test).</param>
    /// <param name="enableAITesting">Whether to enable AI testing driver instead of real AI.</param>
    public TestWebApplicationFactory(string databaseName, bool enableAITesting = false)
    {
        mDatabaseName = databaseName ?? throw new ArgumentNullException(nameof(databaseName));
        mMockSmtpSender = new MockSmtpSender();
        mEnableAITesting = enableAITesting;
    }

    /// <summary>
    /// Configures the web host to use an in-memory SQLite database.
    /// </summary>
    /// <param name="builder">The web host builder.</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var configValues = new Dictionary<string, string>
            {
                ["db:driver"] = "sqlite",
                ["db:connectionString"] = $"Data Source={mDatabaseName};Mode=Memory;Cache=Shared",
                ["db:createTestUser"] = "true",
                ["db:testUserPassword"] = "test123",
                // Email configuration - ensure sending is enabled for tests
                ["email:disableSending"] = "false",
                ["email:sendingFrequencySeconds"] = "1",
                ["email:mailAddressFrom"] = "test@4cdesigner.com", // Required for sending emails
                ["email:delayBetweenMessagesSeconds"] = "0", // No delay between messages in tests
                // Configure test log file (no console output)
                ["Serilog:WriteTo:0:Name"] = "File",
                ["Serilog:WriteTo:0:Args:path"] = "./logs/api-test-log.txt",
                ["Serilog:WriteTo:0:Args:rollingInterval"] = "Day"
            };

            // If AI testing is enabled, configure the AI driver to use mock mode
            if (mEnableAITesting)
            {
                configValues["ai:driver"] = "mock";
                configValues["ai:mock:file"] = "./data/ai-mock-responses-test.json";
            }

            config.AddInMemoryCollection(configValues!);
        });

        builder.ConfigureLogging(logging =>
        {
            // Clear all logging providers (including console) to prevent writeToProviders from writing to console
            logging.ClearProviders();
        });

        builder.ConfigureTestServices(services =>
        {
            // Replace the real SMTP sender with our mock
            // ConfigureTestServices runs AFTER all other service configuration
            // Remove all existing ISmtpSender registrations and replace with our mock
            services.RemoveAll<ISmtpSender>();

            // Register as scoped (matching original pattern) but return same instance
            services.AddScoped<ISmtpSender>(_ => mMockSmtpSender);
        });
    }
}
