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
        await _page.CloseAsync();
        await _context.CloseAsync();
    }

    /// <summary>
    /// Test forgot password with empty email - should show error.
    /// </summary>
    [Fact(Timeout = 10000)]
    public async Task ForgotPassword_EmptyEmail_ShowsError()
    {
        // Navigate to login page
        await _page.GotoAsync("/login.html");
        await _page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

        // Make sure email is empty
        await _page.FillAsync("#email", "");

        // Wait for button state to update
        await Task.Delay(500);

        // Verify forgot password button is disabled
        var isDisabled = await _page.IsDisabledAsync("#forgot-password-button");
        isDisabled.Should().BeTrue("forgot password button should be disabled when email is empty");
    }

    /// <summary>
    /// Test forgot password with non-existing email - should show success message (for security).
    /// </summary>
    [Fact(Timeout = 10000)]
    public async Task ForgotPassword_NonExistingEmail_ShowsSuccessMessage()
    {
        // Navigate to login page
        await _page.GotoAsync("/login.html");
        await _page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

        // Enter non-existing email
        await _page.FillAsync("#email", "test_nonexistent@test.com");

        // Wait for button state to update
        await Task.Delay(500);

        // Verify forgot password button is enabled
        var isDisabled = await _page.IsDisabledAsync("#forgot-password-button");
        isDisabled.Should().BeFalse("forgot password button should be enabled with valid email format");

        // Click forgot password button
        await _page.ClickAsync("#forgot-password-button");

        // Wait for response
        await Task.Delay(500);

        // Verify success message is displayed (for security, always shows success)
        var errorElement = _page.Locator("#error-message");
        var isVisible = await errorElement.IsVisibleAsync();
        isVisible.Should().BeTrue("message should be displayed");

        var messageText = await errorElement.TextContentAsync();
        messageText.Should().Contain("email", "message should mention email was sent");
    }
}
