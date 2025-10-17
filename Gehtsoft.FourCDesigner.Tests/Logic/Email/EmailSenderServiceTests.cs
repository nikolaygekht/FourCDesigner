using FluentAssertions;
using Gehtsoft.FourCDesigner.Logic.Email;
using Gehtsoft.FourCDesigner.Logic.Email.Configuration;
using Gehtsoft.FourCDesigner.Logic.Email.Model;
using Gehtsoft.FourCDesigner.Logic.Email.Queue;
using Gehtsoft.FourCDesigner.Logic.Email.Sender;
using Gehtsoft.FourCDesigner.Logic.Email.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace Gehtsoft.FourCDesigner.Tests.Logic.Email;

/// <summary>
/// Unit tests for EmailSenderService class.
/// </summary>
public class EmailSenderServiceTests
{
    private readonly Mock<IEmailQueue> mQueueMock;
    private readonly Mock<IEmailStorage> mStorageMock;
    private readonly Mock<IEmailConfiguration> mConfigurationMock;
    private readonly Mock<ISmtpSender> mSmtpSenderMock;
    private readonly IServiceProvider mServiceProvider;
    private readonly EmailSenderState mSenderState;
    private readonly Mock<ILogger<EmailSenderService>> mLoggerMock;
    private readonly EmailSenderService mSenderService;

    public EmailSenderServiceTests()
    {
        mQueueMock = new Mock<IEmailQueue>();
        mStorageMock = new Mock<IEmailStorage>();
        mConfigurationMock = new Mock<IEmailConfiguration>();
        mSmtpSenderMock = new Mock<ISmtpSender>();
        mSenderState = new EmailSenderState();
        mLoggerMock = new Mock<ILogger<EmailSenderService>>();

        // Setup default configuration
        mConfigurationMock.Setup(c => c.MailAddressFrom).Returns("sender@example.com");
        mConfigurationMock.Setup(c => c.MaxRetryCount).Returns(3);
        mConfigurationMock.Setup(c => c.PauseAfterErrorMinutes).Returns(30);
        mConfigurationMock.Setup(c => c.DelayBetweenMessagesSeconds).Returns(0.01);

        // Setup service provider using real ServiceCollection
        // This is necessary because CreateScope() is an extension method that cannot be mocked
        var services = new ServiceCollection();
        services.AddScoped<ISmtpSender>(sp => mSmtpSenderMock.Object);
        mServiceProvider = services.BuildServiceProvider();

        mSenderService = new EmailSenderService(
            mQueueMock.Object,
            mStorageMock.Object,
            mConfigurationMock.Object,
            mSenderState,
            mServiceProvider,
            mLoggerMock.Object);
    }

    [Fact(Timeout = 500)]
    public void Constructor_WithNullQueue_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new EmailSenderService(
            null!,
            mStorageMock.Object,
            mConfigurationMock.Object,
            mSenderState,
            mServiceProvider,
            mLoggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("queue");
    }

    [Fact(Timeout = 500)]
    public void Constructor_WithNullStorage_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new EmailSenderService(
            mQueueMock.Object,
            null!,
            mConfigurationMock.Object,
            mSenderState,
            mServiceProvider,
            mLoggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("storage");
    }

    [Fact(Timeout = 500)]
    public void Constructor_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new EmailSenderService(
            mQueueMock.Object,
            mStorageMock.Object,
            null!,
            mSenderState,
            mServiceProvider,
            mLoggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("configuration");
    }

    [Fact(Timeout = 500)]
    public void Constructor_WithNullSenderState_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new EmailSenderService(
            mQueueMock.Object,
            mStorageMock.Object,
            mConfigurationMock.Object,
            null!,
            mServiceProvider,
            mLoggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("senderState");
    }

    [Fact(Timeout = 500)]
    public void Constructor_WithNullServiceProvider_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new EmailSenderService(
            mQueueMock.Object,
            mStorageMock.Object,
            mConfigurationMock.Object,
            mSenderState,
            null!,
            mLoggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("serviceProvider");
    }

    [Fact(Timeout = 500)]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new EmailSenderService(
            mQueueMock.Object,
            mStorageMock.Object,
            mConfigurationMock.Object,
            mSenderState,
            mServiceProvider,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact(Timeout = 500)]
    public async Task ProcessQueueAsync_WithEmptyQueue_ShouldReturnImmediately()
    {
        // Arrange
        mQueueMock.Setup(q => q.Count).Returns(0);

        // Act
        await mSenderService.ProcessQueueAsync();

        // Assert
        mSmtpSenderMock.Verify(s => s.Open(), Times.Never);
    }

