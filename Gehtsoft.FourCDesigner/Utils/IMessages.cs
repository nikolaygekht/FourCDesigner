namespace Gehtsoft.FourCDesigner.Utils;

/// <summary>
/// Interface for localized message strings.
/// </summary>
public interface IMessages
{
    /// <summary>
    /// Gets the language code (e.g., "en", "es", "fr").
    /// </summary>
    string LanguageCode { get; }

    // Password validation messages
    string PasswordCannotBeNull { get; }
    string PasswordTooShort(int minimumLength);
    string PasswordMustContainCapitalLetter { get; }
    string PasswordMustContainLowercaseLetter { get; }
    string PasswordMustContainDigit { get; }
    string PasswordMustContainSpecialSymbol { get; }

    // User validation messages
    string EmailCannotBeEmpty { get; }
    string PasswordCannotBeEmpty { get; }
    string UserAlreadyExists(string email);
    string UserNotFound(string email);
    string InvalidCredentials { get; }

    // Email messages
    string ActivationEmailSubject { get; }
    string ActivationEmailBody(string token, double expirationInSeconds);
    string ActivationEmailBodyWithLink(string activationUrl, double expirationInSeconds);
    string PasswordResetEmailSubject { get; }
    string PasswordResetEmailBody(string token, double expirationInSeconds);
    string PasswordResetEmailBodyWithLink(string resetUrl, double expirationInSeconds);
}
