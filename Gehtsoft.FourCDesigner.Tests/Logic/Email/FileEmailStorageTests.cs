using System.Text;
using FluentAssertions;
using Gehtsoft.FourCDesigner.Logic.Email.Configuration;
using Gehtsoft.FourCDesigner.Logic.Email.Model;
using Gehtsoft.FourCDesigner.Logic.Email.Storage;
using Microsoft.Extensions.Logging;
using Moq;

namespace Gehtsoft.FourCDesigner.Tests.Logic.Email;

/// <summary>
/// Unit tests for FileEmailStorage class.
/// </summary>
public class FileEmailStorageTests : IDisposable
{
    private readonly string mTestQueueFolder;
    private readonly string mTestBadEmailFolder;
    private readonly Mock<IEmailConfiguration> mConfigMock;
    private readonly Mock<ILogger<FileEmailStorage>> mLoggerMock;
    private readonly FileEmailStorage mStorage;

    public FileEmailStorageTests()
    {
        // Create unique test folders for each test run
        string testId = Guid.NewGuid().ToString("N").Substring(0, 8);
        mTestQueueFolder = Path.Combine(Path.GetTempPath(), $"EmailQueueTest_{testId}");
        mTestBadEmailFolder = Path.Combine(Path.GetTempPath(), $"BadEmailTest_{testId}");

        // Setup mocks
        mConfigMock = new Mock<IEmailConfiguration>();
        mConfigMock.Setup(c => c.QueueFolder).Returns(mTestQueueFolder);
        mConfigMock.Setup(c => c.BadEmailFolder).Returns(mTestBadEmailFolder);

        mLoggerMock = new Mock<ILogger<FileEmailStorage>>();

        // Create storage instance
        mStorage = new FileEmailStorage(mConfigMock.Object, mLoggerMock.Object);
    }

    public void Dispose()
    {
        // Clean up test folders
        if (Directory.Exists(mTestQueueFolder))
            Directory.Delete(mTestQueueFolder, true);

        if (Directory.Exists(mTestBadEmailFolder))
            Directory.Delete(mTestBadEmailFolder, true);
    }

    [Fact]
    public void Constructor_ShouldCreateQueueFolder()
    {
        // Assert
        Directory.Exists(mTestQueueFolder).Should().BeTrue();
    }

    [Fact]
    public void Constructor_ShouldCreateBadEmailFolder()
    {
        // Assert
        Directory.Exists(mTestBadEmailFolder).Should().BeTrue();
    }

    [Fact]
    public void WriteMessage_ShouldCreateFileOnDisk()
    {
        // Arrange
        var message = EmailMessage.Create("user@example.com", "Test Subject", "Test Body");

        // Act
        mStorage.WriteMessage(message);

        // Assert
        string expectedFilePath = Path.Combine(mTestQueueFolder, $"{message.Id}.json");
        File.Exists(expectedFilePath).Should().BeTrue("the message file should be created on disk");
    }

    [Fact]
    public void WriteMessage_ShouldWriteValidJsonContent()
    {
        // Arrange
        var message = EmailMessage.Create("user@example.com", "Test Subject", "Test Body");

        // Act
        mStorage.WriteMessage(message);

        // Assert
        string filePath = Path.Combine(mTestQueueFolder, $"{message.Id}.json");
        string fileContent = File.ReadAllText(filePath);

        fileContent.Should().Contain("\"id\":");
        fileContent.Should().Contain("\"subject\": \"Test Subject\"");
        fileContent.Should().Contain("\"body\": \"Test Body\"");
        fileContent.Should().Contain("\"to\": [");
    }

