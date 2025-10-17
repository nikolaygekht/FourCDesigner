using Gehtsoft.FourCDesigner.Logic.Email.Model;
using Gehtsoft.FourCDesigner.Logic.Email.Queue;
using Gehtsoft.FourCDesigner.Logic.Email.Sender;

namespace Gehtsoft.FourCDesigner.Logic.Email;

/// <summary>
/// ECB Controller for email operations.
/// </summary>
public class EmailService : IEmailService
{
    private readonly IEmailQueue mQueue;
    private readonly IEmailSenderService mEmailSenderService;
    private readonly EmailSenderState mSenderState;
    private readonly ILogger<EmailService> mLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailService"/> class.
    /// </summary>
    /// <param name="queue">The email queue.</param>
    /// <param name="emailSenderService">The email sender service.</param>
    /// <param name="senderState">The shared sender state.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public EmailService(IEmailQueue queue, IEmailSenderService emailSenderService, EmailSenderState senderState, ILogger<EmailService> logger)
    {
        mQueue = queue ?? throw new ArgumentNullException(nameof(queue));
        mEmailSenderService = emailSenderService ?? throw new ArgumentNullException(nameof(emailSenderService));
        mSenderState = senderState ?? throw new ArgumentNullException(nameof(senderState));
        mLogger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public void SendEmail(EmailMessage message)
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));

        try
        {
            mLogger.LogInformation("Email: Queueing message {Id} to {Recipients}",
                message.Id, string.Join(", ", message.To));

            mQueue.Enqueue(message);
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Email: Failed to queue message {Id}", message.Id);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task SendEmailAndTriggerProcessorAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));

        try
        {
            mLogger.LogInformation("Email: Queueing message {Id} to {Recipients} with immediate processing",
                message.Id, string.Join(", ", message.To));

            mQueue.Enqueue(message);

            // Trigger immediate processing asynchronously
            await mEmailSenderService.ProcessQueueAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Email: Failed to queue and process message {Id}", message.Id);
            throw;
        }
    }

    /// <inheritdoc/>
    public int QueueSize => mQueue.Count;

    /// <inheritdoc/>
    public bool IsSenderActive => mSenderState.IsSenderActive;

    /// <inheritdoc/>
    public Exception LastError => mSenderState.LastError;

    /// <inheritdoc/>
    public DateTime? LastErrorTime => mSenderState.LastErrorTime;

    /// <inheritdoc/>
    public DateTime? PauseAfterErrorUntil => mSenderState.PauseAfterErrorUntil;
}
