// Suppress nullability warnings for intentional null tests
#pragma warning disable CS8625, CS8602, CS8600, CS8620
using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using FluentAssertions;
using Gehtsoft.FourCDesigner.Controllers.Data;
using Gehtsoft.FourCDesigner.Logic.Email.Model;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace Gehtsoft.FourCDesigner.ApiTests;

/// <summary>
/// API tests for user authentication and session management endpoints.
/// </summary>
[Collection("ApiTests")]
public class UserApiTests : IDisposable
{
    private readonly TestWebApplicationFactory mFactory;
    private readonly HttpClient mClient;
    private readonly SqliteConnection mConnection;
    private readonly string mDatabaseName;

    public UserApiTests()
    {
        // Create unique database name for this test instance
        mDatabaseName = $"TestDb_{Guid.NewGuid():N}";

        // Create and keep alive an in-memory SQLite connection
        // This is required to prevent the in-memory database from being destroyed
        mConnection = new SqliteConnection($"Data Source={mDatabaseName};Mode=Memory;Cache=Shared");
        mConnection.Open();

        // Create the web application factory with the in-memory database
        mFactory = new TestWebApplicationFactory(mDatabaseName);
        mClient = mFactory.CreateClient();
    }

    public void Dispose()
    {
        mClient?.Dispose();
        mFactory?.Dispose();
        mConnection?.Dispose();
    }

    /// <summary>
    /// Extracts the 6-digit token from an email body.
    /// </summary>
    /// <param name="emailBody">The email body content.</param>
    /// <returns>The extracted token, or null if not found.</returns>
    private string? ExtractTokenFromEmail(string emailBody)
    {
        // Look for 6-digit token (100000-999999)
        var match = Regex.Match(emailBody, @"\b\d{6}\b");
        return match.Success ? match.Value : null;
    }

    /// <summary>
    /// Gets the most recent email sent to a specific recipient from the mock SMTP sender.
    /// Retries with exponential backoff to handle async email processing.
    /// </summary>
    /// <param name="recipientEmail">The recipient email address.</param>
    /// <returns>The email message, or null if not found.</returns>
    private async Task<EmailMessage?> GetMostRecentEmailAsync(string recipientEmail)
    {
        // Retry with exponential backoff to handle async email processing
        // Emails are sent via SendEmailAndTriggerProcessorAsync which processes immediately,
        // but we need to account for async completion time
        int[] delays = { 50, 100, 200, 500 }; // Total max wait: 850ms

        foreach (var delay in delays)
        {
            var capturedEmail = mFactory.MockSmtpSender.GetMostRecentEmailFor(recipientEmail);
            if (capturedEmail != null)
                return capturedEmail.Message;

            await Task.Delay(delay);
        }

        // One final check
        var finalCheck = mFactory.MockSmtpSender.GetMostRecentEmailFor(recipientEmail);
        return finalCheck?.Message;
    }

    #region Existing Tests

    [Fact]
    public async Task ValidateNonExistentSession_ShouldReturnInvalid()
    {
        // Arrange
        var request = new ValidateSessionRequest
        {
            SessionId = "nonexistent-session-id"
        };

        // Act
        var response = await mClient.PostAsJsonAsync("/api/user/validate-session", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ValidateSessionResponse>();
        result.Should().NotBeNull();
        result.Valid.Should().BeFalse();
    }

    [Fact]
    public async Task CorrectLogin_ShouldReturnSessionId()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "user@fourcdesign.com",
            Password = "test123"
        };

        // Act
        var response = await mClient.PostAsJsonAsync("/api/user/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result.Should().NotBeNull();
        result.SessionId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task IncorrectLogin_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "user@fourcdesign.com",
            Password = "wrongpassword"
        };

