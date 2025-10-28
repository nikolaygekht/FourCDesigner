using Gehtsoft.FourCDesigner.Logic.AI;
using Gehtsoft.FourCDesigner.Logic.AI.Testing;
using Gehtsoft.FourCDesigner.Logic.AI.Ollama;
using Gehtsoft.FourCDesigner.Logic.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace Gehtsoft.FourCDesigner.Tests.Logic.AI;

/// <summary>
/// Unit tests for AIDriverFactory class.
/// </summary>
public class AIDriverFactoryTests
{
    [Fact]
    public void Constructor_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockLogger = new Mock<ILogger<AIDriverFactory>>();

        // Act
        Action act = () => new AIDriverFactory(null!, mockServiceProvider.Object, mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("configuration");
    }

    [Fact]
    public void Constructor_WithNullServiceProvider_ShouldThrowArgumentNullException()
    {
        // Arrange
        var mockConfig = new Mock<IAIConfiguration>();
        var mockLogger = new Mock<ILogger<AIDriverFactory>>();

        // Act
        Action act = () => new AIDriverFactory(mockConfig.Object, null!, mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("serviceProvider");
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Arrange
        var mockConfig = new Mock<IAIConfiguration>();
        var mockServiceProvider = new Mock<IServiceProvider>();

        // Act
        Action act = () => new AIDriverFactory(mockConfig.Object, mockServiceProvider.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void CreateDriver_WithMockDriver_ShouldReturnAITestingDriver()
    {
        // Arrange
        var mockAIConfig = new Mock<IAIConfiguration>();
        mockAIConfig.Setup(c => c.Driver).Returns("mock");
        mockAIConfig.Setup(c => c.Config).Returns("mock");

        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c["ai:mock:file"]).Returns("non-existent.json");

        var mockTestingLogger = new Mock<ILogger<AITestingDriver>>();

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IConfiguration)))
            .Returns(mockConfiguration.Object);
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(ILogger<AITestingDriver>)))
            .Returns(mockTestingLogger.Object);

        var mockFactoryLogger = new Mock<ILogger<AIDriverFactory>>();

        var factory = new AIDriverFactory(
            mockAIConfig.Object,
            mockServiceProvider.Object,
            mockFactoryLogger.Object);

        // Act
        IAIDriver driver = factory.CreateDriver();

        // Assert
        driver.Should().NotBeNull();
        driver.Should().BeOfType<AITestingDriver>();
    }

    [Fact]
    public void CreateDriver_WithOllamaDriver_ShouldReturnAIOllamaDriver()
    {
        // Arrange
        var mockAIConfig = new Mock<IAIConfiguration>();
        mockAIConfig.Setup(c => c.Driver).Returns("ollama");
        mockAIConfig.Setup(c => c.Config).Returns("ollama-test");

        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c["ai:ollama-test:url"]).Returns("http://localhost:11434");
        mockConfiguration.Setup(c => c["ai:ollama-test:model"]).Returns("llama2");
        mockConfiguration.Setup(c => c["ai:ollama-test:timeoutSeconds"]).Returns("120");

        var mockOllamaLogger = new Mock<ILogger<AIOllamaDriver>>();

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IConfiguration)))
            .Returns(mockConfiguration.Object);
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(ILogger<AIOllamaDriver>)))
            .Returns(mockOllamaLogger.Object);

        var mockFactoryLogger = new Mock<ILogger<AIDriverFactory>>();

        var factory = new AIDriverFactory(
            mockAIConfig.Object,
            mockServiceProvider.Object,
            mockFactoryLogger.Object);

        // Act
        IAIDriver driver = factory.CreateDriver();

        // Assert
        driver.Should().NotBeNull();
        driver.Should().BeOfType<AIOllamaDriver>();
    }

    [Fact]
    public void CreateDriver_WithOpenAIDriver_ShouldReturnAIOpenAIDriver()
    {
        // Arrange
        var mockAIConfig = new Mock<IAIConfiguration>();
        mockAIConfig.Setup(c => c.Driver).Returns("openai");
        mockAIConfig.Setup(c => c.Config).Returns("openai-test");

        var configData = new Dictionary<string, string>
        {
            ["ai:openai-test:url"] = "https://api.openai.com/v1",
            ["ai:openai-test:key"] = "test-key",
            ["ai:openai-test:model"] = "gpt-3.5-turbo",
            ["ai:openai-test:timeoutSeconds"] = "60"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();

        var mockOpenAILogger = new Mock<ILogger<AIOpenAIDriver>>();

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IConfiguration)))
            .Returns(configuration);
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(ILogger<AIOpenAIDriver>)))
            .Returns(mockOpenAILogger.Object);

        var mockFactoryLogger = new Mock<ILogger<AIDriverFactory>>();

        var factory = new AIDriverFactory(
            mockAIConfig.Object,
            mockServiceProvider.Object,
            mockFactoryLogger.Object);

        // Act
        IAIDriver driver = factory.CreateDriver();

        // Assert
        driver.Should().NotBeNull();
        driver.Should().BeOfType<AIOpenAIDriver>();
    }

    [Fact]
    public void CreateDriver_WithUppercaseMock_ShouldReturnAITestingDriver()
    {
        // Arrange
        var mockAIConfig = new Mock<IAIConfiguration>();
        mockAIConfig.Setup(c => c.Driver).Returns("MOCK");
        mockAIConfig.Setup(c => c.Config).Returns("mock");

        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c["ai:mock:file"]).Returns("non-existent.json");

        var mockTestingLogger = new Mock<ILogger<AITestingDriver>>();

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IConfiguration)))
            .Returns(mockConfiguration.Object);
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(ILogger<AITestingDriver>)))
            .Returns(mockTestingLogger.Object);

        var mockFactoryLogger = new Mock<ILogger<AIDriverFactory>>();

        var factory = new AIDriverFactory(
            mockAIConfig.Object,
            mockServiceProvider.Object,
            mockFactoryLogger.Object);

        // Act
        IAIDriver driver = factory.CreateDriver();

        // Assert
        driver.Should().NotBeNull();
        driver.Should().BeOfType<AITestingDriver>();
    }

    [Fact]
    public void CreateDriver_WithMixedCaseOllama_ShouldReturnAIOllamaDriver()
    {
        // Arrange
        var mockAIConfig = new Mock<IAIConfiguration>();
        mockAIConfig.Setup(c => c.Driver).Returns("Ollama");
        mockAIConfig.Setup(c => c.Config).Returns("ollama-test");

        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c["ai:ollama-test:url"]).Returns("http://localhost:11434");
        mockConfiguration.Setup(c => c["ai:ollama-test:model"]).Returns("llama2");
        mockConfiguration.Setup(c => c["ai:ollama-test:timeoutSeconds"]).Returns("120");

        var mockOllamaLogger = new Mock<ILogger<AIOllamaDriver>>();

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IConfiguration)))
            .Returns(mockConfiguration.Object);
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(ILogger<AIOllamaDriver>)))
            .Returns(mockOllamaLogger.Object);

        var mockFactoryLogger = new Mock<ILogger<AIDriverFactory>>();

        var factory = new AIDriverFactory(
            mockAIConfig.Object,
            mockServiceProvider.Object,
            mockFactoryLogger.Object);

        // Act
        IAIDriver driver = factory.CreateDriver();

        // Assert
        driver.Should().NotBeNull();
        driver.Should().BeOfType<AIOllamaDriver>();
    }

    [Fact]
    public void CreateDriver_WithUnsupportedDriver_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var mockConfig = new Mock<IAIConfiguration>();
        mockConfig.Setup(c => c.Driver).Returns("unsupported-driver");

        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockFactoryLogger = new Mock<ILogger<AIDriverFactory>>();

        var factory = new AIDriverFactory(
            mockConfig.Object,
            mockServiceProvider.Object,
            mockFactoryLogger.Object);

        // Act
        Action act = () => factory.CreateDriver();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Unsupported AI driver type: unsupported-driver*");
    }

    [Fact]
    public void CreateDriver_WithEmptyDriver_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var mockConfig = new Mock<IAIConfiguration>();
        mockConfig.Setup(c => c.Driver).Returns(string.Empty);

        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockFactoryLogger = new Mock<ILogger<AIDriverFactory>>();

        var factory = new AIDriverFactory(
            mockConfig.Object,
            mockServiceProvider.Object,
            mockFactoryLogger.Object);

        // Act
        Action act = () => factory.CreateDriver();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Unsupported AI driver type:*");
    }

    [Fact]
    public void CreateDriver_CalledMultipleTimes_ShouldCreateNewInstances()
    {
        // Arrange
        var mockAIConfig = new Mock<IAIConfiguration>();
        mockAIConfig.Setup(c => c.Driver).Returns("mock");
        mockAIConfig.Setup(c => c.Config).Returns("mock");

        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c["ai:mock:file"]).Returns("non-existent.json");

        var mockTestingLogger = new Mock<ILogger<AITestingDriver>>();

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IConfiguration)))
            .Returns(mockConfiguration.Object);
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(ILogger<AITestingDriver>)))
            .Returns(mockTestingLogger.Object);

        var mockFactoryLogger = new Mock<ILogger<AIDriverFactory>>();

        var factory = new AIDriverFactory(
            mockAIConfig.Object,
            mockServiceProvider.Object,
            mockFactoryLogger.Object);

        // Act
        IAIDriver driver1 = factory.CreateDriver();
        IAIDriver driver2 = factory.CreateDriver();

        // Assert
        driver1.Should().NotBeNull();
        driver2.Should().NotBeNull();
        driver1.Should().NotBeSameAs(driver2);
    }
}
