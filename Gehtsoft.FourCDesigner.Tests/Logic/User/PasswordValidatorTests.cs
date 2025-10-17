using Gehtsoft.FourCDesigner.Logic.User;
using Gehtsoft.FourCDesigner.Utils;
using Moq;
using Xunit;
using FluentAssertions;

namespace Gehtsoft.FourCDesigner.Tests.Logic.User;

public class PasswordValidatorTests
{
    private readonly Mock<IUserConfiguration> mMockUserConfig;
    private readonly Mock<IPasswordRules> mMockRules;
    private readonly Mock<IMessages> mMockMessages;

    public PasswordValidatorTests()
    {
        mMockRules = new Mock<IPasswordRules>();
        mMockUserConfig = new Mock<IUserConfiguration>();
        mMockUserConfig.Setup(c => c.PasswordRules).Returns(mMockRules.Object);

        mMockMessages = new Mock<IMessages>();
        mMockMessages.Setup(m => m.PasswordCannotBeNull).Returns("Password cannot be null");
        mMockMessages.Setup(m => m.PasswordTooShort(It.IsAny<int>())).Returns((int len) => $"Password must be at least {len} characters long");
        mMockMessages.Setup(m => m.PasswordMustContainCapitalLetter).Returns("Password must contain at least one capital letter");
        mMockMessages.Setup(m => m.PasswordMustContainLowercaseLetter).Returns("Password must contain at least one lowercase letter");
        mMockMessages.Setup(m => m.PasswordMustContainDigit).Returns("Password must contain at least one digit");
        mMockMessages.Setup(m => m.PasswordMustContainSpecialSymbol).Returns("Password must contain at least one special symbol");
    }

    [Fact]
    public void Constructor_WithNullUserConfiguration_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new PasswordValidator(null!, mMockMessages.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("userConfiguration");
    }

    [Fact]
    public void Constructor_WithNullMessages_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new PasswordValidator(mMockUserConfig.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("messages");
    }

    [Fact]
    public void ValidatePassword_WithNullPassword_ReturnsFalseWithError()
    {
        // Arrange
        SetupRules(8, true, true, true, false);
        var validator = new PasswordValidator(mMockUserConfig.Object, mMockMessages.Object);

        // Act
        bool result = validator.ValidatePassword(null!, out List<string> errors);

        // Assert
        result.Should().BeFalse();
        errors.Should().ContainSingle()
            .Which.Should().Be("Password cannot be null");
    }

    [Theory]
    [InlineData(8, true, true, true, false, "Abcd1234", true)]           // Valid password with default rules
    [InlineData(8, true, true, true, false, "Abc1", false)]                // Too short
    [InlineData(8, true, true, true, false, "abcd1234", false)]            // No capital letter
    [InlineData(8, true, true, true, false, "ABCD1234", false)]            // No lowercase letter
    [InlineData(8, true, true, true, false, "Abcdefgh", false)]            // No digit
    [InlineData(8, true, true, true, true, "Abcd1234", false)]             // No special symbol when required
    [InlineData(8, true, true, true, true, "Abcd1234!", true)]             // Valid with special symbol
    [InlineData(4, false, false, false, false, "test", true)]              // Relaxed rules
    [InlineData(4, false, false, false, false, "abc", false)]              // Too short even with relaxed rules
    [InlineData(8, false, false, false, false, "abcdefgh", true)]          // Only length required
    [InlineData(10, true, false, true, false, "ABCD123456", true)]         // No lowercase required
    [InlineData(10, false, true, true, false, "abcd123456", true)]         // No uppercase required
    [InlineData(10, true, true, false, false, "Abcdefghij", true)]         // No digit required
    [InlineData(12, true, true, true, true, "Abcd1234!@#$", true)]         // All requirements with multiple special chars
    [InlineData(6, true, true, true, false, "Abc123", true)]               // Exact minimum length
    [InlineData(8, true, true, true, false, "abc", false)]                 // Multiple violations
    public void ValidatePassword_WithVariousRulesAndPasswords_ReturnsExpectedResult(
        int minLength,
        bool requireCapital,
        bool requireSmall,
        bool requireDigit,
        bool requireSpecial,
        string password,
        bool expectedValid)
    {
        // Arrange
        SetupRules(minLength, requireCapital, requireSmall, requireDigit, requireSpecial);
        var validator = new PasswordValidator(mMockUserConfig.Object, mMockMessages.Object);

        // Act
        bool result = validator.ValidatePassword(password, out List<string> errors);

        // Assert
        result.Should().Be(expectedValid);
        if (expectedValid)
            errors.Should().BeEmpty();
        else
            errors.Should().NotBeEmpty();
    }

    [Fact]
    public void ValidatePassword_WithMultipleViolations_ReturnsAllErrors()
    {
        // Arrange
        SetupRules(8, true, true, true, false);
        var validator = new PasswordValidator(mMockUserConfig.Object, mMockMessages.Object);

        // Act
        bool result = validator.ValidatePassword("abc", out List<string> errors);

        // Assert
        result.Should().BeFalse();
        errors.Should().HaveCount(3);
        errors.Should().Contain("Password must be at least 8 characters long");
        errors.Should().Contain("Password must contain at least one capital letter");
        errors.Should().Contain("Password must contain at least one digit");
    }

    [Fact]
    public void ValidatePassword_SimpleOverload_WithValidPassword_ReturnsTrue()
    {
        // Arrange
        SetupRules(8, true, true, true, false);
        var validator = new PasswordValidator(mMockUserConfig.Object, mMockMessages.Object);

        // Act
        bool result = validator.ValidatePassword("Abcd1234");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidatePassword_SimpleOverload_WithInvalidPassword_ReturnsFalse()
    {
        // Arrange
        SetupRules(8, true, true, true, false);
        var validator = new PasswordValidator(mMockUserConfig.Object, mMockMessages.Object);

        // Act
        bool result = validator.ValidatePassword("abc");

        // Assert
        result.Should().BeFalse();
    }

    private void SetupRules(int minLength, bool requireCapital, bool requireSmall, bool requireDigit, bool requireSpecial)
    {
        mMockRules.Setup(r => r.MinimumLength).Returns(minLength);
        mMockRules.Setup(r => r.RequireCapitalLetter).Returns(requireCapital);
        mMockRules.Setup(r => r.RequireSmallLetter).Returns(requireSmall);
        mMockRules.Setup(r => r.RequireDigit).Returns(requireDigit);
        mMockRules.Setup(r => r.RequireSpecialSymbol).Returns(requireSpecial);
    }
}
