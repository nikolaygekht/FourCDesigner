// Suppress nullability warnings for intentional null tests
#pragma warning disable CS8625, CS8602, CS8600, CS8620
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Gehtsoft.FourCDesigner.Controllers.Data;
using Gehtsoft.FourCDesigner.Logic.AI;
using Gehtsoft.FourCDesigner.Logic.Plan;
using Microsoft.Data.Sqlite;

namespace Gehtsoft.FourCDesigner.ApiTests;

/// <summary>
/// API tests for Plan AI assistance endpoints.
/// </summary>
[Collection("ApiTests")]
public class PlanApiControllerApiTests : IDisposable
{
    private readonly TestWebApplicationFactory mFactory;
    private readonly HttpClient mClient;
    private readonly SqliteConnection mConnection;
    private readonly string mDatabaseName;
    private string? mSessionId;

    public PlanApiControllerApiTests()
    {
        // Create unique database name for this test instance
        mDatabaseName = $"TestDb_{Guid.NewGuid():N}";

        // Create and keep alive an in-memory SQLite connection
        mConnection = new SqliteConnection($"Data Source={mDatabaseName};Mode=Memory;Cache=Shared");
        mConnection.Open();

        // Create the web application factory with the in-memory database and AI testing driver
        mFactory = new TestWebApplicationFactory(mDatabaseName, enableAITesting: true);
        mClient = mFactory.CreateClient();
    }

    public void Dispose()
    {
        mClient?.Dispose();
        mFactory?.Dispose();
        mConnection?.Dispose();
    }

    /// <summary>
    /// Logs in and stores the session ID for authenticated requests.
    /// </summary>
    private async Task LoginAsync()
    {
        var loginRequest = new LoginRequest
        {
            Email = "user@fourcdesign.com",
            Password = "test123"
        };

        var response = await mClient.PostAsJsonAsync("/api/user/login", loginRequest);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result.Should().NotBeNull();
        mSessionId = result!.SessionId;
    }

    /// <summary>
    /// Adds authentication header to the client.
    /// </summary>
    private void AddAuthHeader()
    {
        if (!string.IsNullOrEmpty(mSessionId))
        {
            mClient.DefaultRequestHeaders.Add("X-fourc-session", mSessionId);
        }
    }

    /// <summary>
    /// Creates a simple lesson plan for testing.
    /// </summary>
    private static LessonPlan CreateSampleLessonPlan()
    {
        return new LessonPlan
        {
            Topic = "Introduction to Programming",
            Audience = "High school students, ages 15-17",
            LearningOutcomes = "Students will be able to write basic programs",
            Connections = new LessonPlanConnections
            {
                Timing = 5,
                Goal = "Connect students to programming",
                Activities = "Icebreaker activities",
                MaterialsToPrepare = "Sticky notes"
            },
            Concepts = new LessonPlanConcepts
            {
                Timing = 15,
                NeedToKnow = "Variables, loops",
                GoodToKnow = "History of programming",
                Theses = "Programming is problem-solving",
                Structure = "Introduction, practice, review",
                Activities = "Coding exercises",
                MaterialsToPrepare = "Computers"
            },
            ConcretePractice = new LessonPlanConcretePractice
            {
                Timing = 25,
                DesiredOutput = "Working program",
                FocusArea = "Variable scope",
                Activities = "Pair programming",
                Details = "Step-by-step guide",
                MaterialsToPrepare = "IDE installed"
            },
            Conclusions = new LessonPlanConclusions
            {
                Timing = 5,
                Goal = "Reflect on learning",
                Activities = "Exit ticket",
                MaterialsToPrepare = "Forms"
            }
        };
    }

    #region Authorization Tests

    [Fact]
    public async Task RequestAssistance_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new PlanAiRequest
        {
            OperationId = "review_topic",
            Plan = CreateSampleLessonPlan()
        };

