namespace Gehtsoft.FourCDesigner.Logic.Config;

/// <summary>
/// Implementation of system configuration that reads from IConfiguration.
/// </summary>
public class SystemConfig : ISystemConfig
{
    private readonly IConfiguration mConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemConfig"/> class.
    /// </summary>
    /// <param name="configuration">The configuration provider.</param>
    /// <exception cref="ArgumentNullException">Thrown when configuration is null.</exception>
    public SystemConfig(IConfiguration configuration)
    {
        mConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <inheritdoc/>
    public string ExternalProtocol
    {
        get
        {
            string? value = mConfiguration["system:routing:externalProtocol"];
            if (string.IsNullOrEmpty(value))
                return "https"; // Default to HTTPS for security

            return value;
        }
    }

    /// <inheritdoc/>
    public string ExternalHost
    {
        get
        {
            string? value = mConfiguration["system:routing:externalHost"];
            if (string.IsNullOrEmpty(value))
                return "localhost"; // Default

            return value;
        }
    }

    /// <inheritdoc/>
    public int ExternalPort
    {
        get
        {
            string? value = mConfiguration["system:routing:externalPort"];
            if (string.IsNullOrEmpty(value))
                return 443; // Default HTTPS port

            if (!int.TryParse(value, out int result) || result < 1 || result > 65535)
                return 443; // Default HTTPS port

            return result;
        }
    }

    /// <inheritdoc/>
    public string ExternalPrefix
    {
        get
        {
            string? value = mConfiguration["system:routing:externalPrefix"];
            if (string.IsNullOrEmpty(value))
                return string.Empty; // Default to no prefix

            // Ensure prefix starts with / if not empty
            if (!value.StartsWith("/"))
                return "/" + value;

            return value;
        }
    }

    /// <inheritdoc/>
    public string UseUrls
    {
        get
        {
            string? value = mConfiguration["system:routing:useUrls"];
            if (string.IsNullOrEmpty(value))
                return "http://localhost:5000"; // Default

            return value;
        }
    }
}
