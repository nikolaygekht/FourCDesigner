using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Gehtsoft.FourCDesigner.Controllers.Data;
using Microsoft.Data.Sqlite;

namespace Gehtsoft.FourCDesigner.ApiTests;

/// <summary>
/// API tests for user authentication and session management endpoints.
/// </summary>
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
}
