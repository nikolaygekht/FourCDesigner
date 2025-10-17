using FluentAssertions;
using Gehtsoft.FourCDesigner.Logic.Email;
using Gehtsoft.FourCDesigner.Logic.Email.Model;
using Gehtsoft.FourCDesigner.Logic.Email.Queue;
using Gehtsoft.FourCDesigner.Logic.Email.Sender;
using Microsoft.Extensions.Logging;
using Moq;

namespace Gehtsoft.FourCDesigner.Tests.Logic.Email;

/// <summary>
/// Unit tests for EmailService class.
/// </summary>
public class EmailServiceTests
{
    private readonly Mock<IEmailQueue> mQueueMock;
    private readonly Mock<IEmailSenderService> mEmailSenderServiceMock;
    private readonly Mock<ILogger<EmailService>> mLoggerMock;
    private readonly EmailSenderState mSenderState;
    private readonly EmailService mService;

    public EmailServiceTests()
    {
        mQueueMock = new Mock<IEmailQueue>();
        mEmailSenderServiceMock = new Mock<IEmailSenderService>();
        mLoggerMock = new Mock<ILogger<EmailService>>();
        mSenderState = new EmailSenderState();
        mService = new EmailService(mQueueMock.Object, mEmailSenderServiceMock.Object, mSenderState, mLoggerMock.Object);
    }

    [Fact]
    public void Constructor_WithNullQueue_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new EmailService(null, mEmailSenderServiceMock.Object, mSenderState, mLoggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("queue");
    }

    [Fact]
    public void Constructor_WithNullEmailSenderService_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new EmailService(mQueueMock.Object, null, mSenderState, mLoggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("emailSenderService");
    }

    [Fact]
    public void Constructor_WithNullSenderState_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new EmailService(mQueueMock.Object, mEmailSenderServiceMock.Object, null, mLoggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("senderState");
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new EmailService(mQueueMock.Object, mEmailSenderServiceMock.Object, mSenderState, null);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void SendEmail_WithValidMessage_ShouldEnqueueMessage()
    {
        // Arrange
        var message = EmailMessage.Create("test@example.com", "Subject", "Body");

        // Act
        mService.SendEmail(message);

        // Assert
        mQueueMock.Verify(q => q.Enqueue(message), Times.Once);
    }

    [Fact]
    public void SendEmail_WithNullMessage_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => mService.SendEmail(null);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("message");
    }

    [Fact]
    public async Task SendEmailAndTriggerProcessorAsync_WithValidMessage_ShouldEnqueueMessage()
    {
        // Arrange
        var message = EmailMessage.Create("test@example.com", "Subject", "Body");

        // Act
        await mService.SendEmailAndTriggerProcessorAsync(message);

        // Assert
        mQueueMock.Verify(q => q.Enqueue(message), Times.Once);
    }

