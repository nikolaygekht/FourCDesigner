using Gehtsoft.FourCDesigner.Logic.AI.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;
using FluentAssertions;

namespace Gehtsoft.FourCDesigner.Tests.Logic.AI.Testing;

/// <summary>
/// Unit tests for AITestingConfiguration class.
/// </summary>
public class AITestingConfigurationTests
{
    [Fact]
    public void Constructor_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new AITestingConfiguration(null!, "mock");

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("configuration");
    }

    [Fact]
    public void MockFilePath_WithDefaultConfiguration_ShouldReturnDefaultPath()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        var testingConfig = new AITestingConfiguration(configuration, "mock");

        // Act
        string filePath = testingConfig.MockFilePath;

        // Assert
        filePath.Should().Be("./data/ai-mock-responses.json");
    }

    [Fact]
    public void MockFilePath_WithCustomConfiguration_ShouldReturnCustomPath()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            ["ai:mock:file"] = "./custom/path/mocks.json"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();
        var testingConfig = new AITestingConfiguration(configuration, "mock");

        // Act
        string filePath = testingConfig.MockFilePath;

        // Assert
        filePath.Should().Be("./custom/path/mocks.json");
    }

    [Fact]
    public void MockFilePath_WithEmptyString_ShouldReturnDefaultPath()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            ["ai:mock:file"] = ""
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();
        var testingConfig = new AITestingConfiguration(configuration, "mock");

        // Act
        string filePath = testingConfig.MockFilePath;

        // Assert
        filePath.Should().Be("./data/ai-mock-responses.json");
    }

    [Fact]
    public void MockFilePath_WithAbsolutePath_ShouldReturnAbsolutePath()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            ["ai:mock:file"] = "C:\\TestData\\mocks.json"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();
        var testingConfig = new AITestingConfiguration(configuration, "mock");

        // Act
        string filePath = testingConfig.MockFilePath;

        // Assert
        filePath.Should().Be("C:\\TestData\\mocks.json");
    }
}
