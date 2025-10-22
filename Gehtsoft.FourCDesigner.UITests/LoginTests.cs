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
        TheTrace.Trace("ba");
        await _page.GotoAsync("/login.html");
        TheTrace.Trace("bb");
        await _page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

        TheTrace.Trace("bc");
        // Fill in email if provided
        if (!string.IsNullOrEmpty(email))
        {
            await _page.FillAsync("#email", email);
        }
        else
        {
            await _page.FillAsync("#email", "");
        }
        TheTrace.Trace("bd");
        // Fill in password if provided
        if (!string.IsNullOrEmpty(password))
        {
            await _page.FillAsync("#password", password);
        }
        else
        {
            await _page.FillAsync("#password", "");
        }
        TheTrace.Trace("be");
        // Click login button
        await _page.ClickAsync("#login-button");
        TheTrace.Trace("bf");
        // Wait a bit for client-side validation
        await Task.Delay(500);
        TheTrace.Trace("bg");
        // Verify error message is displayed
        var errorElement = _page.Locator("#error-message");
        TheTrace.Trace("bh");
        await errorElement.WaitForAsync(new() { State = Microsoft.Playwright.WaitForSelectorState.Visible, Timeout = 1000 });
        TheTrace.Trace("bi");
        var isVisible = await errorElement.IsVisibleAsync();
        TheTrace.Trace("bj");
        isVisible.Should().BeTrue("error message should be displayed for invalid input");
        var errorText = await errorElement.TextContentAsync();
        TheTrace.Trace("bk");
        errorText.Should().Contain("email", "error should mention missing email or password");
        TheTrace.Trace("bl");
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
        TheTrace.Trace("da");
        await _page.GotoAsync("/login.html");
        TheTrace.Trace("db");
        await _page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

        // Fill in email
        TheTrace.Trace("dc");
        await _page.FillAsync("#email", email);

        // Fill in password
        TheTrace.Trace("dd");
        await _page.FillAsync("#password", password);

        // Click login button
        TheTrace.Trace("de");
        await _page.ClickAsync("#login-button");

        // Wait for response
        TheTrace.Trace("df");
        await Task.Delay(500);

        // Verify error message is displayed
        TheTrace.Trace("dg");
        var errorElement = _page.Locator("#error-message");
        TheTrace.Trace("dh");
        await errorElement.WaitForAsync(new() { State = Microsoft.Playwright.WaitForSelectorState.Visible, Timeout = 1000 });

        TheTrace.Trace("di");
        var isVisible = await errorElement.IsVisibleAsync();
        
        TheTrace.Trace("dj");
        isVisible.Should().BeTrue("error message should be displayed for wrong credentials");

        TheTrace.Trace("dk");
                var errorText = await errorElement.TextContentAsync();
        TheTrace.Trace("dl");
        errorText.Should().MatchRegex("(Invalid|password|credentials|Login failed)", "error should mention invalid credentials");

        // Verify we're still on login page
        TheTrace.Trace("dm");
        _page.Url.Should().Contain("login.html", "should remain on login page after failed login");
        TheTrace.Trace("dn");
    }

    /// <summary>
    /// Test successful login with correct credentials.
    /// </summary>
    [Fact(Timeout = 10000)]
    public async Task SuccessfulLogin_NavigatesToIndex()
    {
        // Navigate to login page
        TheTrace.Trace("ea");
        await _page.GotoAsync("/login.html");
        TheTrace.Trace("eb");
        await _page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

        // Fill in email
        TheTrace.Trace("ec");
        await _page.FillAsync("#email", "test2@test.com");

        // Fill in password
        TheTrace.Trace("ed");
        await _page.FillAsync("#password", "Password2!");

        // Click login button
        TheTrace.Trace("ee");
        await _page.ClickAsync("#login-button");

        // Wait for navigation to index.html
        TheTrace.Trace("ef");
        await _page.WaitForURLAsync("**/index.html", new() { Timeout = 1000 });

        // Verify navigation to index.html
        TheTrace.Trace("eg");
        _page.Url.Should().Contain("index.html", "should navigate to index page after successful login");

        // Verify session is stored in localStorage
        TheTrace.Trace("eh");
        var sessionId = await _page.EvaluateAsync<string>("localStorage.getItem('sessionId')");
        TheTrace.Trace("ei");
        sessionId.Should().NotBeNullOrEmpty("session ID should be stored in localStorage");
        TheTrace.Trace("ej");
    }

    /// <summary>
    /// Test navigation to registration page.
    /// </summary>
    [Fact(Timeout = 10000)]
    public async Task CanNavigateToRegistration()
    {
        // Navigate to login page
        TheTrace.Trace("fa");
        await _page.GotoAsync("/login.html");
        TheTrace.Trace("fb");
        await _page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

        // Click registration link
        TheTrace.Trace("fc");
        await _page.ClickAsync("a[href='/register.html']");

        // Wait for navigation
        TheTrace.Trace("fd");
        await _page.WaitForURLAsync("**/register.html", new() { Timeout = 1000 });

        // Verify navigation to register.html
        TheTrace.Trace("fe");
        _page.Url.Should().Contain("register.html", "should navigate to registration page");
        TheTrace.Trace("ff");
    }
}
