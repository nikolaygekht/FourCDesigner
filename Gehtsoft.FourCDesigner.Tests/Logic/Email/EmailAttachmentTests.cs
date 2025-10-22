// Suppress nullability warnings for intentional null tests
#pragma warning disable CS8625, CS8602, CS8600, CS8601, CS8603
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Gehtsoft.FourCDesigner.Logic.Email.Model;

namespace Gehtsoft.FourCDesigner.Tests.Logic.Email;

/// <summary>
/// Unit tests for EmailAttachment class.
/// </summary>
public class EmailAttachmentTests
{
    [Fact]
    public void Constructor_Default_ShouldInitializeWithDefaults()
    {
        // Act
        var attachment = new EmailAttachment();

        // Assert
        attachment.FileName.Should().BeEmpty();
        attachment.ContentType.Should().Be("application/octet-stream");
        attachment.ContentBase64.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithParameters_ShouldInitializeProperties()
    {
        // Arrange
        string fileName = "test.txt";
        string contentType = "text/plain";
        byte[] content = Encoding.UTF8.GetBytes("Test content");

        // Act
        var attachment = new EmailAttachment(fileName, contentType, content);

        // Assert
        attachment.FileName.Should().Be(fileName);
        attachment.ContentType.Should().Be(contentType);
        attachment.ContentBase64.Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_WithNullFileName_ShouldThrowArgumentNullException()
    {
        // Arrange
        byte[] content = Encoding.UTF8.GetBytes("Test content");

        // Act
        Action act = () => new EmailAttachment(null, "text/plain", content);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("fileName");
    }

    [Fact]
    public void Constructor_WithNullContent_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new EmailAttachment("test.txt", "text/plain", null);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("content");
    }

    [Fact]
    public void Constructor_WithNullContentType_ShouldUseDefaultContentType()
    {
        // Arrange
        byte[] content = Encoding.UTF8.GetBytes("Test content");

        // Act
        var attachment = new EmailAttachment("test.txt", null, content);

        // Assert
        attachment.ContentType.Should().Be("application/octet-stream");
    }

    [Fact]
    public void GetContent_ShouldReturnDecodedContent()
    {
        // Arrange
        byte[] originalContent = Encoding.UTF8.GetBytes("Test content");
        var attachment = new EmailAttachment("test.txt", "text/plain", originalContent);

        // Act
        byte[] decodedContent = attachment.GetContent();

        // Assert
        decodedContent.Should().BeEquivalentTo(originalContent);
        Encoding.UTF8.GetString(decodedContent).Should().Be("Test content");
    }

    [Fact]
    public void GetContent_WithEmptyBase64_ShouldReturnEmptyArray()
    {
        // Arrange
        var attachment = new EmailAttachment
        {
            ContentBase64 = string.Empty
        };

        // Act
        byte[] content = attachment.GetContent();

        // Assert
        content.Should().BeEmpty();
    }

    [Fact]
    public void ContentBase64_ShouldBeValidBase64()
    {
        // Arrange
        byte[] originalContent = Encoding.UTF8.GetBytes("Test content with special chars: !@#$%^&*()");
        var attachment = new EmailAttachment("test.txt", "text/plain", originalContent);

        // Act
        bool isValidBase64 = IsValidBase64(attachment.ContentBase64);

        // Assert
        isValidBase64.Should().BeTrue();
        attachment.GetContent().Should().BeEquivalentTo(originalContent);
    }

    [Fact]
    public void JsonSerialization_ShouldPreserveAllProperties()
    {
        // Arrange
        byte[] content = Encoding.UTF8.GetBytes("Test content");
        var original = new EmailAttachment("document.pdf", "application/pdf", content);

        // Act
        string json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<EmailAttachment>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.FileName.Should().Be(original.FileName);
        deserialized.ContentType.Should().Be(original.ContentType);
        deserialized.ContentBase64.Should().Be(original.ContentBase64);
        deserialized.GetContent().Should().BeEquivalentTo(content);
    }

    [Fact]
    public void JsonSerialization_UsesCorrectPropertyNames()
    {
        // Arrange
        byte[] content = Encoding.UTF8.GetBytes("Test");
        var attachment = new EmailAttachment("test.txt", "text/plain", content);

        // Act
        string json = JsonSerializer.Serialize(attachment);

        // Assert
        json.Should().Contain("\"fileName\":");
        json.Should().Contain("\"contentType\":");
        json.Should().Contain("\"contentBase64\":");
    }

    [Fact]
    public void RoundTrip_ShouldPreserveContent()
    {
        // Arrange - Create attachment with binary content
        byte[] originalContent = new byte[] { 0, 1, 2, 3, 255, 254, 253, 252 };
        var original = new EmailAttachment("binary.dat", "application/octet-stream", originalContent);

        // Act - Serialize and deserialize
        string json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<EmailAttachment>(json);
        byte[] roundTrippedContent = deserialized.GetContent();

        // Assert
        roundTrippedContent.Should().BeEquivalentTo(originalContent);
    }

    [Fact]
    public void LargeContent_ShouldBeHandledCorrectly()
    {
        // Arrange - Create 1MB of content
        byte[] largeContent = new byte[1024 * 1024];
        new Random(42).NextBytes(largeContent);

        // Act
        var attachment = new EmailAttachment("large.bin", "application/octet-stream", largeContent);
        byte[] decodedContent = attachment.GetContent();

        // Assert
        decodedContent.Should().BeEquivalentTo(largeContent);
        attachment.ContentBase64.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// Helper method to validate Base64 string.
    /// </summary>
    private static bool IsValidBase64(string base64String)
    {
        if (string.IsNullOrEmpty(base64String))
            return false;

        try
        {
            Convert.FromBase64String(base64String);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
