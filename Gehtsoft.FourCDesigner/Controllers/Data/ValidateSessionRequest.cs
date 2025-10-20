using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Gehtsoft.FourCDesigner.Controllers.Data;

/// <summary>
/// Request DTO for session validation.
/// </summary>
public class ValidateSessionRequest
{
    /// <summary>
    /// Gets or sets the session ID.
    /// </summary>
    [Required(ErrorMessage = "SessionId is required")]
    [JsonPropertyName("sessionId")]
    public string SessionId { get; set; } = string.Empty;
}
