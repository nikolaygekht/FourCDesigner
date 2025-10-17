namespace Gehtsoft.FourCDesigner.Logic.User;

/// <summary>
/// Interface for password validation rules.
/// </summary>
public interface IPasswordRules
{
    /// <summary>
    /// Gets the minimum password length required.
    /// </summary>
    int MinimumLength { get; }

    /// <summary>
    /// Gets a value indicating whether a capital letter is required in passwords.
    /// </summary>
    bool RequireCapitalLetter { get; }

    /// <summary>
    /// Gets a value indicating whether a small letter is required in passwords.
    /// </summary>
    bool RequireSmallLetter { get; }

    /// <summary>
    /// Gets a value indicating whether a digit is required in passwords.
    /// </summary>
    bool RequireDigit { get; }

    /// <summary>
    /// Gets a value indicating whether a special symbol is required in passwords.
    /// </summary>
    bool RequireSpecialSymbol { get; }
}
