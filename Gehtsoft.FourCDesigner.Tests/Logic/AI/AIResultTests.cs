using System.Text.Json;
using Gehtsoft.FourCDesigner.Logic.AI;
using Xunit;
using FluentAssertions;

namespace Gehtsoft.FourCDesigner.Tests.Logic.AI;

/// <summary>
/// Unit tests for AIResult class.
/// </summary>
public class AIResultTests
{
    [Fact]
    public void DefaultConstructor_ShouldInitializeWithDefaults()
    {
        // Act
        var result = new AIResult();

        // Assert
        result.Successful.Should().BeFalse();
        result.ErrorCode.Should().BeEmpty();
        result.Output.Should().BeEmpty();
    }

    [Fact]
    public void ParameterizedConstructor_WithValidValues_ShouldSetProperties()
    {
        // Arrange
        bool successful = true;
        string errorCode = "TEST_ERROR";
        string output = "Test output";

        // Act
        var result = new AIResult(successful, errorCode, output);

        // Assert
        result.Successful.Should().BeTrue();
        result.ErrorCode.Should().Be("TEST_ERROR");
        result.Output.Should().Be("Test output");
    }

    [Fact]
    public void ParameterizedConstructor_WithNullErrorCode_ShouldSetEmptyString()
    {
        // Act
        var result = new AIResult(true, null!, "output");

        // Assert
        result.ErrorCode.Should().BeEmpty();
    }

    [Fact]
    public void ParameterizedConstructor_WithNullOutput_ShouldSetEmptyString()
    {
        // Act
        var result = new AIResult(true, "code", null!);

        // Assert
        result.Output.Should().BeEmpty();
    }

    [Fact]
    public void Success_WithOutput_ShouldCreateSuccessfulResult()
    {
        // Arrange
        string output = "Success output";

        // Act
        AIResult result = AIResult.Success(output);

        // Assert
        result.Successful.Should().BeTrue();
        result.ErrorCode.Should().BeEmpty();
        result.Output.Should().Be("Success output");
    }

    [Fact]
    public void Success_WithNullOutput_ShouldSetEmptyString()
    {
        // Act
        AIResult result = AIResult.Success(null!);

        // Assert
        result.Successful.Should().BeTrue();
        result.ErrorCode.Should().BeEmpty();
        result.Output.Should().BeEmpty();
    }

    [Fact]
    public void Failed_WithErrorCodeAndMessage_ShouldCreateFailedResult()
    {
        // Arrange
        string errorCode = "VALIDATION_ERROR";
        string errorMessage = "Validation failed";

        // Act
        AIResult result = AIResult.Failed(errorCode, errorMessage);

        // Assert
        result.Successful.Should().BeFalse();
        result.ErrorCode.Should().Be("VALIDATION_ERROR");
        result.Output.Should().Be("Validation failed");
    }

    [Fact]
    public void Failed_WithNullErrorCode_ShouldSetEmptyString()
    {
        // Act
        AIResult result = AIResult.Failed(null!, "message");

        // Assert
        result.Successful.Should().BeFalse();
        result.ErrorCode.Should().BeEmpty();
        result.Output.Should().Be("message");
    }

    [Fact]
    public void Failed_WithNullMessage_ShouldSetEmptyString()
    {
        // Act
        AIResult result = AIResult.Failed("ERROR", null!);

        // Assert
        result.Successful.Should().BeFalse();
        result.ErrorCode.Should().Be("ERROR");
        result.Output.Should().BeEmpty();
    }

    [Fact]
    public void JsonSerialization_ShouldUseCamelCase()
    {
        // Arrange
        var result = new AIResult(true, "TEST_CODE", "Test output");

        // Act
        string json = JsonSerializer.Serialize(result);

        // Assert
        json.Should().Contain("\"successful\":true");
        json.Should().Contain("\"errorCode\":\"TEST_CODE\"");
        json.Should().Contain("\"output\":\"Test output\"");
    }

    [Fact]
    public void JsonDeserialization_WithCamelCase_ShouldDeserializeCorrectly()
    {
        // Arrange
        string json = "{\"successful\":true,\"errorCode\":\"TEST\",\"output\":\"Result\"}";

        // Act
        AIResult? result = JsonSerializer.Deserialize<AIResult>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Successful.Should().BeTrue();
        result.ErrorCode.Should().Be("TEST");
        result.Output.Should().Be("Result");
    }

    [Fact]
    public void JsonSerialization_WithSuccessFactory_ShouldSerializeCorrectly()
    {
        // Arrange
        AIResult result = AIResult.Success("Success message");

        // Act
        string json = JsonSerializer.Serialize(result);
        AIResult? deserialized = JsonSerializer.Deserialize<AIResult>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Successful.Should().BeTrue();
        deserialized.ErrorCode.Should().BeEmpty();
        deserialized.Output.Should().Be("Success message");
    }

    [Fact]
    public void JsonSerialization_WithFailedFactory_ShouldSerializeCorrectly()
    {
        // Arrange
        AIResult result = AIResult.Failed("ERROR_CODE", "Error message");

        // Act
        string json = JsonSerializer.Serialize(result);
        AIResult? deserialized = JsonSerializer.Deserialize<AIResult>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Successful.Should().BeFalse();
        deserialized.ErrorCode.Should().Be("ERROR_CODE");
        deserialized.Output.Should().Be("Error message");
    }
}
