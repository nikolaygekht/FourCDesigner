namespace Gehtsoft.FourCDesigner.Utils;

/// <summary>
/// English language messages implementation.
/// </summary>
public class MessagesEn : IMessages
{
    /// <inheritdoc/>
    public string LanguageCode => "en";

    /// <inheritdoc/>
    public string PasswordCannotBeNull => "Password cannot be null";

    /// <inheritdoc/>
    public string PasswordTooShort(int minimumLength) =>
        $"Password must be at least {minimumLength} characters long";

    /// <inheritdoc/>
    public string PasswordMustContainCapitalLetter =>
        "Password must contain at least one capital letter";

    /// <inheritdoc/>
    public string PasswordMustContainLowercaseLetter =>
        "Password must contain at least one lowercase letter";

    /// <inheritdoc/>
    public string PasswordMustContainDigit =>
        "Password must contain at least one digit";

    /// <inheritdoc/>
    public string PasswordMustContainSpecialSymbol =>
        "Password must contain at least one special symbol";

    /// <inheritdoc/>
    public string EmailCannotBeEmpty => "Email cannot be empty";

    /// <inheritdoc/>
    public string PasswordCannotBeEmpty => "Password cannot be empty";

    /// <inheritdoc/>
    public string UserAlreadyExists(string email) =>
        $"User with email '{email}' already exists";

    /// <inheritdoc/>
    public string UserNotFound(string email) =>
        $"User with email '{email}' not found";

    /// <inheritdoc/>
    public string InvalidCredentials => "Invalid email or password";
}
