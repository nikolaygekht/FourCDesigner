namespace Gehtsoft.FourCDesigner.Middleware.Throttling;

/// <summary>
/// Configuration interface for throttling settings.
/// </summary>
public interface IThrottleConfiguration
{
    /// <summary>
    /// Gets a value indicating whether throttling is enabled system-wide.
    /// When false, all throttling attributes are ignored.
    /// </summary>
    bool ThrottlingEnabled { get; }
}
