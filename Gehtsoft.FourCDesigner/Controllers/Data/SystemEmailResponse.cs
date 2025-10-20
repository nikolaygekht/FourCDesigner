using System.Text.Json.Serialization;

namespace Gehtsoft.FourCDesigner.Controllers.Data;

/// <summary>
/// Response DTO for system email configuration.
/// </summary>
public class SystemEmailResponse
{
    /// <summary>
    /// Gets or sets the system email address used for sending emails.
    /// </summary>
    [JsonPropertyName("emailFrom")]
    public string EmailFrom { get; set; } = string.Empty;
}
