using Gehtsoft.FourCDesigner.Logic.AI;
using Gehtsoft.FourCDesigner.Logic.Plan;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using System.Threading.Tasks;

namespace Gehtsoft.FourCDesigner.Tests.Logic.Plan;

public class PlanAiControllerTests
{
    private readonly Mock<IAIDriver> mMockAiDriver;
    private readonly Mock<IPromptFactory> mMockPromptFactory;
    private readonly Mock<ILessonPlanFormatter> mMockFormatter;
    private readonly Mock<ILogger<PlanAiController>> mMockLogger;

    public PlanAiControllerTests()
    {
        mMockAiDriver = new Mock<IAIDriver>();
        mMockPromptFactory = new Mock<IPromptFactory>();
        mMockFormatter = new Mock<ILessonPlanFormatter>();
        mMockLogger = new Mock<ILogger<PlanAiController>>();
    }

    private static LessonPlan CreateSampleLessonPlan()
    {
        return new LessonPlan
        {
            Topic = "Test Topic",
            Audience = "Test Audience",
            LearningOutcomes = "Test Outcomes"
        };
    }

    private PlanAiController CreateController()
    {
        return new PlanAiController(
            mMockAiDriver.Object,
            mMockPromptFactory.Object,
            mMockFormatter.Object,
            mMockLogger.Object);
    }

