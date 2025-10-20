using System.Text.Json.Serialization;

namespace Gehtsoft.FourCDesigner.Controllers.Data;

/// <summary>
/// Response DTO for password reset.
/// </summary>
public class ResetPasswordResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether password reset was successful.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the response message.
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets the list of validation errors (null if no errors).
    /// </summary>
    [JsonPropertyName("errors")]
    public List<FieldValidationError>? Errors { get; set; }
}
