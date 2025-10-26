using FluentAssertions;
using Gehtsoft.FourCDesigner.Logic.Email;
using Gehtsoft.FourCDesigner.Logic.Email.Configuration;
using Gehtsoft.FourCDesigner.Logic.Email.Model;
using Gehtsoft.FourCDesigner.Logic.Email.Queue;
using Gehtsoft.FourCDesigner.Logic.Email.Sender;
using Gehtsoft.FourCDesigner.Logic.Email.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Gehtsoft.FourCDesigner.Tests.Logic.Email;

/// <summary>
/// Fast unit tests for complete email pipeline using mock SMTP sender.
/// Tests queue -> sender service -> SMTP flow without network I/O.
/// </summary>
public class EmailPipelineTests : IDisposable
{
    private readonly string mTempDir;
    private readonly IEmailStorage mStorage;
    private readonly EmailQueue mQueue;
    private readonly Mock<IEmailConfiguration> mConfigMock;
    private readonly MockSmtpSender mMockSmtpSender;
    private readonly EmailSenderService mSenderService;
    private readonly ServiceProvider mServiceProvider;

    public EmailPipelineTests()
    {
        // Create temp directory for email storage
        mTempDir = Path.Combine(Path.GetTempPath(), $"EmailTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(mTempDir);

        // Setup email configuration mock
        mConfigMock = new Mock<IEmailConfiguration>();
        mConfigMock.Setup(c => c.MailAddressFrom).Returns("test@test.com");
        mConfigMock.Setup(c => c.DisableSending).Returns(false);
        mConfigMock.Setup(c => c.QueueFolder).Returns(Path.Combine(mTempDir, "queue"));
        mConfigMock.Setup(c => c.BadEmailFolder).Returns(Path.Combine(mTempDir, "bad"));

        // Setup storage and queue
        mStorage = new FileEmailStorage(mConfigMock.Object, NullLogger<FileEmailStorage>.Instance);
        mQueue = new EmailQueue(mStorage, NullLogger<EmailQueue>.Instance);

        // Create mock SMTP sender
        mMockSmtpSender = new MockSmtpSender();

        // Create service provider with mock SMTP sender
        var services = new ServiceCollection();
        services.AddScoped<ISmtpSender>(provider => mMockSmtpSender);
        mServiceProvider = services.BuildServiceProvider();

        // Create sender service with proper DI
        var senderState = new EmailSenderState();
        mSenderService = new EmailSenderService(
            mQueue,
            mStorage,
            mConfigMock.Object,
            senderState,
            mServiceProvider,
            NullLogger<EmailSenderService>.Instance
        );
    }

    public void Dispose()
    {
        mServiceProvider?.Dispose();
        if (Directory.Exists(mTempDir))
            Directory.Delete(mTempDir, true);
    }

    [Fact]
    public async Task EmailPipeline_SendSimpleEmail_DeliversViaSmtpSender()
    {
        // Arrange
        var testEmail = EmailMessage.Create(
            to: "recipient@test.com",
            subject: "Test Email",
            body: "This is a test email body",
            html: false,
            priority: false
        );

        // Act
        mQueue.Enqueue(testEmail);
        await mSenderService.ProcessQueueAsync();

        // Assert
        mMockSmtpSender.SentEmails.Should().HaveCount(1);
        var sentEmail = mMockSmtpSender.SentEmails.First();
        sentEmail.Message.To.Should().Contain("recipient@test.com");
        sentEmail.Message.Subject.Should().Be("Test Email");
        sentEmail.Message.Body.Should().Be("This is a test email body");
        sentEmail.SenderAddress.Should().Be("test@test.com");
    }

    [Fact]
    public async Task EmailPipeline_SendHtmlEmail_PreservesFormatting()
    {
        // Arrange
        var htmlBody = "<html><body><h1>Test</h1><p>HTML content</p></body></html>";
        var testEmail = EmailMessage.Create(
            to: "user@test.com",
            subject: "HTML Test",
            body: htmlBody,
            html: true,
            priority: false
        );

        // Act
        mQueue.Enqueue(testEmail);
        await mSenderService.ProcessQueueAsync();

        // Assert
        mMockSmtpSender.SentEmails.Should().HaveCount(1);
        var sentEmail = mMockSmtpSender.SentEmails.First();
        sentEmail.Message.HtmlContent.Should().BeTrue();
        sentEmail.Message.Body.Should().Be(htmlBody);
    }

    [Fact]
    public async Task EmailPipeline_PriorityAndNormalEmails_PriorityProcessedFirst()
    {
        // Arrange
        var normalEmail1 = EmailMessage.Create("normal1@test.com", "Normal 1", "Body", false, priority: false);
        var priorityEmail = EmailMessage.Create("priority@test.com", "Priority", "Body", false, priority: true);
        var normalEmail2 = EmailMessage.Create("normal2@test.com", "Normal 2", "Body", false, priority: false);

        // Act - enqueue normal, then priority, then normal
        mQueue.Enqueue(normalEmail1);
        mQueue.Enqueue(priorityEmail);
        mQueue.Enqueue(normalEmail2);

        await mSenderService.ProcessQueueAsync();

        // Assert - priority should be sent first
        mMockSmtpSender.SentEmails.Should().HaveCount(3);
        mMockSmtpSender.SentEmails.First().Message.To.Should().Contain("priority@test.com");
    }

    [Fact]
    public async Task EmailPipeline_SmtpConnectionFailure_KeepsEmailInQueue()
    {
        // Arrange
        mMockSmtpSender.SetFailOnConnect(true);
        var testEmail = EmailMessage.Create("user@test.com", "Test", "Body", false, false);

        // Act
        mQueue.Enqueue(testEmail);

        // Should not throw - errors are logged
        await mSenderService.ProcessQueueAsync();

        // Assert - email should not be sent
        mMockSmtpSender.SentEmails.Should().BeEmpty();

        // Queue should still have the message (connection failed before dequeue)
        mQueue.Count.Should().Be(1);
    }

    [Fact]
    public async Task EmailPipeline_ConnectionRestored_RetriesQueuedEmail()
    {
        // Arrange
        mMockSmtpSender.SetFailOnConnect(true);
        var testEmail = EmailMessage.Create("user@test.com", "Test", "Body", false, priority: true);

        // Act - First attempt: connection fails, email stays in queue
        mQueue.Enqueue(testEmail);
        await mSenderService.ProcessQueueAsync();

        // Assert - email not sent, still in queue
        mMockSmtpSender.SentEmails.Should().BeEmpty();
        mQueue.Count.Should().Be(1);

        // Act - Second attempt: connection restored
        mMockSmtpSender.SetFailOnConnect(false);
        await mSenderService.ProcessQueueAsync();

        // Assert - email now successfully sent
        mMockSmtpSender.SentEmails.Should().HaveCount(1);
        var sentEmail = mMockSmtpSender.SentEmails.First();
        sentEmail.Message.To.Should().Contain("user@test.com");
        sentEmail.Message.Subject.Should().Be("Test");
        mQueue.Count.Should().Be(0);
    }

    [Fact]
    public async Task EmailPipeline_EmailWithUnicodeCharacters_EncodesCorrectly()
    {
        // Arrange - Test Cyrillic, Chinese, and special characters
        var testEmail = EmailMessage.Create(
            to: "user@test.com",
            subject: "Тест 测试 Spëciål",
            body: "Привет мир! 你好世界! Spëciål çhäråctërs!",
            html: false,
            priority: false
        );

        // Act
        mQueue.Enqueue(testEmail);
        await mSenderService.ProcessQueueAsync();

        // Assert
        mMockSmtpSender.SentEmails.Should().HaveCount(1);
        var sentEmail = mMockSmtpSender.SentEmails.First();
        sentEmail.Message.Subject.Should().Be("Тест 测试 Spëciål");
        sentEmail.Message.Body.Should().Be("Привет мир! 你好世界! Spëciål çhäråctërs!");
    }

    [Fact]
    public async Task EmailPipeline_EmailWithEmojis_EncodesCorrectly()
    {
        // Arrange
        var testEmail = EmailMessage.Create(
            to: "user@test.com",
            subject: "🎉 Test 🚀",
            body: "Hello! 👋 This email has emojis 😊 🎈 ✨",
            html: false,
            priority: false
        );

        // Act
        mQueue.Enqueue(testEmail);
        await mSenderService.ProcessQueueAsync();

        // Assert
        mMockSmtpSender.SentEmails.Should().HaveCount(1);
        var sentEmail = mMockSmtpSender.SentEmails.First();
        sentEmail.Message.Subject.Should().Be("🎉 Test 🚀");
        sentEmail.Message.Body.Should().Be("Hello! 👋 This email has emojis 😊 🎈 ✨");
    }
}
