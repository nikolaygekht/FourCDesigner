using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Gehtsoft.FourCDesigner.Controllers.Data;

/// <summary>
/// Request DTO for password reset request.
/// </summary>
public class RequestPasswordResetRequest
{
    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
}
