using Gehtsoft.FourCDesigner.Logic.Email.Configuration;
using Gehtsoft.FourCDesigner.Logic.Email.Queue;
using Gehtsoft.FourCDesigner.Logic.Email.Sender;

namespace Gehtsoft.FourCDesigner.Logic.Email;

/// <summary>
/// Background service that processes the email queue and sends messages.
/// </summary>
public class EmailBackgroundService : BackgroundService
{
    private readonly IEmailQueue mQueue;
    private readonly IEmailConfiguration mConfiguration;
    private readonly IEmailSenderService mEmailSenderService;
    private readonly EmailSenderState mSenderState;
    private readonly ILogger<EmailBackgroundService> mLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailBackgroundService"/> class.
    /// </summary>
    /// <param name="queue">The email queue.</param>
    /// <param name="configuration">The email configuration.</param>
    /// <param name="emailSenderService">The email sender service.</param>
    /// <param name="senderState">The shared sender state.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public EmailBackgroundService(
        IEmailQueue queue,
        IEmailConfiguration configuration,
        IEmailSenderService emailSenderService,
        EmailSenderState senderState,
        ILogger<EmailBackgroundService> logger)
    {
        mQueue = queue ?? throw new ArgumentNullException(nameof(queue));
        mConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        mEmailSenderService = emailSenderService ?? throw new ArgumentNullException(nameof(emailSenderService));
        mSenderState = senderState ?? throw new ArgumentNullException(nameof(senderState));
        mLogger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes the background service.
    /// </summary>
    /// <param name="stoppingToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        mLogger.LogInformation("Email: Background service starting");

        // Load messages from storage on startup
        try
        {
            mQueue.LoadFromStorage();
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Email: Failed to load messages from storage on startup");
        }

        mSenderState.IsSenderActive = true;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Check if we need to pause after error
                if (mSenderState.PauseAfterErrorUntil.HasValue &&
                    DateTime.UtcNow < mSenderState.PauseAfterErrorUntil.Value)
                {
                    TimeSpan remaining = mSenderState.PauseAfterErrorUntil.Value - DateTime.UtcNow;
                    mLogger.LogDebug("Email: Paused after error, resuming in {Seconds} seconds", remaining.TotalSeconds);
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                    continue;
                }

                // Reset pause state
                mSenderState.PauseAfterErrorUntil = null;

                // Process queue
                await mEmailSenderService.ProcessQueueAsync(stoppingToken);

                // Wait before next check
                int delaySeconds = mConfiguration.SendingFrequencySeconds;
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
            catch (Exception ex)
            {
                mLogger.LogError(ex, "Email: Unexpected error in background service");
                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }
        }

        mSenderState.IsSenderActive = false;
        mLogger.LogInformation("Email: Background service stopped");
    }
}
