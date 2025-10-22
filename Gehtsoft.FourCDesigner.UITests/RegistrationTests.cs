using System.Net;
using System.Net.Http.Json;
using Gehtsoft.FourCDesigner.UITests.Infrastructure;
using Microsoft.Playwright;

namespace Gehtsoft.FourCDesigner.UITests;

/// <summary>
/// UI tests for registration functionality.
/// Tests run sequentially using a shared server instance to ensure state consistency.
/// </summary>
[Collection("UI Tests")]
public class RegistrationTests : IAsyncLifetime
{
    private readonly UiTestServerFixture _fixture;
    private Microsoft.Playwright.IBrowserContext _context = null!;
    private Microsoft.Playwright.IPage _page = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegistrationTests"/> class.
    /// </summary>
    /// <param name="fixture">The shared server fixture.</param>
    public RegistrationTests(UiTestServerFixture fixture)
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
    /// Test registration with validation errors.
    /// Theory with the following options:
    /// - Empty Email/Correct Password
    /// - Correct Email/Empty Password
    /// - Correct Email/Password w/o numbers
    /// - Correct Email/Password w/o capital letters
    /// - Correct Email/Password w/o small letters
    /// - Correct Email/Password w/o special characters
    /// - Incorrect Email/Correct Password
    /// - Existing Email (test1@test.com)/Correct Password
    /// </summary>
    [Theory(Timeout = 10000)]
    [InlineData("", "Password1!", "Empty email")]
    [InlineData("test3@test.com", "", "Empty password")]
    [InlineData("test3@test.com", "PasswordABC!", "Password without numbers")]
    [InlineData("test3@test.com", "password123!", "Password without capital letters")]
    [InlineData("test3@test.com", "PASSWORD123!", "Password without small letters")]
    [InlineData("test3@test.com", "Password123", "Password without special characters")]
    [InlineData("invalid-email", "Password1!", "Invalid email")]
    [InlineData("test1@test.com", "Password1!", "Existing email")]
    public async Task Registration_ValidationErrors_ShowsError(string email, string password, string testCase)
    {
        // Navigate to register page
        TheTrace.Trace("ka");
        await _page.GotoAsync("/register.html");
        TheTrace.Trace("kb");
        await _page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

        // Wait for page initialization (password rules loaded AND event listeners attached)
        TheTrace.Trace("kc");
        await _page.WaitForFunctionAsync(@"
            document.getElementById('password-rules')?.textContent?.length > 0 &&
            typeof window.registerFormInitialized !== 'undefined'
        ");

        // Fill in email
        TheTrace.Trace("kd");
        await _page.FillAsync("#email", email);

        // Fill in password
        TheTrace.Trace("ke");
        await _page.FillAsync("#password", password);

        // Submit form directly (bypasses button enabled/disabled state)
        TheTrace.Trace("kf");
        await _page.EvaluateAsync("document.getElementById('register-form').requestSubmit()");

        // Wait for response
        TheTrace.Trace("kg");
        await Task.Delay(500);

        // Verify error message is displayed (register.html uses #message, not #error-message)
        TheTrace.Trace("kh");
        var errorElement = _page.Locator("#message");
        TheTrace.Trace("ki");
        await errorElement.WaitForAsync(new() { State = Microsoft.Playwright.WaitForSelectorState.Visible, Timeout = 1000 });

        TheTrace.Trace("kj");
        var isVisible = await errorElement.IsVisibleAsync();
        TheTrace.Trace("kk");
        isVisible.Should().BeTrue($"error message should be displayed for: {testCase}");

        // Verify we're still on register page
        TheTrace.Trace("kl");
        _page.Url.Should().Contain("register.html", "should remain on register page after failed registration");
        TheTrace.Trace("km");
    }

    /// <summary>
    /// Test successful registration flow.
    /// </summary>
    [Fact(Timeout = 10000)]
    public async Task Registration_Successful_ShowsConfirmationAndActivatesAccount()
    {
        // Ensure email queue is empty
        TheTrace.Trace("la");
        var initialQueueSize = await _fixture.GetEmailQueueSizeAsync();
        TheTrace.Trace("lb");
        for (int i = 0; i < initialQueueSize; i++)
            await _fixture.DequeueEmailAsync();

        // Navigate to register page
        TheTrace.Trace("lc");
        await _page.GotoAsync("/register.html");
        TheTrace.Trace("ld");
        await _page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

        // Wait for page initialization (password rules loaded)
        TheTrace.Trace("le");
        await _page.WaitForFunctionAsync("document.getElementById('password-rules')?.textContent?.length > 0");

        // Fill in email
        TheTrace.Trace("lf");
        await _page.FillAsync("#email", "test3@test.com");

        // Fill in password
        TheTrace.Trace("lg");
        await _page.FillAsync("#password", "Password3!");

        // Click register button
        TheTrace.Trace("lh");
        await _page.ClickAsync("#register-button");

        // Wait for response
        TheTrace.Trace("li");
        await Task.Delay(1000);

        // Verify confirmation message appears
        TheTrace.Trace("lj");
        var confirmationElement = _page.Locator("#message");
        TheTrace.Trace("lk");
        await confirmationElement.WaitForAsync(new() { State = Microsoft.Playwright.WaitForSelectorState.Visible, Timeout = 1000 });

        TheTrace.Trace("ll");
        var confirmationText = await confirmationElement.TextContentAsync();
        TheTrace.Trace("lm");
        confirmationText.Should().MatchRegex("(confirm|activation|email)", "should show confirmation message");

        // Verify email appears in queue
        TheTrace.Trace("ln");
        var queueSize = await _fixture.GetEmailQueueSizeAsync();
        TheTrace.Trace("lo");
        queueSize.Should().Be(1, "exactly one email should be in the queue");

        // Dequeue email
        TheTrace.Trace("lp");
        var email = await _fixture.DequeueEmailAsync();
        TheTrace.Trace("lq");
        email.Should().NotBeNull("email should be dequeued");
        TheTrace.Trace("lr");
        email!.To.Should().Contain("test3@test.com");

        // Extract activation link from email body
        TheTrace.Trace("ls");
        var emailBody = email.Body;
        TheTrace.Trace("lt");
        var linkMatch = System.Text.RegularExpressions.Regex.Match(emailBody, @"(http[s]?://[^\s]+)");
        TheTrace.Trace("lu");
        linkMatch.Success.Should().BeTrue("email should contain activation link");

        TheTrace.Trace("lv");
        var activationUrl = linkMatch.Value;

        // Verify link contains email and token
        TheTrace.Trace("lw");
        activationUrl.Should().Contain("test3@test.com", "link should contain email");

        TheTrace.Trace("lx");
        var tokenMatch = System.Text.RegularExpressions.Regex.Match(activationUrl, @"token=(\d{6})");
        TheTrace.Trace("ly");
        tokenMatch.Success.Should().BeTrue("link should contain 6-digit token");

        TheTrace.Trace("lz");
        var token = tokenMatch.Groups[1].Value;
        TheTrace.Trace("lA");
        token.Should().MatchRegex(@"^\d{6}$", "token should be 6 digits");

        // Open the activation link
        TheTrace.Trace("lA");
        await _page.GotoAsync(activationUrl);
        TheTrace.Trace("lB");
        await _page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

        // Verify navigation to login page
        TheTrace.Trace("lC");
        _page.Url.Should().Contain("login.html", "should navigate to login page after activation");

        // Verify success message on login page
        TheTrace.Trace("lD");
        var successElement = _page.Locator("#error-message");
        TheTrace.Trace("lE");
        var successVisible = await successElement.IsVisibleAsync();
        TheTrace.Trace("lF");
        if (successVisible)
        {
            var successText = await successElement.TextContentAsync();
            successText.Should().MatchRegex("(activated|success)", "should show activation success message");
        }

        // Try to log in with new credentials
        TheTrace.Trace("lG");
        await _page.FillAsync("#email", "test3@test.com");
        TheTrace.Trace("lH");
        await _page.FillAsync("#password", "Password3!");
        TheTrace.Trace("lI");
        await _page.ClickAsync("#login-button");

        // Wait for navigation to index.html
        TheTrace.Trace("lJ");
        await _page.WaitForURLAsync("**/index.html", new() { Timeout = 1000 });

        // Verify navigation to index.html
        TheTrace.Trace("lK");
        _page.Url.Should().Contain("index.html", "should navigate to index page after successful login");
        TheTrace.Trace("lL");
    }

    /// <summary>
    /// Test registration with incorrect activation token.
    /// </summary>
    [Fact(Timeout = 10000)]
    public async Task Registration_IncorrectToken_ShowsError()
    {
        // Ensure email queue is empty
        TheTrace.Trace("ma");
        var initialQueueSize = await _fixture.GetEmailQueueSizeAsync();
        TheTrace.Trace("mb");
        for (int i = 0; i < initialQueueSize; i++)
            await _fixture.DequeueEmailAsync();

        // Navigate to register page
        TheTrace.Trace("mc");
        await _page.GotoAsync("/register.html");
        TheTrace.Trace("md");
        await _page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

        // Wait for page initialization (password rules loaded)
        TheTrace.Trace("me");
        await _page.WaitForFunctionAsync("document.getElementById('password-rules')?.textContent?.length > 0");

        // Fill in email
        TheTrace.Trace("mf");
        await _page.FillAsync("#email", "test4@test.com");

        // Fill in password
        TheTrace.Trace("mf");
        await _page.FillAsync("#password", "Password4!");

        TheTrace.Trace("mf1");
        await _page.Locator("#register-button").IsDisabledAsync(new LocatorIsDisabledOptions { Timeout = 5000 });

        // Click register button
        TheTrace.Trace("mg");
        await _page.ClickAsync("#register-button");

        // Wait for response
        TheTrace.Trace("mh");
        await Task.Delay(1000);

        // Verify confirmation message appears
        TheTrace.Trace("mi");
        var confirmationElement = _page.Locator("#message");
        TheTrace.Trace("mj");
        await confirmationElement.WaitForAsync(new() { State = Microsoft.Playwright.WaitForSelectorState.Visible, Timeout = 1000 });

        // Verify email appears in queue
        TheTrace.Trace("mk");
        var queueSize = await _fixture.GetEmailQueueSizeAsync();
        TheTrace.Trace("ml");
        queueSize.Should().Be(1, "exactly one email should be in the queue");

        // Dequeue email
        TheTrace.Trace("mm");
        var email = await _fixture.DequeueEmailAsync();
        TheTrace.Trace("mn");
        email.Should().NotBeNull("email should be dequeued");

        // Extract activation link from email body
        TheTrace.Trace("mo");
        var emailBody = email!.Body;
        TheTrace.Trace("mp");
        var linkMatch = System.Text.RegularExpressions.Regex.Match(emailBody, @"(http[s]?://[^\s]+)");
        TheTrace.Trace("mq");
        linkMatch.Success.Should().BeTrue("email should contain activation link");

        TheTrace.Trace("mr");
        var activationUrl = linkMatch.Value;

        // Extract token and replace one digit
        TheTrace.Trace("ms");
        var tokenMatch = System.Text.RegularExpressions.Regex.Match(activationUrl, @"token=(\d{6})");
        TheTrace.Trace("mt");
        tokenMatch.Success.Should().BeTrue("link should contain 6-digit token");

        TheTrace.Trace("mu");
        var originalToken = tokenMatch.Groups[1].Value;
        TheTrace.Trace("mv");
        var modifiedToken = ReplaceOneDigit(originalToken);

        // Replace token in URL
        TheTrace.Trace("mw");
        var modifiedUrl = activationUrl.Replace($"token={originalToken}", $"token={modifiedToken}");

        // Open the modified activation link
        TheTrace.Trace("mx");
        await _page.GotoAsync(modifiedUrl);
        TheTrace.Trace("my");
        await _page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

        // Verify navigation to register page
        TheTrace.Trace("mz");
        _page.Url.Should().Contain("register.html", "should navigate back to register page after failed activation");

        // Verify error message on register page
        TheTrace.Trace("mA");
        var errorElement = _page.Locator("#message");
        TheTrace.Trace("mB");
        var errorVisible = await errorElement.IsVisibleAsync();
        TheTrace.Trace("mC");
        if (errorVisible)
        {
            var errorText = await errorElement.TextContentAsync();
            errorText.Should().MatchRegex("(activation|token|invalid|failed)", "should show activation error message");
        }

        // Try to log in with new credentials - should fail
        TheTrace.Trace("mD");
        await _page.GotoAsync("/login.html");
        TheTrace.Trace("mE");
        await _page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

        TheTrace.Trace("mF");
        await _page.FillAsync("#email", "test4@test.com");
        TheTrace.Trace("mG");
        await _page.FillAsync("#password", "Password4!");
        TheTrace.Trace("mH");
        await _page.ClickAsync("#login-button");

        // Wait for response
        TheTrace.Trace("mI");
        await Task.Delay(500);

        // Verify we're still on login page (login should fail)
        TheTrace.Trace("mJ");
        _page.Url.Should().Contain("login.html", "should remain on login page - account not activated");
        TheTrace.Trace("mK");
    }

    /// <summary>
    /// Test registration with another user's activation token.
    /// </summary>
    [Fact(Timeout = 10000)]
    public async Task Registration_WrongUserToken_ShowsError()
    {
        // Ensure email queue is empty
        TheTrace.Trace("na");
        var initialQueueSize = await _fixture.GetEmailQueueSizeAsync();
        TheTrace.Trace("nb");
        for (int i = 0; i < initialQueueSize; i++)
            await _fixture.DequeueEmailAsync();

        // Register first user (test5@test.com)
        TheTrace.Trace("nc");
        await _page.GotoAsync("/register.html");
        TheTrace.Trace("nd");
        await _page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

        // Wait for page initialization (password rules loaded)
        TheTrace.Trace("ne");
        await _page.WaitForFunctionAsync("document.getElementById('password-rules')?.textContent?.length > 0");

        TheTrace.Trace("nf");
        await _page.FillAsync("#email", "test5@test.com");
        TheTrace.Trace("nf");
        await _page.FillAsync("#password", "Password5!");
        TheTrace.Trace("ng");
        await _page.ClickAsync("#register-button");
        TheTrace.Trace("nh");
        await Task.Delay(1000);

        // Dequeue first email
        TheTrace.Trace("ni");
        var email1 = await _fixture.DequeueEmailAsync();
        TheTrace.Trace("nj");
        email1.Should().NotBeNull("first email should be dequeued");

        // Extract token from first email
        TheTrace.Trace("nk");
        var linkMatch1 = System.Text.RegularExpressions.Regex.Match(email1!.Body, @"(http[s]?://[^\s]+)");
        TheTrace.Trace("nl");
        linkMatch1.Success.Should().BeTrue("first email should contain activation link");

        TheTrace.Trace("nm");
        var tokenMatch1 = System.Text.RegularExpressions.Regex.Match(linkMatch1.Value, @"token=(\d{6})");
        TheTrace.Trace("nn");
        tokenMatch1.Success.Should().BeTrue("first link should contain 6-digit token");
        TheTrace.Trace("no");
        var token1 = tokenMatch1.Groups[1].Value;

        // Register second user (test6@test.com)
        TheTrace.Trace("np");
        await _page.GotoAsync("/register.html");
        TheTrace.Trace("nq");
        await _page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

        // Wait for page initialization (password rules loaded)
        TheTrace.Trace("nr");
        await _page.WaitForFunctionAsync("document.getElementById('password-rules')?.textContent?.length > 0");

        TheTrace.Trace("ns");
        await _page.FillAsync("#email", "test6@test.com");
        TheTrace.Trace("ns");
        await _page.FillAsync("#password", "Password6!");
        TheTrace.Trace("nt");
        await _page.ClickAsync("#register-button");
        TheTrace.Trace("nu");
        await Task.Delay(1000);

        // Dequeue second email
        TheTrace.Trace("nv");
        var email2 = await _fixture.DequeueEmailAsync();
        TheTrace.Trace("nw");
        email2.Should().NotBeNull("second email should be dequeued");

        // Extract URL from second email
        TheTrace.Trace("nx");
        var linkMatch2 = System.Text.RegularExpressions.Regex.Match(email2!.Body, @"(http[s]?://[^\s]+)");
        TheTrace.Trace("ny");
        linkMatch2.Success.Should().BeTrue("second email should contain activation link");

        TheTrace.Trace("nz");
        var tokenMatch2 = System.Text.RegularExpressions.Regex.Match(linkMatch2.Value, @"token=(\d{6})");
        TheTrace.Trace("nA");
        tokenMatch2.Success.Should().BeTrue("second link should contain 6-digit token");
        TheTrace.Trace("nB");
        var token2 = tokenMatch2.Groups[1].Value;

        // Replace token2 with token1 in the second activation URL
        TheTrace.Trace("nC");
        var modifiedUrl = linkMatch2.Value.Replace($"token={token2}", $"token={token1}");

        // Open the modified activation link
        TheTrace.Trace("nD");
        await _page.GotoAsync(modifiedUrl);
        TheTrace.Trace("nE");
        await _page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

        // Verify navigation to register page
        TheTrace.Trace("nF");
        _page.Url.Should().Contain("register.html", "should navigate back to register page after failed activation");

        // Verify error message on register page
        TheTrace.Trace("nG");
        var errorElement = _page.Locator("#message");
        TheTrace.Trace("nH");
        var errorVisible = await errorElement.IsVisibleAsync();
        TheTrace.Trace("nI");
        if (errorVisible)
        {
            var errorText = await errorElement.TextContentAsync();
            errorText.Should().MatchRegex("(activation|token|invalid|failed)", "should show activation error message");
        }

        // Try to log in with second user credentials - should fail
        TheTrace.Trace("nJ");
        await _page.GotoAsync("/login.html");
        TheTrace.Trace("nK");
        await _page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

        TheTrace.Trace("nL");
        await _page.FillAsync("#email", "test6@test.com");
        TheTrace.Trace("nM");
        await _page.FillAsync("#password", "Password6!");
        TheTrace.Trace("nN");
        await _page.ClickAsync("#login-button");

        // Wait for response
        TheTrace.Trace("nO");
        await Task.Delay(500);

        // Verify we're still on login page (login should fail)
        TheTrace.Trace("nP");
        _page.Url.Should().Contain("login.html", "should remain on login page - account not activated");
        TheTrace.Trace("nQ");
    }

    /// <summary>
    /// Helper method to replace one digit in a token.
    /// </summary>
    /// <param name="token">The original token.</param>
    /// <returns>Token with one digit replaced.</returns>
    private static string ReplaceOneDigit(string token)
    {
        if (string.IsNullOrEmpty(token) || token.Length == 0)
            return token;

        var chars = token.ToCharArray();
        var firstDigit = chars[0];
        chars[0] = firstDigit == '0' ? '1' : '0';
        return new string(chars);
    }
}
