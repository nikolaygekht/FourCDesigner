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
    [InlineData("test3@test.com", "PasswordABC", "Password without numbers")]
    [InlineData("test3@test.com", "password123", "Password without capital letters")]
    [InlineData("test3@test.com", "PASSWORD123", "Password without small letters")]
    [InlineData("test3@test.com", "Pass", "Password too short")]
    [InlineData("invalid-email", "Password1!", "Invalid email")]
    [InlineData("test1@test.com", "Password1!", "Existing email")]
    public async Task Registration_ValidationErrors_ShowsError(string email, string password, string testCase)
    {
        // Navigate to register page
        await GoRegistrationPage();

        // Fill in email
        await _page.FillAsync("#email", email);

        // Fill in password
        await _page.FillAsync("#password", password);

        // Explicitly blur the password field to trigger validation
        await _page.Locator("#password").BlurAsync();

        // Wait for validation to complete (including async email check)
        await Task.Delay(250);

        // Check if either input has the is-invalid class (which indicates validation error)
        var emailHasError = await _page.Locator("#email.is-invalid").CountAsync() > 0;
        var passwordHasError = await _page.Locator("#password.is-invalid").CountAsync() > 0;

        (emailHasError || passwordHasError).Should().BeTrue($"validation error should be displayed for: {testCase}");

        // Verify button remains disabled
        var isDisabled = await _page.IsDisabledAsync("#register-button");
        isDisabled.Should().BeTrue("register button should be disabled with invalid input");

        // Verify we're still on register page
        _page.Url.Should().Contain("register.html", "should remain on register page");
    }

    /// <summary>
    /// Test registration with validation errors at server level.
    /// Directly calls the API endpoint and verifies server-side validation.
    /// Theory with the following options:
    /// - Empty Email/Correct Password
    /// - Correct Email/Empty Password
    /// - Correct Email/Password w/o numbers
    /// - Correct Email/Password w/o capital letters
    /// - Correct Email/Password w/o small letters
    /// - Correct Email/Password too short
    /// - Incorrect Email/Correct Password
    /// - Existing Email (test1@test.com)/Correct Password
    /// </summary>
    [Theory(Timeout = 10000)]
    [InlineData("", "Password1!", false, "Empty email")]
    [InlineData("test3@test.com", "", true, "Empty password")]
    [InlineData("test3@test.com", "PasswordABC", true, "Password without numbers")]
    [InlineData("test3@test.com", "password123", true, "Password without capital letters")]
    [InlineData("test3@test.com", "PASSWORD123", true, "Password without small letters")]
    [InlineData("test3@test.com", "Pass", true, "Password too short")]
    [InlineData("invalid-email", "Password1!", false, "Invalid email")]
    [InlineData("test1@test.com", "Password1!", false, "Existing email")]
    public async Task Registration_ValidationErrors_ServerErrors(string email, string password, bool checkUserNotCreated, string testCase)
    {
        // Ensure email queue is empty
        var initialQueueSize = await _fixture.GetEmailQueueSizeAsync();
        for (int i = 0; i < initialQueueSize; i++)
            await _fixture.DequeueEmailAsync();

        // Call register API endpoint directly
        var response = await _fixture.HttpClient.PostAsJsonAsync("/api/user/register", new
        {
            email = email,
            password = password
        });

        // Verify the endpoint returns an error (not 200 OK)
        response.IsSuccessStatusCode.Should().BeFalse($"registration should fail for: {testCase}");

        // Verify the user was not created (if requested)
        if (checkUserNotCreated)
        {
            var userCheckResponse = await _fixture.HttpClient.GetAsync($"/api/test/user?email={Uri.EscapeDataString(email)}");
            userCheckResponse.StatusCode.Should().Be(HttpStatusCode.NotFound, $"user should not exist for: {testCase}");
        }

        // Verify no email was queued
        var queueSize = await _fixture.GetEmailQueueSizeAsync();
        queueSize.Should().Be(0, $"no email should be queued for: {testCase}");
    }

    /// <summary>
    /// Test successful registration flow.
    /// </summary>
    [Fact(Timeout = 10000)]
    public async Task Registration_Successful_ShowsConfirmationAndActivatesAccount()
    {
        // Ensure email queue is empty
        var initialQueueSize = await _fixture.GetEmailQueueSizeAsync();
        for (int i = 0; i < initialQueueSize; i++)
            await _fixture.DequeueEmailAsync();

        // Navigate to register page
        await GoRegistrationPage();

        // Fill in email
        await _page.FillAsync("#email", "test3@test.com");

        // Fill in password
        await _page.FillAsync("#password", "Password3!");

        // Blur password field to trigger validation
        await _page.Locator("#password").BlurAsync();

        // Wait for button to become enabled (validation completes, including async email check)
        await _page.WaitForSelectorAsync("#register-button:not([disabled])", new() { Timeout = 2000 });

        // Click register button
        await _page.ClickAsync("#register-button");

        // Wait for response
        await Task.Delay(250);

        // Verify confirmation message appears
        var confirmationElement = _page.Locator("#message");
        await confirmationElement.WaitForAsync(new() { State = Microsoft.Playwright.WaitForSelectorState.Visible, Timeout = 1000 });

        var confirmationText = await confirmationElement.TextContentAsync();
        confirmationText.Should().MatchRegex("(confirm|activation|email)", "should show confirmation message");

        // Verify email appears in queue
        var queueSize = await _fixture.GetEmailQueueSizeAsync();
        queueSize.Should().Be(1, "exactly one email should be in the queue");

        // Dequeue email
        var email = await _fixture.DequeueEmailAsync();
        email.Should().NotBeNull("email should be dequeued");
        email!.To.Should().Contain("test3@test.com");

        // Extract activation link from email body
        var emailBody = email.Body;
        var linkMatch = System.Text.RegularExpressions.Regex.Match(emailBody, @"(http[s]?://[^\s]+)");
        linkMatch.Success.Should().BeTrue("email should contain activation link");

        var activationUrl = linkMatch.Value;

        // Verify link contains email (URL-encoded) and token
        activationUrl.Should().Contain("email=test3%40test.com", "link should contain email parameter");

        var tokenMatch = System.Text.RegularExpressions.Regex.Match(activationUrl, @"token=(\d{6})");
        tokenMatch.Success.Should().BeTrue("link should contain 6-digit token");

        var token = tokenMatch.Groups[1].Value;
        token.Should().MatchRegex(@"^\d{6}$", "token should be 6 digits");

        // Open the activation link
        await _page.GotoAsync(activationUrl);
        await _page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

        // Verify navigation to login page
        _page.Url.Should().Contain("login.html", "should navigate to login page after activation");

        // Verify success message on login page
        var successElement = _page.Locator("#error-message");
        var successVisible = await successElement.IsVisibleAsync();
        if (successVisible)
        {
            var successText = await successElement.TextContentAsync();
            successText.Should().MatchRegex("(activated|success)", "should show activation success message");
        }

        // Try to log in with new credentials
        await _page.FillAsync("#email", "test3@test.com");
        await _page.FillAsync("#password", "Password3!");
        await _page.ClickAsync("#login-button");

        // Wait for navigation to index.html
        await _page.WaitForURLAsync("**/index.html", new() { Timeout = 1000 });

        // Verify navigation to index.html
        _page.Url.Should().Contain("index.html", "should navigate to index page after successful login");
    }

    /// <summary>
    /// Test registration with incorrect activation token.
    /// </summary>
    [Fact(Timeout = 10000)]
    public async Task Registration_IncorrectToken_ShowsError()
    {
        // Ensure email queue is empty
        var initialQueueSize = await _fixture.GetEmailQueueSizeAsync();
        for (int i = 0; i < initialQueueSize; i++)
            await _fixture.DequeueEmailAsync();

        // Navigate to register page
        await GoRegistrationPage();

        // Fill in email
        await _page.FillAsync("#email", "test4@test.com");

        // Fill in password
        await _page.FillAsync("#password", "Password4!");

        // Blur password field to trigger validation
        await _page.Locator("#password").BlurAsync();

        // Wait for button to become enabled (validation completes, including async email check)
        await _page.WaitForSelectorAsync("#register-button:not([disabled])", new() { Timeout = 2000 });

        // Click register button
        await _page.ClickAsync("#register-button");

        // Wait for response
        await Task.Delay(250);

        // Verify confirmation message appears
        var confirmationElement = _page.Locator("#message");
        await confirmationElement.WaitForAsync(new() { State = Microsoft.Playwright.WaitForSelectorState.Visible, Timeout = 1000 });

        // Verify email appears in queue
        var queueSize = await _fixture.GetEmailQueueSizeAsync();
        queueSize.Should().Be(1, "exactly one email should be in the queue");

        // Dequeue email
        var email = await _fixture.DequeueEmailAsync();
        email.Should().NotBeNull("email should be dequeued");

        // Extract activation link from email body
        var emailBody = email!.Body;
        var linkMatch = System.Text.RegularExpressions.Regex.Match(emailBody, @"(http[s]?://[^\s]+)");
        linkMatch.Success.Should().BeTrue("email should contain activation link");

        var activationUrl = linkMatch.Value;

        // Extract token and replace one digit
        var tokenMatch = System.Text.RegularExpressions.Regex.Match(activationUrl, @"token=(\d{6})");
        tokenMatch.Success.Should().BeTrue("link should contain 6-digit token");

        var originalToken = tokenMatch.Groups[1].Value;
        var modifiedToken = ReplaceOneDigit(originalToken);

        // Replace token in URL
        var modifiedUrl = activationUrl.Replace($"token={originalToken}", $"token={modifiedToken}");

        // Open the modified activation link
        await _page.GotoAsync(modifiedUrl);
        await _page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

        // Verify navigation to login page (activation endpoint always redirects to login)
        _page.Url.Should().Contain("login.html", "should navigate to login page after activation attempt");

        // Verify error message on login page
        var errorElement = _page.Locator("#error-message");
        await errorElement.WaitForAsync(new() { State = Microsoft.Playwright.WaitForSelectorState.Visible, Timeout = 1000 });

        var errorText = await errorElement.TextContentAsync();
        errorText.Should().MatchRegex("(activation|token|invalid|failed)", "should show activation error message");

        // Try to log in with new credentials - should fail because account is not activated
        await _page.FillAsync("#email", "test4@test.com");
        await _page.FillAsync("#password", "Password4!");
        await _page.ClickAsync("#login-button");

        // Wait for response
        await Task.Delay(250);

        // Verify we're still on login page (login should fail - account not activated)
        _page.Url.Should().Contain("login.html", "should remain on login page - account not activated");

        // Verify error message is displayed
        var loginErrorVisible = await errorElement.IsVisibleAsync();
        loginErrorVisible.Should().BeTrue("should show error after failed login attempt");
    }

    /// <summary>
    /// Test registration with another user's activation token.
    /// </summary>
    [Fact(Timeout = 10000)]
    public async Task Registration_WrongUserToken_ShowsError()
    {
        // Ensure email queue is empty
        var initialQueueSize = await _fixture.GetEmailQueueSizeAsync();
        for (int i = 0; i < initialQueueSize; i++)
            await _fixture.DequeueEmailAsync();

        // Register first user (test5@test.com)
        await GoRegistrationPage();

        await _page.FillAsync("#email", "test5@test.com");
        await _page.FillAsync("#password", "Password5!");
        await _page.Locator("#password").BlurAsync();
        await _page.WaitForSelectorAsync("#register-button:not([disabled])", new() { Timeout = 2000 });
        await _page.ClickAsync("#register-button");
        await Task.Delay(250);

        // Dequeue first email
        var email1 = await _fixture.DequeueEmailAsync();
        email1.Should().NotBeNull("first email should be dequeued");

        // Extract token from first email
        var linkMatch1 = System.Text.RegularExpressions.Regex.Match(email1!.Body, @"(http[s]?://[^\s]+)");
        linkMatch1.Success.Should().BeTrue("first email should contain activation link");

        var tokenMatch1 = System.Text.RegularExpressions.Regex.Match(linkMatch1.Value, @"token=(\d{6})");
        tokenMatch1.Success.Should().BeTrue("first link should contain 6-digit token");
        var token1 = tokenMatch1.Groups[1].Value;

        // Register second user (test6@test.com)
        await GoRegistrationPage();

        await _page.FillAsync("#email", "test6@test.com");
        await _page.FillAsync("#password", "Password6!");
        await _page.Locator("#password").BlurAsync();
        await _page.WaitForSelectorAsync("#register-button:not([disabled])", new() { Timeout = 2000 });
        await _page.ClickAsync("#register-button");
        await Task.Delay(250);

        // Dequeue second email
        var email2 = await _fixture.DequeueEmailAsync();
        email2.Should().NotBeNull("second email should be dequeued");

        // Extract URL from second email
        var linkMatch2 = System.Text.RegularExpressions.Regex.Match(email2!.Body, @"(http[s]?://[^\s]+)");
        linkMatch2.Success.Should().BeTrue("second email should contain activation link");

        var tokenMatch2 = System.Text.RegularExpressions.Regex.Match(linkMatch2.Value, @"token=(\d{6})");
        tokenMatch2.Success.Should().BeTrue("second link should contain 6-digit token");
        var token2 = tokenMatch2.Groups[1].Value;

        // Replace token2 with token1 in the second activation URL (wrong token for this user)
        var modifiedUrl = linkMatch2.Value.Replace($"token={token2}", $"token={token1}");

        // Open the modified activation link
        await _page.GotoAsync(modifiedUrl);
        await _page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

        // Verify navigation to login page (activation endpoint always redirects to login)
        _page.Url.Should().Contain("login.html", "should navigate to login page after activation attempt");

        // Verify error message on login page
        var errorElement = _page.Locator("#error-message");
        await errorElement.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 1000 });

        var errorText = await errorElement.TextContentAsync();
        errorText.Should().MatchRegex("(activation|token|invalid|failed)", "should show activation error message");

        // Try to log in with second user credentials - should fail because account is not activated
        await _page.FillAsync("#email", "test6@test.com");
        await _page.FillAsync("#password", "Password6!");
        await _page.ClickAsync("#login-button");

        // Wait for response
        await Task.Delay(250);

        // Verify we're still on login page (login should fail - account not activated)
        _page.Url.Should().Contain("login.html", "should remain on login page - account not activated");

        // Verify error message is displayed
        var loginErrorVisible = await errorElement.IsVisibleAsync();
        loginErrorVisible.Should().BeTrue("should show error after failed login attempt");
    }

    /// <summary>
    /// Navigates to the registration page and waits for full initialization.
    /// </summary>
    private async Task GoRegistrationPage()
    {
        await _page.GotoAsync("/register.html");
        await _page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

        // Wait for page initialization (password rules loaded AND event listeners attached)
        await _page.WaitForFunctionAsync(@"
            document.getElementById('password-rules')?.textContent?.length > 0 &&
            typeof window.registerFormInitialized !== 'undefined'
        ");
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
