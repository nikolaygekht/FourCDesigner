using Gehtsoft.FourCDesigner.Logic.Session;
using Gehtsoft.FourCDesigner.Logic.User;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;
using FluentAssertions;

namespace Gehtsoft.FourCDesigner.Tests.Logic.Session;

public class SessionControllerTests : IDisposable
{
    private readonly Mock<ISessionSettings> mMockSettings;
    private readonly Mock<IUserController> mMockUserController;
    private readonly SessionController mController;

    public SessionControllerTests()
    {
        mMockSettings = new Mock<ISessionSettings>();
        mMockUserController = new Mock<IUserController>();

        // Default timeout: 600 seconds
        mMockSettings.Setup(s => s.SessionTimeoutInSeconds).Returns(600);

        mController = new SessionController(
            mMockSettings.Object,
            mMockUserController.Object);
    }

    public void Dispose()
    {
        mController?.Dispose();
    }

    [Fact]
    public void Constructor_WithNullSettings_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new SessionController(null!, mMockUserController.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("settings");
    }

    [Fact]
    public void Constructor_WithNullUserController_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new SessionController(mMockSettings.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("userController");
    }

    [Fact]
    public void Authorize_WithValidCredentials_ReturnsTrue()
    {
        // Arrange
        var userInfo = new UserInfo
        {
            Email = "test@example.com",
            Role = "user",
            ActiveUser = true
        };

        mMockUserController
            .Setup(u => u.ValidateUser("test@example.com", "password123"))
            .Returns(userInfo);

        // Act
        bool result = mController.Authorize("test@example.com", "password123", out string sessionId);

        // Assert
        result.Should().BeTrue();
        sessionId.Should().NotBeNullOrEmpty();
        sessionId.Should().HaveLength(44); // SHA256 hash as base64 string is 44 characters
    }

    [Fact]
    public void Authorize_WithInvalidCredentials_ReturnsFalse()
    {
        // Arrange
        mMockUserController
            .Setup(u => u.ValidateUser("test@example.com", "wrongpassword"))
            .Returns((UserInfo?)null);

        // Act
        bool result = mController.Authorize("test@example.com", "wrongpassword", out string sessionId);

        // Assert
        result.Should().BeFalse();
        sessionId.Should().BeEmpty();
    }

    [Fact]
    public void Authorize_WithInactiveUser_ReturnsFalse()
    {
        // Arrange
        var userInfo = new UserInfo
        {
            Email = "test@example.com",
            Role = "user",
            ActiveUser = false
        };

        mMockUserController
            .Setup(u => u.ValidateUser("test@example.com", "password123"))
            .Returns(userInfo);

        // Act
        bool result = mController.Authorize("test@example.com", "password123", out string sessionId);

        // Assert
        result.Should().BeFalse();
        sessionId.Should().BeEmpty();
    }

    [Fact]
    public void Authorize_WithEmptyEmail_ReturnsFalse()
    {
        // Act
        bool result = mController.Authorize("", "password123", out string sessionId);

        // Assert
        result.Should().BeFalse();
        sessionId.Should().BeEmpty();
    }

    [Fact]
    public void Authorize_WithEmptyPassword_ReturnsFalse()
    {
        // Act
        bool result = mController.Authorize("test@example.com", "", out string sessionId);

        // Assert
        result.Should().BeFalse();
        sessionId.Should().BeEmpty();
    }

    [Fact]
    public void CheckSession_WithValidSession_ReturnsTrue()
    {
        // Arrange
        var userInfo = new UserInfo
        {
            Email = "test@example.com",
            Role = "admin",
            ActiveUser = true
        };

        mMockUserController
            .Setup(u => u.ValidateUser("test@example.com", "password123"))
            .Returns(userInfo);

        mController.Authorize("test@example.com", "password123", out string sessionId);

        // Act
        bool result = mController.CheckSession(sessionId, out string email, out string role);

        // Assert
        result.Should().BeTrue();
        email.Should().Be("test@example.com");
        role.Should().Be("admin");
    }

