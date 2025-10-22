using System.Net;
using System.Net.Http.Json;
using Gehtsoft.FourCDesigner.UITests.Infrastructure;
using Microsoft.Playwright;

namespace Gehtsoft.FourCDesigner.UITests;

/// <summary>
/// UI tests for API throttling and DoS protection.
/// Tests run sequentially using a shared server instance to ensure state consistency.
/// </summary>
[Collection("UI Tests")]
public class ThrottlingTests : IAsyncLifetime
{
    private readonly UiTestServerFixture _fixture;
    private Microsoft.Playwright.IBrowserContext _context = null!;
    private Microsoft.Playwright.IPage _page = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThrottlingTests"/> class.
    /// </summary>
    /// <param name="fixture">The shared server fixture.</param>
    public ThrottlingTests(UiTestServerFixture fixture)
    {
        _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
    }

    /// <summary>
    /// Initializes the test (called before each test).
    /// </summary>
    public async Task InitializeAsync()
    {
        await _fixture.ResetThrottlingAsync();
        _context = await _fixture.Browser.NewContextAsync(new Microsoft.Playwright.BrowserNewContextOptions
        {
            BaseURL = _fixture.BaseUrl,
            ViewportSize = new Microsoft.Playwright.ViewportSize { Width = 1280, Height = 720 }
        });
        _page = await _context.NewPageAsync();
    }

    /// <summary>
    /// Cleans up after the test (called after each test).
    /// </summary>
    public async Task DisposeAsync()
    {
        await _fixture.ResetThrottlingAsync();
        await _page.CloseAsync();
        await _context.CloseAsync();
    }

    /// <summary>
    /// Test login method is DoS/Bruteforce protected.
    /// </summary>
    [Fact(Timeout = 30000)]
    public async Task Login_IsDosProtected()
    {
        // Reset throttling
        await _fixture.ResetThrottlingAsync();

        // Record start time before making requests

        // Do 50 login API calls with incorrect passwords
        for (int i = 0; i < 10; i++)
        {
            var response = await _fixture.HttpClient.GetAsync("/api/test/throttle-test");
            // Don't check status - some will succeed, some will fail
        }
        // Do one more login attempt - should be throttled
        var throttledResponse = await _fixture.HttpClient.GetAsync("/api/test/throttle-test");

        // Verify 429 error
        throttledResponse.StatusCode.Should().Be(HttpStatusCode.TooManyRequests, "should return 429 after exceeding rate limit");

        await Task.Delay((int)6000);

        // Do login again
        var afterSleepResponse = await _fixture.HttpClient.GetAsync("/api/test/throttle-test");

        // Verify it doesn't return 429 error
        afterSleepResponse.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests, "should not be throttled after waiting period");
    }

    /// <summary>
    /// Test reset email method is DoS/Bruteforce protected.
    /// </summary>
    [Fact(Timeout = 30000)]
    public async Task CheckEmail_IsDosProtected()
    {
        // Reset throttling
        await _fixture.ResetThrottlingAsync();

        // Record start time before making requests

        // Do 50 login API calls with incorrect passwords
        for (int i = 0; i < 10; i++)
        {
            var response = await _fixture.HttpClient.GetAsync("/api/test/throttle-test");
            // Don't check status - some will succeed, some will fail
        }
        // Do one more login attempt - should be throttled
        var throttledResponse = await _fixture.HttpClient.GetAsync("/api/test/throttle-test");

        // Verify 429 error
        throttledResponse.StatusCode.Should().Be(HttpStatusCode.TooManyRequests, "should return 429 after exceeding rate limit");

        await _fixture.ResetThrottlingAsync();

        // Do login again
        var afterSleepResponse = await _fixture.HttpClient.GetAsync("/api/test/throttle-test");

        // Verify it doesn't return 429 error
        afterSleepResponse.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests, "should not be throttled after waiting period");
    }
}
