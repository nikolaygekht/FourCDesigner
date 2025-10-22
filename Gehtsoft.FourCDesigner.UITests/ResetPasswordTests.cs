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
        // Ensure email queue is empty
        await ClearEmailQueue();

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

        // Verify no email was queued (security feature - don't leak user existence)
        var queueSize = await _fixture.GetEmailQueueSizeAsync();
        queueSize.Should().Be(0, "no email should be queued for non-existing user");
    }

    /// <summary>
    /// Test complete password reset flow - happy path.
    /// Creates user, requests reset, changes password, verifies login with new password works.
    /// </summary>
    [Fact(Timeout = 10000)]
    public async Task ResetPassword_SuccessfulFlow_ResetsPasswordAndAllowsLogin()
    {
        // Ensure email queue is empty
        await ClearEmailQueue();

        // Create and activate test user
        await CreateAndActivateUser("rp001@test.com", "OldPassword001!");

        // Navigate to login page
        await _page.GotoAsync("/login.html");
        await _page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

        // Request password reset
        await _page.FillAsync("#email", "rp001@test.com");
        await Task.Delay(250);
        await _page.ClickAsync("#forgot-password-button");
        await Task.Delay(250);

        // Verify success message
        var errorElement = _page.Locator("#error-message");
        var isVisible = await errorElement.IsVisibleAsync();
        isVisible.Should().BeTrue("success message should be displayed");

        // Verify email was queued
        var queueSize = await _fixture.GetEmailQueueSizeAsync();
        queueSize.Should().Be(1, "reset email should be queued");

        // Dequeue and verify email
        var email = await _fixture.DequeueEmailAsync();
        email.Should().NotBeNull("reset email should be dequeued");
        email!.To.Should().Contain("rp001@test.com");

        // Extract reset link from email
        var linkMatch = System.Text.RegularExpressions.Regex.Match(email.Body, @"(http[s]?://[^\s]+)");
        linkMatch.Success.Should().BeTrue("email should contain reset link");
        var resetLink = linkMatch.Value;

        // Verify link contains email and token
        resetLink.Should().Contain("email=", "link should contain email parameter");
        resetLink.Should().Contain("token=", "link should contain token parameter");

        var tokenMatch = System.Text.RegularExpressions.Regex.Match(resetLink, @"token=(\d{6})");
        tokenMatch.Success.Should().BeTrue("link should contain 6-digit token");
        var resetToken = tokenMatch.Groups[1].Value;
        resetToken.Should().MatchRegex(@"^\d{6}$", "token should be 6 digits");

        // Open the reset link (simulates user clicking link in email)
        await _page.GotoAsync(resetLink);
        await _page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

        // Verify we're on reset password page
        _page.Url.Should().Contain("resetpassword.html", "should be redirected to reset password page");

        // Wait for page initialization
        await WaitForResetPasswordPageInitialization();

        // Fill in new password
        await _page.FillAsync("#password", "NewPassword001!");

        // Blur password field to trigger validation
        await _page.Locator("#password").BlurAsync();

        // Wait for button to become enabled
        await _page.WaitForSelectorAsync("#reset-button:not([disabled])", new() { Timeout = 2000 });

        // Click reset button
        await _page.ClickAsync("#reset-button");

        // Wait for redirect to login page
        await _page.WaitForURLAsync("**/login.html", new() { Timeout = 5000 });

        // Verify we're on login page
        _page.Url.Should().Contain("login.html", "should redirect to login page after successful reset");

        // Try to login with OLD password - should FAIL
        await _page.FillAsync("#email", "rp001@test.com");
        await _page.FillAsync("#password", "OldPassword001!");
        await _page.ClickAsync("#login-button");

        // Wait for response
        await Task.Delay(250);

        // Verify we're still on login page (login failed)
        _page.Url.Should().Contain("login.html", "should remain on login page - old password should not work");

        // Try to login with NEW password - should SUCCEED
        await _page.FillAsync("#email", "rp001@test.com");
        await _page.FillAsync("#password", "NewPassword001!");
        await _page.ClickAsync("#login-button");

        // Wait for navigation to index.html
        await _page.WaitForURLAsync("**/index.html", new() { Timeout = 2000 });

        // Verify navigation to index.html
        _page.Url.Should().Contain("index.html", "should navigate to index page after successful login with new password");
    }

    /// <summary>
    /// Test client-side password validation errors on reset password page.
    /// Uses pre-seeded test2@test.com user.
    /// </summary>
    [Theory(Timeout = 10000)]
    [InlineData("", "Empty password")]
    [InlineData("PasswordABC", "Password without numbers")]
    [InlineData("password123", "Password without capital letters")]
    [InlineData("PASSWORD123", "Password without small letters")]
    [InlineData("Pass1", "Password too short")]
    public async Task ResetPassword_ValidationErrors_ShowsError(string password, string testCase)
    {
        // Clear email queue and request password reset for pre-seeded user
        await ClearEmailQueue();
        var token = await RequestPasswordResetAndGetToken("test2@test.com");
        token.Should().NotBeNull("reset token should be generated");

        // Set cookies and navigate to reset password page
        await SetResetPasswordCookies("test2@test.com", token!);
        await GoResetPasswordPage();

        // Fill in invalid password
        await _page.FillAsync("#password", password);

        // Blur password field to trigger validation
        await _page.Locator("#password").BlurAsync();

        // Wait for validation to complete
        await Task.Delay(250);

        // Check if password has error
        var passwordHasError = await _page.Locator("#password.is-invalid").CountAsync() > 0;
        passwordHasError.Should().BeTrue($"validation error should be displayed for: {testCase}");

        // Verify button remains disabled
        var isDisabled = await _page.IsDisabledAsync("#reset-button");
        isDisabled.Should().BeTrue("reset button should be disabled with invalid password");

        // Verify we're still on reset password page
        _page.Url.Should().Contain("resetpassword.html", "should remain on reset password page");
    }

    /// <summary>
    /// Test server-side password validation errors.
    /// Verifies that invalid passwords are rejected by the API and password is NOT changed.
    /// Uses pre-seeded test2@test.com user with password "Password2!".
    /// </summary>
    [Theory(Timeout = 10000)]
    [InlineData("", true, "Empty password")]
    [InlineData("PasswordABC", true, "Password without numbers")]
    [InlineData("password123", true, "Password without capital letters")]
    [InlineData("PASSWORD123", true, "Password without small letters")]
    [InlineData("Pass1", true, "Password too short")]
    [InlineData("InvalidToken123", false, "Invalid token")]
    public async Task ResetPassword_ValidationErrors_ServerErrors(string password, bool checkPasswordNotChanged, string testCase)
    {
        // Clear email queue and request password reset for pre-seeded user
        await ClearEmailQueue();
        var token = await RequestPasswordResetAndGetToken("test2@test.com");
        token.Should().NotBeNull("reset token should be generated");

        // For invalid token test, modify the token
        var testToken = testCase == "Invalid token" ? "999999" : token;

        // Call reset password API endpoint directly
        var response = await _fixture.HttpClient.PostAsJsonAsync("/api/user/reset-password", new
        {
            email = "test2@test.com",
            token = testToken,
            password = password
        });

        // Verify the endpoint returns an error (not 200 OK)
        response.IsSuccessStatusCode.Should().BeFalse($"password reset should fail for: {testCase}");

        // Verify password was NOT changed (if requested)
        if (checkPasswordNotChanged)
        {
            // Try to login with OLD password - should succeed
            var loginResponse = await _fixture.HttpClient.PostAsJsonAsync("/api/user/login", new
            {
                email = "test2@test.com",
                password = "Password2!" // Original password
            });

            loginResponse.IsSuccessStatusCode.Should().BeTrue($"login with old password should still work after failed reset for: {testCase}");

            // Try to login with NEW password - should fail
            var loginWithNewPasswordResponse = await _fixture.HttpClient.PostAsJsonAsync("/api/user/login", new
            {
                email = "test2@test.com",
                password = password // New (invalid) password
            });

            loginWithNewPasswordResponse.IsSuccessStatusCode.Should().BeFalse($"login with new password should fail after failed reset for: {testCase}");
        }

        // Verify no additional emails were queued
        var queueSize = await _fixture.GetEmailQueueSizeAsync();
        queueSize.Should().Be(0, $"no additional email should be queued for: {testCase}");
    }

    /// <summary>
    /// Test password reset with incorrect/tampered token.
    /// Verifies that a modified token is rejected and password is NOT changed.
    /// Uses pre-seeded test2@test.com user with password "Password2!".
    /// </summary>
    [Fact(Timeout = 10000)]
    public async Task ResetPassword_IncorrectToken_ShowsError()
    {
        // Clear email queue and request password reset
        await ClearEmailQueue();
        var token = await RequestPasswordResetAndGetToken("test2@test.com");
        token.Should().NotBeNull("reset token should be generated");

        // Tamper with the token by replacing one digit
        var tamperedToken = ReplaceOneDigit(token!);
        tamperedToken.Should().NotBe(token, "tampered token should be different from original");

        // Attempt to reset password with tampered token
        var response = await _fixture.HttpClient.PostAsJsonAsync("/api/user/reset-password", new
        {
            email = "test2@test.com",
            token = tamperedToken,
            password = "NewPassword2!"
        });

        // Verify the endpoint returns an error
        response.IsSuccessStatusCode.Should().BeFalse("password reset should fail with incorrect token");

        // Verify password was NOT changed - try to login with OLD password (should succeed)
        var loginResponse = await _fixture.HttpClient.PostAsJsonAsync("/api/user/login", new
        {
            email = "test2@test.com",
            password = "Password2!" // Original password
        });

        loginResponse.IsSuccessStatusCode.Should().BeTrue("login with old password should still work after failed reset");

        // Try to login with NEW password - should fail
        var loginWithNewPasswordResponse = await _fixture.HttpClient.PostAsJsonAsync("/api/user/login", new
        {
            email = "test2@test.com",
            password = "NewPassword2!"
        });

        loginWithNewPasswordResponse.IsSuccessStatusCode.Should().BeFalse("login with new password should fail after failed reset");
    }

    /// <summary>
    /// Test password reset with another user's token.
    /// Verifies that a token from one user cannot be used to reset another user's password.
    /// Creates two temporary active users for this security test.
    /// </summary>
    [Fact(Timeout = 10000)]
    public async Task ResetPassword_WrongUserToken_ShowsError()
    {
        // Clear email queue
        await ClearEmailQueue();

        // Create two temporary active users
        await CreateAndActivateUser("rpwrong1@test.com", "Password001!");
        await CreateAndActivateUser("rpwrong2@test.com", "Password002!");

        // Request password reset for both users
        var token1 = await RequestPasswordResetAndGetToken("rpwrong1@test.com");
        token1.Should().NotBeNull("reset token for rpwrong1 should be generated");

        var token2 = await RequestPasswordResetAndGetToken("rpwrong2@test.com");
        token2.Should().NotBeNull("reset token for rpwrong2 should be generated");

        // Attempt to reset rpwrong2's password using rpwrong1's token
        var response = await _fixture.HttpClient.PostAsJsonAsync("/api/user/reset-password", new
        {
            email = "rpwrong2@test.com",
            token = token1, // Using rpwrong1's token for rpwrong2!
            password = "NewPassword002!"
        });

        // Verify the endpoint returns an error
        response.IsSuccessStatusCode.Should().BeFalse("password reset should fail with wrong user's token");

        // Verify rpwrong2's password was NOT changed - try to login with OLD password (should succeed)
        var loginResponse = await _fixture.HttpClient.PostAsJsonAsync("/api/user/login", new
        {
            email = "rpwrong2@test.com",
            password = "Password002!" // Original password
        });

        loginResponse.IsSuccessStatusCode.Should().BeTrue("rpwrong2 login with old password should still work");

        // Try to login with NEW password - should fail
        var loginWithNewPasswordResponse = await _fixture.HttpClient.PostAsJsonAsync("/api/user/login", new
        {
            email = "rpwrong2@test.com",
            password = "NewPassword002!"
        });

        loginWithNewPasswordResponse.IsSuccessStatusCode.Should().BeFalse("rpwrong2 login with new password should fail after failed reset");
    }

    /// <summary>
    /// Clears the email queue.
    /// </summary>
    private async Task ClearEmailQueue()
    {
        var initialQueueSize = await _fixture.GetEmailQueueSizeAsync();
        for (int i = 0; i < initialQueueSize; i++)
            await _fixture.DequeueEmailAsync();
    }

    /// <summary>
    /// Waits for reset password page to be fully initialized.
    /// </summary>
    private async Task WaitForResetPasswordPageInitialization()
    {
        await _page.WaitForFunctionAsync(@"
            document.getElementById('password-rules')?.textContent?.length > 0 &&
            typeof window.resetPasswordFormInitialized !== 'undefined'
        ");
    }

    /// <summary>
    /// Navigates to the reset password page and waits for full initialization.
    /// </summary>
    private async Task GoResetPasswordPage()
    {
        await _page.GotoAsync("/resetpassword.html");
        await _page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);
        await WaitForResetPasswordPageInitialization();
    }

    /// <summary>
    /// Sets cookies for reset password flow.
    /// </summary>
    /// <param name="email">The user's email.</param>
    /// <param name="token">The reset token.</param>
    private async Task SetResetPasswordCookies(string email, string token)
    {
        await _context.AddCookiesAsync(new[]
        {
            new Microsoft.Playwright.Cookie
            {
                Name = "reset_email",
                Value = email,
                Domain = new Uri(_fixture.BaseUrl).Host,
                Path = "/"
            },
            new Microsoft.Playwright.Cookie
            {
                Name = "reset_token",
                Value = token,
                Domain = new Uri(_fixture.BaseUrl).Host,
                Path = "/"
            }
        });
    }

    /// <summary>
    /// Creates and activates a test user via API.
    /// </summary>
    /// <param name="email">The user's email.</param>
    /// <param name="password">The user's password.</param>
    private async Task CreateAndActivateUser(string email, string password)
    {
        var response = await _fixture.HttpClient.PostAsJsonAsync("/api/test/db/add-user", new
        {
            email = email,
            password = password,
            activate = true
        });

        response.IsSuccessStatusCode.Should().BeTrue($"user {email} should be created successfully");
    }

    /// <summary>
    /// Requests password reset via test API and returns the reset token from email.
    /// </summary>
    /// <param name="email">The user's email.</param>
    /// <returns>The reset token from the email, or null if no email was sent.</returns>
    private async Task<string?> RequestPasswordResetAndGetToken(string email)
    {
        // Request password reset via test API
        var response = await _fixture.HttpClient.PostAsync($"/api/test/request-password-reset?email={System.Web.HttpUtility.UrlEncode(email)}", null);
        response.IsSuccessStatusCode.Should().BeTrue("password reset request should succeed");

        // Get the reset email
        var resetEmail = await _fixture.DequeueEmailAsync();
        if (resetEmail == null)
            return null;

        // Extract token from email body
        var tokenMatch = System.Text.RegularExpressions.Regex.Match(resetEmail.Body, @"token=(\d{6})");
        if (!tokenMatch.Success)
            return null;

        return tokenMatch.Groups[1].Value;
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