    [Fact]
    public void CheckSession_WithInvalidSession_ReturnsFalse()
    {
        // Act
        bool result = mController.CheckSession("invalid-session-id", out string email, out string role);

        // Assert
        result.Should().BeFalse();
        email.Should().BeEmpty();
        role.Should().BeEmpty();
    }

    [Fact]
    public void CheckSession_WithEmptySessionId_ReturnsFalse()
    {
        // Act
        bool result = mController.CheckSession("", out string email, out string role);

        // Assert
        result.Should().BeFalse();
        email.Should().BeEmpty();
        role.Should().BeEmpty();
    }

    [Fact]
    public void CloseSession_WithValidSession_RemovesSession()
    {
        // Arrange
        var userInfo = new UserInfo
        {
            Email = "test@example.com",
            Role = "user",
            ActiveUser = true
        };

        mMockUserController
            .Setup(u => u.ValidateUser("test@example.com", "password123"))
            .Returns(userInfo);

        mController.Authorize("test@example.com", "password123", out string sessionId);

        // Act
        mController.CloseSession(sessionId);

        // Assert
        bool result = mController.CheckSession(sessionId, out string email, out string role);
        result.Should().BeFalse();
    }

    [Fact]
    public void CloseSession_WithInvalidSession_DoesNotThrow()
    {
        // Act
        Action act = () => mController.CloseSession("invalid-session-id");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void CloseSession_WithEmptySessionId_DoesNotThrow()
    {
        // Act
        Action act = () => mController.CloseSession("");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Authorize_GeneratesUniqueSessionIds()
    {
        // Arrange
        var userInfo = new UserInfo
        {
            Email = "test@example.com",
            Role = "user",
            ActiveUser = true
        };

        mMockUserController
            .Setup(u => u.ValidateUser("test@example.com", "password123"))
            .Returns(userInfo);

        // Act
        mController.Authorize("test@example.com", "password123", out string sessionId1);
        mController.Authorize("test@example.com", "password123", out string sessionId2);

        // Assert
        sessionId1.Should().NotBe(sessionId2);
    }

    [Fact]
    public async Task CheckSession_ExtendsSessionLifetime()
    {
        // Arrange
        var mockSettings = new Mock<ISessionSettings>();
        mockSettings.Setup(s => s.SessionTimeoutInSeconds).Returns(0.05); // 50ms timeout

        using var controller = new SessionController(mockSettings.Object, mMockUserController.Object);

        var userInfo = new UserInfo
        {
            Email = "test@example.com",
            Role = "user",
            ActiveUser = true
        };

        mMockUserController
            .Setup(u => u.ValidateUser("test@example.com", "password123"))
            .Returns(userInfo);

        controller.Authorize("test@example.com", "password123", out string sessionId);

        // Act - Check session before expiry, wait, check again
        await Task.Delay(30); // Wait 30ms (less than 50ms timeout)
        bool firstCheck = controller.CheckSession(sessionId, out _, out _);

        await Task.Delay(30); // Wait another 30ms (total 60ms, but sliding expiration was reset)
        bool secondCheck = controller.CheckSession(sessionId, out _, out _);

        // Assert
        firstCheck.Should().BeTrue("session should still be valid after 30ms");
        secondCheck.Should().BeTrue("session should still be valid because it was extended by the first check");
    }

    [Fact]
    public async Task Session_ExpiresAfterTimeout()
    {
        // Arrange
        var mockSettings = new Mock<ISessionSettings>();
        mockSettings.Setup(s => s.SessionTimeoutInSeconds).Returns(0.05); // 50ms timeout

        using var controller = new SessionController(mockSettings.Object, mMockUserController.Object);

        var userInfo = new UserInfo
        {
            Email = "test@example.com",
            Role = "user",
            ActiveUser = true
        };

        mMockUserController
            .Setup(u => u.ValidateUser("test@example.com", "password123"))
            .Returns(userInfo);

        controller.Authorize("test@example.com", "password123", out string sessionId);

        // Act - Wait for session to expire
        await Task.Delay(100); // Wait 100ms (more than 50ms timeout)
        bool result = controller.CheckSession(sessionId, out string email, out string role);

        // Assert
        result.Should().BeFalse("session should have expired");
        email.Should().BeEmpty();
        role.Should().BeEmpty();
    }
}
