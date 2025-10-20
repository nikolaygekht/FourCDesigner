using AutoMapper;
using FluentAssertions;
using Gehtsoft.FourCDesigner.Logic.Config;
using Gehtsoft.FourCDesigner.Dao;
using Gehtsoft.FourCDesigner.Entities;
using Gehtsoft.FourCDesigner.Logic.Email;
using Gehtsoft.FourCDesigner.Logic.Email.Model;
using Gehtsoft.FourCDesigner.Logic.Token;
using Gehtsoft.FourCDesigner.Logic.User;
using Gehtsoft.FourCDesigner.Utils;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Gehtsoft.FourCDesigner.Tests.Logic.User;

public class UserControllerTokenTests
{
    private readonly Mock<IUserDao> mMockUserDao;
    private readonly Mock<IHashProvider> mMockHashProvider;
    private readonly Mock<IPasswordValidator> mMockPasswordValidator;
    private readonly Mock<ITokenService> mMockTokenService;
    private readonly Mock<IEmailService> mMockEmailService;
    private readonly Mock<IUrlBuilder> mMockUrlBuilder;
    private readonly Mock<IMessages> mMockMessages;
    private readonly Mock<IMapper> mMockMapper;
    private readonly Mock<ILogger<UserController>> mMockLogger;
    private readonly UserController mController;

