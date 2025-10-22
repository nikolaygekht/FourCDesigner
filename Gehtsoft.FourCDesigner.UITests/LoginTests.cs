using System.Net;
using System.Net.Http.Json;
using Gehtsoft.FourCDesigner.UITests.Infrastructure;
using Microsoft.Playwright;

namespace Gehtsoft.FourCDesigner.UITests;

/// <summary>
/// UI tests for login functionality.
/// Tests run sequentially using a shared server instance to ensure state consistency.
/// </summary>
[Collection("UI Tests")]
public class LoginTests : IAsyncLifetime
{
    private readonly UiTestServerFixture _fixture;
    private Microsoft.Playwright.IBrowserContext _context = null!;
    private Microsoft.Playwright.IPage _page = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginTests"/> class.
    /// </summary>
    /// <param name="fixture">The shared server fixture.</param>
    public LoginTests(UiTestServerFixture fixture)
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
    /// Test unsuccessful login with local validation errors.
    /// Theory with three options:
    /// - Empty email / Filled password
    /// - Filled email / Empty password
    /// - Empty email / Empty password
    /// </summary>
    [Theory(Timeout = 10000)]
    [InlineData("", "Password1!")]
    [InlineData("test1@test.com", "")]
    [InlineData("", "")]
    public async Task UnsuccessfulLogin_LocalValidation_ShowsError(string email, string password)
    {
        // Navigate to login page
        await _page.GotoAsync("/login.html");
        await _page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

        // Fill in email if provided
        if (!string.IsNullOrEmpty(email))
        {
            await _page.FillAsync("#email", email);
        }
        else
        {
            await _page.FillAsync("#email", "");
        }
        // Fill in password if provided
        if (!string.IsNullOrEmpty(password))
        {
            await _page.FillAsync("#password", password);
        }
        else
        {
            await _page.FillAsync("#password", "");
        }
        // Click login button
        await _page.ClickAsync("#login-button");
        // Wait a bit for client-side validation
        await Task.Delay(500);
        // Verify error message is displayed
        var errorElement = _page.Locator("#error-message");
        await errorElement.WaitForAsync(new() { State = Microsoft.Playwright.WaitForSelectorState.Visible, Timeout = 1000 });
        var isVisible = await errorElement.IsVisibleAsync();
        isVisible.Should().BeTrue("error message should be displayed for invalid input");
        var errorText = await errorElement.TextContentAsync();
        errorText.Should().Contain("email", "error should mention missing email or password");
    }

    /// <summary>
    /// Test unsuccessful login with wrong user credentials.
    /// Theory with the following options:
    /// - test1@test.com / incorrect password (inactive user)
    /// - test1@test.com / correct password (inactive user - should fail)
    /// - test2@test.com / incorrect password (active user with wrong password)
    /// </summary>
    [Theory(Timeout = 10000)]
    [InlineData("test1@test.com", "WrongPassword!")]
    [InlineData("test1@test.com", "Password1!")]
    [InlineData("test2@test.com", "WrongPassword!")]
    public async Task UnsuccessfulLogin_WrongUser_ShowsError(string email, string password)
    {
        // Navigate to login page
        await _page.GotoAsync("/login.html");
        await _page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

        // Fill in email
        await _page.FillAsync("#email", email);

        // Fill in password
        await _page.FillAsync("#password", password);

        // Click login button
        await _page.ClickAsync("#login-button");

        // Wait for response
        await Task.Delay(500);

        // Verify error message is displayed
        var errorElement = _page.Locator("#error-message");
        await errorElement.WaitForAsync(new() { State = Microsoft.Playwright.WaitForSelectorState.Visible, Timeout = 1000 });

        var isVisible = await errorElement.IsVisibleAsync();
        
        isVisible.Should().BeTrue("error message should be displayed for wrong credentials");

                var errorText = await errorElement.TextContentAsync();
        errorText.Should().MatchRegex("(Invalid|password|credentials|Login failed)", "error should mention invalid credentials");

        // Verify we're still on login page
        _page.Url.Should().Contain("login.html", "should remain on login page after failed login");
    }

    /// <summary>
    /// Test successful login with correct credentials.
    /// </summary>
    [Fact(Timeout = 10000)]
    public async Task SuccessfulLogin_NavigatesToIndex()
    {
        // Navigate to login page
        await _page.GotoAsync("/login.html");
        await _page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

        // Fill in email
        await _page.FillAsync("#email", "test2@test.com");

        // Fill in password
        await _page.FillAsync("#password", "Password2!");

        // Click login button
        await _page.ClickAsync("#login-button");

        // Wait for navigation to index.html
        await _page.WaitForURLAsync("**/index.html", new() { Timeout = 1000 });

        // Verify navigation to index.html
        _page.Url.Should().Contain("index.html", "should navigate to index page after successful login");

        // Verify session is stored in localStorage
        var sessionId = await _page.EvaluateAsync<string>("localStorage.getItem('sessionId')");
        sessionId.Should().NotBeNullOrEmpty("session ID should be stored in localStorage");
    }

    /// <summary>
    /// Test navigation to registration page.
    /// </summary>
    [Fact(Timeout = 10000)]
    public async Task CanNavigateToRegistration()
    {
        // Navigate to login page
        await _page.GotoAsync("/login.html");
        await _page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

        // Click registration link
        await _page.ClickAsync("a[href='/register.html']");

        // Wait for navigation
        await _page.WaitForURLAsync("**/register.html", new() { Timeout = 1000 });

        // Verify navigation to register.html
        _page.Url.Should().Contain("register.html", "should navigate to registration page");
    }
}
