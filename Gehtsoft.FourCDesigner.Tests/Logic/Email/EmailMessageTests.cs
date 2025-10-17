using System.Text.Json;
using FluentAssertions;
using Gehtsoft.FourCDesigner.Logic.Email.Model;

namespace Gehtsoft.FourCDesigner.Tests.Logic.Email;

/// <summary>
/// Unit tests for EmailMessage class.
/// </summary>
public class EmailMessageTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaults()
    {
        // Act
        var message = new EmailMessage();

        // Assert
        message.Id.Should().NotBeEmpty();
        message.Created.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        message.Priority.Should().BeFalse();
        message.HtmlContent.Should().BeFalse();
        message.Subject.Should().BeEmpty();
        message.To.Should().NotBeNull().And.BeEmpty();
        message.Body.Should().BeEmpty();
        message.Attachments.Should().NotBeNull().And.BeEmpty();
        message.RetryCount.Should().Be(0);
        message.LastError.Should().BeEmpty();
        message.LastAttempt.Should().BeNull();
    }

    [Fact]
    public void Create_WithSingleRecipient_ShouldCreateMessage()
    {
        // Arrange
        string to = "user@example.com";
        string subject = "Test Subject";
        string body = "Test Body";

        // Act
        var message = EmailMessage.Create(to, subject, body);

        // Assert
        message.To.Should().HaveCount(1);
        message.To[0].Should().Be(to);
        message.Subject.Should().Be(subject);
        message.Body.Should().Be(body);
        message.HtmlContent.Should().BeFalse();
        message.Priority.Should().BeFalse();
    }

    [Fact]
    public void Create_WithMultipleRecipients_ShouldCreateMessage()
    {
        // Arrange
        string[] to = new[] { "user1@example.com", "user2@example.com" };
        string subject = "Test Subject";
        string body = "Test Body";

        // Act
        var message = EmailMessage.Create(to, subject, body);

        // Assert
        message.To.Should().HaveCount(2);
        message.To.Should().BeEquivalentTo(to);
        message.Subject.Should().Be(subject);
        message.Body.Should().Be(body);
    }

    [Fact]
    public void Create_WithHtmlContent_ShouldSetHtmlFlag()
    {
        // Act
        var message = EmailMessage.Create("user@example.com", "Subject", "Body", html: true);

        // Assert
        message.HtmlContent.Should().BeTrue();
    }

    [Fact]
    public void Create_WithPriority_ShouldSetPriorityFlag()
    {
        // Act
        var message = EmailMessage.Create("user@example.com", "Subject", "Body", priority: true);

        // Assert
        message.Priority.Should().BeTrue();
    }

    [Fact]
    public void JsonSerialization_ShouldPreserveAllProperties()
    {
        // Arrange
        var original = new EmailMessage
        {
            Id = Guid.NewGuid(),
            Created = DateTime.UtcNow,
            Priority = true,
            HtmlContent = true,
            Subject = "Test Subject",
            To = new[] { "user1@example.com", "user2@example.com" },
            Body = "Test Body",
            RetryCount = 2,
            LastError = "Test Error",
            LastAttempt = DateTime.UtcNow
        };

        original.Attachments.Add(new EmailAttachment
        {
            FileName = "test.txt",
            ContentType = "text/plain",
            ContentBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("Test content"))
        });

        // Act
        string json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<EmailMessage>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Id.Should().Be(original.Id);
        deserialized.Created.Should().Be(original.Created);
        deserialized.Priority.Should().Be(original.Priority);
        deserialized.HtmlContent.Should().Be(original.HtmlContent);
        deserialized.Subject.Should().Be(original.Subject);
        deserialized.To.Should().BeEquivalentTo(original.To);
        deserialized.Body.Should().Be(original.Body);
        deserialized.RetryCount.Should().Be(original.RetryCount);
        deserialized.LastError.Should().Be(original.LastError);
        deserialized.LastAttempt.Should().Be(original.LastAttempt);
        deserialized.Attachments.Should().HaveCount(1);
        deserialized.Attachments[0].FileName.Should().Be("test.txt");
    }

    [Fact]
    public void JsonSerialization_UsesCorrectPropertyNames()
    {
        // Arrange
        var message = EmailMessage.Create("user@example.com", "Subject", "Body", html: true, priority: true);

        // Act
        string json = JsonSerializer.Serialize(message);

        // Assert
        json.Should().Contain("\"id\":");
        json.Should().Contain("\"created\":");
        json.Should().Contain("\"priority\":");
        json.Should().Contain("\"htmlContent\":");
        json.Should().Contain("\"subject\":");
        json.Should().Contain("\"to\":");
        json.Should().Contain("\"body\":");
        json.Should().Contain("\"attachments\":");
        json.Should().Contain("\"retryCount\":");
        json.Should().Contain("\"lastError\":");
        json.Should().Contain("\"lastAttempt\":");
    }
}
