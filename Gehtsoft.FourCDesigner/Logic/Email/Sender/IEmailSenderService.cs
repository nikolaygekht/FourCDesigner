namespace Gehtsoft.FourCDesigner.Logic.Email.Sender;

/// <summary>
/// Service for processing and sending email messages from the queue.
/// </summary>
public interface IEmailSenderService
{
    /// <summary>
    /// Processes all messages in the queue and sends them.
    /// </summary>
    /// <param name="stoppingToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ProcessQueueAsync(CancellationToken stoppingToken = default);
}
