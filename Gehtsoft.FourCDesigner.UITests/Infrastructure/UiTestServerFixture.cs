using System.Data.Common;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace Gehtsoft.FourCDesigner.UITests.Infrastructure;

/// <summary>
/// Shared server fixture for all UI tests.
/// Ensures a single server instance is used across all tests.
/// </summary>
public class UiTestServerFixture : IAsyncLifetime
{
    private IHost? _host;
    private HttpClient? _httpClient;
    private DbConnection? _keepAliveConnection;
    private Microsoft.Playwright.IPlaywright? _playwright;
    private Microsoft.Playwright.IBrowser? _browser;
    private const string DatabaseName = "SharedAuthTestDatabase";

    /// <summary>
    /// Gets the HTTP client for API calls.
    /// </summary>
    public HttpClient HttpClient => _httpClient ?? throw new InvalidOperationException("Fixture not initialized");

    /// <summary>
    /// Gets the base URL of the test server.
    /// </summary>
    public string BaseUrl => "http://127.0.0.1:5555";

    /// <summary>
    /// Gets the shared Playwright instance.
    /// </summary>
    public Microsoft.Playwright.IPlaywright Playwright => _playwright ?? throw new InvalidOperationException("Fixture not initialized");

    /// <summary>
    /// Gets the shared browser instance.
    /// </summary>
    public Microsoft.Playwright.IBrowser Browser => _browser ?? throw new InvalidOperationException("Fixture not initialized");

    /// <summary>
    /// Initializes the shared test server (called once before all tests).
    /// </summary>
    public async Task InitializeAsync()
    {
        var connectionString = $"Data Source={DatabaseName};Mode=Memory;Cache=Shared";

        // Build test configuration FIRST (before anything else)
        var testConfiguration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["email:disableSending"] = "true",
                ["email:mailAddressFrom"] = "test@test.com",
                ["db:driver"] = "sqlite",
                ["db:connectionString"] = connectionString,
                ["db:createTestUser"] = "false",
                // Throttle configuration to match production
                ["application:throttle:enabled"] = "true",
                ["application:throttle:defaultRequestsPerPeriod"] = "20",
                ["application:throttle:checkEmailRequestsPerPeriod"] = "10",
                ["application:throttle:periodInSeconds"] = "10",
                // Serilog configuration - write to file only, not console
                ["Serilog:MinimumLevel:Default"] = "Debug",
                ["Serilog:MinimumLevel:Override:Microsoft"] = "Warning",
                ["Serilog:MinimumLevel:Override:System"] = "Warning",
                ["Serilog:MinimumLevel:Override:Microsoft.AspNetCore"] = "Warning",
                ["Serilog:WriteTo:0:Name"] = "File",
                ["Serilog:WriteTo:0:Args:path"] = "./logs/test-log.txt",
                ["Serilog:WriteTo:0:Args:rollingInterval"] = "Day"
            })
            .Build();

        // Configure Serilog from test configuration BEFORE creating host (like Program.Main does)
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(testConfiguration)
            .Enrich.FromLogContext()
            .CreateLogger();

        // Open a keep-alive connection to maintain the in-memory database
        // This connection must stay open for the lifetime of all tests
        _keepAliveConnection = new SqliteConnection(connectionString);
        await _keepAliveConnection.OpenAsync();

        // Create host builder directly
        var hostBuilder = Program.CreateHostBuilder(Array.Empty<string>());

        // Configure the host for testing
        hostBuilder.ConfigureLogging(logging =>
        {
            // Clear all default logging providers (including console)
            logging.ClearProviders();
        });

        hostBuilder.ConfigureWebHost(webHostBuilder =>
        {
            // Set content root to the main application directory where wwwroot is located
            var contentRoot = Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "..",
                "Gehtsoft.FourCDesigner"));

            webHostBuilder.UseContentRoot(contentRoot);

            webHostBuilder.ConfigureAppConfiguration((context, config) =>
            {
                // Clear existing configuration sources and use our test configuration
                config.Sources.Clear();
                config.AddConfiguration(testConfiguration);
            });

            webHostBuilder.UseEnvironment("Testing");
            webHostBuilder.UseKestrel();
            webHostBuilder.UseUrls(BaseUrl);
        });

        // Build and start the host
        _host = hostBuilder.Build();
        await _host.StartAsync();

        // Create HttpClient that points to the server
        _httpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };

        // Wait for server to fully start
        await Task.Delay(1000);

        // Initialize database schema
        await ResetDatabaseAsync();

        // Initialize Playwright and browser (once for all tests)
        _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new Microsoft.Playwright.BrowserTypeLaunchOptions
        {
            Headless = true
        });
    }

    /// <summary>
    /// Resets the database by dropping and recreating all tables.
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        var response = await HttpClient.PostAsync("/api/test/db/reset", null);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Adds a test user to the database.
    /// </summary>
    /// <param name="email">User email.</param>
    /// <param name="password">User password.</param>
    /// <param name="activate">Whether to activate the user immediately.</param>
    public async Task AddUserAsync(string email, string password, bool activate = false)
    {
        var request = new { Email = email, Password = password, Activate = activate };
        var response = await HttpClient.PostAsJsonAsync("/api/test/db/add-user", request);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Resets throttling state via test API.
    /// </summary>
    public async Task ResetThrottlingAsync()
    {
        var response = await HttpClient.PostAsync("/api/test/reset-throttling", null);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Gets the current email queue size.
    /// </summary>
    /// <returns>The number of emails in the queue.</returns>
    public async Task<int> GetEmailQueueSizeAsync()
    {
        var response = await HttpClient.GetAsync("/api/test/email/queue-size");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<QueueSizeResponse>();
        return result?.QueueSize ?? 0;
    }

    /// <summary>
    /// Dequeues the next email from the queue.
    /// </summary>
    /// <returns>The dequeued email or null if queue is empty.</returns>
    public async Task<DequeueEmailResponse?> DequeueEmailAsync()
    {
        var response = await HttpClient.GetAsync("/api/test/email/dequeue");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DequeueEmailResponse>();
    }

    /// <summary>
    /// Cleans up after all tests (called once after all tests).
    /// </summary>
    public async Task DisposeAsync()
    {
        // Dispose Playwright browser and instance
        if (_browser != null)
        {
            await _browser.CloseAsync();
            await _browser.DisposeAsync();
        }
        _playwright?.Dispose();

        _httpClient?.Dispose();
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
        // Close the keep-alive connection last to ensure database persists until cleanup
        if (_keepAliveConnection != null)
        {
            await _keepAliveConnection.CloseAsync();
            await _keepAliveConnection.DisposeAsync();
        }

        // Close and flush Serilog
        Log.CloseAndFlush();
    }
}

/// <summary>
/// Response model for queue size endpoint.
/// </summary>
public class QueueSizeResponse
{
    /// <summary>
    /// Gets or sets the current queue size.
    /// </summary>
    public int QueueSize { get; set; }

    /// <summary>
    /// Gets or sets whether the email sender is active.
    /// </summary>
    public bool IsSenderActive { get; set; }
}

/// <summary>
/// Response model for dequeued email.
/// </summary>
public class DequeueEmailResponse
{
    /// <summary>
    /// Gets or sets the message ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the recipient email addresses.
    /// </summary>
    public string[] To { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the email subject.
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email body.
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the body is HTML.
    /// </summary>
    public bool IsHtml { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is a priority message.
    /// </summary>
    public bool Priority { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the retry count.
    /// </summary>
    public int RetryCount { get; set; }
}