    [Fact(Timeout = 500)]
    public async Task ProcessQueueAsync_WithOneMessage_ShouldSendMessage()
    {
        // Arrange
        var message = EmailMessage.Create("test@example.com", "Subject", "Body");
        mQueueMock.Setup(q => q.Count).Returns(1);
        SetupSingleMessageDequeue(message);

        // Act
        await mSenderService.ProcessQueueAsync();

        // Assert
        mSmtpSenderMock.Verify(s => s.Open(), Times.Once);
        mSmtpSenderMock.Verify(s => s.Send(message, "sender@example.com"), Times.Once);
        mSmtpSenderMock.Verify(s => s.Close(), Times.Once);
    }

    [Fact(Timeout = 500)]
    public async Task ProcessQueueAsync_WithSuccessfulSend_ShouldDeleteMessageFromStorage()
    {
        // Arrange
        var message = EmailMessage.Create("test@example.com", "Subject", "Body");
        mQueueMock.Setup(q => q.Count).Returns(1);
        SetupSingleMessageDequeue(message);

        // Act
        await mSenderService.ProcessQueueAsync();

        // Assert
        mStorageMock.Verify(s => s.DeleteMessage(message.Id), Times.Once);
    }

    [Fact(Timeout = 500)]
    public async Task ProcessQueueAsync_WithSuccessfulSend_ShouldClearErrorState()
    {
        // Arrange
        var message = EmailMessage.Create("test@example.com", "Subject", "Body");
        mQueueMock.Setup(q => q.Count).Returns(1);
        SetupSingleMessageDequeue(message);

        mSenderState.LastError = new Exception("Previous error");
        mSenderState.LastErrorTime = DateTime.UtcNow;

        // Act
        await mSenderService.ProcessQueueAsync();

        // Assert
        mSenderState.LastError.Should().BeNull();
        mSenderState.LastErrorTime.Should().BeNull();
    }

    [Fact(Timeout = 500)]
    public async Task ProcessQueueAsync_WhenSendFails_ShouldIncrementRetryCount()
    {
        // Arrange
        var message = EmailMessage.Create("test@example.com", "Subject", "Body");
        mQueueMock.Setup(q => q.Count).Returns(1);
        SetupSingleMessageDequeue(message);

        mSmtpSenderMock.Setup(s => s.Send(It.IsAny<EmailMessage>(), It.IsAny<string>()))
            .Throws(new InvalidOperationException("SMTP error"));

        // Act
        await mSenderService.ProcessQueueAsync();

        // Assert
        message.RetryCount.Should().Be(1);
    }

    [Fact(Timeout = 500)]
    public async Task ProcessQueueAsync_WhenSendFails_ShouldRequeueMessage()
    {
        // Arrange
        var message = EmailMessage.Create("test@example.com", "Subject", "Body");
        mQueueMock.Setup(q => q.Count).Returns(1);
        SetupSingleMessageDequeue(message);

        mSmtpSenderMock.Setup(s => s.Send(It.IsAny<EmailMessage>(), It.IsAny<string>()))
            .Throws(new InvalidOperationException("SMTP error"));

        // Act
        await mSenderService.ProcessQueueAsync();

        // Assert
        mQueueMock.Verify(q => q.Requeue(message), Times.Once);
    }

    [Fact(Timeout = 500)]
    public async Task ProcessQueueAsync_WhenMaxRetriesReached_ShouldMoveToBadEmail()
    {
        // Arrange
        var message = EmailMessage.Create("test@example.com", "Subject", "Body");
        message.RetryCount = 2; // Will become 3 after increment
        mQueueMock.Setup(q => q.Count).Returns(1);
        SetupSingleMessageDequeue(message);

        mSmtpSenderMock.Setup(s => s.Send(It.IsAny<EmailMessage>(), It.IsAny<string>()))
            .Throws(new InvalidOperationException("SMTP error"));

        // Act
        await mSenderService.ProcessQueueAsync();

        // Assert
        message.RetryCount.Should().Be(3);
        mStorageMock.Verify(s => s.MoveToBadEmail(message), Times.Once);
        mQueueMock.Verify(q => q.Requeue(It.IsAny<EmailMessage>()), Times.Never);
    }

    [Fact(Timeout = 500)]
    public async Task ProcessQueueAsync_WhenSmtpOpenFails_ShouldSetErrorState()
    {
        // Arrange
        mQueueMock.Setup(q => q.Count).Returns(1);
        var expectedException = new InvalidOperationException("SMTP connection failed");
        mSmtpSenderMock.Setup(s => s.Open()).Throws(expectedException);

        // Act
        await mSenderService.ProcessQueueAsync();

        // Assert
        mSenderState.LastError.Should().Be(expectedException);
        mSenderState.LastErrorTime.Should().NotBeNull();
        mSenderState.PauseAfterErrorUntil.Should().NotBeNull();
    }