    [Fact]
    public async Task SendEmailAndTriggerProcessorAsync_WithValidMessage_ShouldTriggerProcessor()
    {
        // Arrange
        var message = EmailMessage.Create("test@example.com", "Subject", "Body");

        // Act
        await mService.SendEmailAndTriggerProcessorAsync(message);

        // Assert
        mEmailSenderServiceMock.Verify(s => s.ProcessQueueAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendEmailAndTriggerProcessorAsync_WithNullMessage_ShouldThrowArgumentNullException()
    {
        // Act
        Func<Task> act = async () => await mService.SendEmailAndTriggerProcessorAsync(null);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("message");
    }

    [Fact]
    public async Task SendEmailAndTriggerProcessorAsync_WhenQueueThrowsException_ShouldRethrowException()
    {
        // Arrange
        var message = EmailMessage.Create("test@example.com", "Subject", "Body");
        var expectedException = new InvalidOperationException("Queue error");
        mQueueMock.Setup(q => q.Enqueue(It.IsAny<EmailMessage>())).Throws(expectedException);

        // Act
        Func<Task> act = async () => await mService.SendEmailAndTriggerProcessorAsync(message);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Queue error");
    }

    [Fact]
    public async Task SendEmailAndTriggerProcessorAsync_WhenProcessorThrowsException_ShouldRethrowException()
    {
        // Arrange
        var message = EmailMessage.Create("test@example.com", "Subject", "Body");
        var expectedException = new InvalidOperationException("Processor error");
        mEmailSenderServiceMock.Setup(s => s.ProcessQueueAsync(It.IsAny<CancellationToken>())).ThrowsAsync(expectedException);

        // Act
        Func<Task> act = async () => await mService.SendEmailAndTriggerProcessorAsync(message);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Processor error");
    }

    [Fact]
    public void SendEmail_WhenQueueThrowsException_ShouldRethrowException()
    {
        // Arrange
        var message = EmailMessage.Create("test@example.com", "Subject", "Body");
        var expectedException = new InvalidOperationException("Queue error");
        mQueueMock.Setup(q => q.Enqueue(It.IsAny<EmailMessage>())).Throws(expectedException);

        // Act
        Action act = () => mService.SendEmail(message);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Queue error");
    }

    [Fact]
    public void QueueSize_ShouldReturnQueueCount()
    {
        // Arrange
        mQueueMock.Setup(q => q.Count).Returns(5);

        // Act
        int size = mService.QueueSize;

        // Assert
        size.Should().Be(5);
    }

    [Fact]
    public void QueueSize_WithEmptyQueue_ShouldReturnZero()
    {
        // Arrange
        mQueueMock.Setup(q => q.Count).Returns(0);

        // Act
        int size = mService.QueueSize;

        // Assert
        size.Should().Be(0);
    }

    [Fact]
    public void IsSenderActive_InitiallyFalse()
    {
        // Assert
        mService.IsSenderActive.Should().BeFalse();
    }

    [Fact]
    public void IsSenderActive_WhenSetToTrue_ShouldReturnTrue()
    {
        // Act
        mSenderState.IsSenderActive = true;

        // Assert
        mService.IsSenderActive.Should().BeTrue();
    }

    [Fact]
    public void LastError_InitiallyNull()
    {
        // Assert
        mService.LastError.Should().BeNull();
    }

    [Fact]
    public void LastError_WhenSet_ShouldReturnException()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error");

        // Act
        mSenderState.LastError = exception;

        // Assert
        mService.LastError.Should().Be(exception);
        mService.LastError.Message.Should().Be("Test error");
    }

    [Fact]
    public void LastErrorTime_InitiallyNull()
    {
        // Assert
        mService.LastErrorTime.Should().BeNull();
    }

    [Fact]
    public void LastErrorTime_WhenSet_ShouldReturnDateTime()
    {
        // Arrange
        var errorTime = DateTime.UtcNow;

        // Act
        mSenderState.LastErrorTime = errorTime;

        // Assert
        mService.LastErrorTime.Should().Be(errorTime);
    }

    [Fact]
    public void PauseAfterErrorUntil_InitiallyNull()
    {
        // Assert
        mService.PauseAfterErrorUntil.Should().BeNull();
    }

    [Fact]
    public void PauseAfterErrorUntil_WhenSet_ShouldReturnDateTime()
    {
        // Arrange
        var pauseUntil = DateTime.UtcNow.AddMinutes(30);

        // Act
        mSenderState.PauseAfterErrorUntil = pauseUntil;

        // Assert
        mService.PauseAfterErrorUntil.Should().Be(pauseUntil);
    }

    [Fact]
    public void SendEmail_MultipleMessages_ShouldEnqueueAll()
    {
        // Arrange
        var message1 = EmailMessage.Create("test1@example.com", "Subject 1", "Body 1");
        var message2 = EmailMessage.Create("test2@example.com", "Subject 2", "Body 2");
        var message3 = EmailMessage.Create("test3@example.com", "Subject 3", "Body 3");

        // Act
        mService.SendEmail(message1);
        mService.SendEmail(message2);
        mService.SendEmail(message3);

        // Assert
        mQueueMock.Verify(q => q.Enqueue(It.IsAny<EmailMessage>()), Times.Exactly(3));
        mQueueMock.Verify(q => q.Enqueue(message1), Times.Once);
        mQueueMock.Verify(q => q.Enqueue(message2), Times.Once);
        mQueueMock.Verify(q => q.Enqueue(message3), Times.Once);
    }

    [Fact]
    public void SendEmail_WithHighPriorityMessage_ShouldEnqueue()
    {
        // Arrange
        var message = EmailMessage.Create("test@example.com", "Urgent", "Body");
        message.Priority = true;

        // Act
        mService.SendEmail(message);

        // Assert
        mQueueMock.Verify(q => q.Enqueue(message), Times.Once);
        message.Priority.Should().BeTrue();
    }

    [Fact]
    public void SendEmail_WithAttachments_ShouldEnqueue()
    {
        // Arrange
        var message = EmailMessage.Create("test@example.com", "Subject", "Body");
        var attachment = new EmailAttachment("test.txt", "text/plain", new byte[] { 1, 2, 3 });
        message.Attachments.Add(attachment);

        // Act
        mService.SendEmail(message);

        // Assert
        mQueueMock.Verify(q => q.Enqueue(message), Times.Once);
        message.Attachments.Should().HaveCount(1);
    }

    [Fact]
    public void SendEmail_WithHtmlContent_ShouldEnqueue()
    {
        // Arrange
        var message = EmailMessage.Create("test@example.com", "Subject", "<html><body>HTML content</body></html>");
        message.HtmlContent = true;

        // Act
        mService.SendEmail(message);

        // Assert
        mQueueMock.Verify(q => q.Enqueue(message), Times.Once);
        message.HtmlContent.Should().BeTrue();
    }

    [Fact]
    public void ErrorState_CanBeSetAndRetrieved()
    {
        // Arrange
        var exception = new InvalidOperationException("SMTP connection failed");
        var errorTime = DateTime.UtcNow;
        var pauseUntil = DateTime.UtcNow.AddMinutes(30);

        // Act
        mSenderState.LastError = exception;
        mSenderState.LastErrorTime = errorTime;
        mSenderState.PauseAfterErrorUntil = pauseUntil;

        // Assert
        mService.LastError.Should().Be(exception);
        mService.LastErrorTime.Should().Be(errorTime);
        mService.PauseAfterErrorUntil.Should().Be(pauseUntil);
    }

    [Fact]
    public void ErrorState_CanBeCleared()
    {
        // Arrange
        mSenderState.LastError = new InvalidOperationException("Error");
        mSenderState.LastErrorTime = DateTime.UtcNow;
        mSenderState.PauseAfterErrorUntil = DateTime.UtcNow.AddMinutes(30);

        // Act
        mSenderState.LastError = null;
        mSenderState.LastErrorTime = null;
        mSenderState.PauseAfterErrorUntil = null;

        // Assert
        mService.LastError.Should().BeNull();
        mService.LastErrorTime.Should().BeNull();
        mService.PauseAfterErrorUntil.Should().BeNull();
    }
}
