namespace Gehtsoft.FourCDesigner.Logic.Token;

/// <summary>
/// Configuration interface for token service.
/// </summary>
public interface ITokenServiceConfiguration
{
    /// <summary>
    /// Gets the expiration time for tokens in seconds.
    /// </summary>
    double ExpirationInSeconds { get; }
}
