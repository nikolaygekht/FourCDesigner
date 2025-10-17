namespace Gehtsoft.FourCDesigner.Logic.Session;

/// <summary>
/// Configuration interface for session-related settings.
/// </summary>
public interface ISessionSettings
{
    /// <summary>
    /// Gets the session timeout in seconds.
    /// Default value is 600 seconds (10 minutes).
    /// </summary>
    double SessionTimeoutInSeconds { get; }
}
