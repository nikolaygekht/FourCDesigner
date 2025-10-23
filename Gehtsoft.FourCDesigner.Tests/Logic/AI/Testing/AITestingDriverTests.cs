using Gehtsoft.FourCDesigner.Logic.AI;
using Gehtsoft.FourCDesigner.Logic.AI.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace Gehtsoft.FourCDesigner.Tests.Logic.AI.Testing;

/// <summary>
/// Unit tests for AITestingDriver class.
/// </summary>
public class AITestingDriverTests
{
    private readonly string mTestDataPath;

    public AITestingDriverTests()
    {
        // Get the base directory of the test assembly
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        mTestDataPath = Path.Combine(baseDir, "TestData", "AI");
    }

    [Fact]
    public void Constructor_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<AITestingDriver>>();

        // Act
        Action act = () => new AITestingDriver(null!, mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("configuration");
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Arrange
        var mockConfig = new Mock<IAITestingConfiguration>();
        mockConfig.Setup(c => c.MockFilePath).Returns("test.json");

        // Act
        Action act = () => new AITestingDriver(mockConfig.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void Constructor_WithMissingFile_ShouldNotThrow()
    {
        // Arrange
        var mockConfig = new Mock<IAITestingConfiguration>();
        mockConfig.Setup(c => c.MockFilePath).Returns("non-existent-file.json");
        var mockLogger = new Mock<ILogger<AITestingDriver>>();

        // Act
        Action act = () => new AITestingDriver(mockConfig.Object, mockLogger.Object);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_WithInvalidJson_ShouldThrowInvalidOperationException()
    {
        // Arrange
        string invalidJsonPath = Path.Combine(mTestDataPath, "invalid-mock.json");
        var mockConfig = new Mock<IAITestingConfiguration>();
        mockConfig.Setup(c => c.MockFilePath).Returns(invalidJsonPath);
        var mockLogger = new Mock<ILogger<AITestingDriver>>();

        // Act
        Action act = () => new AITestingDriver(mockConfig.Object, mockLogger.Object);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Failed to load mock operations*");
    }

    [Fact]
    public void Constructor_WithValidFile_ShouldLoadMockOperations()
    {
        // Arrange
        string validJsonPath = Path.Combine(mTestDataPath, "valid-mock-responses.json");
        var mockConfig = new Mock<IAITestingConfiguration>();
        mockConfig.Setup(c => c.MockFilePath).Returns(validJsonPath);
        var mockLogger = new Mock<ILogger<AITestingDriver>>();

        // Act
        Action act = () => new AITestingDriver(mockConfig.Object, mockLogger.Object);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_WithEmptyFile_ShouldLoadEmptyList()
    {
        // Arrange
        string emptyJsonPath = Path.Combine(mTestDataPath, "empty-mock.json");
        var mockConfig = new Mock<IAITestingConfiguration>();
        mockConfig.Setup(c => c.MockFilePath).Returns(emptyJsonPath);
        var mockLogger = new Mock<ILogger<AITestingDriver>>();

        // Act
        Action act = () => new AITestingDriver(mockConfig.Object, mockLogger.Object);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public async Task ValidateUserInputAsync_WithNullInput_ShouldThrowArgumentNullException()
    {
        // Arrange
        string validJsonPath = Path.Combine(mTestDataPath, "valid-mock-responses.json");
        var mockConfig = new Mock<IAITestingConfiguration>();
        mockConfig.Setup(c => c.MockFilePath).Returns(validJsonPath);
        var mockLogger = new Mock<ILogger<AITestingDriver>>();
        var driver = new AITestingDriver(mockConfig.Object, mockLogger.Object);

        // Act
        Func<Task> act = async () => await driver.ValidateUserInputAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("userInput");
    }

    [Fact]
    public async Task ValidateUserInputAsync_WithMatchingPattern_ShouldReturnMatchingResponse()
    {
        // Arrange
        string validJsonPath = Path.Combine(mTestDataPath, "valid-mock-responses.json");
        var mockConfig = new Mock<IAITestingConfiguration>();
        mockConfig.Setup(c => c.MockFilePath).Returns(validJsonPath);
        var mockLogger = new Mock<ILogger<AITestingDriver>>();
        var driver = new AITestingDriver(mockConfig.Object, mockLogger.Object);

        // Act
        AIResult result = await driver.ValidateUserInputAsync("This contains inject keyword");

        // Assert
        result.Should().NotBeNull();
        result.Successful.Should().BeFalse();
        result.ErrorCode.Should().Be("UNSAFE_INPUT");
        result.Output.Should().Contain("injection");
    }

    [Fact]
    public async Task ValidateUserInputAsync_WithSafeInput_ShouldReturnSafeResponse()
    {
        // Arrange
        string validJsonPath = Path.Combine(mTestDataPath, "valid-mock-responses.json");
        var mockConfig = new Mock<IAITestingConfiguration>();
        mockConfig.Setup(c => c.MockFilePath).Returns(validJsonPath);
        var mockLogger = new Mock<ILogger<AITestingDriver>>();
        var driver = new AITestingDriver(mockConfig.Object, mockLogger.Object);

        // Act
        AIResult result = await driver.ValidateUserInputAsync("This is safe input");

        // Assert
        result.Should().NotBeNull();
        result.Successful.Should().BeTrue();
        result.ErrorCode.Should().BeEmpty();
        result.Output.Should().Be("SAFE");
    }

    [Fact]
    public async Task ValidateUserInputAsync_WithNoMatch_ShouldThrowInvalidOperationException()
    {
        // Arrange
        string emptyJsonPath = Path.Combine(mTestDataPath, "empty-mock.json");
        var mockConfig = new Mock<IAITestingConfiguration>();
        mockConfig.Setup(c => c.MockFilePath).Returns(emptyJsonPath);
        var mockLogger = new Mock<ILogger<AITestingDriver>>();
        var driver = new AITestingDriver(mockConfig.Object, mockLogger.Object);

        // Act
        Func<Task> act = async () => await driver.ValidateUserInputAsync("test input");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No matching mock operation found*");
    }

    [Fact]
    public async Task ValidateUserInputAsync_WithMultipleMatches_ShouldReturnFirstMatch()
    {
        // Arrange
        string validJsonPath = Path.Combine(mTestDataPath, "valid-mock-responses.json");
        var mockConfig = new Mock<IAITestingConfiguration>();
        mockConfig.Setup(c => c.MockFilePath).Returns(validJsonPath);
        var mockLogger = new Mock<ILogger<AITestingDriver>>();
        var driver = new AITestingDriver(mockConfig.Object, mockLogger.Object);

        // Act - "inject" matches the first specific pattern before the catch-all
        AIResult result = await driver.ValidateUserInputAsync("inject");

        // Assert
        result.Should().NotBeNull();
        result.Successful.Should().BeFalse();
        result.ErrorCode.Should().Be("UNSAFE_INPUT");
    }

    [Fact]
    public async Task ValidateUserInputAsync_WithCaseInsensitivePattern_ShouldMatch()
    {
        // Arrange
        string validJsonPath = Path.Combine(mTestDataPath, "valid-mock-responses.json");
        var mockConfig = new Mock<IAITestingConfiguration>();
        mockConfig.Setup(c => c.MockFilePath).Returns(validJsonPath);
        var mockLogger = new Mock<ILogger<AITestingDriver>>();
        var driver = new AITestingDriver(mockConfig.Object, mockLogger.Object);

        // Act - Pattern uses IgnoreCase
        AIResult result = await driver.ValidateUserInputAsync("INJECT");

        // Assert
        result.Should().NotBeNull();
        result.Successful.Should().BeFalse();
        result.ErrorCode.Should().Be("UNSAFE_INPUT");
    }

    [Fact]
    public async Task GetSuggestionsAsync_WithNullInstructions_ShouldThrowArgumentNullException()
    {
        // Arrange
        string validJsonPath = Path.Combine(mTestDataPath, "valid-mock-responses.json");
        var mockConfig = new Mock<IAITestingConfiguration>();
        mockConfig.Setup(c => c.MockFilePath).Returns(validJsonPath);
        var mockLogger = new Mock<ILogger<AITestingDriver>>();
        var driver = new AITestingDriver(mockConfig.Object, mockLogger.Object);

        // Act
        Func<Task> act = async () => await driver.GetSuggestionsAsync(null!, "input");

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("instructions");
    }

    [Fact]
    public async Task GetSuggestionsAsync_WithNullUserInput_ShouldThrowArgumentNullException()
    {
        // Arrange
        string validJsonPath = Path.Combine(mTestDataPath, "valid-mock-responses.json");
        var mockConfig = new Mock<IAITestingConfiguration>();
        mockConfig.Setup(c => c.MockFilePath).Returns(validJsonPath);
        var mockLogger = new Mock<ILogger<AITestingDriver>>();
        var driver = new AITestingDriver(mockConfig.Object, mockLogger.Object);

        // Act
        Func<Task> act = async () => await driver.GetSuggestionsAsync("instructions", null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("userInput");
    }

    [Fact]
    public async Task GetSuggestionsAsync_WithMatchingPatterns_ShouldReturnMatchingResponse()
    {
        // Arrange
        string validJsonPath = Path.Combine(mTestDataPath, "valid-mock-responses.json");
        var mockConfig = new Mock<IAITestingConfiguration>();
        mockConfig.Setup(c => c.MockFilePath).Returns(validJsonPath);
        var mockLogger = new Mock<ILogger<AITestingDriver>>();
        var driver = new AITestingDriver(mockConfig.Object, mockLogger.Object);

        // Act
        AIResult result = await driver.GetSuggestionsAsync(
            "Give me suggestions for teaching",
            "I want to teach math to students");

        // Assert
        result.Should().NotBeNull();
        result.Successful.Should().BeTrue();
        result.Output.Should().Contain("math");
    }

    [Fact]
    public async Task GetSuggestionsAsync_WithNoMatch_ShouldThrowInvalidOperationException()
    {
        // Arrange
        string emptyJsonPath = Path.Combine(mTestDataPath, "empty-mock.json");
        var mockConfig = new Mock<IAITestingConfiguration>();
        mockConfig.Setup(c => c.MockFilePath).Returns(emptyJsonPath);
        var mockLogger = new Mock<ILogger<AITestingDriver>>();
        var driver = new AITestingDriver(mockConfig.Object, mockLogger.Object);

        // Act
        Func<Task> act = async () => await driver.GetSuggestionsAsync("test", "input");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No matching mock operation found*");
    }

    [Fact]
    public async Task GetSuggestionsAsync_WithRequestMatch_UserDataNoMatch_ShouldNotMatch()
    {
        // Arrange
        string validJsonPath = Path.Combine(mTestDataPath, "valid-mock-responses.json");
        var mockConfig = new Mock<IAITestingConfiguration>();
        mockConfig.Setup(c => c.MockFilePath).Returns(validJsonPath);
        var mockLogger = new Mock<ILogger<AITestingDriver>>();
        var driver = new AITestingDriver(mockConfig.Object, mockLogger.Object);

        // Act - "suggestions" matches request pattern but "xyz" doesn't match ".*math.*" or ".*science.*"
        // Should fall through to default ".*" pattern
        AIResult result = await driver.GetSuggestionsAsync("Give suggestions", "xyz");

        // Assert
        result.Should().NotBeNull();
        result.Successful.Should().BeTrue();
        result.Output.Should().Be("Default general response");
    }

    [Fact]
    public async Task GetSuggestionsAsync_WithUserDataMatch_RequestNoMatch_ShouldNotMatch()
    {
        // Arrange
        string validJsonPath = Path.Combine(mTestDataPath, "valid-mock-responses.json");
        var mockConfig = new Mock<IAITestingConfiguration>();
        mockConfig.Setup(c => c.MockFilePath).Returns(validJsonPath);
        var mockLogger = new Mock<ILogger<AITestingDriver>>();
        var driver = new AITestingDriver(mockConfig.Object, mockLogger.Object);

        // Act - "math" matches userdata but "analyze" doesn't match ".*suggestions.*"
        // Should fall through to default pattern
        AIResult result = await driver.GetSuggestionsAsync("analyze this", "math topic");

        // Assert
        result.Should().NotBeNull();
        result.Successful.Should().BeTrue();
        result.Output.Should().Be("Default general response");
    }

    [Fact]
    public async Task GetSuggestionsAsync_WithComplexRegex_ShouldMatchCorrectly()
    {
        // Arrange
        string validJsonPath = Path.Combine(mTestDataPath, "valid-mock-responses.json");
        var mockConfig = new Mock<IAITestingConfiguration>();
        mockConfig.Setup(c => c.MockFilePath).Returns(validJsonPath);
        var mockLogger = new Mock<ILogger<AITestingDriver>>();
        var driver = new AITestingDriver(mockConfig.Object, mockLogger.Object);

        // Act
        AIResult result = await driver.GetSuggestionsAsync(
            "Please analyze the content",
            "test data");

        // Assert
        result.Should().NotBeNull();
        result.Successful.Should().BeTrue();
        result.Output.Should().Be("Analysis result for test data");
    }

    [Fact]
    public async Task FindMatchingResponse_WithInvalidRegexPattern_ShouldSkipAndContinue()
    {
        // Arrange
        string invalidRegexPath = Path.Combine(mTestDataPath, "invalid-regex-mock.json");
        var mockConfig = new Mock<IAITestingConfiguration>();
        mockConfig.Setup(c => c.MockFilePath).Returns(invalidRegexPath);
        var mockLogger = new Mock<ILogger<AITestingDriver>>();
        var driver = new AITestingDriver(mockConfig.Object, mockLogger.Object);

        // Act - First pattern has invalid regex, should skip to second
        AIResult result = await driver.ValidateUserInputAsync("this is valid text");

        // Assert
        result.Should().NotBeNull();
        result.Successful.Should().BeTrue();
        result.Output.Should().Be("Valid pattern match");
    }
}
