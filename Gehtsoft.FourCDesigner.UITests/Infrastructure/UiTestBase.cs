using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Playwright;

namespace Gehtsoft.FourCDesigner.UITests.Infrastructure;

/// <summary>
/// Base class for UI tests with Playwright support.
/// </summary>
public abstract class UiTestBase : IAsyncLifetime
{
    protected CustomWebApplicationFactory Factory { get; private set; } = null!;
    protected HttpClient HttpClient { get; private set; } = null!;
    protected string BaseUrl { get; private set; } = null!;

    protected IPlaywright Playwright { get; private set; } = null!;
    protected IBrowser Browser { get; private set; } = null!;
    protected IBrowserContext Context { get; private set; } = null!;
    protected IPage Page { get; private set; } = null!;

    /// <summary>
    /// Gets the name for the in-memory database (override for test isolation).
    /// </summary>
    protected virtual string DatabaseName => GetType().Name;

    /// <summary>
    /// Initializes the test (called before each test).
    /// </summary>
    public virtual async Task InitializeAsync()
    {
        // Create and configure the factory
        Factory = new CustomWebApplicationFactory(DatabaseName);

        // Start the server
        Factory.EnsureServerStarted();

        // Use the fixed test URL (configured in CustomWebApplicationFactory)
        BaseUrl = "http://127.0.0.1:5555";

        // Create HttpClient that points to the actual server
        HttpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };

        // Wait a bit for server to fully start
        await Task.Delay(500);

        // Initialize database schema via API
        await ResetDatabaseAsync();

        // Seed database if needed
        await SeedDatabaseAsync();

        // Initialize Playwright
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();

        // Launch browser in headless mode
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });

        // Create browser context
        Context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = BaseUrl,
            ViewportSize = new ViewportSize { Width = 1280, Height = 720 }
        });

        // Create page
        Page = await Context.NewPageAsync();
    }

    /// <summary>
    /// Override this method to seed test data.
    /// </summary>
    protected virtual Task SeedDatabaseAsync()
    {
        // Override in derived classes to add test data
        return Task.CompletedTask;
    }

    /// <summary>
    /// Resets the database by dropping and recreating all tables.
    /// </summary>
    protected async Task ResetDatabaseAsync()
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
    protected async Task AddUserAsync(string email, string password, bool activate = false)
    {
        var request = new { Email = email, Password = password, Activate = activate };
        var response = await HttpClient.PostAsJsonAsync("/api/test/db/add-user", request);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Cleans up after the test (called after each test).
    /// </summary>
    public virtual async Task DisposeAsync()
    {
        await Page.CloseAsync();
        await Context.CloseAsync();
        await Browser.CloseAsync();
        Playwright.Dispose();

        HttpClient.Dispose();
        await Factory.DisposeAsync();
    }

    /// <summary>
    /// Dequeues an email from the test email queue.
    /// </summary>
    /// <returns>The dequeued email message or null if queue is empty.</returns>
    protected async Task<DequeuedEmail?> DequeueEmailAsync()
    {
        var response = await HttpClient.GetAsync("/api/test/email/dequeue");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        var email = await response.Content.ReadFromJsonAsync<DequeuedEmail>();
        return email;
    }

    /// <summary>
    /// Waits for an email to appear in the queue and dequeues it.
    /// </summary>
    /// <param name="timeoutSeconds">Maximum time to wait in seconds.</param>
    /// <returns>The dequeued email message.</returns>
    protected async Task<DequeuedEmail> WaitForEmailAsync(int timeoutSeconds = 5)
    {
        var endTime = DateTime.UtcNow.AddSeconds(timeoutSeconds);

        while (DateTime.UtcNow < endTime)
        {
            var email = await DequeueEmailAsync();
            if (email != null)
                return email;

            await Task.Delay(100); // Wait 100ms before retry
        }

        throw new TimeoutException($"No email received within {timeoutSeconds} seconds");
    }

    /// <summary>
    /// Gets the current email queue size.
    /// </summary>
    /// <returns>Queue size information.</returns>
    protected async Task<QueueSize> GetQueueSizeAsync()
    {
        var response = await HttpClient.GetAsync("/api/test/email/queue-size");
        response.EnsureSuccessStatusCode();

        var queueSize = await response.Content.ReadFromJsonAsync<QueueSize>();
        return queueSize ?? throw new InvalidOperationException("Failed to get queue size");
    }

    /// <summary>
    /// Asserts that the email queue is empty.
    /// </summary>
    protected async Task AssertNoEmailsInQueueAsync()
    {
        var queueSize = await GetQueueSizeAsync();
        queueSize.QueueSizeValue.Should().Be(0, "no emails should be in queue");
    }
}

/// <summary>
/// Represents a dequeued email message.
/// </summary>
public class DequeuedEmail
{
    public string Id { get; set; } = string.Empty;
    public string[] To { get; set; } = Array.Empty<string>();
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; }
    public bool Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public int RetryCount { get; set; }
}

/// <summary>
/// Represents email queue size information.
/// </summary>
public class QueueSize
{
    public int QueueSizeValue { get; set; }
    public bool IsSenderActive { get; set; }
}
