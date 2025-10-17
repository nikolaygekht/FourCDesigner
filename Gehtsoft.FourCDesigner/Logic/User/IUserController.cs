namespace Gehtsoft.FourCDesigner.Logic.User;

/// <summary>
/// ECB Controller interface for user operations.
/// </summary>
public interface IUserController
{
    /// <summary>
    /// Registers a new user with the specified email and password.
    /// Creates an inactive user with 'user' role and sends an activation email.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="password">The user's password.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if registration was successful; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when email or password is empty or password doesn't meet requirements.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a user with the email already exists.</exception>
    Task<bool> RegisterUser(string email, string password, CancellationToken cancellationToken = default);

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
    /// Activates a user using a token.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="token">The activation token.</param>
    /// <returns>True if the user was activated successfully; otherwise, false.</returns>
    /// <exception cref="InvalidOperationException">Thrown when user is not found.</exception>
    bool ActivateUser(string email, string token);

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

    /// <summary>
    /// Requests a password reset for the specified email address.
    /// Creates a token and sends a password reset email if the user exists and is active.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RequestPasswordReset(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a token for the specified email address.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="token">The token to validate.</param>
    /// <returns>True if the token is valid for the email; otherwise, false.</returns>
    bool ValidateToken(string email, string token);

    /// <summary>
    /// Resets the password using a token.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="token">The password reset token.</param>
    /// <param name="newPassword">The new password.</param>
    /// <returns>True if the password was reset successfully; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when password is empty or doesn't meet requirements.</exception>
    bool ResetPassword(string email, string token, string newPassword);
}