    public UserControllerTokenTests()
    {
        mMockUserDao = new Mock<IUserDao>();
        mMockHashProvider = new Mock<IHashProvider>();
        mMockPasswordValidator = new Mock<IPasswordValidator>();
        mMockTokenService = new Mock<ITokenService>();
        mMockEmailService = new Mock<IEmailService>();
        mMockUrlBuilder = new Mock<IUrlBuilder>();
        mMockMessages = new Mock<IMessages>();
        mMockMapper = new Mock<IMapper>();
        mMockLogger = new Mock<ILogger<UserController>>();

        // Setup default message responses
        mMockMessages.Setup(m => m.EmailCannotBeEmpty).Returns("Email cannot be empty");
        mMockMessages.Setup(m => m.PasswordCannotBeEmpty).Returns("Password cannot be empty");
        mMockMessages.Setup(m => m.UserNotFound(It.IsAny<string>())).Returns((string email) => $"User with email '{email}' not found");
        mMockMessages.Setup(m => m.ActivationEmailSubject).Returns("Activate Your Account");
        mMockMessages.Setup(m => m.ActivationEmailBodyWithLink(It.IsAny<string>(), It.IsAny<double>()))
            .Returns((string url, double exp) => $"Activation link: {url}");
        mMockMessages.Setup(m => m.PasswordResetEmailSubject).Returns("Password Reset");
        mMockMessages.Setup(m => m.PasswordResetEmailBodyWithLink(It.IsAny<string>(), It.IsAny<double>()))
            .Returns((string url, double exp) => $"Reset link: {url}");
        mMockUrlBuilder.Setup(u => u.BuildUrl(It.IsAny<string>(), It.IsAny<object>()))
            .Returns((string path, object query) => $"https://example.com{path}");

        // Setup token service defaults
        mMockTokenService.Setup(t => t.ExpirationInSeconds).Returns(300.0);

        mController = new UserController(
            mMockUserDao.Object,
            mMockHashProvider.Object,
            mMockPasswordValidator.Object,
            mMockTokenService.Object,
            mMockEmailService.Object,
            mMockUrlBuilder.Object,
            mMockMessages.Object,
            mMockMapper.Object,
            mMockLogger.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullTokenService_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new UserController(
            mMockUserDao.Object,
            mMockHashProvider.Object,
            mMockPasswordValidator.Object,
            null!,
            mMockEmailService.Object,
            mMockUrlBuilder.Object,
            mMockMessages.Object,
            mMockMapper.Object,
            mMockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("tokenService");
    }

    [Fact]
    public void Constructor_WithNullEmailService_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new UserController(
            mMockUserDao.Object,
            mMockHashProvider.Object,
            mMockPasswordValidator.Object,
            mMockTokenService.Object,
            null!,
            mMockUrlBuilder.Object,
            mMockMessages.Object,
            mMockMapper.Object,
            mMockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("emailService");
    }

    #endregion

    #region RegisterUser Tests

    [Fact]
    public async Task RegisterUser_WithValidCredentials_CreatesInactiveUserAndSendsEmail()
    {
        // Arrange
        string email = "test@example.com";
        string password = "ValidPass123";
        string token = "123456";

        mMockPasswordValidator.Setup(v => v.ValidatePassword(password, out It.Ref<List<string>>.IsAny))
            .Returns(true);
        mMockUserDao.Setup(d => d.IsEmailUsed(email, null)).Returns(false);
        mMockHashProvider.Setup(h => h.ComputeHash(password)).Returns("hashedPassword");
        mMockTokenService.Setup(t => t.GenerateToken(email)).Returns(token);

        // Act
        bool result = await mController.RegisterUser(email, password);

        // Assert
        result.Should().BeTrue();
        mMockUserDao.Verify(d => d.SaveUser(It.Is<Entities.User>(u =>
            u.Email == email &&
            u.ActiveUser == false &&
            u.Role == "user" &&
            u.PasswordHash == "hashedPassword")), Times.Once);
        mMockTokenService.Verify(t => t.GenerateToken(email), Times.Once);
        mMockEmailService.Verify(e => e.SendEmailAndTriggerProcessorAsync(It.Is<EmailMessage>(m =>
            m.To[0] == email &&
            m.Priority == true), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region ActivateUser Tests

    [Theory]
    [InlineData(null, "123456")]
    [InlineData("", "123456")]
    [InlineData("   ", "123456")]
    [InlineData("test@example.com", null)]
    [InlineData("test@example.com", "")]
    [InlineData("test@example.com", "   ")]
    public void ActivateUser_WithInvalidParameters_ReturnsFalse(string email, string token)
    {
        // Act
        bool result = mController.ActivateUser(email, token);

        // Assert
        result.Should().BeFalse();
        mMockTokenService.Verify(t => t.ValidateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public void ActivateUser_WithInvalidToken_ReturnsFalse()
    {
        // Arrange
        string email = "test@example.com";
        string token = "123456";
        mMockTokenService.Setup(t => t.ValidateToken(token, email, true)).Returns(false);

        // Act
        bool result = mController.ActivateUser(email, token);

        // Assert
        result.Should().BeFalse();
        mMockUserDao.Verify(d => d.ActivateUserByEmail(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void ActivateUser_WithValidToken_ActivatesUserAndRemovesToken()
    {
        // Arrange
        string email = "test@example.com";
        string token = "123456";
        mMockTokenService.Setup(t => t.ValidateToken(token, email, true)).Returns(true);
        mMockUserDao.Setup(d => d.ActivateUserByEmail(email)).Returns(true);

        // Act
        bool result = mController.ActivateUser(email, token);

        // Assert
        result.Should().BeTrue();
        mMockTokenService.Verify(t => t.ValidateToken(token, email, true), Times.Once);
        mMockUserDao.Verify(d => d.ActivateUserByEmail(email), Times.Once);
    }

    #endregion

    #region RequestPasswordReset Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RequestPasswordReset_WithInvalidEmail_DoesNothing(string email)
    {
        // Act
        await mController.RequestPasswordReset(email);

        // Assert
        mMockUserDao.Verify(d => d.GetUserByEmail(It.IsAny<string>()), Times.Never);
        mMockTokenService.Verify(t => t.GenerateToken(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RequestPasswordReset_WithNonExistentUser_DoesNotSendEmail()
    {
        // Arrange
        string email = "test@example.com";
        mMockUserDao.Setup(d => d.GetUserByEmail(email)).Returns((Entities.User)null!);

        // Act
        await mController.RequestPasswordReset(email);

        // Assert
        mMockTokenService.Verify(t => t.GenerateToken(It.IsAny<string>()), Times.Never);
        mMockEmailService.Verify(e => e.SendEmailAndTriggerProcessorAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RequestPasswordReset_WithInactiveUser_DoesNotSendEmail()
    {
        // Arrange
        string email = "test@example.com";
        var user = new Entities.User { Email = email, ActiveUser = false };
        mMockUserDao.Setup(d => d.GetUserByEmail(email)).Returns(user);

        // Act
        await mController.RequestPasswordReset(email);

        // Assert
        mMockTokenService.Verify(t => t.GenerateToken(It.IsAny<string>()), Times.Never);
        mMockEmailService.Verify(e => e.SendEmailAndTriggerProcessorAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RequestPasswordReset_WithActiveUser_GeneratesTokenAndSendsEmail()
    {
        // Arrange
        string email = "test@example.com";
        string token = "123456";
        var user = new Entities.User { Email = email, ActiveUser = true };
        mMockUserDao.Setup(d => d.GetUserByEmail(email)).Returns(user);
        mMockTokenService.Setup(t => t.GenerateToken(email)).Returns(token);

        // Act
        await mController.RequestPasswordReset(email);

        // Assert
        mMockTokenService.Verify(t => t.GenerateToken(email), Times.Once);
        mMockEmailService.Verify(e => e.SendEmailAndTriggerProcessorAsync(It.Is<EmailMessage>(m =>
            m.To[0] == email &&
            m.Priority == true), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region ValidateToken Tests

    [Theory]
    [InlineData(null, "123456")]
    [InlineData("", "123456")]
    [InlineData("   ", "123456")]
    [InlineData("test@example.com", null)]
    [InlineData("test@example.com", "")]
    [InlineData("test@example.com", "   ")]
    public void ValidateToken_WithInvalidParameters_ReturnsFalse(string email, string token)
    {
        // Act
        bool result = mController.ValidateToken(email, token);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateToken_WithValidToken_ReturnsTrue()
    {
        // Arrange
        string email = "test@example.com";
        string token = "123456";
        mMockTokenService.Setup(t => t.ValidateToken(token, email, false)).Returns(true);

        // Act
        bool result = mController.ValidateToken(email, token);

        // Assert
        result.Should().BeTrue();
        mMockTokenService.Verify(t => t.ValidateToken(token, email, false), Times.Once);
    }

    [Fact]
    public void ValidateToken_DoesNotRemoveToken()
    {
        // Arrange
        string email = "test@example.com";
        string token = "123456";
        mMockTokenService.Setup(t => t.ValidateToken(token, email, false)).Returns(true);

        // Act
        mController.ValidateToken(email, token);

        // Assert - Verify that remove parameter is false
        mMockTokenService.Verify(t => t.ValidateToken(token, email, false), Times.Once);
        mMockTokenService.Verify(t => t.ValidateToken(token, email, true), Times.Never);
    }

    #endregion

    #region ResetPassword Tests

    [Theory]
    [InlineData(null, "123456", "NewPass123")]
    [InlineData("", "123456", "NewPass123")]
    [InlineData("   ", "123456", "NewPass123")]
    [InlineData("test@example.com", null, "NewPass123")]
    [InlineData("test@example.com", "", "NewPass123")]
    [InlineData("test@example.com", "   ", "NewPass123")]
    public void ResetPassword_WithInvalidEmailOrToken_ReturnsFalse(string email, string token, string newPassword)
    {
        // Act
        bool result = mController.ResetPassword(email, token, newPassword);

        // Assert
        result.Should().BeFalse();
        mMockTokenService.Verify(t => t.ValidateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public void ResetPassword_WithInvalidToken_ReturnsFalse()
    {
        // Arrange
        string email = "test@example.com";
        string token = "123456";
        string newPassword = "NewPass123";
        mMockPasswordValidator.Setup(v => v.ValidatePassword(newPassword, out It.Ref<List<string>>.IsAny))
            .Returns(true);
        mMockTokenService.Setup(t => t.ValidateToken(token, email, true)).Returns(false);

        // Act
        bool result = mController.ResetPassword(email, token, newPassword);

        // Assert
        result.Should().BeFalse();
        mMockUserDao.Verify(d => d.UpdatePasswordByEmail(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void ResetPassword_WithValidToken_ResetsPasswordAndRemovesToken()
    {
        // Arrange
        string email = "test@example.com";
        string token = "123456";
        string newPassword = "NewPass123";
        mMockPasswordValidator.Setup(v => v.ValidatePassword(newPassword, out It.Ref<List<string>>.IsAny))
            .Returns(true);
        mMockTokenService.Setup(t => t.ValidateToken(token, email, true)).Returns(true);
        mMockHashProvider.Setup(h => h.ComputeHash(newPassword)).Returns("newHashedPassword");
        mMockUserDao.Setup(d => d.UpdatePasswordByEmail(email, "newHashedPassword")).Returns(true);

        // Act
        bool result = mController.ResetPassword(email, token, newPassword);

        // Assert
        result.Should().BeTrue();
        mMockTokenService.Verify(t => t.ValidateToken(token, email, true), Times.Once);
        mMockHashProvider.Verify(h => h.ComputeHash(newPassword), Times.Once);
        mMockUserDao.Verify(d => d.UpdatePasswordByEmail(email, "newHashedPassword"), Times.Once);
    }

    [Fact]
    public void ResetPassword_WhenUserNotFound_ReturnsFalse()
    {
        // Arrange
        string email = "test@example.com";
        string token = "123456";
        string newPassword = "NewPass123";
        mMockPasswordValidator.Setup(v => v.ValidatePassword(newPassword, out It.Ref<List<string>>.IsAny))
            .Returns(true);
        mMockTokenService.Setup(t => t.ValidateToken(token, email, true)).Returns(true);
        mMockHashProvider.Setup(h => h.ComputeHash(newPassword)).Returns("newHashedPassword");
        mMockUserDao.Setup(d => d.UpdatePasswordByEmail(email, "newHashedPassword")).Returns(false);

        // Act
        bool result = mController.ResetPassword(email, token, newPassword);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
