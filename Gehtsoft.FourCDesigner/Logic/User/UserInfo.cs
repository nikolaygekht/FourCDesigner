using System.Text.Json.Serialization;

namespace Gehtsoft.FourCDesigner.Logic.User;

/// <summary>
/// User information data transfer object.
/// </summary>
public class UserInfo
{
    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    [JsonPropertyName("email")]
    public string Email { get; set; }

    /// <summary>
    /// Gets or sets the user's role.
    /// </summary>
    [JsonPropertyName("role")]
    public string Role { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user is active.
    /// </summary>
    [JsonPropertyName("activeUser")]
    public bool ActiveUser { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserInfo"/> class.
    /// </summary>
    public UserInfo()
    {
        Email = string.Empty;
        Role = string.Empty;
    }
}
