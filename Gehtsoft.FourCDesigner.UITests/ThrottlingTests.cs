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
        TheTrace.Enable = true;
        TheTrace.Timing = true;
        _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
    }

    /// <summary>
    /// Initializes the test (called before each test).
    /// </summary>
    public async Task InitializeAsync()
    {
        TheTrace.Trace("aa");
        // Reset database before each test
        await _fixture.ResetDatabaseAsync();

        // Reset throttling before each test to prevent accumulation
        TheTrace.Trace("ab");
        await _fixture.ResetThrottlingAsync();

        // Seed test users
        TheTrace.Trace("ac");
        await _fixture.AddUserAsync("test1@test.com", "Password1!", activate: false);
        TheTrace.Trace("ad");
        await _fixture.AddUserAsync("test2@test.com", "Password2!", activate: true);

        // Create new browser context for this test (isolates cookies/storage)
        TheTrace.Trace("ae");
        _context = await _fixture.Browser.NewContextAsync(new Microsoft.Playwright.BrowserNewContextOptions
        {
            BaseURL = _fixture.BaseUrl,
            ViewportSize = new Microsoft.Playwright.ViewportSize { Width = 1280, Height = 720 }
        });

        TheTrace.Trace("af");
        // Create page
        _page = await _context.NewPageAsync();
        TheTrace.Trace("ag");
    }

    /// <summary>
    /// Cleans up after the test (called after each test).
    /// </summary>
    public async Task DisposeAsync()
    {
        await _fixture.ResetThrottlingAsync();
        
        TheTrace.Trace("ca");
        await _page.CloseAsync();
        TheTrace.Trace("cb");
        await _context.CloseAsync();
        TheTrace.Trace("cc");
    }

    /// <summary>
    /// Test login method is DoS/Bruteforce protected.
    /// </summary>
    [Fact(Timeout = 30000)]
    public async Task Login_IsDosProtected()
    {
        // Reset throttling
        TheTrace.Trace("ia");
        await _fixture.ResetThrottlingAsync();

        // Record start time before making requests
        TheTrace.Trace("ib");
        var startTime = DateTime.UtcNow;

        // Do 50 login API calls with incorrect passwords
        TheTrace.Trace("ic");
        for (int i = 0; i < 50; i++)
        {
            var request = new { Email = "test2@test.com", Password = "WrongPassword!" };
            var response = await _fixture.HttpClient.PostAsJsonAsync("/api/user/login", request);
            // Don't check status - some will succeed, some will fail
        }

        // Do one more login attempt - should be throttled
        TheTrace.Trace("id");
        var throttledRequest = new { Email = "test2@test.com", Password = "WrongPassword!" };
        TheTrace.Trace("ie");
        var throttledResponse = await _fixture.HttpClient.PostAsJsonAsync("/api/user/login", throttledRequest);

        // Verify 429 error
        TheTrace.Trace("if");
        throttledResponse.StatusCode.Should().Be(HttpStatusCode.TooManyRequests, "should return 429 after exceeding rate limit");

        // Calculate how long to wait - we need to wait until at least 11 seconds have passed since startTime
        // (window is 10 seconds, we add 1 second margin)
        TheTrace.Trace("ig");
        var elapsed = (DateTime.UtcNow - startTime).TotalMilliseconds;
        TheTrace.Trace("ih");
        var remainingWait = Math.Max(0, 11000 - elapsed);

        TheTrace.Trace("ii");
        if (remainingWait > 0)
        {
            await Task.Delay((int)remainingWait);
        }

        // Do login again
        TheTrace.Trace("ij");
        var afterSleepRequest = new { Email = "test2@test.com", Password = "Password2!" };
        TheTrace.Trace("ik");
        var afterSleepResponse = await _fixture.HttpClient.PostAsJsonAsync("/api/user/login", afterSleepRequest);

        // Verify it doesn't return 429 error
        TheTrace.Trace("il");
        afterSleepResponse.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests, "should not be throttled after waiting period");
        TheTrace.Trace("im");
    }

    /// <summary>
    /// Test reset email method is DoS/Bruteforce protected.
    /// </summary>
    [Fact(Timeout = 30000)]
    public async Task CheckEmail_IsDosProtected()
    {
        // Reset throttling
        TheTrace.Trace("ja");
        await _fixture.ResetThrottlingAsync();

        // Do 10 check email calls
        TheTrace.Trace("jb");
        for (int i = 0; i < 10; i++)
        {
            var response = await _fixture.HttpClient.GetAsync("/api/user/check-email?email=test@test.com");
            // Don't check status
        }

        // Do one more validation attempt - should be throttled
        TheTrace.Trace("jc");
        var throttledResponse = await _fixture.HttpClient.GetAsync("/api/user/check-email?email=test@test.com");

        // Verify 429 error
        TheTrace.Trace("jd");
        throttledResponse.StatusCode.Should().Be(HttpStatusCode.TooManyRequests, "should return 429 after exceeding email check rate limit");

        // Reset throttling again
        TheTrace.Trace("je");
        await _fixture.ResetThrottlingAsync();

        // Do a validation attempt
        TheTrace.Trace("jf");
        var afterResetResponse = await _fixture.HttpClient.GetAsync("/api/user/check-email?email=test@test.com");

        // Verify it doesn't return 429 error
        TheTrace.Trace("jg");
        afterResetResponse.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests, "should not be throttled after reset");
        TheTrace.Trace("jh");
    }
}
