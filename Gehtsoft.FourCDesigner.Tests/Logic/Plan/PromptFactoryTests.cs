using Gehtsoft.FourCDesigner.Logic.Plan;
using Xunit;
using FluentAssertions;

namespace Gehtsoft.FourCDesigner.Tests.Logic.Plan;

public class PromptFactoryTests
{
    [Fact]
    public void Constructor_InitializesSuccessfully()
    {
        // Act
        Action act = () => new PromptFactory();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void GetPrompt_ForReviewTopic_ReturnsValidPrompt()
    {
        // Arrange
        var factory = new PromptFactory();

        // Act
        string prompt = factory.GetPrompt(RequestId.ReviewTopic);

        // Assert
        prompt.Should().NotBeNullOrEmpty();
        prompt.Should().Contain("topic");
    }

    [Fact]
    public void GetPrompt_ForSuggestTopic_ReturnsValidPrompt()
    {
        // Arrange
        var factory = new PromptFactory();

        // Act
        string prompt = factory.GetPrompt(RequestId.SuggestTopic);

        // Assert
        prompt.Should().NotBeNullOrEmpty();
        prompt.Should().Contain("topic");
    }

    [Fact]
    public void GetPrompt_ForReviewWholeLesson_ReturnsValidPrompt()
    {
        // Arrange
        var factory = new PromptFactory();

        // Act
        string prompt = factory.GetPrompt(RequestId.ReviewWholeLesson);

        // Assert
        prompt.Should().NotBeNullOrEmpty();
        prompt.Should().Contain("lesson");
    }

    [Fact]
    public void GetPrompt_ForAllRequestIds_ReturnsNonEmptyPrompts()
    {
        // Arrange
        var factory = new PromptFactory();
        var allRequestIds = Enum.GetValues<RequestId>();

        // Act & Assert
        foreach (RequestId requestId in allRequestIds)
        {
            string prompt = factory.GetPrompt(requestId);
            prompt.Should().NotBeNullOrEmpty($"RequestId {requestId} should have a prompt");
        }
    }

    [Fact]
    public void GetPrompt_ReturnsPromptWithAiRole()
    {
        // Arrange
        var factory = new PromptFactory();

        // Act
        string prompt = factory.GetPrompt(RequestId.ReviewTopic);

        // Assert
        prompt.Should().Contain("Knowledge Base");
        prompt.Should().Contain("Coach");
        prompt.Should().Contain("Critic");
    }

    [Fact]
    public void GetPrompt_ReturnsPromptWithContext()
    {
        // Arrange
        var factory = new PromptFactory();

        // Act
        string prompt = factory.GetPrompt(RequestId.ReviewTopic);

        // Assert
        prompt.Should().Contain("Training from the Back of the Room");
        prompt.Should().Contain("4C");
        prompt.Should().Contain("Connections");
        prompt.Should().Contain("Concepts");
        prompt.Should().Contain("Concrete Practice");
        prompt.Should().Contain("Conclusions");
        prompt.Should().Contain("Six Trumps");
    }

    [Fact]
    public void GetPrompt_ReturnsPromptWithSpecificInstruction()
    {
        // Arrange
        var factory = new PromptFactory();

        // Act
        string promptReview = factory.GetPrompt(RequestId.ReviewTopic);
        string promptSuggest = factory.GetPrompt(RequestId.SuggestTopic);

        // Assert
        promptReview.Should().Contain("Review");
        promptSuggest.Should().Contain("suggest");
        promptReview.Should().NotBe(promptSuggest);
    }

    [Fact]
    public void GetPrompt_WithInvalidRequestId_ThrowsArgumentException()
    {
        // Arrange
        var factory = new PromptFactory();
        RequestId invalidRequestId = (RequestId)(-1);

        // Act
        Action act = () => factory.GetPrompt(invalidRequestId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Unknown request ID*")
            .WithParameterName("requestId");
    }

    [Fact]
    public void GetPrompt_AllPrompts_ContainRequiredComponents()
    {
        // Arrange
        var factory = new PromptFactory();
        var allRequestIds = Enum.GetValues<RequestId>();

        // Act & Assert
        foreach (RequestId requestId in allRequestIds)
        {
            string prompt = factory.GetPrompt(requestId);

            prompt.Should().Contain("Knowledge Base", $"RequestId {requestId} prompt should contain AI role");
            prompt.Should().Contain("4C", $"RequestId {requestId} prompt should contain context");
            prompt.Should().NotBeNullOrEmpty($"RequestId {requestId} should have a prompt");
        }
    }

    [Fact]
    public void GetPrompt_ConnectionPhaseOperations_ContainConnectionKeywords()
    {
        // Arrange
        var factory = new PromptFactory();

        // Act
        string promptGoal = factory.GetPrompt(RequestId.ReviewConnGoal);
        string promptActivities = factory.GetPrompt(RequestId.ReviewConnActivities);

        // Assert
        promptGoal.Should().Contain("connection");
        promptActivities.Should().Contain("connection");
    }

    [Fact]
    public void GetPrompt_ConceptsPhaseOperations_ContainConceptsKeywords()
    {
        // Arrange
        var factory = new PromptFactory();

        // Act
        string promptNeedToKnow = factory.GetPrompt(RequestId.ReviewConceptsNeedToKnow);
        string promptActivities = factory.GetPrompt(RequestId.ReviewConceptsActivities);

        // Assert
        promptNeedToKnow.Should().Contain("concept");
        promptActivities.Should().Contain("VARK");
        promptActivities.Should().Contain("six trumps");
    }

    [Fact]
    public void GetPrompt_PracticePhaseOperations_ContainPracticeKeywords()
    {
        // Arrange
        var factory = new PromptFactory();

        // Act
        string promptOutput = factory.GetPrompt(RequestId.ReviewPracticeOutput);
        string promptActivities = factory.GetPrompt(RequestId.ReviewPracticeActivities);

        // Assert
        promptOutput.Should().Contain("practice");
        promptActivities.Should().Contain("practice");
    }

    [Fact]
    public void GetPrompt_ConclusionPhaseOperations_ContainConclusionKeywords()
    {
        // Arrange
        var factory = new PromptFactory();

        // Act
        string promptGoal = factory.GetPrompt(RequestId.ReviewConclGoal);
        string promptActivities = factory.GetPrompt(RequestId.ReviewConclActivities);

        // Assert
        promptGoal.Should().Contain("conclusion");
        promptActivities.Should().Contain("summarize");
    }
}
