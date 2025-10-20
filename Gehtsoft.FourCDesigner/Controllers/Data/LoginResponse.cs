using System.Text.Json.Serialization;

namespace Gehtsoft.FourCDesigner.Controllers.Data;

/// <summary>
/// Response DTO for user login.
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// Gets or sets the session ID.
    /// </summary>
    [JsonPropertyName("sessionId")]
    public string SessionId { get; set; } = string.Empty;
}
