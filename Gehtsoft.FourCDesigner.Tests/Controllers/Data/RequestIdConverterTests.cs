using Gehtsoft.FourCDesigner.Controllers.Data;
using Gehtsoft.FourCDesigner.Logic.Plan;
using Xunit;
using FluentAssertions;

namespace Gehtsoft.FourCDesigner.Tests.Controllers.Data;

public class RequestIdConverterTests
{
    // TryConvert - Success Cases

    [Theory]
    [InlineData("review_context", RequestId.ReviewContext)]
    [InlineData("suggest_context", RequestId.SuggestContext)]
    [InlineData("review_topic", RequestId.ReviewTopic)]
    [InlineData("suggest_topic", RequestId.SuggestTopic)]
    [InlineData("review_audience", RequestId.ReviewAudience)]
    [InlineData("suggest_audience", RequestId.SuggestAudience)]
    [InlineData("review_outcomes", RequestId.ReviewOutcomes)]
    [InlineData("suggest_outcomes", RequestId.SuggestOutcomes)]
    public void TryConvert_WithValidOverviewOperations_ReturnsTrue(string operationId, RequestId expected)
    {
        // Act
        bool result = RequestIdConverter.TryConvert(operationId, out RequestId actual);

        // Assert
        result.Should().BeTrue();
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData("review_conn_goal", RequestId.ReviewConnGoal)]
    [InlineData("suggest_conn_goal", RequestId.SuggestConnGoal)]
    [InlineData("review_conn_activities", RequestId.ReviewConnActivities)]
    [InlineData("suggest_conn_activities", RequestId.SuggestConnActivities)]
    [InlineData("review_conn_materials", RequestId.ReviewConnMaterials)]
    [InlineData("suggest_conn_materials", RequestId.SuggestConnMaterials)]
    public void TryConvert_WithValidConnectionOperations_ReturnsTrue(string operationId, RequestId expected)
    {
        // Act
        bool result = RequestIdConverter.TryConvert(operationId, out RequestId actual);

        // Assert
        result.Should().BeTrue();
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData("review_concepts_needToKnow", RequestId.ReviewConceptsNeedToKnow)]
    [InlineData("suggest_concepts_needToKnow", RequestId.SuggestConceptsNeedToKnow)]
    [InlineData("review_concepts_goodToKnow", RequestId.ReviewConceptsGoodToKnow)]
    [InlineData("suggest_concepts_goodToKnow", RequestId.SuggestConceptsGoodToKnow)]
    [InlineData("review_concepts_theses", RequestId.ReviewConceptsTheses)]
    [InlineData("suggest_concepts_theses", RequestId.SuggestConceptsTheses)]
    [InlineData("review_concepts_structure", RequestId.ReviewConceptsStructure)]
    [InlineData("suggest_concepts_structure", RequestId.SuggestConceptsStructure)]
    [InlineData("review_concepts_activities", RequestId.ReviewConceptsActivities)]
    [InlineData("suggest_concepts_activities", RequestId.SuggestConceptsActivities)]
    [InlineData("review_concepts_materials", RequestId.ReviewConceptsMaterials)]
    [InlineData("suggest_concepts_materials", RequestId.SuggestConceptsMaterials)]
    public void TryConvert_WithValidConceptsOperations_ReturnsTrue(string operationId, RequestId expected)
    {
        // Act
        bool result = RequestIdConverter.TryConvert(operationId, out RequestId actual);

        // Assert
        result.Should().BeTrue();
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData("review_practice_output", RequestId.ReviewPracticeOutput)]
    [InlineData("suggest_practice_output", RequestId.SuggestPracticeOutput)]
    [InlineData("review_practice_focus", RequestId.ReviewPracticeFocus)]
    [InlineData("suggest_practice_focus", RequestId.SuggestPracticeFocus)]
    [InlineData("review_practice_activities", RequestId.ReviewPracticeActivities)]
    [InlineData("suggest_practice_activities", RequestId.SuggestPracticeActivities)]
    [InlineData("review_practice_details", RequestId.ReviewPracticeDetails)]
    [InlineData("suggest_practice_details", RequestId.SuggestPracticeDetails)]
    [InlineData("review_practice_materials", RequestId.ReviewPracticeMaterials)]
    [InlineData("suggest_practice_materials", RequestId.SuggestPracticeMaterials)]
    public void TryConvert_WithValidPracticeOperations_ReturnsTrue(string operationId, RequestId expected)
    {
        // Act
        bool result = RequestIdConverter.TryConvert(operationId, out RequestId actual);

        // Assert
        result.Should().BeTrue();
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData("review_concl_goal", RequestId.ReviewConclGoal)]
    [InlineData("suggest_concl_goal", RequestId.SuggestConclGoal)]
    [InlineData("review_concl_activities", RequestId.ReviewConclActivities)]
    [InlineData("suggest_concl_activities", RequestId.SuggestConclActivities)]
    [InlineData("review_concl_materials", RequestId.ReviewConclMaterials)]
    [InlineData("suggest_concl_materials", RequestId.SuggestConclMaterials)]
    public void TryConvert_WithValidConclusionOperations_ReturnsTrue(string operationId, RequestId expected)
    {
        // Act
        bool result = RequestIdConverter.TryConvert(operationId, out RequestId actual);

        // Assert
        result.Should().BeTrue();
        actual.Should().Be(expected);
    }

