using System.Net;
using System.Net.Http.Json;
using Gehtsoft.FourCDesigner.UITests.Infrastructure;
using Microsoft.Playwright;

namespace Gehtsoft.FourCDesigner.UITests;

/// <summary>
/// UI tests for password reset functionality.
/// Tests run sequentially using a shared server instance to ensure state consistency.
/// </summary>
[Collection("UI Tests")]
public class ResetPasswordTests : IAsyncLifetime
{
    private readonly UiTestServerFixture _fixture;
    private Microsoft.Playwright.IBrowserContext _context = null!;
    private Microsoft.Playwright.IPage _page = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResetPasswordTests"/> class.
    /// </summary>
    /// <param name="fixture">The shared server fixture.</param>
    public ResetPasswordTests(UiTestServerFixture fixture)
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
        TheTrace.Trace("ca");
        await _page.CloseAsync();
        TheTrace.Trace("cb");
        await _context.CloseAsync();
        TheTrace.Trace("cc");
    }

    /// <summary>
    /// Test forgot password with empty email - should show error.
    /// </summary>
    [Fact(Timeout = 10000)]
    public async Task ForgotPassword_EmptyEmail_ShowsError()
    {
        // Navigate to login page
        TheTrace.Trace("ga");
        await _page.GotoAsync("/login.html");
        TheTrace.Trace("gb");
        await _page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

        // Make sure email is empty
        TheTrace.Trace("gc");
        await _page.FillAsync("#email", "");

        // Wait for button state to update
        TheTrace.Trace("gd");
        await Task.Delay(500);

        // Verify forgot password button is disabled
        TheTrace.Trace("ge");
        var isDisabled = await _page.IsDisabledAsync("#forgot-password-button");
        TheTrace.Trace("gf");
        isDisabled.Should().BeTrue("forgot password button should be disabled when email is empty");
        TheTrace.Trace("gg");
    }

    /// <summary>
    /// Test forgot password with non-existing email - should show success message (for security).
    /// </summary>
    [Fact(Timeout = 10000)]
    public async Task ForgotPassword_NonExistingEmail_ShowsSuccessMessage()
    {
        // Navigate to login page
        TheTrace.Trace("ha");
        await _page.GotoAsync("/login.html");
        TheTrace.Trace("hb");
        await _page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

        // Enter non-existing email
        TheTrace.Trace("hc");
        await _page.FillAsync("#email", "test_nonexistent@test.com");

        // Wait for button state to update
        TheTrace.Trace("hd");
        await Task.Delay(500);

        // Verify forgot password button is enabled
        TheTrace.Trace("he");
        var isDisabled = await _page.IsDisabledAsync("#forgot-password-button");
        TheTrace.Trace("hf");
        isDisabled.Should().BeFalse("forgot password button should be enabled with valid email format");

        // Click forgot password button
        TheTrace.Trace("hg");
        await _page.ClickAsync("#forgot-password-button");

        // Wait for response
        TheTrace.Trace("hh");
        await Task.Delay(500);

        // Verify success message is displayed (for security, always shows success)
        TheTrace.Trace("hi");
        var errorElement = _page.Locator("#error-message");
        TheTrace.Trace("hj");
        var isVisible = await errorElement.IsVisibleAsync();
        TheTrace.Trace("hk");
        isVisible.Should().BeTrue("message should be displayed");

        TheTrace.Trace("hl");
        var messageText = await errorElement.TextContentAsync();
        TheTrace.Trace("hm");
        messageText.Should().Contain("email", "message should mention email was sent");
        TheTrace.Trace("hn");
    }
}
