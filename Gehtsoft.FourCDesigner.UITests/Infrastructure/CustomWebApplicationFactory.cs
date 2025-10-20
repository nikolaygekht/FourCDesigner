using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Gehtsoft.FourCDesigner.UITests.Infrastructure;

/// <summary>
/// Custom WebApplicationFactory for UI tests using in-memory SQLite.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName;
    private IHost? _realHost;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomWebApplicationFactory"/> class.
    /// </summary>
    /// <param name="databaseName">The name of the in-memory database (for test isolation).</param>
    public CustomWebApplicationFactory(string databaseName = "TestDatabase")
    {
        _databaseName = databaseName;
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Create a test host for TestServer (required by WebApplicationFactory)
        var testHost = builder.Build();

        // Create a completely new host builder for the real Kestrel server
        var realHostBuilder = Program.CreateHostBuilder(Array.Empty<string>());

        // Apply the same configuration from ConfigureWebHost
        realHostBuilder.ConfigureWebHost(webHostBuilder =>
        {
            var connectionString = $"Data Source={_databaseName};Mode=Memory;Cache=Shared";

            // Set content root to the main application directory where wwwroot is located
            var contentRoot = Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "..",
                "Gehtsoft.FourCDesigner"));

            webHostBuilder.UseContentRoot(contentRoot);

            webHostBuilder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["email:disableSending"] = "true",
                    ["db:driver"] = "sqlite",
                    ["db:connectionString"] = connectionString,
                    ["db:createTestUser"] = "false"
                });
            });

            webHostBuilder.UseEnvironment("Testing");
            webHostBuilder.UseKestrel();
            webHostBuilder.UseUrls("http://127.0.0.1:5555");
        });

        _realHost = realHostBuilder.Build();
        _realHost.Start();

        // Return the test host (WebApplicationFactory expects this)
        return testHost;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Use named in-memory database for reproducibility
        var connectionString = $"Data Source={_databaseName};Mode=Memory;Cache=Shared";

        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Add test-specific configuration
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["email:disableSending"] = "true",
                ["db:driver"] = "sqlite",
                ["db:connectionString"] = connectionString,
                ["db:createTestUser"] = "false" // Don't create test user in UI tests
            });
        });

        builder.UseEnvironment("Testing");
    }

    /// <summary>
    /// Starts the server by triggering the creation of the host.
    /// </summary>
    public void EnsureServerStarted()
    {
        // Accessing Server property triggers CreateHost which starts our real Kestrel server
        _ = Server;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _realHost?.Dispose();
        }
        base.Dispose(disposing);
    }
}
