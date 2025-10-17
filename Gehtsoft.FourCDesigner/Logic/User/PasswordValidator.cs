using Gehtsoft.FourCDesigner.Utils;

namespace Gehtsoft.FourCDesigner.Logic.User;

/// <summary>
/// Implementation of password validator that validates passwords against configured rules.
/// </summary>
public class PasswordValidator : IPasswordValidator
{
    private readonly IPasswordRules mRules;
    private readonly IMessages mMessages;

    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordValidator"/> class.
    /// </summary>
    /// <param name="userConfiguration">The user configuration containing password rules.</param>
    /// <param name="messages">The localized messages provider.</param>
    /// <exception cref="ArgumentNullException">Thrown when userConfiguration or messages is null.</exception>
    public PasswordValidator(IUserConfiguration userConfiguration, IMessages messages)
    {
        if (userConfiguration == null)
            throw new ArgumentNullException(nameof(userConfiguration));

        mMessages = messages ?? throw new ArgumentNullException(nameof(messages));
        mRules = userConfiguration.PasswordRules;
    }

    /// <inheritdoc/>
    public bool ValidatePassword(string password)
    {
        return ValidatePassword(password, out _);
    }

    /// <inheritdoc/>
    public bool ValidatePassword(string password, out List<string> errors)
    {
        errors = new List<string>();

        if (password == null)
        {
            errors.Add(mMessages.PasswordCannotBeNull);
            return false;
        }

        // Check minimum length
        if (password.Length < mRules.MinimumLength)
            errors.Add(mMessages.PasswordTooShort(mRules.MinimumLength));

        // Check for capital letter
        if (mRules.RequireCapitalLetter && !password.Any(char.IsUpper))
            errors.Add(mMessages.PasswordMustContainCapitalLetter);

        // Check for small letter
        if (mRules.RequireSmallLetter && !password.Any(char.IsLower))
            errors.Add(mMessages.PasswordMustContainLowercaseLetter);

        // Check for digit
        if (mRules.RequireDigit && !password.Any(char.IsDigit))
            errors.Add(mMessages.PasswordMustContainDigit);

        // Check for special symbol
        if (mRules.RequireSpecialSymbol && !password.Any(c => !char.IsLetterOrDigit(c)))
            errors.Add(mMessages.PasswordMustContainSpecialSymbol);

        return errors.Count == 0;
    }
}