    [Fact(Timeout = 500)]
    public async Task ProcessQueueAsync_WhenSmtpOpenFails_ShouldSetPauseUntil()
    {
        // Arrange
        mQueueMock.Setup(q => q.Count).Returns(1);
        mSmtpSenderMock.Setup(s => s.Open()).Throws(new InvalidOperationException("SMTP connection failed"));

        var beforeCall = DateTime.UtcNow;

        // Act
        await mSenderService.ProcessQueueAsync();

        // Assert
        mSenderState.PauseAfterErrorUntil.Should().NotBeNull();
        mSenderState.PauseAfterErrorUntil.Value.Should().BeAfter(beforeCall.AddMinutes(29));
        mSenderState.PauseAfterErrorUntil.Value.Should().BeBefore(beforeCall.AddMinutes(31));
    }

    [Fact(Timeout = 500)]
    public async Task ProcessQueueAsync_ShouldCloseSmtpEvenOnError()
    {
        // Arrange
        var message = EmailMessage.Create("test@example.com", "Subject", "Body");
        mQueueMock.Setup(q => q.Count).Returns(1);
        SetupSingleMessageDequeue(message);

        mSmtpSenderMock.Setup(s => s.Send(It.IsAny<EmailMessage>(), It.IsAny<string>()))
            .Throws(new InvalidOperationException("SMTP error"));

        // Act
        await mSenderService.ProcessQueueAsync();

        // Assert
        mSmtpSenderMock.Verify(s => s.Close(), Times.Once);
    }

    [Fact(Timeout = 500)]
    public async Task ProcessQueueAsync_WithMultipleMessages_ShouldProcessAll()
    {
        // Arrange
        var message1 = EmailMessage.Create("test1@example.com", "Subject 1", "Body 1");
        var message2 = EmailMessage.Create("test2@example.com", "Subject 2", "Body 2");
        var message3 = EmailMessage.Create("test3@example.com", "Subject 3", "Body 3");

        var messages = new Queue<EmailMessage>(new[] { message1, message2, message3 });
        mQueueMock.Setup(q => q.Count).Returns(3);
        mQueueMock.Setup(q => q.TryDequeue(out It.Ref<EmailMessage>.IsAny))
            .Returns(new TryDequeueDelegate((out EmailMessage msg) =>
            {
                if (messages.Count > 0)
                {
                    msg = messages.Dequeue();
                    return true;
                }
                msg = null!;
                return false;
            }));

        // Act
        await mSenderService.ProcessQueueAsync();

        // Assert
        mSmtpSenderMock.Verify(s => s.Send(It.IsAny<EmailMessage>(), It.IsAny<string>()), Times.Exactly(3));
        mStorageMock.Verify(s => s.DeleteMessage(It.IsAny<Guid>()), Times.Exactly(3));
    }

    [Fact(Timeout = 500)]
    public async Task ProcessQueueAsync_ConcurrentCalls_ShouldOnlyProcessOnce()
    {
        // Arrange
        var message = EmailMessage.Create("test@example.com", "Subject", "Body");
        mQueueMock.Setup(q => q.Count).Returns(1);
        SetupSingleMessageDequeue(message);

        // Act - Start two concurrent calls
        var task1 = mSenderService.ProcessQueueAsync();
        var task2 = mSenderService.ProcessQueueAsync();

        await Task.WhenAll(task1, task2);

        // Assert - Only one should have processed (mutex prevents concurrent execution)
        mSmtpSenderMock.Verify(s => s.Open(), Times.Once);
    }

    [Fact(Timeout = 500)]
    public async Task ProcessQueueAsync_ShouldSetLastAttemptTime()
    {
        // Arrange
        var message = EmailMessage.Create("test@example.com", "Subject", "Body");
        message.LastAttempt.Should().BeNull();

        mQueueMock.Setup(q => q.Count).Returns(1);
        SetupSingleMessageDequeue(message);

        var beforeSend = DateTime.UtcNow;

        // Act
        await mSenderService.ProcessQueueAsync();

        // Assert
        message.LastAttempt.Should().NotBeNull();
        message.LastAttempt.Value.Should().BeAfter(beforeSend.AddSeconds(-1));
    }

    // Delegate for TryDequeue mock
    private delegate bool TryDequeueDelegate(out EmailMessage message);

    /// <summary>
    /// Helper method to setup TryDequeue mock for a single message.
    /// The mock will return the message once, then return false on subsequent calls.
    /// </summary>
    /// <param name="message">The message to return from the queue.</param>
    private void SetupSingleMessageDequeue(EmailMessage message)
    {
        bool dequeued = false;
        mQueueMock.Setup(q => q.TryDequeue(out It.Ref<EmailMessage>.IsAny))
            .Returns(new TryDequeueDelegate((out EmailMessage msg) =>
            {
                if (!dequeued)
                {
                    dequeued = true;
                    msg = message;
                    return true;
                }
                msg = null!;
                return false;
            }));
    }
}
