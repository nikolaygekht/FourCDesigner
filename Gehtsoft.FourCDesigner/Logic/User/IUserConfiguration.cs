namespace Gehtsoft.FourCDesigner.Logic.User;

/// <summary>
/// Configuration interface for user-related settings.
/// </summary>
public interface IUserConfiguration
{
    /// <summary>
    /// Gets the password validation rules.
    /// </summary>
    IPasswordRules PasswordRules { get; }
}
