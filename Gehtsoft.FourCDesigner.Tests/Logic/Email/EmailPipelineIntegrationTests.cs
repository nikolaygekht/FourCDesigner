using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Gehtsoft.FourCDesigner.Logic.Email;
using Gehtsoft.FourCDesigner.Logic.Email.Configuration;
using Gehtsoft.FourCDesigner.Logic.Email.Model;
using Gehtsoft.FourCDesigner.Logic.Email.Queue;
using Gehtsoft.FourCDesigner.Logic.Email.Sender;
using Gehtsoft.FourCDesigner.Logic.Email.Storage;

namespace Gehtsoft.FourCDesigner.Tests.Logic.Email;

/// <summary>
/// Integration tests for the complete email pipeline using a real SMTP server with authentication.
/// Tests cover serialization, SMTP protocol, authentication, and email delivery.
/// </summary>
[Collection("SmtpServer")]
public class EmailPipelineIntegrationTests : IDisposable
{
    private readonly SmtpServerFixture mSmtpServerFixture;
    private readonly IEmailConfiguration mEmailConfig;
    private readonly IEmailQueue mEmailQueue;
    private readonly IEmailStorage mEmailStorage;
    private readonly IEmailSenderService mEmailSenderService;
    private readonly string mTempEmailFolder;

    public EmailPipelineIntegrationTests(SmtpServerFixture smtpServerFixture)
    {
        mSmtpServerFixture = smtpServerFixture;
        mTempEmailFolder = Path.Combine(Path.GetTempPath(), $"EmailTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(mTempEmailFolder);

        // Configure email components to use test SMTP server with authentication
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["email:smtpServer"] = "localhost",
                ["email:smtpPort"] = mSmtpServerFixture.SmtpServer.Port.ToString(),
                ["email:sslMode"] = "None",
                ["email:smtpUser"] = mSmtpServerFixture.SmtpServer.Username,
                ["email:smtpPassword"] = mSmtpServerFixture.SmtpServer.Password,
                ["email:mailAddressFrom"] = "test@4cdesigner.com",
                ["email:queueFolder"] = Path.Combine(mTempEmailFolder, "queue"),
                ["email:badEmailFolder"] = Path.Combine(mTempEmailFolder, "bad"),
                ["email:disableSending"] = "false",
                ["email:delayBetweenMessagesSeconds"] = "0"
            }!)
            .Build();

        // Initialize logging for tests
        TestLogging.Initialize();

        // Create real email components with real loggers
        mEmailConfig = new EmailConfiguration(config);
        mEmailStorage = new FileEmailStorage(mEmailConfig, TestLogging.CreateLogger<FileEmailStorage>());
        mEmailQueue = new EmailQueue(mEmailStorage, TestLogging.CreateLogger<EmailQueue>());

        var senderState = new EmailSenderState();
        var services = new ServiceCollection();
        services.AddScoped<ISmtpSender>(_ => new SmtpSender(mEmailConfig, TestLogging.CreateLogger<SmtpSender>()));
        var serviceProvider = services.BuildServiceProvider();

        mEmailSenderService = new EmailSenderService(
            mEmailQueue, mEmailStorage, mEmailConfig, senderState,
            serviceProvider, TestLogging.CreateLogger<EmailSenderService>()
        );

