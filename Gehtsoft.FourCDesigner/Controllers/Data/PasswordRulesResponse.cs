using System.Text.Json.Serialization;

namespace Gehtsoft.FourCDesigner.Controllers.Data;

/// <summary>
/// Response DTO for password validation rules.
/// </summary>
public class PasswordRulesResponse
{
    /// <summary>
    /// Gets or sets the minimum password length required.
    /// </summary>
    [JsonPropertyName("minimumLength")]
    public int MinimumLength { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a capital letter is required.
    /// </summary>
    [JsonPropertyName("requireCapitalLetter")]
    public bool RequireCapitalLetter { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a small letter is required.
    /// </summary>
    [JsonPropertyName("requireSmallLetter")]
    public bool RequireSmallLetter { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a digit is required.
    /// </summary>
    [JsonPropertyName("requireDigit")]
    public bool RequireDigit { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a special symbol is required.
    /// </summary>
    [JsonPropertyName("requireSpecialSymbol")]
    public bool RequireSpecialSymbol { get; set; }
}
