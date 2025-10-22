using System.Text;
using System.Web;

namespace Gehtsoft.FourCDesigner.Logic.Config;

/// <summary>
/// Implementation of URL builder that constructs external URLs using system configuration.
/// </summary>
public class UrlBuilder : IUrlBuilder
{
    private readonly ISystemConfig mSystemConfig;
    private readonly ILogger<UrlBuilder> mLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlBuilder"/> class.
    /// </summary>
    /// <param name="systemConfig">The system configuration.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public UrlBuilder(ISystemConfig systemConfig, ILogger<UrlBuilder> logger)
    {
        mSystemConfig = systemConfig ?? throw new ArgumentNullException(nameof(systemConfig));
        mLogger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public string BuildUrl(string relativePath, object? queryParameters = null)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            throw new ArgumentException("Relative path cannot be null or empty", nameof(relativePath));

        try
        {
            // Ensure relative path starts with /
            if (!relativePath.StartsWith("/"))
                relativePath = "/" + relativePath;

            var urlBuilder = new StringBuilder();

            // Protocol
            urlBuilder.Append(mSystemConfig.ExternalProtocol);
            urlBuilder.Append("://");

            // Host
            urlBuilder.Append(mSystemConfig.ExternalHost);

            // Port (omit if it's the default port for the protocol)
            bool isDefaultPort = (mSystemConfig.ExternalProtocol == "http" && mSystemConfig.ExternalPort == 80) ||
                                 (mSystemConfig.ExternalProtocol == "https" && mSystemConfig.ExternalPort == 443);

            if (!isDefaultPort)
            {
                urlBuilder.Append(":");
                urlBuilder.Append(mSystemConfig.ExternalPort);
            }

            // Prefix
            if (!string.IsNullOrEmpty(mSystemConfig.ExternalPrefix))
            {
                urlBuilder.Append(mSystemConfig.ExternalPrefix);
            }

            // Relative path
            urlBuilder.Append(relativePath);

            // Query parameters
            if (queryParameters != null)
            {
                var queryString = BuildQueryString(queryParameters);
                if (!string.IsNullOrEmpty(queryString))
                {
                    urlBuilder.Append("?");
                    urlBuilder.Append(queryString);
                }
            }

            string url = urlBuilder.ToString();
            mLogger.LogDebug("Built external URL: {Url}", url);

            return url;
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Failed to build URL for relative path: {RelativePath}", relativePath);
            throw;
        }
    }

    /// <summary>
    /// Builds a query string from an object's properties.
    /// </summary>
    /// <param name="parameters">The object containing query parameters.</param>
    /// <returns>The query string (without leading '?').</returns>
    private string BuildQueryString(object parameters)
    {
        var properties = parameters.GetType().GetProperties();
        var queryParts = new List<string>();

        foreach (var property in properties)
        {
            var value = property.GetValue(parameters);
            if (value != null)
            {
                string encodedName = HttpUtility.UrlEncode(property.Name);
                string? encodedValue = HttpUtility.UrlEncode(value.ToString());
                queryParts.Add($"{encodedName}={encodedValue}");
            }
        }

        return string.Join("&", queryParts);
    }
}
