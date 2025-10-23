using Gehtsoft.FourCDesigner.Logic.AI;
using Microsoft.Extensions.Configuration;
using Xunit;
using FluentAssertions;

namespace Gehtsoft.FourCDesigner.Tests.Logic.AI;

/// <summary>
/// Unit tests for AIConfiguration class.
/// </summary>
public class AIConfigurationTests
{
    [Fact]
    public void Constructor_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new AIConfiguration(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("configuration");
    }

    [Fact]
    public void Driver_WithDefaultConfiguration_ShouldReturnMock()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        var aiConfig = new AIConfiguration(configuration);

        // Act
        string driver = aiConfig.Driver;

        // Assert
        driver.Should().Be("mock");
    }

    [Fact]
    public void Driver_WithMockConfiguration_ShouldReturnMock()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            ["ai:driver"] = "mock"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();
        var aiConfig = new AIConfiguration(configuration);

        // Act
        string driver = aiConfig.Driver;

        // Assert
        driver.Should().Be("mock");
    }

    [Fact]
    public void Driver_WithOllamaConfiguration_ShouldReturnOllama()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            ["ai:driver"] = "ollama"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();
        var aiConfig = new AIConfiguration(configuration);

        // Act
        string driver = aiConfig.Driver;

        // Assert
        driver.Should().Be("ollama");
    }

    [Fact]
    public void Driver_WithOpenAIConfiguration_ShouldReturnOpenAI()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            ["ai:driver"] = "openai"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();
        var aiConfig = new AIConfiguration(configuration);

        // Act
        string driver = aiConfig.Driver;

        // Assert
        driver.Should().Be("openai");
    }

    [Fact]
    public void Driver_WithEmptyString_ShouldReturnMock()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            ["ai:driver"] = ""
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();
        var aiConfig = new AIConfiguration(configuration);

        // Act
        string driver = aiConfig.Driver;

        // Assert
        driver.Should().Be("mock");
    }

    [Fact]
    public void Driver_WithCustomValue_ShouldReturnCustomValue()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            ["ai:driver"] = "custom-driver"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();
        var aiConfig = new AIConfiguration(configuration);

        // Act
        string driver = aiConfig.Driver;

        // Assert
        driver.Should().Be("custom-driver");
    }
}
