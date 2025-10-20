using System.Text.Json.Serialization;

namespace Gehtsoft.FourCDesigner.Controllers.Data;

/// <summary>
/// Response DTO for email availability check.
/// </summary>
public class CheckEmailResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the email is available for registration.
    /// </summary>
    [JsonPropertyName("available")]
    public bool Available { get; set; }
}