        mSmtpServerFixture.SmtpServer.ClearEmails();
    }

    #region 2.1 Basic SMTP Integration Tests

    [Fact]
    public async Task EmailPipeline_SendSimpleEmail_AuthenticatesAndDeliversViaRealSmtp()
    {
        // Arrange: Ensure SMTP server is running
        await mSmtpServerFixture.SmtpServer.EnsureServerRunningAsync();

        var testEmail = EmailMessage.Create(
            to: "recipient@test.com",
            subject: "Test Email",
            body: "This is a test email body",
            html: false,
            priority: false
        );

        // Act
        mEmailQueue.Enqueue(testEmail);
        await mEmailSenderService.ProcessQueueAsync();
        await Task.Delay(1000); // Give SMTP server time to process

        // Assert
        var receivedEmails = mSmtpServerFixture.SmtpServer.GetReceivedEmails();
        receivedEmails.Should().HaveCount(1);

        var receivedEmail = receivedEmails[0];
        var message = receivedEmail.Message;

        message.To.Mailboxes.Should().Contain(mb => mb.Address == "recipient@test.com");
        message.Subject.Should().Be("Test Email");
        message.TextBody.Should().Contain("This is a test email body");
        message.From.Mailboxes.Should().Contain(mb => mb.Address == "test@4cdesigner.com");
    }

    [Fact]
    public async Task EmailPipeline_SendHtmlEmail_PreservesFormatting()
    {
        // Arrange: Ensure SMTP server is running
        await mSmtpServerFixture.SmtpServer.EnsureServerRunningAsync();

        var htmlBody = "<html><body><h1>Test Header</h1><p>Test paragraph</p></body></html>";
        var testEmail = EmailMessage.Create(
            to: "recipient@test.com",
            subject: "HTML Test Email",
            body: htmlBody,
            html: true,
            priority: false
        );

        // Act
        mEmailQueue.Enqueue(testEmail);
        await mEmailSenderService.ProcessQueueAsync();
        await Task.Delay(1000);

        // Assert
        var receivedEmail = mSmtpServerFixture.SmtpServer.GetMostRecentEmailFor("recipient@test.com");
        receivedEmail.Should().NotBeNull();

        var message = receivedEmail!.Message;
        message.Subject.Should().Be("HTML Test Email");
        message.HtmlBody.Should().Contain("<h1>Test Header</h1>");
        message.HtmlBody.Should().Contain("<p>Test paragraph</p>");
    }

    #endregion

    #region 2.3 Activation & Password Reset Email Tests

    [Fact]
    public async Task EmailPipeline_ActivationEmail_ContainsValidTokenAndUrl()
    {
        // Arrange: Ensure SMTP server is running
        await mSmtpServerFixture.SmtpServer.EnsureServerRunningAsync();

        var token = "123456";
        var email = "newuser@test.com";
        var activationUrl = $"https://4cdesigner.com/activate-account?email={Uri.EscapeDataString(email)}&token={token}";

        var activationEmail = EmailMessage.Create(
            to: email,
            subject: "Activate Your 4C Designer Account",
            body: $"Welcome! Please activate your account using this code: {token}\n\n" +
                  $"Or click here: {activationUrl}\n\n" +
                  $"This code expires in 24 hours.",
            html: false,
            priority: false
        );

        // Act
        mEmailQueue.Enqueue(activationEmail);
        await mEmailSenderService.ProcessQueueAsync();
        await Task.Delay(1000);

        // Assert
        var receivedEmail = mSmtpServerFixture.SmtpServer.GetMostRecentEmailFor(email);
        receivedEmail.Should().NotBeNull();

        var message = receivedEmail!.Message;
        message.Subject.Should().Contain("Activate");
        message.TextBody.Should().Contain(token);
        message.TextBody.Should().Contain("activate-account");

        // Verify token format (6 digits)
        var tokenMatch = Regex.Match(message.TextBody, @"\b\d{6}\b");
        tokenMatch.Success.Should().BeTrue();
        tokenMatch.Value.Should().Be(token);
    }

    [Fact]
    public async Task EmailPipeline_PasswordResetEmail_ContainsValidTokenAndUrl()
    {
        // Arrange: Ensure SMTP server is running
        await mSmtpServerFixture.SmtpServer.EnsureServerRunningAsync();

        var token = "987654";
        var email = "user@test.com";
        var resetUrl = $"https://4cdesigner.com/reset-password?email={Uri.EscapeDataString(email)}&token={token}";

        var resetEmail = EmailMessage.Create(
            to: email,
            subject: "Reset Your 4C Designer Password",
            body: $"You requested a password reset. Use this code: {token}\n\n" +
                  $"Or click here: {resetUrl}\n\n" +
                  $"This code expires in 1 hour.",
            html: false,
            priority: false
        );

        // Act
        mEmailQueue.Enqueue(resetEmail);
        await mEmailSenderService.ProcessQueueAsync();
        await Task.Delay(1000);

        // Assert
        var receivedEmail = mSmtpServerFixture.SmtpServer.GetMostRecentEmailFor(email);
        receivedEmail.Should().NotBeNull();

        var message = receivedEmail!.Message;
        message.Subject.Should().Contain("Reset");
        message.TextBody.Should().Contain(token);
        message.TextBody.Should().Contain("reset-password");

        // Verify token format (6 digits)
        var tokenMatch = Regex.Match(message.TextBody, @"\b\d{6}\b");
        tokenMatch.Success.Should().BeTrue();
        tokenMatch.Value.Should().Be(token);
    }

    #endregion

    #region 2.4 MIME Encoding & Special Characters Tests

    [Fact]
    public async Task EmailPipeline_EmailWithUnicodeCharacters_EncodesCorrectly()
    {
        // Arrange: Ensure SMTP server is running
        await mSmtpServerFixture.SmtpServer.EnsureServerRunningAsync();

        var testEmail = EmailMessage.Create(
            to: "recipient@test.com",
            subject: "Test Unicode: Тест • Café • 中文",
            body: "Unicode content: Тестовый текст • Café • 中文测试",
            html: false,
            priority: false
        );

        // Act
        mEmailQueue.Enqueue(testEmail);
        await mEmailSenderService.ProcessQueueAsync();
        await Task.Delay(1000);

        // Assert
        var receivedEmail = mSmtpServerFixture.SmtpServer.GetMostRecentEmailFor("recipient@test.com");
        receivedEmail.Should().NotBeNull();

        var message = receivedEmail!.Message;
        message.Subject.Should().Contain("Тест");
        message.Subject.Should().Contain("Café");
        message.Subject.Should().Contain("中文");
        message.TextBody.Should().Contain("Тестовый текст");
        message.TextBody.Should().Contain("Café");
        message.TextBody.Should().Contain("中文测试");
    }

    [Fact]
    public async Task EmailPipeline_EmailWithEmojis_EncodesCorrectly()
    {
        // Arrange: Ensure SMTP server is running
        await mSmtpServerFixture.SmtpServer.EnsureServerRunningAsync();

        var testEmail = EmailMessage.Create(
            to: "recipient@test.com",
            subject: "Test Emojis 🎉 🚀 ✨",
            body: "Emoji content: Welcome! 🎉 Ready to launch! 🚀 Amazing! ✨",
            html: false,
            priority: false
        );

        // Act
        mEmailQueue.Enqueue(testEmail);
        await mEmailSenderService.ProcessQueueAsync();
        await Task.Delay(1000);

        // Assert
        var receivedEmail = mSmtpServerFixture.SmtpServer.GetMostRecentEmailFor("recipient@test.com");
        receivedEmail.Should().NotBeNull();

        var message = receivedEmail!.Message;
        message.Subject.Should().Contain("🎉");
        message.Subject.Should().Contain("🚀");
        message.Subject.Should().Contain("✨");
        message.TextBody.Should().Contain("Welcome! 🎉");
        message.TextBody.Should().Contain("Ready to launch! 🚀");
        message.TextBody.Should().Contain("Amazing! ✨");
    }

    #endregion

    #region 2.5 High Volume Tests

    [Fact]
    public async Task EmailPipeline_MultipleEmailsInSequence_AllDeliveredCorrectly()
    {
        // Arrange: Ensure SMTP server is running
        await mSmtpServerFixture.SmtpServer.EnsureServerRunningAsync();

        var emails = new[]
        {
            EmailMessage.Create("user1@test.com", "Subject 1", "Body 1", false, false),
            EmailMessage.Create("user2@test.com", "Subject 2", "Body 2", false, false),
            EmailMessage.Create("user3@test.com", "Subject 3", "Body 3", false, false),
            EmailMessage.Create("user4@test.com", "Subject 4", "Body 4", false, false),
            EmailMessage.Create("user5@test.com", "Subject 5", "Body 5", false, false)
        };

        // Act
        foreach (var email in emails)
        {
            mEmailQueue.Enqueue(email);
        }
        await mEmailSenderService.ProcessQueueAsync();
        await Task.Delay(2000); // Give more time for multiple emails

        // Assert
        var receivedEmails = mSmtpServerFixture.SmtpServer.GetReceivedEmails();
        receivedEmails.Should().HaveCount(5);

        // Verify each email was delivered
        for (int i = 0; i < 5; i++)
        {
            var expectedEmail = $"user{i + 1}@test.com";
            var receivedEmail = mSmtpServerFixture.SmtpServer.GetMostRecentEmailFor(expectedEmail);
            receivedEmail.Should().NotBeNull();
            receivedEmail!.Message.Subject.Should().Be($"Subject {i + 1}");
            receivedEmail.Message.TextBody.Should().Contain($"Body {i + 1}");
        }
    }

    [Fact]
    public async Task EmailPipeline_PriorityAndNormalEmails_AllDeliveredCorrectly()
    {
        // Arrange: Ensure SMTP server is running
        await mSmtpServerFixture.SmtpServer.EnsureServerRunningAsync();

        var normalEmail = EmailMessage.Create("normal@test.com", "Normal Email", "Normal body", false, false);
        var priorityEmail = EmailMessage.Create("priority@test.com", "Priority Email", "Priority body", false, true);

        // Act
        mEmailQueue.Enqueue(normalEmail);
        mEmailQueue.Enqueue(priorityEmail);
        await mEmailSenderService.ProcessQueueAsync();
        await Task.Delay(1500);

        // Assert
        var receivedEmails = mSmtpServerFixture.SmtpServer.GetReceivedEmails();
        receivedEmails.Should().HaveCount(2);

        // Both should be delivered
        var normalReceived = mSmtpServerFixture.SmtpServer.GetMostRecentEmailFor("normal@test.com");
        normalReceived.Should().NotBeNull();
        normalReceived!.Message.Subject.Should().Be("Normal Email");

        var priorityReceived = mSmtpServerFixture.SmtpServer.GetMostRecentEmailFor("priority@test.com");
        priorityReceived.Should().NotBeNull();
        priorityReceived!.Message.Subject.Should().Be("Priority Email");
    }

    #endregion

    public void Dispose()
    {
        if (Directory.Exists(mTempEmailFolder))
        {
            try { Directory.Delete(mTempEmailFolder, true); }
            catch { /* Ignore cleanup errors */ }
        }
        mSmtpServerFixture.SmtpServer.ClearEmails();
    }
}