    [Fact]
    public void DoesMessageExist_WithExistingMessage_ShouldReturnTrue()
    {
        // Arrange
        var message = EmailMessage.Create("user@example.com", "Test", "Body");
        mStorage.WriteMessage(message);

        // Act
        bool exists = mStorage.DoesMessageExist(message.Id);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public void DoesMessageExist_WithNonExistingMessage_ShouldReturnFalse()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();

        // Act
        bool exists = mStorage.DoesMessageExist(nonExistentId);

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public void ReadMessage_ShouldReturnCorrectMessage()
    {
        // Arrange
        var originalMessage = new EmailMessage
        {
            Subject = "Test Subject",
            Body = "Test Body",
            To = new[] { "user1@example.com", "user2@example.com" },
            Priority = true,
            HtmlContent = true
        };

        mStorage.WriteMessage(originalMessage);

        // Act
        var readMessage = mStorage.ReadMessage(originalMessage.Id);

        // Assert
        readMessage.Should().NotBeNull();
        readMessage.Id.Should().Be(originalMessage.Id);
        readMessage.Subject.Should().Be(originalMessage.Subject);
        readMessage.Body.Should().Be(originalMessage.Body);
        readMessage.To.Should().BeEquivalentTo(originalMessage.To);
        readMessage.Priority.Should().Be(originalMessage.Priority);
        readMessage.HtmlContent.Should().Be(originalMessage.HtmlContent);
    }

    [Fact]
    public void ReadMessage_WithAttachments_ShouldPreserveAttachments()
    {
        // Arrange
        var message = EmailMessage.Create("user@example.com", "Test", "Body");
        byte[] attachmentContent = Encoding.UTF8.GetBytes("Attachment content");
        message.Attachments.Add(new EmailAttachment("test.txt", "text/plain", attachmentContent));

        mStorage.WriteMessage(message);

        // Act
        var readMessage = mStorage.ReadMessage(message.Id);

        // Assert
        readMessage.Attachments.Should().HaveCount(1);
        readMessage.Attachments[0].FileName.Should().Be("test.txt");
        readMessage.Attachments[0].ContentType.Should().Be("text/plain");
        readMessage.Attachments[0].GetContent().Should().BeEquivalentTo(attachmentContent);
    }

    [Fact]
    public void ReadMessage_WithNonExistentId_ShouldReturnNull()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();

        // Act
        var message = mStorage.ReadMessage(nonExistentId);

        // Assert
        message.Should().BeNull();
    }

    [Fact]
    public void GetMessageIds_ShouldReturnAllMessageIds()
    {
        // Arrange
        var message1 = EmailMessage.Create("user1@example.com", "Test 1", "Body 1");
        var message2 = EmailMessage.Create("user2@example.com", "Test 2", "Body 2");
        var message3 = EmailMessage.Create("user3@example.com", "Test 3", "Body 3");

        mStorage.WriteMessage(message1);
        mStorage.WriteMessage(message2);
        mStorage.WriteMessage(message3);

        // Act
        Guid[] ids = mStorage.GetMessageIds();

        // Assert
        ids.Should().HaveCount(3);
        ids.Should().Contain(message1.Id);
        ids.Should().Contain(message2.Id);
        ids.Should().Contain(message3.Id);
    }

    [Fact]
    public void GetMessageIds_WithEmptyStorage_ShouldReturnEmptyArray()
    {
        // Act
        Guid[] ids = mStorage.GetMessageIds();

        // Assert
        ids.Should().BeEmpty();
    }

    [Fact]
    public void DeleteMessage_ShouldRemoveFileFromDisk()
    {
        // Arrange
        var message = EmailMessage.Create("user@example.com", "Test", "Body");
        mStorage.WriteMessage(message);

        string filePath = Path.Combine(mTestQueueFolder, $"{message.Id}.json");
        File.Exists(filePath).Should().BeTrue("file should exist before deletion");

        // Act
        mStorage.DeleteMessage(message.Id);

        // Assert
        File.Exists(filePath).Should().BeFalse("file should be deleted");
    }

    [Fact]
    public void DeleteMessage_WithNonExistentMessage_ShouldNotThrow()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();

