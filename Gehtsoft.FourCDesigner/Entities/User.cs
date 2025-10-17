using Gehtsoft.EF.Entities;

namespace Gehtsoft.FourCDesigner.Entities;

/// <summary>
/// User entity representing application users.
/// </summary>
[Entity(Table = "users")]
public class User
{
    /// <summary>
    /// Gets or sets the user ID (auto-increment primary key).
    /// </summary>
    [AutoId]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the user's email address (indexed for fast lookup).
    /// </summary>
    [EntityProperty(Field = "email", Size = 256, Sorted = true, Unique = true)]
    public string Email { get; set; }

    /// <summary>
    /// Gets or sets the password hash (SHA256 as base64, future-proof size).
    /// </summary>
    [EntityProperty(Field = "password_hash", Size = 128)]
    public string PasswordHash { get; set; }

    /// <summary>
    /// Gets or sets the user's role (e.g., "user", "admin").
    /// </summary>
    [EntityProperty(Field = "role", Size = 50)]
    public string Role { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user is active.
    /// </summary>
    [EntityProperty(Field = "active_user")]
    public bool ActiveUser { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class.
    /// </summary>
    public User()
    {
        Email = string.Empty;
        PasswordHash = string.Empty;
        Role = "user";
        ActiveUser = true;
    }
}
