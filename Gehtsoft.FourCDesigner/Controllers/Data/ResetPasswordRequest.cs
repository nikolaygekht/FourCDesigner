using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Gehtsoft.FourCDesigner.Controllers.Data;

/// <summary>
/// Request DTO for password reset.
/// </summary>
public class ResetPasswordRequest
{
    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the new password.
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the reset token.
    /// </summary>
    [Required(ErrorMessage = "Token is required")]
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;
}
