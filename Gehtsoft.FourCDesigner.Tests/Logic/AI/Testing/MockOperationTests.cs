using System.Text.Json;
using Gehtsoft.FourCDesigner.Logic.AI;
using Gehtsoft.FourCDesigner.Logic.AI.Testing;
using Xunit;
using FluentAssertions;

namespace Gehtsoft.FourCDesigner.Tests.Logic.AI.Testing;

/// <summary>
/// Unit tests for MockOperation class.
/// </summary>
public class MockOperationTests
{
    [Fact]
    public void DefaultConstructor_ShouldInitializeWithDefaults()
    {
        // Act
        var mockOp = new MockOperation();

        // Assert
        mockOp.Operation.Should().BeEmpty();
        mockOp.RequestPattern.Should().BeEmpty();
        mockOp.UserDataPattern.Should().BeEmpty();
        mockOp.Response.Should().NotBeNull();
        mockOp.Response.Successful.Should().BeFalse();
        mockOp.Response.ErrorCode.Should().BeEmpty();
        mockOp.Response.Output.Should().BeEmpty();
    }

    [Fact]
    public void JsonSerialization_ShouldUseCamelCase()
    {
        // Arrange
        var mockOp = new MockOperation
        {
            Operation = "validate",
            RequestPattern = ".*test.*",
            UserDataPattern = ".*input.*",
            Response = new AIResult(true, "", "Test output")
        };

        // Act
        string json = JsonSerializer.Serialize(mockOp);

        // Assert
        json.Should().Contain("\"operation\":\"validate\"");
        json.Should().Contain("\"requestPattern\":\".*test.*\"");
        json.Should().Contain("\"userDataPattern\":\".*input.*\"");
        json.Should().Contain("\"response\":");
    }

    [Fact]
    public void JsonDeserialization_WithValidJson_ShouldDeserializeCorrectly()
    {
        // Arrange
        string json = @"{
            ""operation"": ""process"",
            ""requestPattern"": "".*suggestions.*"",
            ""userDataPattern"": "".*math.*"",
            ""response"": {
                ""successful"": true,
                ""errorCode"": """",
                ""output"": ""Math suggestions""
            }
        }";

        // Act
        MockOperation? mockOp = JsonSerializer.Deserialize<MockOperation>(json);

        // Assert
        mockOp.Should().NotBeNull();
        mockOp!.Operation.Should().Be("process");
        mockOp.RequestPattern.Should().Be(".*suggestions.*");
        mockOp.UserDataPattern.Should().Be(".*math.*");
        mockOp.Response.Should().NotBeNull();
        mockOp.Response.Successful.Should().BeTrue();
        mockOp.Response.ErrorCode.Should().BeEmpty();
        mockOp.Response.Output.Should().Be("Math suggestions");
    }

    [Fact]
    public void JsonDeserialization_WithNestedResponse_ShouldDeserializeCorrectly()
    {
        // Arrange
        string json = @"{
            ""operation"": ""validate"",
            ""requestPattern"": "".*"",
            ""userDataPattern"": "".*inject.*"",
            ""response"": {
                ""successful"": false,
                ""errorCode"": ""UNSAFE_INPUT"",
                ""output"": ""Input contains injection attempt""
            }
        }";

        // Act
        MockOperation? mockOp = JsonSerializer.Deserialize<MockOperation>(json);

        // Assert
        mockOp.Should().NotBeNull();
        mockOp!.Operation.Should().Be("validate");
        mockOp.RequestPattern.Should().Be(".*");
        mockOp.UserDataPattern.Should().Be(".*inject.*");
        mockOp.Response.Should().NotBeNull();
        mockOp.Response.Successful.Should().BeFalse();
        mockOp.Response.ErrorCode.Should().Be("UNSAFE_INPUT");
        mockOp.Response.Output.Should().Be("Input contains injection attempt");
    }

    [Fact]
    public void JsonRoundTrip_ShouldPreserveAllData()
    {
        // Arrange
        var original = new MockOperation
        {
            Operation = "process",
            RequestPattern = "^test$",
            UserDataPattern = "^data$",
            Response = AIResult.Success("Success message")
        };

        // Act
        string json = JsonSerializer.Serialize(original);
        MockOperation? deserialized = JsonSerializer.Deserialize<MockOperation>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Operation.Should().Be(original.Operation);
        deserialized.RequestPattern.Should().Be(original.RequestPattern);
        deserialized.UserDataPattern.Should().Be(original.UserDataPattern);
        deserialized.Response.Successful.Should().Be(original.Response.Successful);
        deserialized.Response.ErrorCode.Should().Be(original.Response.ErrorCode);
        deserialized.Response.Output.Should().Be(original.Response.Output);
    }

    [Fact]
    public void JsonDeserialization_WithArray_ShouldDeserializeList()
    {
        // Arrange
        string json = @"[
            {
                ""operation"": ""validate"",
                ""requestPattern"": "".*"",
                ""userDataPattern"": "".*safe.*"",
                ""response"": {
                    ""successful"": true,
                    ""errorCode"": """",
                    ""output"": ""SAFE""
                }
            },
            {
                ""operation"": ""process"",
                ""requestPattern"": "".*"",
                ""userDataPattern"": "".*"",
                ""response"": {
                    ""successful"": true,
                    ""errorCode"": """",
                    ""output"": ""Default response""
                }
            }
        ]";

        // Act
        List<MockOperation>? mockOps = JsonSerializer.Deserialize<List<MockOperation>>(json);

        // Assert
        mockOps.Should().NotBeNull();
        mockOps.Should().HaveCount(2);
        mockOps![0].Operation.Should().Be("validate");
        mockOps[1].Operation.Should().Be("process");
    }
}
