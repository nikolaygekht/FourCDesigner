using Gehtsoft.FourCDesigner.Entities;

namespace Gehtsoft.FourCDesigner.Dao;

/// <summary>
/// Data access interface for User operations.
/// </summary>
public interface IUserDao
{
    /// <summary>
    /// Gets a user by email address.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <returns>The user entity, or null if not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when email is null.</exception>
    User? GetUserByEmail(string email);

    /// <summary>
    /// Gets a user by ID.
    /// </summary>
    /// <param name="id">The user's ID.</param>
    /// <returns>The user entity, or null if not found.</returns>
    /// <exception cref="ArgumentException">Thrown when id is less than or equal to 0.</exception>
    User? GetUserById(int id);

    /// <summary>
    /// Saves a user (insert if new, update if existing).
    /// </summary>
    /// <param name="user">The user entity to save.</param>
    /// <exception cref="ArgumentNullException">Thrown when user is null.</exception>
    void SaveUser(User user);

    /// <summary>
    /// Deletes a user.
    /// </summary>
    /// <param name="user">The user entity to delete.</param>
    /// <exception cref="ArgumentNullException">Thrown when user is null.</exception>
    /// <exception cref="ArgumentException">Thrown when user.Id is less than or equal to 0.</exception>
    void DeleteUser(User user);

    /// <summary>
    /// Deletes a user by email address.
    /// </summary>
    /// <param name="email">The email address of the user to delete.</param>
    /// <returns>True if a user was found and deleted, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when email is null.</exception>
    bool DeleteUserByEmail(string email);

    /// <summary>
    /// Updates the password for a user identified by email.
    /// </summary>
    /// <param name="email">The email address of the user.</param>
    /// <param name="passwordHash">The new password hash.</param>
    /// <returns>True if the user was found and updated, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when email or passwordHash is null.</exception>
    bool UpdatePasswordByEmail(string email, string passwordHash);

    /// <summary>
    /// Activates a user identified by email.
    /// </summary>
    /// <param name="email">The email address of the user.</param>
    /// <returns>True if the user was found and activated, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when email is null.</exception>
    bool ActivateUserByEmail(string email);

    /// <summary>
    /// Deactivates a user identified by email.
    /// </summary>
    /// <param name="email">The email address of the user.</param>
    /// <returns>True if the user was found and deactivated, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when email is null.</exception>
    bool DeactivateUserByEmail(string email);

    /// <summary>
    /// Checks if an email is already used by another user.
    /// </summary>
    /// <param name="email">The email address to check.</param>
    /// <param name="excludeUserId">Optional user ID to exclude from the check (for updates).</param>
    /// <returns>True if the email is used by another user, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when email is null.</exception>
    bool IsEmailUsed(string email, int? excludeUserId = null);
}
