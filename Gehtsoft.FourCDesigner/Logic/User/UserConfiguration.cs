namespace Gehtsoft.FourCDesigner.Logic.User;

/// <summary>
/// Implementation of password rules that reads from IConfiguration.
/// </summary>
internal class PasswordRules : IPasswordRules
{
    private readonly IConfiguration mConfiguration;

    public PasswordRules(IConfiguration configuration)
    {
        mConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <inheritdoc/>
    public int MinimumLength
    {
        get
        {
            string? value = mConfiguration["system:passwordRules:minimumLength"];
            if (string.IsNullOrEmpty(value))
                return 8; // Default

            if (!int.TryParse(value, out int result) || result < 1)
                return 8; // Default

            return result;
        }
    }

    /// <inheritdoc/>
    public bool RequireCapitalLetter
    {
        get
        {
            string? value = mConfiguration["system:passwordRules:requireCapitalLetter"];
            if (string.IsNullOrEmpty(value))
                return true; // Default

            return bool.TryParse(value, out bool result) && result;
        }
    }

    /// <inheritdoc/>
    public bool RequireSmallLetter
    {
        get
        {
            string? value = mConfiguration["system:passwordRules:requireSmallLetter"];
            if (string.IsNullOrEmpty(value))
                return true; // Default

            return bool.TryParse(value, out bool result) && result;
        }
    }

    /// <inheritdoc/>
    public bool RequireDigit
    {
        get
        {
            string? value = mConfiguration["system:passwordRules:requireDigit"];
            if (string.IsNullOrEmpty(value))
                return true; // Default

            return bool.TryParse(value, out bool result) && result;
        }
    }

    /// <inheritdoc/>
    public bool RequireSpecialSymbol
    {
        get
        {
            string? value = mConfiguration["system:passwordRules:requireSpecialSymbol"];
            if (string.IsNullOrEmpty(value))
                return false; // Default

            return bool.TryParse(value, out bool result) && result;
        }
    }
}

/// <summary>
/// Implementation of user configuration that reads from IConfiguration.
/// </summary>
public class UserConfiguration : IUserConfiguration
{
    private readonly IPasswordRules mPasswordRules;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserConfiguration"/> class.
    /// </summary>
    /// <param name="configuration">The configuration provider.</param>
    /// <exception cref="ArgumentNullException">Thrown when configuration is null.</exception>
    public UserConfiguration(IConfiguration configuration)
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        mPasswordRules = new PasswordRules(configuration);
    }

    /// <inheritdoc/>
    public IPasswordRules PasswordRules => mPasswordRules;
}
