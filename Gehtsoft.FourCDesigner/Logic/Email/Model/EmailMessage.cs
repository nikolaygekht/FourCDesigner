using System.Text.Json.Serialization;

namespace Gehtsoft.FourCDesigner.Logic.Email.Model;

/// <summary>
/// Represents an email message in the queue.
/// </summary>
public class EmailMessage
{
    /// <summary>
    /// Gets or sets the unique identifier of the message.
    /// </summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the date and time when the message was created.
    /// </summary>
    [JsonPropertyName("created")]
    public DateTime Created { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets a value indicating whether this is a high-priority message.
    /// </summary>
    [JsonPropertyName("priority")]
    public bool Priority { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the message body is HTML.
    /// </summary>
    [JsonPropertyName("htmlContent")]
    public bool HtmlContent { get; set; }

    /// <summary>
    /// Gets or sets the email subject.
    /// </summary>
    [JsonPropertyName("subject")]
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of recipient email addresses.
    /// </summary>
    [JsonPropertyName("to")]
    public string[] To { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the email body content.
    /// </summary>
    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of attachments.
    /// </summary>
    [JsonPropertyName("attachments")]
    public List<EmailAttachment> Attachments { get; set; } = new List<EmailAttachment>();

    /// <summary>
    /// Gets or sets the number of send attempts.
    /// </summary>
    [JsonPropertyName("retryCount")]
    public int RetryCount { get; set; }

    /// <summary>
    /// Gets or sets the last error message if send failed.
    /// </summary>
    [JsonPropertyName("lastError")]
    public string LastError { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time of the last send attempt.
    /// </summary>
    [JsonPropertyName("lastAttempt")]
    public DateTime? LastAttempt { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailMessage"/> class.
    /// </summary>
    public EmailMessage()
    {
    }

    /// <summary>
    /// Creates a new email message to one recipient.
    /// </summary>
    /// <param name="to">The recipient email address.</param>
    /// <param name="subject">The email subject.</param>
    /// <param name="body">The email body.</param>
    /// <param name="html">True if body is HTML, false for plain text.</param>
    /// <param name="priority">True for high-priority message.</param>
    /// <returns>A new email message.</returns>
    public static EmailMessage Create(string to, string subject, string body, bool html = false, bool priority = false)
    {
        return new EmailMessage
        {
            To = new[] { to },
            Subject = subject,
            Body = body,
            HtmlContent = html,
            Priority = priority
        };
    }

    /// <summary>
    /// Creates a new email message to multiple recipients.
    /// </summary>
    /// <param name="to">The list of recipient email addresses.</param>
    /// <param name="subject">The email subject.</param>
    /// <param name="body">The email body.</param>
    /// <param name="html">True if body is HTML, false for plain text.</param>
    /// <param name="priority">True for high-priority message.</param>
    /// <returns>A new email message.</returns>
    public static EmailMessage Create(string[] to, string subject, string body, bool html = false, bool priority = false)
    {
        return new EmailMessage
        {
            To = to,
            Subject = subject,
            Body = body,
            HtmlContent = html,
            Priority = priority
        };
    }
}
