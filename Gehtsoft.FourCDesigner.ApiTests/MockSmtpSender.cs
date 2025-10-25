using System.Collections.Concurrent;
using Gehtsoft.FourCDesigner.Logic.Email.Model;
using Gehtsoft.FourCDesigner.Logic.Email.Sender;

namespace Gehtsoft.FourCDesigner.ApiTests;

/// <summary>
/// Mock SMTP sender that captures emails in memory for testing.
/// Thread-safe for parallel test execution.
/// </summary>
public class MockSmtpSender : ISmtpSender
{
    private readonly ConcurrentBag<CapturedEmail> mCapturedEmails = new();
    private bool mConnected;

    /// <summary>
    /// Gets a value indicating whether the SMTP connection is currently open.
    /// </summary>
    public bool Connected => mConnected;

    /// <summary>
    /// Opens a connection to the SMTP server (no-op for mock).
    /// </summary>
    public void Open()
    {
        mConnected = true;
    }

    /// <summary>
    /// Sends an email message by capturing it in memory.
    /// </summary>
    /// <param name="message">The email message to send.</param>
    /// <param name="senderAddress">The sender email address.</param>
    public void Send(EmailMessage message, string senderAddress)
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));

        mCapturedEmails.Add(new CapturedEmail
        {
            Message = message,
            SenderAddress = senderAddress,
            CapturedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Closes the connection to the SMTP server (no-op for mock).
    /// </summary>
    public void Close()
    {
        mConnected = false;
    }

    /// <summary>
    /// Gets all captured emails.
    /// </summary>
    /// <returns>Array of all captured emails.</returns>
    public CapturedEmail[] GetAllEmails()
    {
        return mCapturedEmails.ToArray();
    }

    /// <summary>
    /// Gets all emails sent to a specific recipient.
    /// </summary>
    /// <param name="recipientEmail">The recipient email address.</param>
    /// <returns>Array of emails sent to the recipient, ordered by capture time (newest first).</returns>
    public CapturedEmail[] GetEmailsFor(string recipientEmail)
    {
        if (string.IsNullOrWhiteSpace(recipientEmail))
            return Array.Empty<CapturedEmail>();

        return mCapturedEmails
            .Where(e => e.Message.To.Contains(recipientEmail, StringComparer.OrdinalIgnoreCase))
            .OrderByDescending(e => e.CapturedAt)
            .ToArray();
    }

    /// <summary>
    /// Gets the most recent email sent to a specific recipient.
    /// </summary>
    /// <param name="recipientEmail">The recipient email address.</param>
    /// <returns>The most recent email, or null if not found.</returns>
    public CapturedEmail? GetMostRecentEmailFor(string recipientEmail)
    {
        return GetEmailsFor(recipientEmail).FirstOrDefault();
    }

    /// <summary>
    /// Clears all captured emails.
    /// </summary>
    public void Clear()
    {
        mCapturedEmails.Clear();
    }

    /// <summary>
    /// Gets the count of captured emails.
    /// </summary>
    public int Count => mCapturedEmails.Count;
}

/// <summary>
/// Represents a captured email with metadata.
/// </summary>
public class CapturedEmail
{
    /// <summary>
    /// Gets or sets the email message.
    /// </summary>
    public EmailMessage Message { get; set; } = null!;

    /// <summary>
    /// Gets or sets the sender email address.
    /// </summary>
    public string SenderAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the time when the email was captured.
    /// </summary>
    public DateTime CapturedAt { get; set; }
}
