using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Gehtsoft.FourCDesigner.Logic.Email.Configuration;
using Gehtsoft.FourCDesigner.Logic.Email.Model;

namespace Gehtsoft.FourCDesigner.Logic.Email.Sender;

/// <summary>
/// SMTP email sender implementation using MailKit.
/// </summary>
public class SmtpSender : ISmtpSender, IDisposable
{
    private readonly IEmailConfiguration mConfiguration;
    private readonly ILogger<SmtpSender> mLogger;
    private SmtpClient mClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmtpSender"/> class.
    /// </summary>
    /// <param name="configuration">The email configuration.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public SmtpSender(IEmailConfiguration configuration, ILogger<SmtpSender> logger)
    {
        mConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        mLogger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public bool Connected => mClient != null && mClient.IsConnected;

    /// <inheritdoc/>
    public void Open()
    {
        if (Connected)
            return;

        try
        {
            mClient = new SmtpClient();

            // Handle SSL certificate validation if needed
            if (mConfiguration.SslAcceptAllCertificates)
            {
                mLogger.LogWarning("Email: SSL certificate validation is disabled");
                mClient.ServerCertificateValidationCallback = (s, c, h, e) => true;
            }

            // Parse SSL mode
            SecureSocketOptions sslOptions = ParseSslMode(mConfiguration.SslMode);

            mLogger.LogDebug("Email: Connecting to SMTP server {Server}:{Port} (SSL: {SslMode}) {User} {Password}",
                mConfiguration.SmtpServer, mConfiguration.SmtpPort, mConfiguration.SslMode, mConfiguration.SmtpUser, mConfiguration.SmtpPassword);

            mClient.Connect(mConfiguration.SmtpServer, mConfiguration.SmtpPort, sslOptions);

            // Authenticate if credentials are provided
            if (!string.IsNullOrEmpty(mConfiguration.SmtpUser))
            {
                mLogger.LogDebug("Email: Authenticating as {User}", mConfiguration.SmtpUser);
                mClient.Authenticate(mConfiguration.SmtpUser, mConfiguration.SmtpPassword);
            }

            mLogger.LogInformation("Email: Connected to SMTP server");
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Email: Failed to connect to SMTP server");
            mClient?.Dispose();
            mClient = null;
            throw;
        }
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

        try
        {
            MimeMessage mimeMessage = CreateMimeMessage(message, senderAddress);

            mLogger.LogDebug("Email: Sending message {Id} to {Recipients}",
                message.Id, string.Join(", ", message.To));

            mClient.Send(mimeMessage);

            mLogger.LogInformation("Email: Message {Id} sent successfully", message.Id);
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Email: Failed to send message {Id}", message.Id);
            throw;
        }
    }

    /// <inheritdoc/>
    public void Close()
    {
        if (mClient != null && mClient.IsConnected)
        {
            try
            {
                mClient.Disconnect(true);
                mLogger.LogDebug("Email: Disconnected from SMTP server");
            }
            catch (Exception ex)
            {
                mLogger.LogError(ex, "Email: Error disconnecting from SMTP server");
            }
        }

        mClient?.Dispose();
        mClient = null;
    }

    /// <summary>
    /// Disposes the SMTP client.
    /// </summary>
    public void Dispose()
    {
        Close();
    }

    /// <summary>
    /// Creates a MIME message from an email message.
    /// </summary>
    /// <param name="message">The email message.</param>
    /// <param name="senderAddress">The sender address.</param>
    /// <returns>The MIME message.</returns>
    private MimeMessage CreateMimeMessage(EmailMessage message, string senderAddress)
    {
        MimeMessage mimeMessage = new MimeMessage();

        // From
        mimeMessage.From.Add(MailboxAddress.Parse(senderAddress));

        // To
        foreach (string recipient in message.To)
        {
            if (!string.IsNullOrWhiteSpace(recipient))
                mimeMessage.To.Add(MailboxAddress.Parse(recipient));
        }

        // Subject
        mimeMessage.Subject = message.Subject;

        // Priority
        if (message.Priority)
            mimeMessage.Priority = MessagePriority.Urgent;

        // Body
        BodyBuilder bodyBuilder = new BodyBuilder();
        if (message.HtmlContent)
            bodyBuilder.HtmlBody = message.Body;
        else
            bodyBuilder.TextBody = message.Body;

        // Attachments
        if (message.Attachments != null && message.Attachments.Count > 0)
        {
            foreach (EmailAttachment attachment in message.Attachments)
            {
                if (attachment != null && !string.IsNullOrEmpty(attachment.FileName))
                {
                    byte[] content = attachment.GetContent();
                    bodyBuilder.Attachments.Add(attachment.FileName, content, ContentType.Parse(attachment.ContentType));
                }
            }
        }

        mimeMessage.Body = bodyBuilder.ToMessageBody();

        return mimeMessage;
    }

    /// <summary>
    /// Parses the SSL mode string into SecureSocketOptions.
    /// </summary>
    /// <param name="sslMode">The SSL mode string.</param>
    /// <returns>The secure socket options.</returns>
    private SecureSocketOptions ParseSslMode(string sslMode)
    {
        if (string.IsNullOrEmpty(sslMode))
            return SecureSocketOptions.StartTlsWhenAvailable;

        return sslMode.ToLowerInvariant() switch
        {
            "none" => SecureSocketOptions.None,
            "auto" => SecureSocketOptions.Auto,
            "starttls" => SecureSocketOptions.StartTls,
            "onconnect" => SecureSocketOptions.SslOnConnect,
            "whenavailable" => SecureSocketOptions.StartTlsWhenAvailable,
            _ => throw new InvalidOperationException($"Invalid SSL mode: {sslMode}. Valid values are: none, auto, starttls, onconnect, whenavailable")
        };
    }
}
