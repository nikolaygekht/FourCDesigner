namespace Gehtsoft.FourCDesigner.Logic.Email.Configuration;

/// <summary>
/// Implementation of email configuration that reads from IConfiguration.
/// </summary>
public class EmailConfiguration : IEmailConfiguration
{
    private readonly IConfiguration mConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailConfiguration"/> class.
    /// </summary>
    /// <param name="configuration">The configuration provider.</param>
    /// <exception cref="ArgumentNullException">Thrown when configuration is null.</exception>
    public EmailConfiguration(IConfiguration configuration)
    {
        mConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <inheritdoc/>
    public string SmtpServer =>
        mConfiguration["email:smtpServer"] ??
        throw new InvalidOperationException("Email SMTP server not configured. Set 'email:smtpServer' in appsettings.json");

    /// <inheritdoc/>
    public int SmtpPort
    {
        get
        {
            string value = mConfiguration["email:smtpPort"];
            if (string.IsNullOrEmpty(value))
                return 587; // Default SMTP port

            if (!int.TryParse(value, out int result))
                throw new InvalidOperationException($"Invalid value for 'email:smtpPort': {value}. Expected an integer.");

            return result;
        }
    }

    /// <inheritdoc/>
    public string SslMode => mConfiguration["email:sslMode"] ?? "StartTls";

    /// <inheritdoc/>
    public string SmtpUser => mConfiguration["email:smtpUser"] ?? string.Empty;

    /// <inheritdoc/>
    public string SmtpPassword => mConfiguration["email:smtpPassword"] ?? string.Empty;

    /// <inheritdoc/>
    public string MailAddressFrom =>
        mConfiguration["email:mailAddressFrom"] ??
        throw new InvalidOperationException("Email 'from' address not configured. Set 'email:mailAddressFrom' in appsettings.json");

    /// <inheritdoc/>
    public int SendingFrequencySeconds
    {
        get
        {
            string value = mConfiguration["email:sendingFrequencySeconds"];
            if (string.IsNullOrEmpty(value))
                return 60; // Default: check every 60 seconds

            if (!int.TryParse(value, out int result))
                throw new InvalidOperationException($"Invalid value for 'email:sendingFrequencySeconds': {value}. Expected an integer.");

            return result;
        }
    }

    /// <inheritdoc/>
    public int PauseAfterErrorMinutes
    {
        get
        {
            string value = mConfiguration["email:pauseAfterErrorMinutes"];
            if (string.IsNullOrEmpty(value))
                return 30; // Default: pause for 30 minutes after error

            if (!int.TryParse(value, out int result))
                throw new InvalidOperationException($"Invalid value for 'email:pauseAfterErrorMinutes': {value}. Expected an integer.");

            return result;
        }
    }

    /// <inheritdoc/>
    public int MaxRetryCount
    {
        get
        {
            string value = mConfiguration["email:maxRetryCount"];
            if (string.IsNullOrEmpty(value))
                return 3; // Default: retry 3 times

            if (!int.TryParse(value, out int result))
                throw new InvalidOperationException($"Invalid value for 'email:maxRetryCount': {value}. Expected an integer.");

            return result;
        }
    }

    /// <inheritdoc/>
    public string QueueFolder => mConfiguration["email:queueFolder"] ?? "./data/EmailQueue";

    /// <inheritdoc/>
    public string BadEmailFolder => mConfiguration["email:badEmailFolder"] ?? "./data/BadEmail";

    /// <inheritdoc/>
    public bool SslAcceptAllCertificates
    {
        get
        {
            string value = mConfiguration["email:sslAcceptAllCertificates"];
            if (string.IsNullOrEmpty(value))
                return false;

            if (!bool.TryParse(value, out bool result))
                throw new InvalidOperationException($"Invalid value for 'email:sslAcceptAllCertificates': {value}. Expected 'true' or 'false'.");

            return result;
        }
    }

    /// <inheritdoc/>
    public double DelayBetweenMessagesSeconds
    {
        get
        {
            string value = mConfiguration["email:delayBetweenMessagesSeconds"];
            if (string.IsNullOrEmpty(value))
                return 1.0; // Default: 1 second delay between messages

            if (!double.TryParse(value, out double result))
                throw new InvalidOperationException($"Invalid value for 'email:delayBetweenMessagesSeconds': {value}. Expected a number.");

            return result;
        }
    }

    /// <inheritdoc/>
    public bool DisableSending
    {
        get
        {
            string value = mConfiguration["email:disableSending"];
            if (string.IsNullOrEmpty(value))
                return false; // Default: sending enabled

            if (!bool.TryParse(value, out bool result))
                throw new InvalidOperationException($"Invalid value for 'email:disableSending': {value}. Expected 'true' or 'false'.");

            return result;
        }
    }
}
