namespace Gehtsoft.FourCDesigner.Logic.Config;

/// <summary>
/// Interface for building external URLs using system configuration.
/// </summary>
public interface IUrlBuilder
{
    /// <summary>
    /// Builds a complete external URL from a relative path.
    /// </summary>
    /// <param name="relativePath">The relative path (e.g., "/api/activate-account").</param>
    /// <param name="queryParameters">Optional query string parameters.</param>
    /// <returns>The complete external URL.</returns>
    /// <example>
    /// BuildUrl("/api/activate-account", new { email = "user@example.com", token = "abc123" })
    /// Returns: "https://example.com:443/4c/api/activate-account?email=user@example.com&amp;token=abc123"
    /// </example>
    string BuildUrl(string relativePath, object? queryParameters = null);
}
