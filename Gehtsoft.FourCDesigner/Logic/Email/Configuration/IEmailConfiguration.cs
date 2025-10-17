namespace Gehtsoft.FourCDesigner.Logic.Email.Configuration;

/// <summary>
/// Configuration interface for email service settings.
/// </summary>
public interface IEmailConfiguration
{
    /// <summary>
    /// Gets the SMTP server address.
    /// </summary>
    string SmtpServer { get; }

    /// <summary>
    /// Gets the SMTP server port.
    /// </summary>
    int SmtpPort { get; }

    /// <summary>
    /// Gets the SSL mode (None, Auto, StartTls, OnConnect).
    /// </summary>
    string SslMode { get; }

    /// <summary>
    /// Gets the SMTP username for authentication.
    /// </summary>
    string SmtpUser { get; }

    /// <summary>
    /// Gets the SMTP password for authentication.
    /// </summary>
    string SmtpPassword { get; }

    /// <summary>
    /// Gets the email address to use in the "From" field.
    /// </summary>
    string MailAddressFrom { get; }

    /// <summary>
    /// Gets the sending frequency in seconds (how often the background service checks the queue).
    /// </summary>
    int SendingFrequencySeconds { get; }

    /// <summary>
    /// Gets the pause duration in minutes after an error occurs.
    /// </summary>
    int PauseAfterErrorMinutes { get; }

    /// <summary>
    /// Gets the maximum number of retry attempts for failed messages.
    /// </summary>
    int MaxRetryCount { get; }

    /// <summary>
    /// Gets the folder path for storing queued email messages.
    /// </summary>
    string QueueFolder { get; }

    /// <summary>
    /// Gets the folder path for storing failed email messages.
    /// </summary>
    string BadEmailFolder { get; }

    /// <summary>
    /// Gets a value indicating whether to accept all SSL certificates (for development/testing only).
    /// </summary>
    bool SslAcceptAllCertificates { get; }

    /// <summary>
    /// Gets the delay between sending messages in seconds.
    /// </summary>
    double DelayBetweenMessagesSeconds { get; }
}
