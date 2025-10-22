using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Gehtsoft.FourCDesigner.Dao;

namespace Gehtsoft.FourCDesigner.ApiTests;

/// <summary>
/// Custom web application factory for API tests with in-memory SQLite database.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string mDatabaseName;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestWebApplicationFactory"/> class.
    /// </summary>
    /// <param name="databaseName">The name of the in-memory database (must be unique per test).</param>
    public TestWebApplicationFactory(string databaseName)
    {
        mDatabaseName = databaseName ?? throw new ArgumentNullException(nameof(databaseName));
    }

    /// <summary>
    /// Configures the web host to use an in-memory SQLite database.
    /// </summary>
    /// <param name="builder">The web host builder.</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Override database configuration with in-memory SQLite
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["db:driver"] = "sqlite",
                ["db:connectionString"] = $"Data Source={mDatabaseName};Mode=Memory;Cache=Shared",
                ["db:createTestUser"] = "true",
                // Configure test log file (no console output)
                ["Serilog:WriteTo:0:Name"] = "File",
                ["Serilog:WriteTo:0:Args:path"] = "./logs/api-test-log.txt",
                ["Serilog:WriteTo:0:Args:rollingInterval"] = "Day"
            });
        });

        builder.ConfigureLogging(logging =>
        {
            // Clear all logging providers (including console) to prevent writeToProviders from writing to console
            logging.ClearProviders();
        });

        builder.ConfigureServices(services =>
        {
            // Additional test-specific service configuration can go here
        });
    }
}