    [Fact]
    public void TryConvert_WithReviewWholeLesson_ReturnsTrue()
    {
        // Act
        bool result = RequestIdConverter.TryConvert("review_whole_lesson", out RequestId actual);

        // Assert
        result.Should().BeTrue();
        actual.Should().Be(RequestId.ReviewWholeLesson);
    }

    // TryConvert - Failure Cases

    [Theory]
    [InlineData("invalid_operation")]
    [InlineData("review_invalid")]
    [InlineData("REVIEW_TOPIC")]
    [InlineData("ReviewTopic")]
    [InlineData("review-topic")]
    [InlineData("")]
    [InlineData("   ")]
    public void TryConvert_WithInvalidOperationId_ReturnsFalse(string operationId)
    {
        // Act
        bool result = RequestIdConverter.TryConvert(operationId, out RequestId actual);

        // Assert
        result.Should().BeFalse();
        actual.Should().Be(default(RequestId));
    }

    [Fact]
    public void TryConvert_WithNullOperationId_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => RequestIdConverter.TryConvert(null!, out _);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("operationId");
    }

    // Convert - Success Cases

    [Fact]
    public void Convert_WithValidOperationId_ReturnsCorrectRequestId()
    {
        // Arrange
        string operationId = "review_topic";

        // Act
        RequestId result = RequestIdConverter.Convert(operationId);

        // Assert
        result.Should().Be(RequestId.ReviewTopic);
    }

    [Fact]
    public void Convert_WithSuggestOperation_ReturnsCorrectRequestId()
    {
        // Arrange
        string operationId = "suggest_audience";

        // Act
        RequestId result = RequestIdConverter.Convert(operationId);

        // Assert
        result.Should().Be(RequestId.SuggestAudience);
    }

    // Convert - Failure Cases

    [Fact]
    public void Convert_WithInvalidOperationId_ThrowsArgumentException()
    {
        // Arrange
        string operationId = "invalid_operation";

        // Act
        Action act = () => RequestIdConverter.Convert(operationId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Unknown operation identifier*")
            .WithParameterName("operationId");
    }

    [Fact]
    public void Convert_WithEmptyString_ThrowsArgumentException()
    {
        // Arrange
        string operationId = "";

        // Act
        Action act = () => RequestIdConverter.Convert(operationId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Unknown operation identifier*");
    }

    [Fact]
    public void Convert_WithNullOperationId_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => RequestIdConverter.Convert(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("operationId");
    }

    // IsValid Method Tests

    [Theory]
    [InlineData("review_topic")]
    [InlineData("suggest_audience")]
    [InlineData("review_whole_lesson")]
    [InlineData("review_conn_goal")]
    [InlineData("suggest_practice_output")]
    public void IsValid_WithValidOperationId_ReturnsTrue(string operationId)
    {
        // Act
        bool result = RequestIdConverter.IsValid(operationId);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("invalid_operation")]
    [InlineData("REVIEW_TOPIC")]
    [InlineData("ReviewTopic")]
    [InlineData("review-topic")]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValid_WithInvalidOperationId_ReturnsFalse(string operationId)
    {
        // Act
        bool result = RequestIdConverter.IsValid(operationId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithNullOperationId_ReturnsFalse()
    {
        // Act
        bool result = RequestIdConverter.IsValid(null!);

        // Assert
        result.Should().BeFalse();
    }

    // Case Sensitivity Tests

    [Fact]
    public void TryConvert_IsCaseSensitive()
    {
        // Act
        bool resultLower = RequestIdConverter.TryConvert("review_topic", out _);
        bool resultUpper = RequestIdConverter.TryConvert("REVIEW_TOPIC", out _);
        bool resultMixed = RequestIdConverter.TryConvert("Review_Topic", out _);

        // Assert
        resultLower.Should().BeTrue("lowercase should be valid");
        resultUpper.Should().BeFalse("uppercase should be invalid");
        resultMixed.Should().BeFalse("mixed case should be invalid");
    }

    // Coverage Test - All 43 Operations

    [Fact]
    public void Converter_SupportsAll43Operations()
    {
        // Arrange
        var allOperationIds = new[]
        {
            "review_context", "suggest_context",
            "review_topic", "suggest_topic",
            "review_audience", "suggest_audience",
            "review_outcomes", "suggest_outcomes",
            "review_conn_goal", "suggest_conn_goal",
            "review_conn_activities", "suggest_conn_activities",
            "review_conn_materials", "suggest_conn_materials",
            "review_concepts_needToKnow", "suggest_concepts_needToKnow",
            "review_concepts_goodToKnow", "suggest_concepts_goodToKnow",
            "review_concepts_theses", "suggest_concepts_theses",
            "review_concepts_structure", "suggest_concepts_structure",
            "review_concepts_activities", "suggest_concepts_activities",
            "review_concepts_materials", "suggest_concepts_materials",
            "review_practice_output", "suggest_practice_output",
            "review_practice_focus", "suggest_practice_focus",
            "review_practice_activities", "suggest_practice_activities",
            "review_practice_details", "suggest_practice_details",
            "review_practice_materials", "suggest_practice_materials",
            "review_concl_goal", "suggest_concl_goal",
            "review_concl_activities", "suggest_concl_activities",
            "review_concl_materials", "suggest_concl_materials",
            "review_whole_lesson"
        };

        // Act & Assert
        allOperationIds.Length.Should().Be(43, "should have exactly 43 operation IDs");

        foreach (string operationId in allOperationIds)
        {
            bool isValid = RequestIdConverter.IsValid(operationId);
            isValid.Should().BeTrue($"'{operationId}' should be a valid operation ID");

            bool canConvert = RequestIdConverter.TryConvert(operationId, out RequestId requestId);
            canConvert.Should().BeTrue($"'{operationId}' should be convertible");

            // Verify it maps to a defined enum value
            Enum.IsDefined(typeof(RequestId), requestId).Should().BeTrue($"'{operationId}' should map to a defined RequestId");
        }
    }

    // Bidirectional Mapping Test

    [Fact]
    public void Converter_MapsAllRequestIdEnumValues()
    {
        // Arrange
        var allRequestIds = Enum.GetValues<RequestId>();

        // Act & Assert
        foreach (RequestId requestId in allRequestIds)
        {
            bool found = false;

            // Try to find a matching operation ID
            var testOperationIds = new[]
            {
                "review_context", "suggest_context",
                "review_topic", "suggest_topic",
                "review_audience", "suggest_audience",
                "review_outcomes", "suggest_outcomes",
                "review_conn_goal", "suggest_conn_goal",
                "review_conn_activities", "suggest_conn_activities",
                "review_conn_materials", "suggest_conn_materials",
                "review_concepts_needToKnow", "suggest_concepts_needToKnow",
                "review_concepts_goodToKnow", "suggest_concepts_goodToKnow",
                "review_concepts_theses", "suggest_concepts_theses",
                "review_concepts_structure", "suggest_concepts_structure",
                "review_concepts_activities", "suggest_concepts_activities",
                "review_concepts_materials", "suggest_concepts_materials",
                "review_practice_output", "suggest_practice_output",
                "review_practice_focus", "suggest_practice_focus",
                "review_practice_activities", "suggest_practice_activities",
                "review_practice_details", "suggest_practice_details",
                "review_practice_materials", "suggest_practice_materials",
                "review_concl_goal", "suggest_concl_goal",
                "review_concl_activities", "suggest_concl_activities",
                "review_concl_materials", "suggest_concl_materials",
                "review_whole_lesson"
            };

            foreach (string opId in testOperationIds)
            {
                if (RequestIdConverter.TryConvert(opId, out RequestId converted) && converted == requestId)
                {
                    found = true;
                    break;
                }
            }

            found.Should().BeTrue($"RequestId.{requestId} should have a corresponding operation ID");
        }
    }
}
