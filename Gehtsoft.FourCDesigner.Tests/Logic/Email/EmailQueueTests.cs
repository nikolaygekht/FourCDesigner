// Suppress nullability warnings for intentional null tests
#pragma warning disable CS8625, CS8602, CS8600, CS8601, CS8603
using FluentAssertions;
using Gehtsoft.FourCDesigner.Logic.Email.Model;
using Gehtsoft.FourCDesigner.Logic.Email.Queue;
using Gehtsoft.FourCDesigner.Logic.Email.Storage;
using Microsoft.Extensions.Logging;
using Moq;

namespace Gehtsoft.FourCDesigner.Tests.Logic.Email;

/// <summary>
/// Unit tests for EmailQueue class.
/// </summary>
public class EmailQueueTests
{
    private readonly Mock<IEmailStorage> mStorageMock;
    private readonly Mock<ILogger<EmailQueue>> mLoggerMock;
    private readonly EmailQueue mQueue;

    public EmailQueueTests()
    {
        mStorageMock = new Mock<IEmailStorage>();
        mLoggerMock = new Mock<ILogger<EmailQueue>>();
        mQueue = new EmailQueue(mStorageMock.Object, mLoggerMock.Object);
    }

    [Fact]
    public void Constructor_WithNullStorage_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new EmailQueue(null, mLoggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("storage");
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new EmailQueue(mStorageMock.Object, null);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void Count_InitiallyEmpty_ShouldReturnZero()
    {
        // Assert
        mQueue.Count.Should().Be(0);
    }

    [Fact]
    public void Enqueue_WithValidMessage_ShouldWriteToStorage()
    {
        // Arrange
        var message = EmailMessage.Create("test@example.com", "Subject", "Body");

        // Act
        mQueue.Enqueue(message);

        // Assert
        mStorageMock.Verify(s => s.WriteMessage(message), Times.Once);
    }

    [Fact]
    public void Enqueue_WithValidMessage_ShouldIncreaseCount()
    {
        // Arrange
        var message = EmailMessage.Create("test@example.com", "Subject", "Body");

        // Act
        mQueue.Enqueue(message);

        // Assert
        mQueue.Count.Should().Be(1);
    }

    [Fact]
    public void Enqueue_WithNullMessage_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => mQueue.Enqueue(null);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("message");
    }

    [Fact]
    public void Enqueue_MultipleMessages_ShouldIncreaseCount()
    {
        // Arrange
        var message1 = EmailMessage.Create("test1@example.com", "Subject 1", "Body 1");
        var message2 = EmailMessage.Create("test2@example.com", "Subject 2", "Body 2");
        var message3 = EmailMessage.Create("test3@example.com", "Subject 3", "Body 3");

        // Act
        mQueue.Enqueue(message1);
        mQueue.Enqueue(message2);
        mQueue.Enqueue(message3);

        // Assert
        mQueue.Count.Should().Be(3);
    }

    [Fact]
    public void TryDequeue_WithEmptyQueue_ShouldReturnFalse()
    {
        // Act
        bool result = mQueue.TryDequeue(out EmailMessage message);

        // Assert
        result.Should().BeFalse();
        message.Should().BeNull();
    }

    [Fact]
    public void TryDequeue_WithOneMessage_ShouldReturnMessage()
    {
        // Arrange
        var originalMessage = EmailMessage.Create("test@example.com", "Subject", "Body");
        mQueue.Enqueue(originalMessage);

        // Act
        bool result = mQueue.TryDequeue(out EmailMessage dequeuedMessage);

        // Assert
        result.Should().BeTrue();
        dequeuedMessage.Should().NotBeNull();
        dequeuedMessage.Id.Should().Be(originalMessage.Id);
    }

    [Fact]
    public void TryDequeue_AfterDequeue_ShouldDecreaseCount()
    {
        // Arrange
        var message = EmailMessage.Create("test@example.com", "Subject", "Body");
        mQueue.Enqueue(message);
        mQueue.Count.Should().Be(1);

        // Act
        mQueue.TryDequeue(out _);

        // Assert
        mQueue.Count.Should().Be(0);
    }

