using Gehtsoft.FourCDesigner.Logic.Email.Configuration;
using Gehtsoft.FourCDesigner.Logic.Email.Model;
using Gehtsoft.FourCDesigner.Logic.Email.Queue;
using Gehtsoft.FourCDesigner.Logic.Email.Storage;

namespace Gehtsoft.FourCDesigner.Logic.Email.Sender;

/// <summary>
/// Service for processing and sending email messages from the queue.
/// </summary>
public class EmailSenderService : IEmailSenderService
{
    private readonly IEmailQueue mQueue;
    private readonly IEmailStorage mStorage;
    private readonly IEmailConfiguration mConfiguration;
    private readonly IServiceProvider mServiceProvider;
    private readonly EmailSenderState mSenderState;
    private readonly ILogger<EmailSenderService> mLogger;
    private readonly SemaphoreSlim mMutexSlim = new SemaphoreSlim(1);

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailSenderService"/> class.
    /// </summary>
    /// <param name="queue">The email queue.</param>
    /// <param name="storage">The email storage.</param>
    /// <param name="configuration">The email configuration.</param>
    /// <param name="senderState">The shared sender state.</param>
    /// <param name="serviceProvider">The service provider (for creating scoped SMTP sender).</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public EmailSenderService(
        IEmailQueue queue,
        IEmailStorage storage,
        IEmailConfiguration configuration,
        EmailSenderState senderState,
        IServiceProvider serviceProvider,
        ILogger<EmailSenderService> logger)
    {
        mQueue = queue ?? throw new ArgumentNullException(nameof(queue));
        mStorage = storage ?? throw new ArgumentNullException(nameof(storage));
        mConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        mSenderState = senderState ?? throw new ArgumentNullException(nameof(senderState));
        mServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        mLogger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task ProcessQueueAsync(CancellationToken stoppingToken = default)
    {
        // Prevent concurrent execution
        if (!await mMutexSlim.WaitAsync(0, stoppingToken))
        {
            mLogger.LogDebug("Email: Queue processing already in progress, skipping");
            return;
        }

        try
        {
            // Check if sending is disabled
            if (mConfiguration.DisableSending)
            {
                mLogger.LogDebug("Email: Email sending is disabled. Messages remain in queue.");
                return;
            }

            // Check if we're paused due to a critical error (e.g., auth failure)
            if (mSenderState.PauseAfterErrorUntil.HasValue &&
                DateTime.UtcNow < mSenderState.PauseAfterErrorUntil.Value)
            {
                if (mSenderState.PauseAfterErrorUntil.Value == DateTime.MaxValue)
                {
                    mLogger.LogDebug("Email: Email processing is permanently disabled due to authentication failure");
                }
                else
                {
                    TimeSpan remaining = mSenderState.PauseAfterErrorUntil.Value - DateTime.UtcNow;
                    mLogger.LogDebug("Email: Email processing is paused, resuming in {Seconds} seconds", remaining.TotalSeconds);
                }
                return;
            }

            if (mQueue.Count == 0)
                return;

            mLogger.LogDebug("Email: Processing queue ({Count} messages)", mQueue.Count);

            // Create scoped SMTP sender
            using (IServiceScope scope = mServiceProvider.CreateScope())
            {
                ISmtpSender sender = scope.ServiceProvider.GetRequiredService<ISmtpSender>();

                try
                {
                    // Open SMTP connection
                    sender.Open();

                    // Process messages
                    while (mQueue.TryDequeue(out EmailMessage? message) && !stoppingToken.IsCancellationRequested)
                    {
                        if (message != null)
                            ProcessMessage(message, sender);
                        // Configurable delay between messages
                        if (!stoppingToken.IsCancellationRequested)
                            await Task.Delay(TimeSpan.FromSeconds(mConfiguration.DelayBetweenMessagesSeconds), stoppingToken);
                    }

                    // Clear error state on success
                    mSenderState.LastError = null;
                    mSenderState.LastErrorTime = null;
                }
                catch (MailServerAuthFailureException ex)
                {
                    // Authentication failure - stop permanently to prevent account lockout
                    mLogger.LogCritical(ex, "Email: SMTP authentication failed. Email processing stopped permanently to prevent account lockout.");

                    mSenderState.LastError = ex;
                    mSenderState.LastErrorTime = DateTime.UtcNow;
                    // Set pause to a very long time (effectively permanent)
                    mSenderState.PauseAfterErrorUntil = DateTime.MaxValue;

                    mLogger.LogCritical("Email: Email processing has been disabled. Please check SMTP credentials and restart the application.");
                }
                catch (Exception ex)
                {
                    mLogger.LogError(ex, "Email: Error during queue processing");

                    // Set error state and pause
                    mSenderState.LastError = ex;
                    mSenderState.LastErrorTime = DateTime.UtcNow;
                    mSenderState.PauseAfterErrorUntil = DateTime.UtcNow.AddMinutes(mConfiguration.PauseAfterErrorMinutes);

                    mLogger.LogWarning("Email: Pausing for {Minutes} minutes after error", mConfiguration.PauseAfterErrorMinutes);
                }
                finally
                {
                    // Close SMTP connection
                    sender.Close();
                }
            }
        }
        finally
        {
            mMutexSlim.Release();
        }
    }

    /// <summary>
    /// Processes a single email message.
    /// </summary>
    /// <param name="message">The email message.</param>
    /// <param name="sender">The SMTP sender.</param>
    private void ProcessMessage(EmailMessage message, ISmtpSender sender)
    {
        try
        {
            mLogger.LogDebug("Email: Sending message {Id}", message.Id);

            message.LastAttempt = DateTime.UtcNow;

            // Send the message
            sender.Send(message, mConfiguration.MailAddressFrom);

            // Delete from storage on success
            mStorage.DeleteMessage(message.Id);

            mLogger.LogInformation("Email: Message {Id} sent successfully", message.Id);
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Email: Failed to send message {Id}", message.Id);

            message.RetryCount++;
            message.LastError = ex.Message;

            if (message.RetryCount >= mConfiguration.MaxRetryCount)
            {
                // Max retries reached, move to bad email folder
                mLogger.LogWarning("Email: Message {Id} exceeded max retries ({Count}), moving to bad email folder",
                    message.Id, mConfiguration.MaxRetryCount);

                mStorage.MoveToBadEmail(message);
            }
            else
            {
                // Re-enqueue for retry
                mLogger.LogInformation("Email: Re-enqueueing message {Id} (Retry {Retry}/{Max})",
                    message.Id, message.RetryCount, mConfiguration.MaxRetryCount);

                mQueue.Requeue(message);
            }
        }
    }
}
