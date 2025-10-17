namespace Gehtsoft.FourCDesigner.Logic.User;

/// <summary>
/// Interface for password validation.
/// </summary>
public interface IPasswordValidator
{
    /// <summary>
    /// Validates a password against configured rules.
    /// </summary>
    /// <param name="password">The password to validate.</param>
    /// <returns>True if the password meets all requirements; otherwise, false.</returns>
    bool ValidatePassword(string password);

    /// <summary>
    /// Validates a password and returns detailed validation errors.
    /// </summary>
    /// <param name="password">The password to validate.</param>
    /// <param name="errors">List of validation error messages.</param>
    /// <returns>True if the password meets all requirements; otherwise, false.</returns>
    bool ValidatePassword(string password, out List<string> errors);
}