        // Act - Don't add auth header
        var response = await mClient.PostAsJsonAsync("/api/plan/assistance", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RequestAssistance_WithInvalidSession_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new PlanAiRequest
        {
            OperationId = "review_topic",
            Plan = CreateSampleLessonPlan()
        };

        // Add invalid session header
        mClient.DefaultRequestHeaders.Add("X-fourc-session", "invalid-session-id");

        // Act
        var response = await mClient.PostAsJsonAsync("/api/plan/assistance", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Happy Path Tests

    [Fact]
    public async Task RequestAssistance_ReviewTopic_ShouldReturnSuccessfulResult()
    {
        // Arrange
        await LoginAsync();
        AddAuthHeader();

        var request = new PlanAiRequest
        {
            OperationId = "review_topic",
            Plan = CreateSampleLessonPlan()
        };

        // Act
        var response = await mClient.PostAsJsonAsync("/api/plan/assistance", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<AIResult>();
        result.Should().NotBeNull();
        result!.Successful.Should().BeTrue();
        result.Output.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RequestAssistance_SuggestAudience_ShouldReturnSuccessfulResult()
    {
        // Arrange
        await LoginAsync();
        AddAuthHeader();

        var request = new PlanAiRequest
        {
            OperationId = "suggest_audience",
            Plan = CreateSampleLessonPlan()
        };

        // Act
        var response = await mClient.PostAsJsonAsync("/api/plan/assistance", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<AIResult>();
        result.Should().NotBeNull();
        result!.Successful.Should().BeTrue();
        result.Output.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RequestAssistance_ReviewWholeLesson_ShouldReturnSuccessfulResult()
    {
        // Arrange
        await LoginAsync();
        AddAuthHeader();

        var request = new PlanAiRequest
        {
            OperationId = "review_whole_lesson",
            Plan = CreateSampleLessonPlan()
        };

        // Act
        var response = await mClient.PostAsJsonAsync("/api/plan/assistance", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<AIResult>();
        result.Should().NotBeNull();
        result!.Successful.Should().BeTrue();
        result.Output.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task RequestAssistance_WithInvalidOperationId_ShouldReturnBadRequest()
    {
        // Arrange
        await LoginAsync();
        AddAuthHeader();

        var request = new PlanAiRequest
        {
            OperationId = "invalid_operation",
            Plan = CreateSampleLessonPlan()
        };

        // Act
        var response = await mClient.PostAsJsonAsync("/api/plan/assistance", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Invalid operation ID");
    }

    [Fact]
    public async Task RequestAssistance_WithEmptyOperationId_ShouldReturnBadRequest()
    {
        // Arrange
        await LoginAsync();
        AddAuthHeader();

        var request = new PlanAiRequest
        {
            OperationId = "",
            Plan = CreateSampleLessonPlan()
        };

        // Act
        var response = await mClient.PostAsJsonAsync("/api/plan/assistance", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RequestAssistance_WithNullPlan_ShouldReturnBadRequest()
    {
        // Arrange
        await LoginAsync();
        AddAuthHeader();

        var request = new PlanAiRequest
        {
            OperationId = "review_topic",
            Plan = null!
        };

        // Act
        var response = await mClient.PostAsJsonAsync("/api/plan/assistance", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Multiple Operations Test

    [Theory]
    [InlineData("review_topic")]
    [InlineData("suggest_topic")]
    [InlineData("review_audience")]
    [InlineData("review_outcomes")]
    [InlineData("review_conn_goal")]
    [InlineData("review_concepts_needToKnow")]
    [InlineData("review_practice_output")]
    [InlineData("review_concl_goal")]
    public async Task RequestAssistance_WithVariousOperations_ShouldSucceed(string operationId)
    {
        // Arrange
        await LoginAsync();
        AddAuthHeader();

        var request = new PlanAiRequest
        {
            OperationId = operationId,
            Plan = CreateSampleLessonPlan()
        };

        // Act
        var response = await mClient.PostAsJsonAsync("/api/plan/assistance", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, $"Operation {operationId} should succeed");

        var result = await response.Content.ReadFromJsonAsync<AIResult>();
        result.Should().NotBeNull();
        result!.Successful.Should().BeTrue($"Operation {operationId} should return successful result");
    }

    #endregion

    #region Response Structure Tests

    [Fact]
    public async Task RequestAssistance_ShouldReturnAIResultWithCorrectStructure()
    {
        // Arrange
        await LoginAsync();
        AddAuthHeader();

        var request = new PlanAiRequest
        {
            OperationId = "review_topic",
            Plan = CreateSampleLessonPlan()
        };

        // Act
        var response = await mClient.PostAsJsonAsync("/api/plan/assistance", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<AIResult>();
        result.Should().NotBeNull();
        result.Should().BeOfType<AIResult>();
        result!.Successful.Should().BeTrue();
        result.ErrorCode.Should().NotBeNull();
        result.Output.Should().NotBeNull();
    }

    #endregion
}
