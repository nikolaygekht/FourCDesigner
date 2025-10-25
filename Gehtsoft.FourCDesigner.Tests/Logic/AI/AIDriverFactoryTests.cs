using Gehtsoft.FourCDesigner.Logic.AI;
using Gehtsoft.FourCDesigner.Logic.AI.Testing;
using Gehtsoft.FourCDesigner.Logic.AI.Ollama;
using Gehtsoft.FourCDesigner.Logic.AI.OpenAI;
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
        var mockConfig = new Mock<IAIConfiguration>();
        mockConfig.Setup(c => c.Driver).Returns("mock");

        var mockTestingConfig = new Mock<IAITestingConfiguration>();
        mockTestingConfig.Setup(c => c.MockFilePath).Returns("non-existent.json");

        var mockTestingLogger = new Mock<ILogger<AITestingDriver>>();

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IAITestingConfiguration)))
            .Returns(mockTestingConfig.Object);
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(ILogger<AITestingDriver>)))
            .Returns(mockTestingLogger.Object);

        var mockFactoryLogger = new Mock<ILogger<AIDriverFactory>>();

        var factory = new AIDriverFactory(
            mockConfig.Object,
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
        var mockConfig = new Mock<IAIConfiguration>();
        mockConfig.Setup(c => c.Driver).Returns("ollama");

        var mockOllamaConfig = new Mock<IAIOllamaConfiguration>();
        mockOllamaConfig.Setup(c => c.ServiceUrl).Returns("http://localhost:11434");
        mockOllamaConfig.Setup(c => c.Model).Returns("llama2");
        mockOllamaConfig.Setup(c => c.TimeoutSeconds).Returns(120);

        var mockOllamaLogger = new Mock<ILogger<AIOllamaDriver>>();

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IAIOllamaConfiguration)))
            .Returns(mockOllamaConfig.Object);
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(ILogger<AIOllamaDriver>)))
            .Returns(mockOllamaLogger.Object);

        var mockFactoryLogger = new Mock<ILogger<AIDriverFactory>>();

        var factory = new AIDriverFactory(
            mockConfig.Object,
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
        var mockConfig = new Mock<IAIConfiguration>();
        mockConfig.Setup(c => c.Driver).Returns("openai");

        var mockOpenAIConfig = new Mock<IAIOpenAIConfiguration>();
        mockOpenAIConfig.Setup(c => c.ServiceUrl).Returns("https://api.openai.com/v1");
        mockOpenAIConfig.Setup(c => c.ApiKey).Returns("test-key");
        mockOpenAIConfig.Setup(c => c.Model).Returns("gpt-3.5-turbo");
        mockOpenAIConfig.Setup(c => c.TimeoutSeconds).Returns(60);

        var mockOpenAILogger = new Mock<ILogger<AIOpenAIDriver>>();

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IAIOpenAIConfiguration)))
            .Returns(mockOpenAIConfig.Object);
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(ILogger<AIOpenAIDriver>)))
            .Returns(mockOpenAILogger.Object);

        var mockFactoryLogger = new Mock<ILogger<AIDriverFactory>>();

        var factory = new AIDriverFactory(
            mockConfig.Object,
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
        var mockConfig = new Mock<IAIConfiguration>();
        mockConfig.Setup(c => c.Driver).Returns("MOCK");

        var mockTestingConfig = new Mock<IAITestingConfiguration>();
        mockTestingConfig.Setup(c => c.MockFilePath).Returns("non-existent.json");

        var mockTestingLogger = new Mock<ILogger<AITestingDriver>>();

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IAITestingConfiguration)))
            .Returns(mockTestingConfig.Object);
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(ILogger<AITestingDriver>)))
            .Returns(mockTestingLogger.Object);

        var mockFactoryLogger = new Mock<ILogger<AIDriverFactory>>();

        var factory = new AIDriverFactory(
            mockConfig.Object,
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
        var mockConfig = new Mock<IAIConfiguration>();
        mockConfig.Setup(c => c.Driver).Returns("Ollama");

        var mockOllamaConfig = new Mock<IAIOllamaConfiguration>();
        mockOllamaConfig.Setup(c => c.ServiceUrl).Returns("http://localhost:11434");
        mockOllamaConfig.Setup(c => c.Model).Returns("llama2");
        mockOllamaConfig.Setup(c => c.TimeoutSeconds).Returns(120);

        var mockOllamaLogger = new Mock<ILogger<AIOllamaDriver>>();

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IAIOllamaConfiguration)))
            .Returns(mockOllamaConfig.Object);
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(ILogger<AIOllamaDriver>)))
            .Returns(mockOllamaLogger.Object);

        var mockFactoryLogger = new Mock<ILogger<AIDriverFactory>>();

        var factory = new AIDriverFactory(
            mockConfig.Object,
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
        var mockConfig = new Mock<IAIConfiguration>();
        mockConfig.Setup(c => c.Driver).Returns("mock");

        var mockTestingConfig = new Mock<IAITestingConfiguration>();
        mockTestingConfig.Setup(c => c.MockFilePath).Returns("non-existent.json");

        var mockTestingLogger = new Mock<ILogger<AITestingDriver>>();

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IAITestingConfiguration)))
            .Returns(mockTestingConfig.Object);
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(ILogger<AITestingDriver>)))
            .Returns(mockTestingLogger.Object);

        var mockFactoryLogger = new Mock<ILogger<AIDriverFactory>>();

        var factory = new AIDriverFactory(
            mockConfig.Object,
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
