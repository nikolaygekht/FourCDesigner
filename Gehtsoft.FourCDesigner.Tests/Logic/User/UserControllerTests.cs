using AutoMapper;
using Gehtsoft.FourCDesigner.Logic.Config;
using Gehtsoft.FourCDesigner.Dao;
using Gehtsoft.FourCDesigner.Entities;
using Gehtsoft.FourCDesigner.Logic;
using Gehtsoft.FourCDesigner.Logic.Email;
using Gehtsoft.FourCDesigner.Logic.Token;
using Gehtsoft.FourCDesigner.Logic.User;
using Gehtsoft.FourCDesigner.Utils;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace Gehtsoft.FourCDesigner.Tests.Logic.User;

public class UserControllerTests
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

    public UserControllerTests()
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
        mMockMessages.Setup(m => m.UserAlreadyExists(It.IsAny<string>())).Returns((string email) => $"User with email '{email}' already exists");
        mMockMessages.Setup(m => m.UserNotFound(It.IsAny<string>())).Returns((string email) => $"User with email '{email}' not found");
        mMockMessages.Setup(m => m.ActivationEmailSubject).Returns("Activate Your Account");
        mMockMessages.Setup(m => m.ActivationEmailBodyWithLink(It.IsAny<string>(), It.IsAny<double>()))
            .Returns((string url, double exp) => $"Activation link: {url}");
        mMockMessages.Setup(m => m.PasswordResetEmailBodyWithLink(It.IsAny<string>(), It.IsAny<double>()))
            .Returns((string url, double exp) => $"Reset link: {url}");
        mMockTokenService.Setup(t => t.ExpirationInSeconds).Returns(300.0);
        mMockUrlBuilder.Setup(u => u.BuildUrl(It.IsAny<string>(), It.IsAny<object>()))
            .Returns((string path, object query) => $"https://example.com{path}");

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

    [Fact]
    public void Constructor_WithNullUserDao_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new UserController(null!, mMockHashProvider.Object, mMockPasswordValidator.Object, mMockTokenService.Object, mMockEmailService.Object, mMockUrlBuilder.Object, mMockMessages.Object, mMockMapper.Object, mMockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("userDao");
    }

    [Fact]
    public void Constructor_WithNullHashProvider_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new UserController(mMockUserDao.Object, null!, mMockPasswordValidator.Object, mMockTokenService.Object, mMockEmailService.Object, mMockUrlBuilder.Object, mMockMessages.Object, mMockMapper.Object, mMockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("hashProvider");
    }

    [Fact]
    public void Constructor_WithNullPasswordValidator_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new UserController(mMockUserDao.Object, mMockHashProvider.Object, null!, mMockTokenService.Object, mMockEmailService.Object, mMockUrlBuilder.Object, mMockMessages.Object, mMockMapper.Object, mMockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("passwordValidator");
    }

    [Fact]
    public void Constructor_WithNullMessages_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new UserController(mMockUserDao.Object, mMockHashProvider.Object, mMockPasswordValidator.Object, mMockTokenService.Object, mMockEmailService.Object, mMockUrlBuilder.Object, null!, mMockMapper.Object, mMockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("messages");
    }

    [Fact]
    public void Constructor_WithNullMapper_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new UserController(mMockUserDao.Object, mMockHashProvider.Object, mMockPasswordValidator.Object, mMockTokenService.Object, mMockEmailService.Object, mMockUrlBuilder.Object, mMockMessages.Object, null!, mMockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("mapper");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new UserController(mMockUserDao.Object, mMockHashProvider.Object, mMockPasswordValidator.Object, mMockTokenService.Object, mMockEmailService.Object, mMockUrlBuilder.Object, mMockMessages.Object, mMockMapper.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public async Task RegisterUser_WithValidData_ReturnsTrue()
    {
        // Arrange
        string email = "test@example.com";
        string password = "ValidPass123";
        mMockPasswordValidator.Setup(v => v.ValidatePassword(password, out It.Ref<List<string>>.IsAny)).Returns(true);
        mMockUserDao.Setup(d => d.IsEmailUsed(email, It.IsAny<int?>())).Returns(false);
        mMockHashProvider.Setup(h => h.ComputeHash(password)).Returns("hashedpassword");
        mMockTokenService.Setup(t => t.GenerateToken(email)).Returns("123456");
        mMockUserDao.Setup(d => d.SaveUser(It.IsAny<Entities.User>()))
            .Callback<Entities.User>(u => u.Id = 123);

        // Act
        bool result = await mController.RegisterUser(email, password);

        // Assert
        result.Should().BeTrue();
        mMockUserDao.Verify(d => d.SaveUser(It.Is<Entities.User>(u =>
            u.Email == email &&
            u.PasswordHash == "hashedpassword" &&
            u.ActiveUser == false &&
            u.Role == "user")), Times.Once);
        mMockTokenService.Verify(t => t.GenerateToken(email), Times.Once);
        mMockEmailService.Verify(e => e.SendEmailAndTriggerProcessorAsync(It.IsAny<Gehtsoft.FourCDesigner.Logic.Email.Model.EmailMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RegisterUser_WithInvalidEmail_ThrowsValidationException(string email)
    {
        // Act
        Func<Task> act = async () => await mController.RegisterUser(email, "ValidPass123");

        // Assert
        var exception = (await act.Should().ThrowAsync<ValidationException>()).Which;
        exception.Errors.Should().ContainSingle(e => e.Field == "email");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RegisterUser_WithInvalidPassword_ThrowsValidationException(string password)
    {
        // Act
        Func<Task> act = async () => await mController.RegisterUser("test@example.com", password);

        // Assert
        var exception = (await act.Should().ThrowAsync<ValidationException>()).Which;
        exception.Errors.Should().ContainSingle(e => e.Field == "password");
    }

    [Fact]
    public async Task RegisterUser_WithInvalidPasswordRules_ThrowsValidationException()
    {
        // Arrange
        string email = "test@example.com";
        string password = "weak";
        var errors = new List<string> { "Password too weak" };
        mMockPasswordValidator.Setup(v => v.ValidatePassword(password, out errors)).Returns(false);

        // Act
        Func<Task> act = async () => await mController.RegisterUser(email, password);

        // Assert
        var exception = (await act.Should().ThrowAsync<ValidationException>()).Which;
        exception.Errors.Should().ContainSingle(e => e.Field == "password");
        exception.Errors.First(e => e.Field == "password").Messages.Should().Contain("Password too weak");
    }

    [Fact]
    public async Task RegisterUser_WithExistingEmail_ThrowsValidationException()
    {
        // Arrange
        string email = "existing@example.com";
        string password = "ValidPass123";
        mMockPasswordValidator.Setup(v => v.ValidatePassword(password, out It.Ref<List<string>>.IsAny)).Returns(true);
        mMockUserDao.Setup(d => d.IsEmailUsed(email, It.IsAny<int?>())).Returns(true);

        // Act
        Func<Task> act = async () => await mController.RegisterUser(email, password);

        // Assert
        var exception = (await act.Should().ThrowAsync<ValidationException>()).Which;
        exception.Errors.Should().ContainSingle(e => e.Field == "email");
        exception.Errors.First(e => e.Field == "email").Messages.First().Should().Contain(email);
    }

    [Fact]
    public void ValidateUser_WithValidCredentials_ReturnsUserInfo()
    {
        // Arrange
        string email = "test@example.com";
        string password = "ValidPass123";
        var user = new Entities.User { Id = 1, Email = email, PasswordHash = "hash", Role = "user", ActiveUser = true };
        var userInfo = new UserInfo { Email = email, Role = "user", ActiveUser = true };

        mMockUserDao.Setup(d => d.GetUserByEmail(email)).Returns(user);
        mMockHashProvider.Setup(h => h.ValidatePassword(password, "hash")).Returns(true);
        mMockMapper.Setup(m => m.Map<UserInfo>(user)).Returns(userInfo);

        // Act
        UserInfo? result = mController.ValidateUser(email, password);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
    }

    [Fact]
    public void ValidateUser_WithNonExistingUser_ReturnsNull()
    {
        // Arrange
        mMockUserDao.Setup(d => d.GetUserByEmail("notfound@example.com")).Returns((Entities.User?)null);

        // Act
        UserInfo? result = mController.ValidateUser("notfound@example.com", "password");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ValidateUser_WithInvalidPassword_ReturnsNull()
    {
        // Arrange
        string email = "test@example.com";
        var user = new Entities.User { Id = 1, Email = email, PasswordHash = "hash" };
        mMockUserDao.Setup(d => d.GetUserByEmail(email)).Returns(user);
        mMockHashProvider.Setup(h => h.ValidatePassword("wrongpass", "hash")).Returns(false);

        // Act
        UserInfo? result = mController.ValidateUser(email, "wrongpass");

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData(null, "password")]
    [InlineData("", "password")]
    [InlineData("   ", "password")]
    [InlineData("email@example.com", null)]
    [InlineData("email@example.com", "")]
    [InlineData("email@example.com", "   ")]
    public void ValidateUser_WithEmptyCredentials_ReturnsNull(string email, string password)
    {
        // Act
        UserInfo? result = mController.ValidateUser(email, password);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void UpdateUser_WithValidData_ReturnsTrue()
    {
        // Arrange
        string currentEmail = "old@example.com";
        string newEmail = "updated@example.com";
        string password = "NewPass123";
        var user = new Entities.User { Id = 1, Email = currentEmail, PasswordHash = "oldhash" };

        mMockPasswordValidator.Setup(v => v.ValidatePassword(password, out It.Ref<List<string>>.IsAny)).Returns(true);
        mMockUserDao.Setup(d => d.GetUserByEmail(currentEmail)).Returns(user);
        mMockUserDao.Setup(d => d.IsEmailUsed(newEmail, 1)).Returns(false);
        mMockHashProvider.Setup(h => h.ComputeHash(password)).Returns("newhash");

        // Act
        bool result = mController.UpdateUser(currentEmail, newEmail, password);

        // Assert
        result.Should().BeTrue();
        mMockUserDao.Verify(d => d.SaveUser(It.Is<Entities.User>(u =>
            u.Id == 1 &&
            u.Email == newEmail &&
            u.PasswordHash == "newhash")), Times.Once);
    }

    [Fact]
    public void UpdateUser_WithDuplicateEmailForDifferentUser_ThrowsValidationException()
    {
        // Arrange
        string currentEmail = "current@example.com";
        string newEmail = "existing@example.com";
        string password = "ValidPass123";
        var user = new Entities.User { Id = 1, Email = currentEmail };

        mMockPasswordValidator.Setup(v => v.ValidatePassword(password, out It.Ref<List<string>>.IsAny)).Returns(true);
        mMockUserDao.Setup(d => d.GetUserByEmail(currentEmail)).Returns(user);
        mMockUserDao.Setup(d => d.IsEmailUsed(newEmail, 1)).Returns(true);

        // Act
        Action act = () => mController.UpdateUser(currentEmail, newEmail, password);

        // Assert
        var exception = act.Should().Throw<ValidationException>().Which;
        exception.Errors.Should().ContainSingle(e => e.Field == "email");
        exception.Errors.First(e => e.Field == "email").Messages.First().Should().Contain(newEmail);
    }

    [Fact]
    public void UpdateUser_WithSameEmailForSameUser_Succeeds()
    {
        // Arrange
        string email = "same@example.com";
        string password = "ValidPass123";
        var user = new Entities.User { Id = 1, Email = email, PasswordHash = "oldhash" };

        mMockPasswordValidator.Setup(v => v.ValidatePassword(password, out It.Ref<List<string>>.IsAny)).Returns(true);
        mMockUserDao.Setup(d => d.GetUserByEmail(email)).Returns(user);
        mMockUserDao.Setup(d => d.IsEmailUsed(email, 1)).Returns(false);
        mMockHashProvider.Setup(h => h.ComputeHash(password)).Returns("newhash");

        // Act
        bool result = mController.UpdateUser(email, email, password);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void UpdateUser_WithNonExistingUser_ThrowsValidationException()
    {
        // Arrange
        string currentEmail = "notfound@example.com";
        mMockPasswordValidator.Setup(v => v.ValidatePassword("pass", out It.Ref<List<string>>.IsAny)).Returns(true);
        mMockUserDao.Setup(d => d.GetUserByEmail(currentEmail)).Returns((Entities.User?)null);

        // Act
        Action act = () => mController.UpdateUser(currentEmail, "newemail@example.com", "pass");

        // Assert
        var exception = act.Should().Throw<ValidationException>().Which;
        exception.Errors.Should().ContainSingle(e => e.Field == "email");
        exception.Errors.First(e => e.Field == "email").Messages.First().Should().Contain(currentEmail);
    }

    [Fact]
    public void ActivateUser_WithExistingUser_ReturnsTrue()
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

    [Fact]
    public void ActivateUser_WithNonExistingUser_ThrowsValidationException()
    {
        // Arrange
        string email = "notfound@example.com";
        string token = "123456";
        mMockTokenService.Setup(t => t.ValidateToken(token, email, true)).Returns(true);
        mMockUserDao.Setup(d => d.ActivateUserByEmail(email)).Returns(false);

        // Act
        Action act = () => mController.ActivateUser(email, token);

        // Assert
        var exception = act.Should().Throw<ValidationException>().Which;
        exception.Errors.Should().ContainSingle(e => e.Field == "email");
        exception.Errors.First(e => e.Field == "email").Messages.First().Should().Contain(email);
    }

    [Fact]
    public void DeactivateUser_WithExistingUser_ReturnsTrue()
    {
        // Arrange
        string email = "test@example.com";
        mMockUserDao.Setup(d => d.DeactivateUserByEmail(email)).Returns(true);

        // Act
        bool result = mController.DeactivateUser(email);

        // Assert
        result.Should().BeTrue();
        mMockUserDao.Verify(d => d.DeactivateUserByEmail(email), Times.Once);
    }

    [Fact]
    public void DeactivateUser_WithNonExistingUser_ThrowsValidationException()
    {
        // Arrange
        string email = "notfound@example.com";
        mMockUserDao.Setup(d => d.DeactivateUserByEmail(email)).Returns(false);

        // Act
        Action act = () => mController.DeactivateUser(email);

        // Assert
        var exception = act.Should().Throw<ValidationException>().Which;
        exception.Errors.Should().ContainSingle(e => e.Field == "email");
        exception.Errors.First(e => e.Field == "email").Messages.First().Should().Contain(email);
    }

    [Fact]
    public void ChangePassword_WithValidData_ReturnsTrue()
    {
        // Arrange
        string email = "test@example.com";
        string newPassword = "NewPass123";

        mMockPasswordValidator.Setup(v => v.ValidatePassword(newPassword, out It.Ref<List<string>>.IsAny)).Returns(true);
        mMockHashProvider.Setup(h => h.ComputeHash(newPassword)).Returns("newhash");
        mMockUserDao.Setup(d => d.UpdatePasswordByEmail(email, "newhash")).Returns(true);

        // Act
        bool result = mController.ChangePassword(email, newPassword);

        // Assert
        result.Should().BeTrue();
        mMockUserDao.Verify(d => d.UpdatePasswordByEmail(email, "newhash"), Times.Once);
    }

    [Fact]
    public void ChangePassword_WithInvalidPassword_ThrowsValidationException()
    {
        // Arrange
        var errors = new List<string> { "Password too weak" };
        mMockPasswordValidator.Setup(v => v.ValidatePassword("weak", out errors)).Returns(false);

        // Act
        Action act = () => mController.ChangePassword("test@example.com", "weak");

        // Assert
        var exception = act.Should().Throw<ValidationException>().Which;
        exception.Errors.Should().ContainSingle(e => e.Field == "password");
        exception.Errors.First(e => e.Field == "password").Messages.Should().Contain("Password too weak");
    }

    [Fact]
    public void ChangePassword_WithNonExistingUser_ThrowsValidationException()
    {
        // Arrange
        string email = "notfound@example.com";
        mMockPasswordValidator.Setup(v => v.ValidatePassword("pass", out It.Ref<List<string>>.IsAny)).Returns(true);
        mMockHashProvider.Setup(h => h.ComputeHash("pass")).Returns("newhash");
        mMockUserDao.Setup(d => d.UpdatePasswordByEmail(email, "newhash")).Returns(false);

        // Act
        Action act = () => mController.ChangePassword(email, "pass");

        // Assert
        var exception = act.Should().Throw<ValidationException>().Which;
        exception.Errors.Should().ContainSingle(e => e.Field == "email");
        exception.Errors.First(e => e.Field == "email").Messages.First().Should().Contain(email);
    }
}
