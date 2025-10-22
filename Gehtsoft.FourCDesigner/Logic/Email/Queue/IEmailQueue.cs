using Gehtsoft.FourCDesigner.Logic.Email.Model;

namespace Gehtsoft.FourCDesigner.Logic.Email.Queue;

/// <summary>
/// Interface for email message queue operations.
/// </summary>
public interface IEmailQueue
{
    /// <summary>
    /// Enqueues a message for sending.
    /// </summary>
    /// <param name="message">The email message to enqueue.</param>
    void Enqueue(EmailMessage message);

    /// <summary>
    /// Tries to dequeue the next message for sending (high-priority first).
    /// </summary>
    /// <param name="message">The dequeued message, or null if queue is empty.</param>
    /// <returns>True if a message was dequeued, false if queue is empty.</returns>
    bool TryDequeue(out EmailMessage? message);

    /// <summary>
    /// Re-enqueues a message that failed to send (for retry).
    /// </summary>
    /// <param name="message">The message to re-enqueue.</param>
    void Requeue(EmailMessage message);

    /// <summary>
    /// Gets the count of messages in the queue.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Loads messages from storage into the queue (called on startup).
    /// </summary>
    void LoadFromStorage();
}