        // Act
        Action act = () => mStorage.DeleteMessage(nonExistentId);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void DeleteAllMessages_ShouldRemoveAllFiles()
    {
        // Arrange
        mStorage.WriteMessage(EmailMessage.Create("user1@example.com", "Test 1", "Body 1"));
        mStorage.WriteMessage(EmailMessage.Create("user2@example.com", "Test 2", "Body 2"));
        mStorage.WriteMessage(EmailMessage.Create("user3@example.com", "Test 3", "Body 3"));

        mStorage.MessagesCount().Should().Be(3);

        // Act
        mStorage.DeleteAllMessages();

        // Assert
        mStorage.MessagesCount().Should().Be(0);
        Directory.GetFiles(mTestQueueFolder, "*.json").Should().BeEmpty();
    }

    [Fact]
    public void MessagesCount_ShouldReturnCorrectCount()
    {
        // Arrange
        mStorage.WriteMessage(EmailMessage.Create("user1@example.com", "Test 1", "Body 1"));
        mStorage.WriteMessage(EmailMessage.Create("user2@example.com", "Test 2", "Body 2"));

        // Act
        int count = mStorage.MessagesCount();

        // Assert
        count.Should().Be(2);
    }

    [Fact]
    public void MessagesCount_WithEmptyStorage_ShouldReturnZero()
    {
        // Act
        int count = mStorage.MessagesCount();

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public void MoveToBadEmail_ShouldMoveFileToBadEmailFolder()
    {
        // Arrange
        var message = EmailMessage.Create("user@example.com", "Test", "Body");
        message.LastError = "Test error";
        mStorage.WriteMessage(message);

        string queueFilePath = Path.Combine(mTestQueueFolder, $"{message.Id}.json");
        string badEmailFilePath = Path.Combine(mTestBadEmailFolder, $"{message.Id}.json");

        File.Exists(queueFilePath).Should().BeTrue("file should exist in queue before move");

        // Act
        mStorage.MoveToBadEmail(message);

        // Assert
        File.Exists(queueFilePath).Should().BeFalse("file should be removed from queue");
        File.Exists(badEmailFilePath).Should().BeTrue("file should exist in bad email folder");
    }

    [Fact]
    public void MoveToBadEmail_ShouldPreserveMessageContent()
    {
        // Arrange
        var message = EmailMessage.Create("user@example.com", "Test Subject", "Test Body");
        message.LastError = "SMTP connection failed";
        message.RetryCount = 3;
        mStorage.WriteMessage(message);

        // Act
        mStorage.MoveToBadEmail(message);

        // Assert
        string badEmailFilePath = Path.Combine(mTestBadEmailFolder, $"{message.Id}.json");
        string fileContent = File.ReadAllText(badEmailFilePath);

        fileContent.Should().Contain("\"subject\": \"Test Subject\"");
        fileContent.Should().Contain("\"lastError\": \"SMTP connection failed\"");
        fileContent.Should().Contain("\"retryCount\": 3");
    }

    [Fact]
    public void WriteMessage_WithNullMessage_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => mStorage.WriteMessage(null);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("message");
    }

    [Fact]
    public void MoveToBadEmail_WithNullMessage_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => mStorage.MoveToBadEmail(null);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("message");
    }

    [Fact]
    public void ConcurrentWrites_ShouldNotCorruptData()
    {
        // Arrange
        var messages = Enumerable.Range(0, 100)
            .Select(i => EmailMessage.Create($"user{i}@example.com", $"Subject {i}", $"Body {i}"))
            .ToList();

        // Act - Write messages concurrently
        Parallel.ForEach(messages, message =>
        {
            mStorage.WriteMessage(message);
        });

        // Assert
        mStorage.MessagesCount().Should().Be(100);

        // Verify each message can be read back correctly
        foreach (var originalMessage in messages)
        {
            var readMessage = mStorage.ReadMessage(originalMessage.Id);
            readMessage.Should().NotBeNull();
            readMessage.Subject.Should().Be(originalMessage.Subject);
        }
    }
}
