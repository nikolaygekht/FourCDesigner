using Gehtsoft.FourCDesigner.Logic.Plan;
using Xunit;
using FluentAssertions;

namespace Gehtsoft.FourCDesigner.Tests.Logic.Plan;

public class LessonPlanFormatterTests
{
    private static LessonPlan CreateSampleLessonPlan()
    {
        return new LessonPlan
        {
            Topic = "Introduction to Programming",
            Audience = "High school students, ages 15-17, basic computer skills",
            LearningOutcomes = "Students will be able to write basic programs using variables and loops",
            Connections = new LessonPlanConnections
            {
                Timing = 5,
                Goal = "Connect students to programming concepts and to each other",
                Activities = "Icebreaker: share experiences with technology in daily life",
                MaterialsToPrepare = "Sticky notes, markers, whiteboard"
            },
            Concepts = new LessonPlanConcepts
            {
                Timing = 15,
                NeedToKnow = "Variables, data types, loops, conditional statements",
                GoodToKnow = "History of programming languages, career paths in software development",
                Theses = "Programming is problem-solving; Code is written for humans first, computers second",
                Structure = "Introduction to variables (5 min), Data types (3 min), Control flow (7 min)",
                Activities = "Visual: code syntax diagrams; Kinesthetic: human loops game; Writing: pseudo-code exercises",
                MaterialsToPrepare = "Printed code examples, markers, large paper sheets"
            },
            ConcretePractice = new LessonPlanConcretePractice
            {
                Timing = 25,
                DesiredOutput = "A working program that takes user input and produces output",
                FocusArea = "Understanding variable scope and loop termination conditions",
                Activities = "Pair programming: create a number guessing game",
                Details = "Step 1: Design algorithm (5 min); Step 2: Write code (15 min); Step 3: Test and debug (5 min)",
                MaterialsToPrepare = "Computers with IDE installed, starter code templates, debugging checklist"
            },
            Conclusions = new LessonPlanConclusions
            {
                Timing = 5,
                Goal = "Reflect on learning and plan next steps",
                Activities = "Exit ticket: write one thing learned, one question, and one way to use programming",
                MaterialsToPrepare = "Exit ticket forms, pens"
            }
        };
    }

