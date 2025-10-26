using System.Collections.Concurrent;
using Gehtsoft.FourCDesigner.Logic.Email.Model;
using Gehtsoft.FourCDesigner.Logic.Email.Sender;

namespace Gehtsoft.FourCDesigner.Tests.Logic.Email;

/// <summary>
/// Mock SMTP sender for unit testing.
/// Captures sent emails in memory without actual network I/O.
/// </summary>
public class MockSmtpSender : ISmtpSender
{
    private readonly List<SentEmail> mSentEmails = new List<SentEmail>();
    private readonly object mLock = new object();
    private bool mIsConnected;
    private bool mShouldFailOnConnect;
    private bool mShouldFailOnSend;

    /// <summary>
    /// Gets a value indicating whether the mock connection is open.
    /// </summary>
    public bool Connected => mIsConnected;

    /// <summary>
    /// Gets all emails that were sent through this mock (in order sent).
    /// </summary>
    public IReadOnlyCollection<SentEmail> SentEmails
    {
        get
        {
            lock (mLock)
            {
                return mSentEmails.ToList();
            }
        }
    }

    /// <summary>
    /// Configures the mock to fail on the next Open() call.
    /// </summary>
    public void SetFailOnConnect(bool shouldFail = true)
    {
        mShouldFailOnConnect = shouldFail;
    }

    /// <summary>
    /// Configures the mock to fail on the next Send() call.
    /// </summary>
    public void SetFailOnSend(bool shouldFail = true)
    {
        mShouldFailOnSend = shouldFail;
    }

    /// <summary>
    /// Clears all recorded sent emails.
    /// </summary>
    public void Clear()
    {
        lock (mLock)
        {
            mSentEmails.Clear();
        }
    }

    /// <inheritdoc/>
    public void Open()
    {
        if (mShouldFailOnConnect)
            throw new InvalidOperationException("Mock configured to fail on connect");

        mIsConnected = true;
    }

    /// <inheritdoc/>
    public void Send(EmailMessage message, string senderAddress)
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));

        if (string.IsNullOrEmpty(senderAddress))
            throw new ArgumentException("Sender address cannot be empty", nameof(senderAddress));

        if (!Connected)
            throw new InvalidOperationException("SMTP connection is not open. Call Open() first.");

        if (mShouldFailOnSend)
            throw new InvalidOperationException("Mock configured to fail on send");

        lock (mLock)
        {
            mSentEmails.Add(new SentEmail(message, senderAddress));
        }
    }

    /// <inheritdoc/>
    public void Close()
    {
        mIsConnected = false;
    }
}

/// <summary>
/// Represents an email that was sent through the mock SMTP sender.
/// </summary>
public class SentEmail
{
    /// <summary>
    /// Gets the email message.
    /// </summary>
    public EmailMessage Message { get; }

    /// <summary>
    /// Gets the sender address used.
    /// </summary>
    public string SenderAddress { get; }

    /// <summary>
    /// Gets the timestamp when the email was sent.
    /// </summary>
    public DateTime SentAt { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SentEmail"/> class.
    /// </summary>
    /// <param name="message">The email message.</param>
    /// <param name="senderAddress">The sender address.</param>
    public SentEmail(EmailMessage message, string senderAddress)
    {
        Message = message;
        SenderAddress = senderAddress;
        SentAt = DateTime.UtcNow;
    }
}
