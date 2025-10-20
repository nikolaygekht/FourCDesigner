using System.Text.Json.Serialization;

namespace Gehtsoft.FourCDesigner.Controllers.Data;

/// <summary>
/// Response DTO for session validation.
/// </summary>
public class ValidateSessionResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the session is valid.
    /// </summary>
    [JsonPropertyName("valid")]
    public bool Valid { get; set; }
}