        // Act
        var response = await mClient.PostAsJsonAsync("/api/user/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ValidateExistentSession_ShouldReturnValid()
    {
        // Arrange - First login to get a valid session
        var loginRequest = new LoginRequest
        {
            Email = "user@fourcdesign.com",
            Password = "test123"
        };

        var loginResponse = await mClient.PostAsJsonAsync("/api/user/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var validateRequest = new ValidateSessionRequest
        {
            SessionId = loginResult.SessionId
        };

        // Act
        var response = await mClient.PostAsJsonAsync("/api/user/validate-session", validateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ValidateSessionResponse>();
        result.Should().NotBeNull();
        result.Valid.Should().BeTrue();
    }

    [Fact]
    public async Task Logout_ShouldSucceed()
    {
        // Arrange - First login to get a valid session
        var loginRequest = new LoginRequest
        {
            Email = "user@fourcdesign.com",
            Password = "test123"
        };

        var loginResponse = await mClient.PostAsJsonAsync("/api/user/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var logoutRequest = new LogoutRequest
        {
            SessionId = loginResult.SessionId
        };

        // Act
        var response = await mClient.PostAsJsonAsync("/api/user/logout", logoutRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ValidateSessionAfterLogout_ShouldReturnInvalid()
    {
        // Arrange - First login to get a valid session
        var loginRequest = new LoginRequest
        {
            Email = "user@fourcdesign.com",
            Password = "test123"
        };

        var loginResponse = await mClient.PostAsJsonAsync("/api/user/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        // Logout
        var logoutRequest = new LogoutRequest
        {
            SessionId = loginResult.SessionId
        };
        await mClient.PostAsJsonAsync("/api/user/logout", logoutRequest);

        var validateRequest = new ValidateSessionRequest
        {
            SessionId = loginResult.SessionId
        };

        // Act
        var response = await mClient.PostAsJsonAsync("/api/user/validate-session", validateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ValidateSessionResponse>();
        result.Should().NotBeNull();
        result.Valid.Should().BeFalse();
    }

    #endregion

    #region Registration → Activation → Login Flow Tests

    [Fact]
    public void SmtpSender_ShouldBeMock()
    {
        // Get ISmtpSender from DI container
        using var scope = mFactory.Services.CreateScope();
        var smtpSender = scope.ServiceProvider.GetRequiredService<Gehtsoft.FourCDesigner.Logic.Email.Sender.ISmtpSender>();

        // Assert - Check if it's our mock
        smtpSender.Should().NotBeNull();
        smtpSender.Should().BeSameAs(mFactory.MockSmtpSender, "ISmtpSender should be replaced with MockSmtpSender");
    }

    [Fact]
    public async Task EmailSender_ShouldBeActive()
    {
        // Get IEmailService from DI container
        using var scope = mFactory.Services.CreateScope();
        var emailService = scope.ServiceProvider.GetRequiredService<Gehtsoft.FourCDesigner.Logic.Email.IEmailService>();

        // Wait a moment for hosted service to start
        await Task.Delay(500);

        // Assert - Check if sender is active
        bool isActive = emailService.IsSenderActive;
        isActive.Should().BeTrue("Email background service should be running");
    }

    [Fact]
    public async Task EmailService_SendEmail_ShouldTriggerMockSmtp()
    {
        // Arrange - Create a simple test email
        var testEmail = EmailMessage.Create(
            to: "test@example.com",
            subject: "Test Email",
            body: "This is a test email",
            html: false,
            priority: false
        );

        // Act - Send email directly via IEmailService
        using var scope = mFactory.Services.CreateScope();
        var emailService = scope.ServiceProvider.GetRequiredService<Gehtsoft.FourCDesigner.Logic.Email.IEmailService>();

        await emailService.SendEmailAndTriggerProcessorAsync(testEmail);
        await Task.Delay(500); // Wait for async processing

        // Assert - Check if MockSmtpSender received the email
        int emailCount = mFactory.MockSmtpSender.Count;
        emailCount.Should().BeGreaterThan(0, "MockSmtpSender.Send() should have been called");

        // Also verify the email content
        var capturedEmail = mFactory.MockSmtpSender.GetMostRecentEmailFor("test@example.com");
        capturedEmail.Should().NotBeNull();
        capturedEmail!.Message.Subject.Should().Be("Test Email");
    }

    [Fact]
    public async Task RegisterUser_ShouldTriggerSmtpSender()
    {
        // Arrange
        string email = $"test{Guid.NewGuid():N}@test.com";
        string password = "Test123!";

        var registerRequest = new RegisterUserRequest
        {
            Email = email,
            Password = password
        };

        // Act
        await mClient.PostAsJsonAsync("/api/user/register", registerRequest);
        await Task.Delay(1000); // Give plenty of time for async processing

        // Assert - Check if MockSmtpSender received any emails
        int emailCount = mFactory.MockSmtpSender.Count;
        emailCount.Should().BeGreaterThan(0, "MockSmtpSender.Send() should have been called");
    }

    [Fact]
    public async Task RegisterActivateLogin_HappyPath_ShouldSucceed()
    {
        // Arrange
        string email = $"newuser{Guid.NewGuid():N}@test.com";
        string password = "Test123!";

        // Step 1: Register new user
        var registerRequest = new RegisterUserRequest
        {
            Email = email,
            Password = password
        };

        var registerResponse = await mClient.PostAsJsonAsync("/api/user/register", registerRequest);

        // Assert registration succeeded
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var registerResult = await registerResponse.Content.ReadFromJsonAsync<RegisterUserResponse>();
        registerResult.Should().NotBeNull();
        registerResult!.Success.Should().BeTrue();

        // Step 2: Extract activation token from email (with retry/wait for async processing)
        var emailMessage = await GetMostRecentEmailAsync(email);
        emailMessage.Should().NotBeNull("Activation email should be sent");
        emailMessage!.Subject.Should().Contain("Activate", "Email subject should mention activation");

        string? token = ExtractTokenFromEmail(emailMessage.Body);
        token.Should().NotBeNullOrEmpty("Token should be present in activation email");

        // Step 3: Activate account using the token
        var activateResponse = await mClient.GetAsync($"/activate-account?email={Uri.EscapeDataString(email)}&token={token}");

        // Assert activation succeeded (redirects to login page)
        activateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 4: Login with the activated account
        var loginRequest = new LoginRequest
        {
            Email = email,
            Password = password
        };

        var loginResponse = await mClient.PostAsJsonAsync("/api/user/login", loginRequest);

        // Assert login succeeded
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        loginResult.Should().NotBeNull();
        loginResult!.SessionId.Should().NotBeNullOrEmpty();

        // Step 5: Validate the session
        var validateRequest = new ValidateSessionRequest
        {
            SessionId = loginResult.SessionId
        };

        var validateResponse = await mClient.PostAsJsonAsync("/api/user/validate-session", validateRequest);
        validateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var validateResult = await validateResponse.Content.ReadFromJsonAsync<ValidateSessionResponse>();
        validateResult.Should().NotBeNull();
        validateResult!.Valid.Should().BeTrue();
    }

    [Fact]
    public async Task RegisterWithoutActivation_LoginShouldFail()
    {
        // Arrange
        string email = $"inactive{Guid.NewGuid():N}@test.com";
        string password = "Test123!";

        // Step 1: Register new user
        var registerRequest = new RegisterUserRequest
        {
            Email = email,
            Password = password
        };

        var registerResponse = await mClient.PostAsJsonAsync("/api/user/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 2: Try to login without activation
        var loginRequest = new LoginRequest
        {
            Email = email,
            Password = password
        };

        var loginResponse = await mClient.PostAsJsonAsync("/api/user/login", loginRequest);

        // Assert login failed because account is not activated
        loginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ActivateWithInvalidToken_ShouldFail()
    {
        // Arrange
        string email = $"user{Guid.NewGuid():N}@test.com";
        string password = "Test123!";

        // Step 1: Register new user
        var registerRequest = new RegisterUserRequest
        {
            Email = email,
            Password = password
        };

        await mClient.PostAsJsonAsync("/api/user/register", registerRequest);

        // Step 2: Try to activate with invalid token
        string invalidToken = "999999";
        var activateResponse = await mClient.GetAsync($"/activate-account?email={Uri.EscapeDataString(email)}&token={invalidToken}");

        // Assert activation failed (redirects to login with error)
        activateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 3: Try to login - should fail because not activated
        var loginRequest = new LoginRequest
        {
            Email = email,
            Password = password
        };

        var loginResponse = await mClient.PostAsJsonAsync("/api/user/login", loginRequest);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Password Reset Flow Tests

    [Fact]
    public async Task PasswordResetFlow_HappyPath_ShouldSucceed()
    {
        // Arrange - Use pre-seeded test user
        string email = "user@fourcdesign.com";
        string oldPassword = "test123";
        string newPassword = "NewPass123!";

        // Step 1: Request password reset
        var resetRequest = new RequestPasswordResetRequest
        {
            Email = email
        };

        var resetResponse = await mClient.PostAsJsonAsync("/api/user/request-password-reset", resetRequest);
        resetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 2: Extract reset token from email (with retry/wait for async processing)
        var emailMessage = await GetMostRecentEmailAsync(email);
        emailMessage.Should().NotBeNull("Password reset email should be sent");
        emailMessage!.Subject.Should().Contain("Reset", "Email subject should mention password reset");

        string? token = ExtractTokenFromEmail(emailMessage.Body);
        token.Should().NotBeNullOrEmpty("Token should be present in password reset email");

        // Step 3: Reset password using the token
        var resetPasswordRequest = new ResetPasswordRequest
        {
            Email = email,
            Token = token!,
            Password = newPassword
        };

        var resetPasswordResponse = await mClient.PostAsJsonAsync("/api/user/reset-password", resetPasswordRequest);
        resetPasswordResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var resetPasswordResult = await resetPasswordResponse.Content.ReadFromJsonAsync<ResetPasswordResponse>();
        resetPasswordResult.Should().NotBeNull();
        resetPasswordResult!.Success.Should().BeTrue();

        // Step 4: Verify old password no longer works
        var loginWithOldPassword = new LoginRequest
        {
            Email = email,
            Password = oldPassword
        };

        var oldPasswordResponse = await mClient.PostAsJsonAsync("/api/user/login", loginWithOldPassword);
        oldPasswordResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "Old password should not work");

        // Step 5: Verify new password works
        var loginWithNewPassword = new LoginRequest
        {
            Email = email,
            Password = newPassword
        };

        var newPasswordResponse = await mClient.PostAsJsonAsync("/api/user/login", loginWithNewPassword);
        newPasswordResponse.StatusCode.Should().Be(HttpStatusCode.OK, "New password should work");

        var loginResult = await newPasswordResponse.Content.ReadFromJsonAsync<LoginResponse>();
        loginResult.Should().NotBeNull();
        loginResult!.SessionId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task PasswordResetWithInvalidToken_ShouldFail()
    {
        // Arrange - Use pre-seeded test user
        string email = "user@fourcdesign.com";
        string newPassword = "NewPass123!";

        // Step 1: Request password reset
        var resetRequest = new RequestPasswordResetRequest
        {
            Email = email
        };

        await mClient.PostAsJsonAsync("/api/user/request-password-reset", resetRequest);
        await Task.Delay(100);

        // Step 2: Try to reset with invalid token
        var resetPasswordRequest = new ResetPasswordRequest
        {
            Email = email,
            Token = "999999",
            Password = newPassword
        };

        var resetPasswordResponse = await mClient.PostAsJsonAsync("/api/user/reset-password", resetPasswordRequest);

        // Assert reset failed
        resetPasswordResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var result = await resetPasswordResponse.Content.ReadFromJsonAsync<ResetPasswordResponse>();
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
    }

    [Fact]
    public async Task PasswordResetForInactiveUser_ShouldNotSendEmail()
    {
        // Arrange - Register and don't activate
        string email = $"inactive{Guid.NewGuid():N}@test.com";
        string password = "Test123!";

        var registerRequest = new RegisterUserRequest
        {
            Email = email,
            Password = password
        };

        await mClient.PostAsJsonAsync("/api/user/register", registerRequest);

        // Wait for activation email
        var existingEmail = await GetMostRecentEmailAsync(email);
        if (existingEmail != null)
        {
            // Note the creation time
            var lastEmailTime = existingEmail.Created;
            var lastEmailSubject = existingEmail.Subject;

            // Step 1: Request password reset for inactive user
            var resetRequest = new RequestPasswordResetRequest
            {
                Email = email
            };

            await mClient.PostAsJsonAsync("/api/user/request-password-reset", resetRequest);

            // Step 2: Wait and verify no new password reset email was sent
            // Give it time to potentially send (if bug exists)
            await Task.Delay(500);

            var newEmail = await GetMostRecentEmailAsync(email);
            if (newEmail != null)
            {
                // If there's an email, it should be the old activation email, not a reset email
                newEmail.Created.Should().Be(lastEmailTime, "No new email should be sent for inactive user");
                newEmail.Subject.Should().Be(lastEmailSubject, "Email should still be the activation email");
            }
        }
    }

    #endregion
}
