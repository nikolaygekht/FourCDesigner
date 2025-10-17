namespace Gehtsoft.FourCDesigner.Logic.Token;

/// <summary>
/// Service interface for managing email verification and password reset tokens.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Gets the token expiration time in seconds.
    /// </summary>
    double ExpirationInSeconds { get; }

    /// <summary>
    /// Generates a new token for the specified email and stores it in the cache.
    /// </summary>
    /// <param name="email">The email address to associate with the token.</param>
    /// <returns>A 6-digit token string from "100000" to "999999".</returns>
    string GenerateToken(string email);

    /// <summary>
    /// Validates a token for the specified email address.
    /// </summary>
    /// <param name="token">The token to validate.</param>
    /// <param name="email">The email address to validate the token against.</param>
    /// <param name="remove">If true, removes the token after validation to prevent reuse.</param>
    /// <returns>True if the token is valid for the email; otherwise, false.</returns>
    bool ValidateToken(string token, string email, bool remove);
}
