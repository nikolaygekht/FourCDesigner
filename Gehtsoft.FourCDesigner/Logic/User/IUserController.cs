namespace Gehtsoft.FourCDesigner.Logic.User;

/// <summary>
/// ECB Controller interface for user operations.
/// </summary>
public interface IUserController
{
    /// <summary>
    /// Registers a new user with the specified email and password.
    /// Creates an active user with 'user' role.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="password">The user's password.</param>
    /// <returns>The ID of the created user.</returns>
    /// <exception cref="ArgumentException">Thrown when email or password is empty or password doesn't meet requirements.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a user with the email already exists.</exception>
    int RegisterUser(string email, string password);

    /// <summary>
    /// Validates a user by email and password.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="password">The user's password.</param>
    /// <returns>The user information if credentials are valid; otherwise, null.</returns>
    UserInfo? ValidateUser(string email, string password);

    /// <summary>
    /// Updates user information (email and password).
    /// </summary>
    /// <param name="currentEmail">The current email address of the user.</param>
    /// <param name="newEmail">The new email address.</param>
    /// <param name="password">The new password.</param>
    /// <returns>True if the user was updated successfully; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when email or password is empty or password doesn't meet requirements.</exception>
    /// <exception cref="InvalidOperationException">Thrown when user is not found.</exception>
    bool UpdateUser(string currentEmail, string newEmail, string password);

    /// <summary>
    /// Activates a user.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <returns>True if the user was activated successfully; otherwise, false.</returns>
    /// <exception cref="InvalidOperationException">Thrown when user is not found.</exception>
    bool ActivateUser(string email);

    /// <summary>
    /// Deactivates a user.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <returns>True if the user was deactivated successfully; otherwise, false.</returns>
    /// <exception cref="InvalidOperationException">Thrown when user is not found.</exception>
    bool DeactivateUser(string email);

    /// <summary>
    /// Changes the password for a user.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="password">The new password.</param>
    /// <returns>True if the password was changed successfully; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when password is empty or doesn't meet requirements.</exception>
    /// <exception cref="InvalidOperationException">Thrown when user is not found.</exception>
    bool ChangePassword(string email, string password);
}
