using FluentAssertions;
using Gehtsoft.FourCDesigner.Logic.Token;
using Moq;
using Xunit;

namespace Gehtsoft.FourCDesigner.Tests.Logic.Token;

public class TokenServiceTests
{
    private readonly Mock<ITokenServiceConfiguration> mMockConfiguration;
    private readonly TokenService mTokenService;

    public TokenServiceTests()
    {
        mMockConfiguration = new Mock<ITokenServiceConfiguration>();
        mMockConfiguration.Setup(c => c.ExpirationInSeconds).Returns(300.0); // 5 minutes
        mTokenService = new TokenService(mMockConfiguration.Object);
    }

    [Fact]
    public void Constructor_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new TokenService(null);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("configuration");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GenerateToken_WithInvalidEmail_ShouldThrowArgumentNullException(string email)
    {
        // Act
        Action act = () => mTokenService.GenerateToken(email);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("email");
    }

    [Fact]
    public void GenerateToken_WithValidEmail_ShouldReturnSixDigitToken()
    {
        // Arrange
        string email = "test@example.com";

        // Act
        string token = mTokenService.GenerateToken(email);

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Length.Should().Be(6);
        int.TryParse(token, out int tokenNumber).Should().BeTrue();
        tokenNumber.Should().BeInRange(100000, 999999);
    }

    [Fact]
    public void GenerateToken_CalledMultipleTimes_ShouldReturnDifferentTokens()
    {
        // Arrange
        string email = "test@example.com";

        // Act
        string token1 = mTokenService.GenerateToken(email);
        string token2 = mTokenService.GenerateToken(email);
        string token3 = mTokenService.GenerateToken(email);

        // Assert
        // While theoretically possible to get duplicates, it's extremely unlikely
        var tokens = new[] { token1, token2, token3 };
        tokens.Distinct().Count().Should().BeGreaterThan(1);
    }

    [Fact]
    public void ValidateToken_WithValidTokenAndEmail_ShouldReturnTrue()
    {
        // Arrange
        string email = "test@example.com";
        string token = mTokenService.GenerateToken(email);

        // Act
        bool isValid = mTokenService.ValidateToken(token, email, false);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateToken_WithWrongToken_ShouldReturnFalse()
    {
        // Arrange
        string email = "test@example.com";
        mTokenService.GenerateToken(email);

        // Act
        bool isValid = mTokenService.ValidateToken("000000", email, false);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void ValidateToken_WithWrongEmail_ShouldReturnFalse()
    {
        // Arrange
        string email = "test@example.com";
        string token = mTokenService.GenerateToken(email);

        // Act
        bool isValid = mTokenService.ValidateToken(token, "wrong@example.com", false);

        // Assert
        isValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(null, "123456")]
    [InlineData("", "123456")]
    [InlineData("   ", "123456")]
    [InlineData("test@example.com", null)]
    [InlineData("test@example.com", "")]
    [InlineData("test@example.com", "   ")]
    public void ValidateToken_WithInvalidParameters_ShouldReturnFalse(string email, string token)
    {
        // Act
        bool isValid = mTokenService.ValidateToken(token, email, false);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void ValidateToken_WithRemoveTrue_ShouldRemoveTokenAfterValidation()
    {
        // Arrange
        string email = "test@example.com";
        string token = mTokenService.GenerateToken(email);

        // Act
        bool firstValidation = mTokenService.ValidateToken(token, email, true);
        bool secondValidation = mTokenService.ValidateToken(token, email, false);

        // Assert
        firstValidation.Should().BeTrue();
        secondValidation.Should().BeFalse(); // Token should be removed
    }

    [Fact]
    public void ValidateToken_WithRemoveFalse_ShouldNotRemoveToken()
    {
        // Arrange
        string email = "test@example.com";
        string token = mTokenService.GenerateToken(email);

        // Act
        bool firstValidation = mTokenService.ValidateToken(token, email, false);
        bool secondValidation = mTokenService.ValidateToken(token, email, false);

        // Assert
        firstValidation.Should().BeTrue();
        secondValidation.Should().BeTrue(); // Token should still be valid
    }

    [Fact]
    public void ValidateToken_WithRemoveTrue_AndWrongEmail_ShouldNotAffectToken()
    {
        // Arrange
        string email = "test@example.com";
        string token = mTokenService.GenerateToken(email);

        // Act - First validate with wrong email and remove=true
        bool firstValidation = mTokenService.ValidateToken(token, "wrong@example.com", true);
        // Then validate with correct email
        bool secondValidation = mTokenService.ValidateToken(token, email, false);

        // Assert
        firstValidation.Should().BeFalse(); // Invalid because wrong email
        secondValidation.Should().BeTrue(); // Token should still exist because cache key didn't match
    }

    [Fact]
    public async Task ValidateToken_AfterExpiration_ShouldReturnFalse()
    {
        // Arrange
        var mockConfig = new Mock<ITokenServiceConfiguration>();
        mockConfig.Setup(c => c.ExpirationInSeconds).Returns(1.0); // 1 second
        using var tokenService = new TokenService(mockConfig.Object);

        string email = "test@example.com";
        string token = tokenService.GenerateToken(email);

        // Act - Wait for token to expire
        await Task.Delay(1500); // Wait 1.5 seconds
        bool isValid = tokenService.ValidateToken(token, email, false);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void GenerateToken_ForDifferentEmails_ShouldAllowMultipleTokens()
    {
        // Arrange
        string email1 = "user1@example.com";
        string email2 = "user2@example.com";

        // Act
        string token1 = mTokenService.GenerateToken(email1);
        string token2 = mTokenService.GenerateToken(email2);

        // Assert
        mTokenService.ValidateToken(token1, email1, false).Should().BeTrue();
        mTokenService.ValidateToken(token2, email2, false).Should().BeTrue();
        mTokenService.ValidateToken(token1, email2, false).Should().BeFalse();
        mTokenService.ValidateToken(token2, email1, false).Should().BeFalse();
    }

    [Fact]
    public void Dispose_ShouldDisposeMemoryCache()
    {
        // Arrange
        var tokenService = new TokenService(mMockConfiguration.Object);
        string email = "test@example.com";
        string token = tokenService.GenerateToken(email);

        // Act
        tokenService.Dispose();

        // Assert - Should not throw
        Action act = () => tokenService.Dispose();
        act.Should().NotThrow();
    }
}
