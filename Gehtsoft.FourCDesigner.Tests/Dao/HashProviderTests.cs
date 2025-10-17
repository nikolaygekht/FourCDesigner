using Gehtsoft.FourCDesigner.Dao;
using Xunit;
using FluentAssertions;

namespace Gehtsoft.FourCDesigner.Tests.Dao;

public class HashProviderTests
{
    private readonly HashProvider mHashProvider;

    public HashProviderTests()
    {
        mHashProvider = new HashProvider();
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

        // At least 50% of characters should be different (SHA256 avalanche effect)
        differentChars.Should().BeGreaterThan(hash1.Length / 2,
            "SHA256 should have avalanche effect - small input changes should cause large output changes");
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
}