    [Fact]
    public void TryDequeue_ShouldProcessHighPriorityFirst()
    {
        // Arrange
        var normalMessage1 = EmailMessage.Create("normal1@example.com", "Normal 1", "Body");
        var normalMessage2 = EmailMessage.Create("normal2@example.com", "Normal 2", "Body");
        var highPriorityMessage = EmailMessage.Create("high@example.com", "High Priority", "Body");
        highPriorityMessage.Priority = true;

        // Enqueue in this order: normal, high, normal
        mQueue.Enqueue(normalMessage1);
        mQueue.Enqueue(highPriorityMessage);
        mQueue.Enqueue(normalMessage2);

        // Act - Dequeue should get high priority first
        mQueue.TryDequeue(out EmailMessage firstMessage);

        // Assert
        firstMessage.Id.Should().Be(highPriorityMessage.Id);
        firstMessage.Subject.Should().Be("High Priority");
    }

    [Fact]
    public void TryDequeue_WithMultipleHighPriority_ShouldProcessInFIFOOrder()
    {
        // Arrange
        var highMessage1 = EmailMessage.Create("high1@example.com", "High 1", "Body");
        highMessage1.Priority = true;
        var highMessage2 = EmailMessage.Create("high2@example.com", "High 2", "Body");
        highMessage2.Priority = true;
        var highMessage3 = EmailMessage.Create("high3@example.com", "High 3", "Body");
        highMessage3.Priority = true;

        mQueue.Enqueue(highMessage1);
        mQueue.Enqueue(highMessage2);
        mQueue.Enqueue(highMessage3);

        // Act
        mQueue.TryDequeue(out EmailMessage first);
        mQueue.TryDequeue(out EmailMessage second);
        mQueue.TryDequeue(out EmailMessage third);

        // Assert
        first.Id.Should().Be(highMessage1.Id);
        second.Id.Should().Be(highMessage2.Id);
        third.Id.Should().Be(highMessage3.Id);
    }

    [Fact]
    public void TryDequeue_AfterHighPriorityExhausted_ShouldProcessNormalPriority()
    {
        // Arrange
        var highMessage = EmailMessage.Create("high@example.com", "High", "Body");
        highMessage.Priority = true;
        var normalMessage = EmailMessage.Create("normal@example.com", "Normal", "Body");

        mQueue.Enqueue(highMessage);
        mQueue.Enqueue(normalMessage);

        // Act
        mQueue.TryDequeue(out EmailMessage first);
        mQueue.TryDequeue(out EmailMessage second);

        // Assert
        first.Id.Should().Be(highMessage.Id);
        second.Id.Should().Be(normalMessage.Id);
    }

    [Fact]
    public void Requeue_WithValidMessage_ShouldWriteToStorage()
    {
        // Arrange
        var message = EmailMessage.Create("test@example.com", "Subject", "Body");
        message.RetryCount = 1;
        message.LastError = "SMTP error";

        // Act
        mQueue.Requeue(message);

        // Assert
        mStorageMock.Verify(s => s.WriteMessage(message), Times.Once);
    }

    [Fact]
    public void Requeue_WithValidMessage_ShouldAddBackToQueue()
    {
        // Arrange
        var message = EmailMessage.Create("test@example.com", "Subject", "Body");
        message.RetryCount = 1;

        // Act
        mQueue.Requeue(message);

        // Assert
        mQueue.Count.Should().Be(1);
    }

