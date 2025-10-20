using System.Text.Json.Serialization;

namespace Gehtsoft.FourCDesigner.Controllers.Data;

/// <summary>
/// Validation error for a specific field.
/// </summary>
public class FieldValidationError
{
    /// <summary>
    /// Gets or sets the field name that caused the error.
    /// </summary>
    [JsonPropertyName("field")]
    public string Field { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error messages.
    /// </summary>
    [JsonPropertyName("messages")]
    public List<string> Messages { get; set; } = new List<string>();
}

/// <summary>
/// Response DTO for user registration.
/// </summary>
public class RegisterUserResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether registration was successful.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the success message.
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets the list of validation errors (null if no errors).
    /// </summary>
    [JsonPropertyName("errors")]
    public List<FieldValidationError>? Errors { get; set; }
}