    [Fact]
    public void Constructor_InitializesSuccessfully()
    {
        // Act
        Action act = () => new LessonPlanFormatter();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void FormatLessonPlan_WithNullPlan_ThrowsArgumentNullException()
    {
        // Arrange
        var formatter = new LessonPlanFormatter();

        // Act
        Action act = () => formatter.FormatLessonPlan(null!, RequestId.ReviewTopic);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("plan");
    }

    [Fact]
    public void FormatLessonPlan_WithInvalidRequestId_ThrowsArgumentException()
    {
        // Arrange
        var formatter = new LessonPlanFormatter();
        var plan = CreateSampleLessonPlan();
        RequestId invalidRequestId = (RequestId)(-1);

        // Act
        Action act = () => formatter.FormatLessonPlan(plan, invalidRequestId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Unknown request ID*")
            .WithParameterName("requestId");
    }

    [Fact]
    public void FormatLessonPlan_ReviewTopic_ReturnsOnlyTopic()
    {
        // Arrange
        var formatter = new LessonPlanFormatter();
        var plan = CreateSampleLessonPlan();

        // Act
        string result = formatter.FormatLessonPlan(plan, RequestId.ReviewTopic);

        // Assert
        result.Should().Contain("Topic to Review");
        result.Should().Contain(plan.Topic);
        result.Should().NotContain(plan.Audience);
        result.Should().NotContain(plan.LearningOutcomes);
    }

    [Fact]
    public void FormatLessonPlan_ReviewAudience_ReturnsTopicAndAudience()
    {
        // Arrange
        var formatter = new LessonPlanFormatter();
        var plan = CreateSampleLessonPlan();

        // Act
        string result = formatter.FormatLessonPlan(plan, RequestId.ReviewAudience);

        // Assert
        result.Should().Contain("Topic");
        result.Should().Contain(plan.Topic);
        result.Should().Contain("Audience to Review");
        result.Should().Contain(plan.Audience);
        result.Should().NotContain(plan.LearningOutcomes);
    }

    [Fact]
    public void FormatLessonPlan_ReviewOutcomes_ReturnsTopicAudienceOutcomes()
    {
        // Arrange
        var formatter = new LessonPlanFormatter();
        var plan = CreateSampleLessonPlan();

        // Act
        string result = formatter.FormatLessonPlan(plan, RequestId.ReviewOutcomes);

        // Assert
        result.Should().Contain("Topic");
        result.Should().Contain(plan.Topic);
        result.Should().Contain("Audience");
        result.Should().Contain(plan.Audience);
        result.Should().Contain("Learning Outcomes to Review");
        result.Should().Contain(plan.LearningOutcomes);
    }

    [Fact]
    public void FormatLessonPlan_SuggestTopic_ContainsCurrentTopic()
    {
        // Arrange
        var formatter = new LessonPlanFormatter();
        var plan = CreateSampleLessonPlan();

        // Act
        string result = formatter.FormatLessonPlan(plan, RequestId.SuggestTopic);

        // Assert
        result.Should().Contain("Current Topic");
        result.Should().Contain(plan.Topic);
    }

    [Fact]
    public void FormatLessonPlan_ReviewConnGoal_ReturnsCorrectFields()
    {
        // Arrange
        var formatter = new LessonPlanFormatter();
        var plan = CreateSampleLessonPlan();

        // Act
        string result = formatter.FormatLessonPlan(plan, RequestId.ReviewConnGoal);

        // Assert
        result.Should().Contain("Topic");
        result.Should().Contain(plan.Topic);
        result.Should().Contain("Audience");
        result.Should().Contain(plan.Audience);
        result.Should().Contain("Learning Outcomes");
        result.Should().Contain(plan.LearningOutcomes);
        result.Should().Contain("Connection Phase Timing");
        result.Should().Contain("5 minutes");
        result.Should().Contain("Connection Goal to Review");
        result.Should().Contain(plan.Connections.Goal);
    }

    [Fact]
    public void FormatLessonPlan_SuggestConnGoal_ExcludesCurrentGoal()
    {
        // Arrange
        var formatter = new LessonPlanFormatter();
        var plan = CreateSampleLessonPlan();

        // Act
        string result = formatter.FormatLessonPlan(plan, RequestId.SuggestConnGoal);

        // Assert
        result.Should().Contain("Topic");
        result.Should().Contain("Connection Phase Timing");
        result.Should().NotContain("Connection Goal to Review");
        result.Should().NotContain(plan.Connections.Goal);
    }

    [Fact]
    public void FormatLessonPlan_ReviewConnActivities_ReturnsCorrectFields()
    {
        // Arrange
        var formatter = new LessonPlanFormatter();
        var plan = CreateSampleLessonPlan();

        // Act
        string result = formatter.FormatLessonPlan(plan, RequestId.ReviewConnActivities);

        // Assert
        result.Should().Contain("Connection Goal");
        result.Should().Contain(plan.Connections.Goal);
        result.Should().Contain("Connection Activities to Review");
        result.Should().Contain(plan.Connections.Activities);
    }

    [Fact]
    public void FormatLessonPlan_ReviewConnMaterials_ReturnsCorrectFields()
    {
        // Arrange
        var formatter = new LessonPlanFormatter();
        var plan = CreateSampleLessonPlan();

        // Act
        string result = formatter.FormatLessonPlan(plan, RequestId.ReviewConnMaterials);

        // Assert
        result.Should().Contain("Connection Activities");
        result.Should().Contain(plan.Connections.Activities);
        result.Should().Contain("Materials to Review");
        result.Should().Contain(plan.Connections.MaterialsToPrepare);
    }

    [Fact]
    public void FormatLessonPlan_ReviewConceptsNeedToKnow_ReturnsCorrectFields()
    {
        // Arrange
        var formatter = new LessonPlanFormatter();
        var plan = CreateSampleLessonPlan();

        // Act
        string result = formatter.FormatLessonPlan(plan, RequestId.ReviewConceptsNeedToKnow);

        // Assert
        result.Should().Contain("Topic");
        result.Should().Contain("Audience");
        result.Should().Contain("Learning Outcomes");
        result.Should().Contain("Concepts Phase Timing");
        result.Should().Contain("15 minutes");
        result.Should().Contain("Need to Know Concepts to Review");
        result.Should().Contain(plan.Concepts.NeedToKnow);
    }

    [Fact]
    public void FormatLessonPlan_ReviewConceptsActivities_ReturnsCorrectFields()
    {
        // Arrange
        var formatter = new LessonPlanFormatter();
        var plan = CreateSampleLessonPlan();

        // Act
        string result = formatter.FormatLessonPlan(plan, RequestId.ReviewConceptsActivities);

        // Assert
        result.Should().Contain("Need to Know Concepts");
        result.Should().Contain("Theses");
        result.Should().Contain("Structure");
        result.Should().Contain("Activities to Review");
        result.Should().Contain(plan.Concepts.Activities);
    }

    [Fact]
    public void FormatLessonPlan_ReviewPracticeOutput_ReturnsCorrectFields()
    {
        // Arrange
        var formatter = new LessonPlanFormatter();
        var plan = CreateSampleLessonPlan();

        // Act
        string result = formatter.FormatLessonPlan(plan, RequestId.ReviewPracticeOutput);

        // Assert
        result.Should().Contain("Practice Phase Timing");
        result.Should().Contain("25 minutes");
        result.Should().Contain("Need to Know Concepts");
        result.Should().Contain("Desired Output to Review");
        result.Should().Contain(plan.ConcretePractice.DesiredOutput);
    }

    [Fact]
    public void FormatLessonPlan_ReviewPracticeActivities_ReturnsCorrectFields()
    {
        // Arrange
        var formatter = new LessonPlanFormatter();
        var plan = CreateSampleLessonPlan();

        // Act
        string result = formatter.FormatLessonPlan(plan, RequestId.ReviewPracticeActivities);

        // Assert
        result.Should().Contain("Desired Output");
        result.Should().Contain("Focus Area");
        result.Should().Contain("Activities to Review");
        result.Should().Contain(plan.ConcretePractice.Activities);
    }

    [Fact]
    public void FormatLessonPlan_ReviewConclGoal_ReturnsCorrectFields()
    {
        // Arrange
        var formatter = new LessonPlanFormatter();
        var plan = CreateSampleLessonPlan();

        // Act
        string result = formatter.FormatLessonPlan(plan, RequestId.ReviewConclGoal);

        // Assert
        result.Should().Contain("Conclusion Phase Timing");
        result.Should().Contain("5 minutes");
        result.Should().Contain("Conclusion Goal to Review");
        result.Should().Contain(plan.Conclusions.Goal);
    }

    [Fact]
    public void FormatLessonPlan_ReviewWholeLesson_ReturnsAllFields()
    {
        // Arrange
        var formatter = new LessonPlanFormatter();
        var plan = CreateSampleLessonPlan();

        // Act
        string result = formatter.FormatLessonPlan(plan, RequestId.ReviewWholeLesson);

        // Assert
        result.Should().Contain(plan.Topic);
        result.Should().Contain(plan.Audience);
        result.Should().Contain(plan.LearningOutcomes);
        result.Should().Contain("Connections Phase");
        result.Should().Contain("Concepts Phase");
        result.Should().Contain("Concrete Practice Phase");
        result.Should().Contain("Conclusions Phase");
        result.Should().Contain(plan.Connections.Goal);
        result.Should().Contain(plan.Concepts.NeedToKnow);
        result.Should().Contain(plan.ConcretePractice.DesiredOutput);
        result.Should().Contain(plan.Conclusions.Activities);
    }

    [Fact]
    public void FormatLessonPlan_AllRequests_ReturnMarkdownFormat()
    {
        // Arrange
        var formatter = new LessonPlanFormatter();
        var plan = CreateSampleLessonPlan();
        var requestIds = new[] { RequestId.ReviewTopic, RequestId.ReviewAudience, RequestId.ReviewConnGoal };

        // Act & Assert
        foreach (RequestId requestId in requestIds)
        {
            string result = formatter.FormatLessonPlan(plan, requestId);
            result.Should().Contain("# ", $"RequestId {requestId} should use markdown headers");
        }
    }

    [Fact]
    public void FormatLessonPlan_AllRequestIds_ReturnNonEmptyString()
    {
        // Arrange
        var formatter = new LessonPlanFormatter();
        var plan = CreateSampleLessonPlan();
        var allRequestIds = Enum.GetValues<RequestId>();

        // Act & Assert
        foreach (RequestId requestId in allRequestIds)
        {
            string result = formatter.FormatLessonPlan(plan, requestId);
            result.Should().NotBeNullOrEmpty($"RequestId {requestId} should return formatted output");
        }
    }

    [Fact]
    public void FormatLessonPlan_PreservesUserContent_NoEscaping()
    {
        // Arrange
        var formatter = new LessonPlanFormatter();
        var plan = CreateSampleLessonPlan();
        plan.Topic = "Special characters: <>&\"'";

        // Act
        string result = formatter.FormatLessonPlan(plan, RequestId.ReviewTopic);

        // Assert
        result.Should().Contain("<>&\"'");
    }

    [Fact]
    public void FormatLessonPlan_WithEmptyFields_HandlesGracefully()
    {
        // Arrange
        var formatter = new LessonPlanFormatter();
        var plan = new LessonPlan
        {
            Topic = "",
            Audience = "",
            LearningOutcomes = "",
            Connections = new LessonPlanConnections(),
            Concepts = new LessonPlanConcepts(),
            ConcretePractice = new LessonPlanConcretePractice(),
            Conclusions = new LessonPlanConclusions()
        };

        // Act
        Action act = () => formatter.FormatLessonPlan(plan, RequestId.ReviewTopic);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void FormatLessonPlan_AllRequestIds_HaveConfiguration()
    {
        // Arrange
        var formatter = new LessonPlanFormatter();
        var plan = CreateSampleLessonPlan();
        var allRequestIds = Enum.GetValues<RequestId>();

        // Act & Assert
        foreach (RequestId requestId in allRequestIds)
        {
            Action act = () => formatter.FormatLessonPlan(plan, requestId);
            act.Should().NotThrow($"RequestId {requestId} should have a format configuration");
        }
    }

    [Fact]
    public void FormatLessonPlan_ReviewVsSuggest_ProducesDifferentOutput()
    {
        // Arrange
        var formatter = new LessonPlanFormatter();
        var plan = CreateSampleLessonPlan();

        // Act
        string reviewResult = formatter.FormatLessonPlan(plan, RequestId.ReviewConnGoal);
        string suggestResult = formatter.FormatLessonPlan(plan, RequestId.SuggestConnGoal);

        // Assert
        reviewResult.Should().NotBe(suggestResult);
        reviewResult.Should().Contain(plan.Connections.Goal);
        suggestResult.Should().NotContain(plan.Connections.Goal);
    }

    [Fact]
    public void FormatLessonPlan_IncludesTiming_InCorrectFormat()
    {
        // Arrange
        var formatter = new LessonPlanFormatter();
        var plan = CreateSampleLessonPlan();
        plan.Connections.Timing = 10;

        // Act
        string result = formatter.FormatLessonPlan(plan, RequestId.ReviewConnGoal);

        // Assert
        result.Should().Contain("10 minutes");
    }

    [Fact]
    public void FormatLessonPlan_SeparatesFieldsWithBlankLines()
    {
        // Arrange
        var formatter = new LessonPlanFormatter();
        var plan = CreateSampleLessonPlan();

        // Act
        string result = formatter.FormatLessonPlan(plan, RequestId.ReviewOutcomes);

        // Assert
        string[] lines = result.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        bool hasBlankLine = lines.Any(line => string.IsNullOrWhiteSpace(line));
        hasBlankLine.Should().BeTrue("Fields should be separated by blank lines");
    }
}