    [Fact]
    public void Requeue_WithNullMessage_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => mQueue.Requeue(null);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("message");
    }

    [Fact]
    public void Requeue_WithHighPriorityMessage_ShouldAddToHighPriorityQueue()
    {
        // Arrange
        var highPriorityMessage = EmailMessage.Create("high@example.com", "High", "Body");
        highPriorityMessage.Priority = true;
        highPriorityMessage.RetryCount = 1;

        var normalMessage = EmailMessage.Create("normal@example.com", "Normal", "Body");

        // Act
        mQueue.Enqueue(normalMessage);
        mQueue.Requeue(highPriorityMessage);

        // Dequeue should get the requeued high priority message first
        mQueue.TryDequeue(out EmailMessage dequeuedMessage);

        // Assert
        dequeuedMessage.Id.Should().Be(highPriorityMessage.Id);
    }

    [Fact]
    public void LoadFromStorage_WithEmptyStorage_ShouldNotAddMessages()
    {
        // Arrange
        mStorageMock.Setup(s => s.GetMessageIds()).Returns(Array.Empty<Guid>());

        // Act
        mQueue.LoadFromStorage();

        // Assert
        mQueue.Count.Should().Be(0);
    }

    [Fact]
    public void LoadFromStorage_WithMessages_ShouldLoadIntoQueue()
    {
        // Arrange
        var message1 = EmailMessage.Create("test1@example.com", "Subject 1", "Body 1");
        var message2 = EmailMessage.Create("test2@example.com", "Subject 2", "Body 2");
        var message3 = EmailMessage.Create("test3@example.com", "Subject 3", "Body 3");

        var messageIds = new[] { message1.Id, message2.Id, message3.Id };
        mStorageMock.Setup(s => s.GetMessageIds()).Returns(messageIds);
        mStorageMock.Setup(s => s.ReadMessage(message1.Id)).Returns(message1);
        mStorageMock.Setup(s => s.ReadMessage(message2.Id)).Returns(message2);
        mStorageMock.Setup(s => s.ReadMessage(message3.Id)).Returns(message3);

        // Act
        mQueue.LoadFromStorage();

        // Assert
        mQueue.Count.Should().Be(3);
    }

    [Fact]
    public void LoadFromStorage_WithHighAndNormalPriority_ShouldPreserveOrdering()
    {
        // Arrange
        var normalMessage = EmailMessage.Create("normal@example.com", "Normal", "Body");
        var highMessage = EmailMessage.Create("high@example.com", "High", "Body");
        highMessage.Priority = true;

        // Note: Storage returns them in this order
        var messageIds = new[] { normalMessage.Id, highMessage.Id };
        mStorageMock.Setup(s => s.GetMessageIds()).Returns(messageIds);
        mStorageMock.Setup(s => s.ReadMessage(normalMessage.Id)).Returns(normalMessage);
        mStorageMock.Setup(s => s.ReadMessage(highMessage.Id)).Returns(highMessage);

        // Act
        mQueue.LoadFromStorage();

        // Assert
        mQueue.Count.Should().Be(2);

        // High priority should be dequeued first regardless of storage order
        mQueue.TryDequeue(out EmailMessage first);
        first.Id.Should().Be(highMessage.Id);
    }

    [Fact]
    public void LoadFromStorage_WithNullMessages_ShouldSkipThem()
    {
        // Arrange
        var message1 = EmailMessage.Create("test1@example.com", "Subject 1", "Body 1");
        var invalidId = Guid.NewGuid();
        var message2 = EmailMessage.Create("test2@example.com", "Subject 2", "Body 2");

        var messageIds = new[] { message1.Id, invalidId, message2.Id };
        mStorageMock.Setup(s => s.GetMessageIds()).Returns(messageIds);
        mStorageMock.Setup(s => s.ReadMessage(message1.Id)).Returns(message1);
        mStorageMock.Setup(s => s.ReadMessage(invalidId)).Returns((EmailMessage)null);
        mStorageMock.Setup(s => s.ReadMessage(message2.Id)).Returns(message2);

        // Act
        mQueue.LoadFromStorage();

        // Assert
        mQueue.Count.Should().Be(2);
    }

    [Fact]
    public void ConcurrentOperations_ShouldHandleThreadSafely()
    {
        // Arrange
        var messages = Enumerable.Range(0, 50)
            .Select(i => EmailMessage.Create($"user{i}@example.com", $"Subject {i}", $"Body {i}"))
            .ToList();

        // Act - Enqueue messages concurrently
        Parallel.ForEach(messages, message =>
        {
            mQueue.Enqueue(message);
        });

        // Assert
        mQueue.Count.Should().Be(50);

        // Dequeue all and verify count goes to zero
        int dequeuedCount = 0;
        while (mQueue.TryDequeue(out _))
            dequeuedCount++;

        dequeuedCount.Should().Be(50);
        mQueue.Count.Should().Be(0);
    }

    [Fact]
    public void MixedOperations_EnqueueDequeueRequeue_ShouldMaintainConsistency()
    {
        // Arrange
        var message1 = EmailMessage.Create("test1@example.com", "Subject 1", "Body 1");
        var message2 = EmailMessage.Create("test2@example.com", "Subject 2", "Body 2");

        // Act
        mQueue.Enqueue(message1);
        mQueue.Enqueue(message2);
        mQueue.Count.Should().Be(2);

        mQueue.TryDequeue(out EmailMessage dequeued);
        mQueue.Count.Should().Be(1);

        dequeued.RetryCount++;
        mQueue.Requeue(dequeued);
        mQueue.Count.Should().Be(2);

        // Assert
        mStorageMock.Verify(s => s.WriteMessage(It.IsAny<EmailMessage>()), Times.Exactly(3));
    }
}
