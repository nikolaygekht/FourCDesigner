using Gehtsoft.FourCDesigner.Logic.User;
using Microsoft.Extensions.Configuration;
using Xunit;
using FluentAssertions;

namespace Gehtsoft.FourCDesigner.Tests.Logic.User;

public class UserConfigurationTests
{
    [Fact]
    public void Constructor_WithNullConfiguration_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new UserConfiguration(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("configuration");
    }

    [Fact]
    public void PasswordRules_WithDefaultConfiguration_ReturnsDefaults()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        var userConfig = new UserConfiguration(configuration);

        // Act
        var rules = userConfig.PasswordRules;

        // Assert
        rules.MinimumLength.Should().Be(8);
        rules.RequireCapitalLetter.Should().BeTrue();
        rules.RequireSmallLetter.Should().BeTrue();
        rules.RequireDigit.Should().BeTrue();
        rules.RequireSpecialSymbol.Should().BeFalse();
    }

    [Fact]
    public void PasswordRules_WithCustomConfiguration_ReturnsCustomValues()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            ["system:passwordRules:minimumLength"] = "12",
            ["system:passwordRules:requireCapitalLetter"] = "false",
            ["system:passwordRules:requireSmallLetter"] = "false",
            ["system:passwordRules:requireDigit"] = "false",
            ["system:passwordRules:requireSpecialSymbol"] = "true"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();
        var userConfig = new UserConfiguration(configuration);

        // Act
        var rules = userConfig.PasswordRules;

        // Assert
        rules.MinimumLength.Should().Be(12);
        rules.RequireCapitalLetter.Should().BeFalse();
        rules.RequireSmallLetter.Should().BeFalse();
        rules.RequireDigit.Should().BeFalse();
        rules.RequireSpecialSymbol.Should().BeTrue();
    }

    [Fact]
    public void PasswordRules_WithInvalidMinimumLength_ReturnsDefault()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            ["system:passwordRules:minimumLength"] = "invalid"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();
        var userConfig = new UserConfiguration(configuration);

        // Act
        var rules = userConfig.PasswordRules;

        // Assert
        rules.MinimumLength.Should().Be(8);
    }

    [Fact]
    public void PasswordRules_WithZeroMinimumLength_ReturnsDefault()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            ["system:passwordRules:minimumLength"] = "0"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();
        var userConfig = new UserConfiguration(configuration);

        // Act
        var rules = userConfig.PasswordRules;

        // Assert
        rules.MinimumLength.Should().Be(8);
    }

    [Fact]
    public void PasswordRules_WithNegativeMinimumLength_ReturnsDefault()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            ["system:passwordRules:minimumLength"] = "-5"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();
        var userConfig = new UserConfiguration(configuration);

        // Act
        var rules = userConfig.PasswordRules;

        // Assert
        rules.MinimumLength.Should().Be(8);
    }

    [Fact]
    public void PasswordRules_WithPartialConfiguration_ReturnsDefaultsForMissing()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            ["system:passwordRules:minimumLength"] = "10",
            ["system:passwordRules:requireDigit"] = "false"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();
        var userConfig = new UserConfiguration(configuration);

        // Act
        var rules = userConfig.PasswordRules;

        // Assert
        rules.MinimumLength.Should().Be(10);
        rules.RequireCapitalLetter.Should().BeTrue(); // Default
        rules.RequireSmallLetter.Should().BeTrue(); // Default
        rules.RequireDigit.Should().BeFalse(); // Configured
        rules.RequireSpecialSymbol.Should().BeFalse(); // Default
    }
}
