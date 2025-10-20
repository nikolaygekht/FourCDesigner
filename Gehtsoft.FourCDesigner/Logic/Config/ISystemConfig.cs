namespace Gehtsoft.FourCDesigner.Logic.Config;

/// <summary>
/// Configuration interface for system-wide settings.
/// </summary>
public interface ISystemConfig
{
    /// <summary>
    /// Gets the external protocol for routing (e.g., "http" or "https").
    /// Maps to system:routing:externalProtocol in configuration.
    /// </summary>
    string ExternalProtocol { get; }

    /// <summary>
    /// Gets the external host for routing (e.g., "example.com").
    /// Maps to system:routing:externalHost in configuration.
    /// </summary>
    string ExternalHost { get; }

    /// <summary>
    /// Gets the external port for routing (e.g., 80, 443, 8080).
    /// Maps to system:routing:externalPort in configuration.
    /// </summary>
    int ExternalPort { get; }

    /// <summary>
    /// Gets the external prefix for routing (e.g., "/api" or empty string).
    /// Maps to system:routing:externalPrefix in configuration.
    /// </summary>
    string ExternalPrefix { get; }

    /// <summary>
    /// Gets the URLs to listen on for the application (e.g., "http://localhost:5000").
    /// Maps to system:routing:useUrls in configuration.
    /// </summary>
    string UseUrls { get; }
}
