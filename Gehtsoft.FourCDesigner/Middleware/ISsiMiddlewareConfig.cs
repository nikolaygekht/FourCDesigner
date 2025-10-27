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
}
