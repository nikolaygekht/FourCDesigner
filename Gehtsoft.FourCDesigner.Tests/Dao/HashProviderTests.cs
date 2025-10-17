using Gehtsoft.FourCDesigner.Dao;
using Gehtsoft.FourCDesigner.Dao.Configuration;
using Xunit;
using FluentAssertions;
using Moq;

namespace Gehtsoft.FourCDesigner.Tests.Dao;

public class HashProviderTests
{
    private readonly Mock<IHashProviderConfiguration> mConfigurationMock;
    private readonly HashProvider mHashProvider;

    public HashProviderTests()
    {
        mConfigurationMock = new Mock<IHashProviderConfiguration>();
        mConfigurationMock.Setup(c => c.Salt).Returns("TestSalt123");
        mHashProvider = new HashProvider(mConfigurationMock.Object);
    }

    [Fact]
    public void Constructor_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new HashProvider(null);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("configuration");
    }

    [Fact]
    public void ComputeHash_WithNullPassword_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => mHashProvider.ComputeHash(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("password");
    }

    [Fact]
    public void ComputeHash_WithValidPassword_ReturnsBase64String()
    {
        // Act
        string hash = mHashProvider.ComputeHash("testpassword");

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().MatchRegex("^[A-Za-z0-9+/]+=*$"); // Base64 pattern
    }

    [Fact]
    public void ComputeHash_WithEmptyPassword_ReturnsNonEmptyHash()
    {
        // Act
        string hash = mHashProvider.ComputeHash("");

        // Assert
        hash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ComputeHash_WithDifferentInputs_AlwaysReturnsSameLength()
    {
        // Arrange
        string[] testInputs = { "", "a", "short", "medium length password", "very long password with many characters including special !@#$%^&*()" };

        // Act
        var hashes = testInputs.Select(input => mHashProvider.ComputeHash(input)).ToList();

        // Assert
        var firstLength = hashes[0].Length;
        hashes.Should().AllSatisfy(hash => hash.Length.Should().Be(firstLength));
    }

    [Fact]
    public void ComputeHash_WithSameValue_ReturnsSameHash()
    {
        // Arrange
        string password = "mypassword123";

        // Act
        string hash1 = mHashProvider.ComputeHash(password);
        string hash2 = mHashProvider.ComputeHash(password);

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void ComputeHash_WithDifferentValues_ReturnsDifferentHashes()
    {
        // Act
        string hash1 = mHashProvider.ComputeHash("password1");
        string hash2 = mHashProvider.ComputeHash("password2");

        // Assert
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void ComputeHash_WithOneCharacterDifference_ReturnsVeryDifferentHashes()
    {
        // Arrange
        string password1 = "password";
        string password2 = "passwora"; // Last character differs by one

        // Act
        string hash1 = mHashProvider.ComputeHash(password1);
        string hash2 = mHashProvider.ComputeHash(password2);

        // Assert
        hash1.Should().NotBe(hash2);

        // Count different characters - should be significantly different (avalanche effect)
        int differentChars = 0;
        for (int i = 0; i < Math.Min(hash1.Length, hash2.Length); i++)
        {
            if (hash1[i] != hash2[i])
                differentChars++;
        }

        // At least 50% of characters should be different (SHA512 avalanche effect)
        differentChars.Should().BeGreaterThan(hash1.Length / 2,
            "SHA512 should have avalanche effect - small input changes should cause large output changes");
    }

    [Fact]
    public void ValidatePassword_WithNullPassword_ThrowsArgumentNullException()
    {
        // Arrange
        string hash = mHashProvider.ComputeHash("test");

        // Act
        Action act = () => mHashProvider.ValidatePassword(null!, hash);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("password");
    }

    [Fact]
    public void ValidatePassword_WithNullStoredHash_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => mHashProvider.ValidatePassword("test", null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("storedHash");
    }

    [Fact]
    public void ValidatePassword_WithMatchingPassword_ReturnsTrue()
    {
        // Arrange
        string password = "correctpassword";
        string hash = mHashProvider.ComputeHash(password);

        // Act
        bool result = mHashProvider.ValidatePassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidatePassword_WithNonMatchingPassword_ReturnsFalse()
    {
        // Arrange
        string originalPassword = "correctpassword";
        string wrongPassword = "wrongpassword";
        string hash = mHashProvider.ComputeHash(originalPassword);

        // Act
        bool result = mHashProvider.ValidatePassword(wrongPassword, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidatePassword_WithEmptyPassword_ReturnsFalseForNonEmptyHash()
    {
        // Arrange
        string hash = mHashProvider.ComputeHash("nonemptypassword");

        // Act
        bool result = mHashProvider.ValidatePassword("", hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ComputeHash_WithDifferentSalts_ProducesDifferentHashes()
    {
        // Arrange
        var config1Mock = new Mock<IHashProviderConfiguration>();
        config1Mock.Setup(c => c.Salt).Returns("Salt1");
        var hashProvider1 = new HashProvider(config1Mock.Object);

        var config2Mock = new Mock<IHashProviderConfiguration>();
        config2Mock.Setup(c => c.Salt).Returns("Salt2");
        var hashProvider2 = new HashProvider(config2Mock.Object);

        string password = "testpassword";

        // Act
        string hash1 = hashProvider1.ComputeHash(password);
        string hash2 = hashProvider2.ComputeHash(password);

        // Assert
        hash1.Should().NotBe(hash2, "different salts should produce different hashes for the same password");
    }

    [Fact]
    public void ComputeHash_WithSameSalt_ProducesSameHashes()
    {
        // Arrange
        var config1Mock = new Mock<IHashProviderConfiguration>();
        config1Mock.Setup(c => c.Salt).Returns("SameSalt");
        var hashProvider1 = new HashProvider(config1Mock.Object);

        var config2Mock = new Mock<IHashProviderConfiguration>();
        config2Mock.Setup(c => c.Salt).Returns("SameSalt");
        var hashProvider2 = new HashProvider(config2Mock.Object);

        string password = "testpassword";

        // Act
        string hash1 = hashProvider1.ComputeHash(password);
        string hash2 = hashProvider2.ComputeHash(password);

        // Assert
        hash1.Should().Be(hash2, "same salt should produce same hash for the same password");
    }

    [Fact]
    public void ComputeHash_UsesConfiguredSalt()
    {
        // Arrange
        const string testSalt = "MyTestSalt";
        mConfigurationMock.Setup(c => c.Salt).Returns(testSalt);

        // Act
        string hash = mHashProvider.ComputeHash("password");

        // Assert
        mConfigurationMock.Verify(c => c.Salt, Times.Once);
        hash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ValidatePassword_WithDifferentSaltProvider_ReturnsFalse()
    {
        // Arrange
        var config1Mock = new Mock<IHashProviderConfiguration>();
        config1Mock.Setup(c => c.Salt).Returns("OriginalSalt");
        var hashProvider1 = new HashProvider(config1Mock.Object);

        var config2Mock = new Mock<IHashProviderConfiguration>();
        config2Mock.Setup(c => c.Salt).Returns("DifferentSalt");
        var hashProvider2 = new HashProvider(config2Mock.Object);

        string password = "testpassword";
        string hash = hashProvider1.ComputeHash(password);

        // Act
        bool result = hashProvider2.ValidatePassword(password, hash);

        // Assert
        result.Should().BeFalse("password validation should fail when using different salt");
    }
}
