namespace Gehtsoft.FourCDesigner.Middleware;

/// <summary>
/// Configuration interface for SSI middleware.
/// </summary>
public interface ISsiMiddlewareConfig
{
    /// <summary>
    /// Gets the application version.
    /// Maps to system:version in configuration.
    /// </summary>
    string AppVersion { get; }

    /// <summary>
    /// Gets the external prefix for routing (e.g., "/api" or empty string).
    /// Maps to system:routing:externalPrefix in configuration.
    /// </summary>
    string ExternalPrefix { get; }
}
