using Gehtsoft.FourCDesigner.Logic.Email.Model;

namespace Gehtsoft.FourCDesigner.Logic.Email;

/// <summary>
/// ECB Controller interface for email operations.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email message by adding it to the queue.
    /// </summary>
    /// <param name="message">The email message to send.</param>
    void SendEmail(EmailMessage message);

    /// <summary>
    /// Sends an email message by adding it to the queue and triggers immediate processing.
    /// Useful for activation and password reset emails.
    /// </summary>
    /// <param name="message">The email message to send.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SendEmailAndTriggerProcessorAsync(EmailMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of messages in the queue.
    /// </summary>
    int QueueSize { get; }

    /// <summary>
    /// Gets a value indicating whether the email sender is currently active.
    /// </summary>
    bool IsSenderActive { get; }

    /// <summary>
    /// Gets the last error that occurred during sending.
    /// </summary>
    Exception LastError { get; }

    /// <summary>
    /// Gets the date and time of the last error.
    /// </summary>
    DateTime? LastErrorTime { get; }

    /// <summary>
    /// Gets the date and time when the sender will resume after an error.
    /// </summary>
    DateTime? PauseAfterErrorUntil { get; }
}
