using System.Collections.Concurrent;
using Gehtsoft.FourCDesigner.Logic.Email.Model;
using Gehtsoft.FourCDesigner.Logic.Email.Storage;

namespace Gehtsoft.FourCDesigner.Logic.Email.Queue;

/// <summary>
/// Thread-safe email queue implementation with priority support and persistent storage.
/// </summary>
public class EmailQueue : IEmailQueue
{
    private readonly IEmailStorage mStorage;
    private readonly ILogger<EmailQueue> mLogger;
    private readonly ConcurrentQueue<EmailMessage> mHighPriorityQueue;
    private readonly ConcurrentQueue<EmailMessage> mNormalPriorityQueue;
    private readonly object mLock = new object();

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailQueue"/> class.
    /// </summary>
    /// <param name="storage">The email storage.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public EmailQueue(IEmailStorage storage, ILogger<EmailQueue> logger)
    {
        mStorage = storage ?? throw new ArgumentNullException(nameof(storage));
        mLogger = logger ?? throw new ArgumentNullException(nameof(logger));

        mHighPriorityQueue = new ConcurrentQueue<EmailMessage>();
        mNormalPriorityQueue = new ConcurrentQueue<EmailMessage>();
    }

    /// <inheritdoc/>
    public int Count
    {
        get
        {
            lock (mLock)
            {
                return mHighPriorityQueue.Count + mNormalPriorityQueue.Count;
            }
        }
    }

    /// <inheritdoc/>
    public void Enqueue(EmailMessage message)
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));

        lock (mLock)
        {
            try
            {
                // Save to storage first
                mStorage.WriteMessage(message);

                // Add to appropriate queue
                if (message.Priority)
                    mHighPriorityQueue.Enqueue(message);
                else
                    mNormalPriorityQueue.Enqueue(message);

                mLogger.LogDebug("Email: Message {Id} enqueued (Priority: {Priority})",
                    message.Id, message.Priority);
            }
            catch (Exception ex)
            {
                mLogger.LogError(ex, "Email: Failed to enqueue message {Id}", message.Id);
                throw;
            }
        }
    }

    /// <inheritdoc/>
    public bool TryDequeue(out EmailMessage? message)
    {
        lock (mLock)
        {
            // Try high priority first
            if (mHighPriorityQueue.TryDequeue(out message))
            {
                mLogger.LogDebug("Email: Message {Id} dequeued from high-priority queue", message.Id);
                return true;
            }

            // Then try normal priority
            if (mNormalPriorityQueue.TryDequeue(out message))
            {
                mLogger.LogDebug("Email: Message {Id} dequeued from normal-priority queue", message.Id);
                return true;
            }

            message = null;
            return false;
        }
    }

    /// <inheritdoc/>
    public void Requeue(EmailMessage message)
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));

        lock (mLock)
        {
            try
            {
                // Update storage with retry info
                mStorage.WriteMessage(message);

                // Add back to queue
                if (message.Priority)
                    mHighPriorityQueue.Enqueue(message);
                else
                    mNormalPriorityQueue.Enqueue(message);

                mLogger.LogDebug("Email: Message {Id} re-enqueued (Retry: {RetryCount})",
                    message.Id, message.RetryCount);
            }
            catch (Exception ex)
            {
                mLogger.LogError(ex, "Email: Failed to re-enqueue message {Id}", message.Id);
                throw;
            }
        }
    }

    /// <inheritdoc/>
    public void LoadFromStorage()
    {
        lock (mLock)
        {
            try
            {
                Guid[] messageIds = mStorage.GetMessageIds();

                mLogger.LogInformation("Email: Loading {Count} messages from storage", messageIds.Length);

                foreach (Guid id in messageIds)
                {
                    EmailMessage? message = mStorage.ReadMessage(id);
                    if (message != null)
                    {
                        if (message.Priority)
                            mHighPriorityQueue.Enqueue(message);
                        else
                            mNormalPriorityQueue.Enqueue(message);
                    }
                }

                mLogger.LogInformation("Email: Loaded {Count} messages into queue (High: {HighCount}, Normal: {NormalCount})",
                    Count, mHighPriorityQueue.Count, mNormalPriorityQueue.Count);
            }
            catch (Exception ex)
            {
                mLogger.LogError(ex, "Email: Failed to load messages from storage");
                throw;
            }
        }
    }
}
