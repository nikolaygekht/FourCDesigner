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

    /// <inheritdoc/>
    public string ActivationEmailSubject => "Activate Your Account";

    /// <inheritdoc/>
    public string ActivationEmailBody(string token, double expirationInSeconds)
    {
        int minutes = (int)(expirationInSeconds / 60);
        return $"Your activation code is: {token}\n\nThis code will expire in {minutes} minutes.";
    }

    /// <inheritdoc/>
    public string PasswordResetEmailSubject => "Password Reset Request";

    /// <inheritdoc/>
    public string PasswordResetEmailBody(string token, double expirationInSeconds)
    {
        int minutes = (int)(expirationInSeconds / 60);
        return $"Your password reset code is: {token}\n\nThis code will expire in {minutes} minutes.\n\nIf you did not request a password reset, please ignore this email.";
    }

    /// <inheritdoc/>
    public string ActivationEmailBodyWithLink(string activationUrl, double expirationInSeconds)
    {
        int minutes = (int)(expirationInSeconds / 60);
        return $"Thank you for registering!\n\nPlease click the link below to activate your account:\n\n{activationUrl}\n\nThis link will expire in {minutes} minutes.\n\nIf you did not create an account, please ignore this email.";
    }

    /// <inheritdoc/>
    public string PasswordResetEmailBodyWithLink(string resetUrl, double expirationInSeconds)
    {
        int minutes = (int)(expirationInSeconds / 60);
        return $"A password reset was requested for your account.\n\nPlease click the link below to reset your password:\n\n{resetUrl}\n\nThis link will expire in {minutes} minutes.\n\nIf you did not request a password reset, please ignore this email.";
    }
}