    private void SetupSuccessfulMocks(
        string prompt = "Test prompt",
        string formattedInput = "Formatted plan",
        string aiOutput = "AI response")
    {
        mMockPromptFactory
            .Setup(p => p.GetPrompt(It.IsAny<RequestId>()))
            .Returns(prompt);

        mMockFormatter
            .Setup(f => f.FormatLessonPlan(It.IsAny<LessonPlan>(), It.IsAny<RequestId>()))
            .Returns(formattedInput);

        mMockAiDriver
            .Setup(d => d.ValidateUserInputAsync(It.IsAny<string>()))
            .ReturnsAsync(AIResult.Success("Validation passed"));

        mMockAiDriver
            .Setup(d => d.GetSuggestionsAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(AIResult.Success(aiOutput));
    }

    // Constructor Tests

    [Fact]
    public void Constructor_WithNullAiDriver_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new PlanAiController(
            null!,
            mMockPromptFactory.Object,
            mMockFormatter.Object,
            mMockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("aiDriver");
    }

    [Fact]
    public void Constructor_WithNullPromptFactory_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new PlanAiController(
            mMockAiDriver.Object,
            null!,
            mMockFormatter.Object,
            mMockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("promptFactory");
    }

    [Fact]
    public void Constructor_WithNullFormatter_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new PlanAiController(
            mMockAiDriver.Object,
            mMockPromptFactory.Object,
            null!,
            mMockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("formatter");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new PlanAiController(
            mMockAiDriver.Object,
            mMockPromptFactory.Object,
            mMockFormatter.Object,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void Constructor_WithValidDependencies_InitializesSuccessfully()
    {
        // Act
        Action act = () => CreateController();

        // Assert
        act.Should().NotThrow();
    }

    // Request Method - Argument Validation

    [Fact]
    public async Task Request_WithNullPlan_ThrowsArgumentNullException()
    {
        // Arrange
        var controller = CreateController();

        // Act
        Func<Task> act = async () => await controller.Request(RequestId.ReviewTopic, null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("plan");
    }

    // Request Method - Happy Path

    [Fact]
    public async Task Request_WithValidInput_ReturnsSuccessfulResult()
    {
        // Arrange
        SetupSuccessfulMocks(aiOutput: "AI response");
        var controller = CreateController();
        var plan = CreateSampleLessonPlan();

        // Act
        AIResult result = await controller.Request(RequestId.ReviewTopic, plan);

        // Assert
        result.Should().NotBeNull();
        result.Successful.Should().BeTrue();
        result.Output.Should().Be("AI response");
    }

    [Fact]
    public async Task Request_CallsPromptFactory_WithCorrectRequestId()
    {
        // Arrange
        SetupSuccessfulMocks();
        var controller = CreateController();
        var plan = CreateSampleLessonPlan();

        // Act
        await controller.Request(RequestId.ReviewAudience, plan);

        // Assert
        mMockPromptFactory.Verify(
            p => p.GetPrompt(RequestId.ReviewAudience),
            Times.Once);
    }

    [Fact]
    public async Task Request_CallsFormatter_WithCorrectParameters()
    {
        // Arrange
        SetupSuccessfulMocks();
        var controller = CreateController();
        var plan = CreateSampleLessonPlan();

        // Act
        await controller.Request(RequestId.ReviewOutcomes, plan);

        // Assert
        mMockFormatter.Verify(
            f => f.FormatLessonPlan(plan, RequestId.ReviewOutcomes),
            Times.Once);
    }

    [Fact]
    public async Task Request_CallsValidation_WithFormattedInput()
    {
        // Arrange
        string formattedInput = "Formatted lesson plan";
        SetupSuccessfulMocks(formattedInput: formattedInput);
        var controller = CreateController();
        var plan = CreateSampleLessonPlan();

        // Act
        await controller.Request(RequestId.ReviewTopic, plan);

        // Assert
        mMockAiDriver.Verify(
            d => d.ValidateUserInputAsync(formattedInput),
            Times.Once);
    }

    [Fact]
    public async Task Request_AfterSuccessfulValidation_CallsGetSuggestions()
    {
        // Arrange
        string prompt = "Test prompt";
        string formattedInput = "Formatted plan";
        SetupSuccessfulMocks(prompt: prompt, formattedInput: formattedInput);
        var controller = CreateController();
        var plan = CreateSampleLessonPlan();

        // Act
        await controller.Request(RequestId.ReviewTopic, plan);

        // Assert
        mMockAiDriver.Verify(
            d => d.GetSuggestionsAsync(prompt, formattedInput),
            Times.Once);
    }

    // Request Method - Validation Failure

    [Fact]
    public async Task Request_WhenValidationFails_ReturnsFailedResult()
    {
        // Arrange
        mMockPromptFactory
            .Setup(p => p.GetPrompt(It.IsAny<RequestId>()))
            .Returns("Test prompt");

        mMockFormatter
            .Setup(f => f.FormatLessonPlan(It.IsAny<LessonPlan>(), It.IsAny<RequestId>()))
            .Returns("Formatted plan");

        mMockAiDriver
            .Setup(d => d.ValidateUserInputAsync(It.IsAny<string>()))
            .ReturnsAsync(AIResult.Failed("UNSAFE_INPUT", "Input contains unsafe content"));

        var controller = CreateController();
        var plan = CreateSampleLessonPlan();

        // Act
        AIResult result = await controller.Request(RequestId.ReviewTopic, plan);

        // Assert
        result.Should().NotBeNull();
        result.Successful.Should().BeFalse();
        result.ErrorCode.Should().Be("VALIDATION_FAILED");
        result.Output.Should().Contain("UNSAFE_INPUT");
    }

    [Fact]
    public async Task Request_WhenValidationFails_DoesNotCallGetSuggestions()
    {
        // Arrange
        mMockPromptFactory
            .Setup(p => p.GetPrompt(It.IsAny<RequestId>()))
            .Returns("Test prompt");

        mMockFormatter
            .Setup(f => f.FormatLessonPlan(It.IsAny<LessonPlan>(), It.IsAny<RequestId>()))
            .Returns("Formatted plan");

        mMockAiDriver
            .Setup(d => d.ValidateUserInputAsync(It.IsAny<string>()))
            .ReturnsAsync(AIResult.Failed("UNSAFE_INPUT", "Input contains unsafe content"));

        var controller = CreateController();
        var plan = CreateSampleLessonPlan();

        // Act
        await controller.Request(RequestId.ReviewTopic, plan);

        // Assert
        mMockAiDriver.Verify(
            d => d.GetSuggestionsAsync(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    // Request Method - AI Driver Failure

    [Fact]
    public async Task Request_WhenAiDriverFails_ReturnsFailedResult()
    {
        // Arrange
        mMockPromptFactory
            .Setup(p => p.GetPrompt(It.IsAny<RequestId>()))
            .Returns("Test prompt");

        mMockFormatter
            .Setup(f => f.FormatLessonPlan(It.IsAny<LessonPlan>(), It.IsAny<RequestId>()))
            .Returns("Formatted plan");

        mMockAiDriver
            .Setup(d => d.ValidateUserInputAsync(It.IsAny<string>()))
            .ReturnsAsync(AIResult.Success("Validation passed"));

        mMockAiDriver
            .Setup(d => d.GetSuggestionsAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(AIResult.Failed("AI_ERROR", "AI service unavailable"));

        var controller = CreateController();
        var plan = CreateSampleLessonPlan();

        // Act
        AIResult result = await controller.Request(RequestId.ReviewTopic, plan);

        // Assert
        result.Should().NotBeNull();
        result.Successful.Should().BeFalse();
        result.ErrorCode.Should().Be("AI_ERROR");
        result.Output.Should().Be("AI service unavailable");
    }

    // Request Method - Exception Handling

    [Fact]
    public async Task Request_WhenPromptFactoryThrowsArgumentException_ReturnsInvalidRequestError()
    {
        // Arrange
        mMockPromptFactory
            .Setup(p => p.GetPrompt(It.IsAny<RequestId>()))
            .Throws(new ArgumentException("Invalid request ID"));

        var controller = CreateController();
        var plan = CreateSampleLessonPlan();

        // Act
        AIResult result = await controller.Request(RequestId.ReviewTopic, plan);

        // Assert
        result.Should().NotBeNull();
        result.Successful.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_REQUEST");
        result.Output.Should().Contain("Invalid request ID");
    }

    [Fact]
    public async Task Request_WhenFormatterThrowsArgumentException_ReturnsInvalidRequestError()
    {
        // Arrange
        mMockPromptFactory
            .Setup(p => p.GetPrompt(It.IsAny<RequestId>()))
            .Returns("Test prompt");

        mMockFormatter
            .Setup(f => f.FormatLessonPlan(It.IsAny<LessonPlan>(), It.IsAny<RequestId>()))
            .Throws(new ArgumentException("Invalid format configuration"));

        var controller = CreateController();
        var plan = CreateSampleLessonPlan();

        // Act
        AIResult result = await controller.Request(RequestId.ReviewTopic, plan);

        // Assert
        result.Should().NotBeNull();
        result.Successful.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_REQUEST");
        result.Output.Should().Contain("Invalid format configuration");
    }

    [Fact]
    public async Task Request_WhenUnexpectedExceptionOccurs_ReturnsInternalError()
    {
        // Arrange
        mMockPromptFactory
            .Setup(p => p.GetPrompt(It.IsAny<RequestId>()))
            .Throws(new InvalidOperationException("Unexpected error"));

        var controller = CreateController();
        var plan = CreateSampleLessonPlan();

        // Act
        AIResult result = await controller.Request(RequestId.ReviewTopic, plan);

        // Assert
        result.Should().NotBeNull();
        result.Successful.Should().BeFalse();
        result.ErrorCode.Should().Be("INTERNAL_ERROR");
        result.Output.Should().Be("An unexpected error occurred");
    }

    // Request Method - Logging Verification

    [Fact]
    public async Task Request_LogsInformationAtStart_WithRequestIdAndTopic()
    {
        // Arrange
        SetupSuccessfulMocks();
        var controller = CreateController();
        var plan = CreateSampleLessonPlan();
        plan.Topic = "Programming Basics";

        // Act
        await controller.Request(RequestId.ReviewTopic, plan);

        // Assert
        mMockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ReviewTopic") && v.ToString()!.Contains("Programming Basics")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Request_OnSuccess_LogsInformationWithRequestId()
    {
        // Arrange
        SetupSuccessfulMocks();
        var controller = CreateController();
        var plan = CreateSampleLessonPlan();

        // Act
        await controller.Request(RequestId.ReviewAudience, plan);

        // Assert
        mMockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully processed") && v.ToString()!.Contains("ReviewAudience")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Request_OnValidationFailure_LogsWarning()
    {
        // Arrange
        mMockPromptFactory
            .Setup(p => p.GetPrompt(It.IsAny<RequestId>()))
            .Returns("Test prompt");

        mMockFormatter
            .Setup(f => f.FormatLessonPlan(It.IsAny<LessonPlan>(), It.IsAny<RequestId>()))
            .Returns("Formatted plan");

        mMockAiDriver
            .Setup(d => d.ValidateUserInputAsync(It.IsAny<string>()))
            .ReturnsAsync(AIResult.Failed("UNSAFE_INPUT", "Input contains unsafe content"));

        var controller = CreateController();
        var plan = CreateSampleLessonPlan();

        // Act
        await controller.Request(RequestId.ReviewTopic, plan);

        // Assert
        mMockLogger.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("validation failed")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Request_OnAiFailure_LogsWarning()
    {
        // Arrange
        mMockPromptFactory
            .Setup(p => p.GetPrompt(It.IsAny<RequestId>()))
            .Returns("Test prompt");

        mMockFormatter
            .Setup(f => f.FormatLessonPlan(It.IsAny<LessonPlan>(), It.IsAny<RequestId>()))
            .Returns("Formatted plan");

        mMockAiDriver
            .Setup(d => d.ValidateUserInputAsync(It.IsAny<string>()))
            .ReturnsAsync(AIResult.Success("Validation passed"));

        mMockAiDriver
            .Setup(d => d.GetSuggestionsAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(AIResult.Failed("AI_ERROR", "AI service unavailable"));

        var controller = CreateController();
        var plan = CreateSampleLessonPlan();

        // Act
        await controller.Request(RequestId.ReviewTopic, plan);

        // Assert
        mMockLogger.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("failed") && v.ToString()!.Contains("AI_ERROR")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Request_OnException_LogsError()
    {
        // Arrange
        mMockPromptFactory
            .Setup(p => p.GetPrompt(It.IsAny<RequestId>()))
            .Throws(new InvalidOperationException("Unexpected error"));

        var controller = CreateController();
        var plan = CreateSampleLessonPlan();

        // Act
        await controller.Request(RequestId.ReviewTopic, plan);

        // Assert
        mMockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unexpected error")),
                It.IsAny<InvalidOperationException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // Integration Scenarios

    [Fact]
    public async Task Request_WithDifferentRequestIds_WorksCorrectly()
    {
        // Arrange
        var requestIds = new[]
        {
            RequestId.ReviewTopic,
            RequestId.SuggestAudience,
            RequestId.ReviewConnGoal,
            RequestId.ReviewWholeLesson
        };

        var controller = CreateController();
        var plan = CreateSampleLessonPlan();

        // Act & Assert
        foreach (RequestId requestId in requestIds)
        {
            SetupSuccessfulMocks();

            AIResult result = await controller.Request(requestId, plan);

            result.Should().NotBeNull($"RequestId {requestId} should return a result");
            result.Successful.Should().BeTrue($"RequestId {requestId} should succeed");

            mMockPromptFactory.Verify(
                p => p.GetPrompt(requestId),
                Times.Once,
                $"Should call GetPrompt for {requestId}");

            mMockFormatter.Verify(
                f => f.FormatLessonPlan(plan, requestId),
                Times.Once,
                $"Should call FormatLessonPlan for {requestId}");

            // Reset mocks for next iteration
            mMockPromptFactory.Reset();
            mMockFormatter.Reset();
            mMockAiDriver.Reset();
        }
    }

    [Fact]
    public async Task Request_PreservesAiDriverOutput_InResult()
    {
        // Arrange
        string expectedOutput = "This is the AI-generated suggestion with specific content";
        SetupSuccessfulMocks(aiOutput: expectedOutput);
        var controller = CreateController();
        var plan = CreateSampleLessonPlan();

        // Act
        AIResult result = await controller.Request(RequestId.SuggestTopic, plan);

        // Assert
        result.Output.Should().Be(expectedOutput);
    }
}
