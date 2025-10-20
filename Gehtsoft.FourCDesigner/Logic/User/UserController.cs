using System.Text.RegularExpressions;
using AutoMapper;
using Gehtsoft.FourCDesigner.Logic.Config;
using Gehtsoft.FourCDesigner.Dao;
using Gehtsoft.FourCDesigner.Logic.Email;
using Gehtsoft.FourCDesigner.Logic.Email.Model;
using Gehtsoft.FourCDesigner.Logic.Token;
using Gehtsoft.FourCDesigner.Utils;

namespace Gehtsoft.FourCDesigner.Logic.User;

/// <summary>
/// ECB Controller for user operations.
/// </summary>
public class UserController : IUserController
{
    private readonly IUserDao mUserDao;
    private readonly IHashProvider mHashProvider;
    private readonly IPasswordValidator mPasswordValidator;
    private readonly ITokenService mTokenService;
    private readonly IEmailService mEmailService;
    private readonly IUrlBuilder mUrlBuilder;
    private readonly IMessages mMessages;
    private readonly IMapper mMapper;
    private readonly ILogger<UserController> mLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserController"/> class.
    /// </summary>
    /// <param name="userDao">The user data access object.</param>
    /// <param name="hashProvider">The password hash provider.</param>
    /// <param name="passwordValidator">The password validator.</param>
    /// <param name="tokenService">The token service.</param>
    /// <param name="emailService">The email service.</param>
    /// <param name="urlBuilder">The URL builder for generating external URLs.</param>
    /// <param name="messages">The localized messages provider.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public UserController(
        IUserDao userDao,
        IHashProvider hashProvider,
        IPasswordValidator passwordValidator,
        ITokenService tokenService,
        IEmailService emailService,
        IUrlBuilder urlBuilder,
        IMessages messages,
        IMapper mapper,
        ILogger<UserController> logger)
    {
        mUserDao = userDao ?? throw new ArgumentNullException(nameof(userDao));
        mHashProvider = hashProvider ?? throw new ArgumentNullException(nameof(hashProvider));
        mPasswordValidator = passwordValidator ?? throw new ArgumentNullException(nameof(passwordValidator));
        mTokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        mEmailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        mUrlBuilder = urlBuilder ?? throw new ArgumentNullException(nameof(urlBuilder));
        mMessages = messages ?? throw new ArgumentNullException(nameof(messages));
        mMapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        mLogger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Validates email format according to RFC 2822 Section 3.4.
    /// </summary>
    /// <param name="email">The email to validate.</param>
    /// <returns>True if email is valid; otherwise, false.</returns>
    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        // RFC 2822 Section 3.4 simplified email regex pattern
        // This pattern validates common email formats
        string pattern = @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$";

        return Regex.IsMatch(email, pattern);
    }

    /// <summary>
    /// Validates user data (email and password).
    /// </summary>
    /// <param name="email">The email to validate.</param>
    /// <param name="password">The password to validate.</param>
    /// <param name="excludeEmail">Email to exclude from duplicate email check (for updates).</param>
    private void ValidateUserData(string email, string password, string? excludeEmail = null)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(email))
            errors.Add(new ValidationError("email", mMessages.EmailCannotBeEmpty));
        else if (!IsValidEmail(email))
            errors.Add(new ValidationError("email", "Invalid email format"));

        if (string.IsNullOrWhiteSpace(password))
            errors.Add(new ValidationError("password", mMessages.PasswordCannotBeEmpty));
        else
        {
            // Validate password
            List<string> passwordErrors;
            if (!mPasswordValidator.ValidatePassword(password, out passwordErrors))
            {
                mLogger.LogWarning("Password validation failed");
                if (passwordErrors != null && passwordErrors.Count > 0)
                    errors.Add(new ValidationError("password", passwordErrors));
                else
                    errors.Add(new ValidationError("password", "Password validation failed"));
            }
        }

        // Check for duplicate email
        if (!string.IsNullOrWhiteSpace(email))
        {
            // Get the user ID to exclude if excludeEmail is provided
            int? excludeUserId = null;
            if (!string.IsNullOrWhiteSpace(excludeEmail))
            {
                var existingUser = mUserDao.GetUserByEmail(excludeEmail);
                if (existingUser != null)
                    excludeUserId = existingUser.Id;
            }

            if (mUserDao.IsEmailUsed(email, excludeUserId))
            {
                mLogger.LogWarning("Email already exists: {Email}", email);
                errors.Add(new ValidationError("email", mMessages.UserAlreadyExists(email)));
            }
        }

        if (errors.Count > 0)
            throw new ValidationException(errors.ToArray());
    }

    /// <inheritdoc/>
    public async Task<bool> RegisterUser(string email, string password, CancellationToken cancellationToken = default)
    {
        ValidateUserData(email, password);

        try
        {
            mLogger.LogInformation("Registering new user with email: {Email}", email);

            // Create new inactive user
            var user = new Entities.User
            {
                Email = email,
                PasswordHash = mHashProvider.ComputeHash(password),
                Role = "user",
                ActiveUser = false
            };

            mUserDao.SaveUser(user);
            mLogger.LogInformation("Successfully created inactive user {UserId} with email: {Email}", user.Id, email);

            // Generate activation token
            string token = mTokenService.GenerateToken(email);
            mLogger.LogDebug("Generated activation token for user: {Email}", email);

            // Build activation URL
            string activationUrl = mUrlBuilder.BuildUrl("/activate-account", new { email, token });
            mLogger.LogDebug("Generated activation URL for user: {Email}", email);

            // Send activation email and trigger immediate processing
            var emailMessage = EmailMessage.Create(
                email,
                mMessages.ActivationEmailSubject,
                mMessages.ActivationEmailBodyWithLink(activationUrl, mTokenService.ExpirationInSeconds),
                html: false,
                priority: true
            );

            await mEmailService.SendEmailAndTriggerProcessorAsync(emailMessage, cancellationToken);
            mLogger.LogInformation("Sent activation email to: {Email}", email);

            return true;
        }
        catch (Exception ex) when (ex is not ValidationException)
        {
            mLogger.LogError(ex, "Failed to register user with email: {Email}", email);
            throw;
        }
    }

    /// <inheritdoc/>
    public UserInfo? ValidateUser(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return null;

        try
        {
            mLogger.LogDebug("Validating user credentials for email: {Email}", email);

            Entities.User? user = mUserDao.GetUserByEmail(email);
            if (user == null)
            {
                mLogger.LogDebug("User not found: {Email}", email);
                return null;
            }

            if (!mHashProvider.ValidatePassword(password, user.PasswordHash))
            {
                mLogger.LogDebug("Invalid password for user: {Email}", email);
                return null;
            }

            mLogger.LogInformation("Successfully validated user {UserId}", user.Id);
            return mMapper.Map<UserInfo>(user);
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Failed to validate user with email: {Email}", email);
            throw;
        }
    }

    /// <inheritdoc/>
    public bool UpdateUser(string currentEmail, string newEmail, string password)
    {
        ValidateUserData(newEmail, password, currentEmail);

        try
        {
            mLogger.LogInformation("Updating user {Email}", currentEmail);

            Entities.User? user = mUserDao.GetUserByEmail(currentEmail);
            if (user == null)
            {
                mLogger.LogWarning("Update failed: user {Email} not found", currentEmail);
                throw new ValidationException(new ValidationError("email", mMessages.UserNotFound(currentEmail)));
            }

            user.Email = newEmail;
            user.PasswordHash = mHashProvider.ComputeHash(password);

            mUserDao.SaveUser(user);

            mLogger.LogInformation("Successfully updated user {Email} to {NewEmail}", currentEmail, newEmail);
            return true;
        }
        catch (Exception ex) when (ex is not ValidationException)
        {
            mLogger.LogError(ex, "Failed to update user {Email}", currentEmail);
            throw;
        }
    }

    /// <inheritdoc/>
    public bool ActivateUser(string email, string token)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            return false;

        try
        {
            mLogger.LogInformation("Activating user {Email} with token", email);

            // Validate token and remove it to prevent reuse
            if (!mTokenService.ValidateToken(token, email, true))
            {
                mLogger.LogWarning("Activation failed: invalid token for user {Email}", email);
                return false;
            }

            bool updated = mUserDao.ActivateUserByEmail(email);
            if (!updated)
            {
                mLogger.LogWarning("Activation failed: user {Email} not found", email);
                throw new ValidationException(new ValidationError("email", mMessages.UserNotFound(email)));
            }

            mLogger.LogInformation("Successfully activated user {Email}", email);
            return true;
        }
        catch (Exception ex) when (ex is not ValidationException)
        {
            mLogger.LogError(ex, "Failed to activate user {Email}", email);
            throw;
        }
    }

    /// <inheritdoc/>
    public bool DeactivateUser(string email)
    {
        try
        {
            mLogger.LogInformation("Deactivating user {Email}", email);

            bool updated = mUserDao.DeactivateUserByEmail(email);
            if (!updated)
            {
                mLogger.LogWarning("Deactivation failed: user {Email} not found", email);
                throw new ValidationException(new ValidationError("email", mMessages.UserNotFound(email)));
            }

            mLogger.LogInformation("Successfully deactivated user {Email}", email);
            return true;
        }
        catch (Exception ex) when (ex is not ValidationException)
        {
            mLogger.LogError(ex, "Failed to deactivate user {Email}", email);
            throw;
        }
    }

    /// <inheritdoc/>
    public bool ChangePassword(string email, string password)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(password))
            errors.Add(new ValidationError("password", mMessages.PasswordCannotBeEmpty));
        else
        {
            // Validate password
            List<string> passwordErrors;
            if (!mPasswordValidator.ValidatePassword(password, out passwordErrors))
            {
                mLogger.LogWarning("Password validation failed for password change: {Email}", email);
                if (passwordErrors != null && passwordErrors.Count > 0)
                    errors.Add(new ValidationError("password", passwordErrors));
                else
                    errors.Add(new ValidationError("password", "Password validation failed"));
            }
        }

        if (errors.Count > 0)
            throw new ValidationException(errors.ToArray());

        try
        {
            mLogger.LogInformation("Changing password for user {Email}", email);

            string passwordHash = mHashProvider.ComputeHash(password);
            bool updated = mUserDao.UpdatePasswordByEmail(email, passwordHash);

            if (!updated)
            {
                mLogger.LogWarning("Password change failed: user {Email} not found", email);
                throw new ValidationException(new ValidationError("email", mMessages.UserNotFound(email)));
            }

            mLogger.LogInformation("Successfully changed password for user {Email}", email);
            return true;
        }
        catch (Exception ex) when (ex is not ValidationException)
        {
            mLogger.LogError(ex, "Failed to change password for user {Email}", email);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task RequestPasswordReset(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return;

        try
        {
            mLogger.LogInformation("Password reset requested for email: {Email}", email);

            // Check if user exists and is active
            Entities.User? user = mUserDao.GetUserByEmail(email);
            if (user == null || !user.ActiveUser)
            {
                mLogger.LogDebug("Password reset request ignored: user {Email} not found or inactive", email);
                return; // Silent failure for security reasons
            }

            // Generate password reset token
            string token = mTokenService.GenerateToken(email);
            mLogger.LogDebug("Generated password reset token for user: {Email}", email);

            // Build password reset URL
            string resetUrl = mUrlBuilder.BuildUrl("/request-reset-password", new { email, token });
            mLogger.LogDebug("Generated password reset URL for user: {Email}", email);

            // Send password reset email and trigger immediate processing
            var emailMessage = EmailMessage.Create(
                email,
                mMessages.PasswordResetEmailSubject,
                mMessages.PasswordResetEmailBodyWithLink(resetUrl, mTokenService.ExpirationInSeconds),
                html: false,
                priority: true
            );

            await mEmailService.SendEmailAndTriggerProcessorAsync(emailMessage, cancellationToken);
            mLogger.LogInformation("Sent password reset email to: {Email}", email);
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Failed to process password reset request for email: {Email}", email);
            // Silent failure for security reasons
        }
    }

    /// <inheritdoc/>
    public bool ValidateToken(string email, string token)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            return false;

        // Do not remove the token, just validate it
        return mTokenService.ValidateToken(token, email, false);
    }

    /// <inheritdoc/>
    public bool ResetPassword(string email, string token, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            return false;

        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(newPassword))
            errors.Add(new ValidationError("password", mMessages.PasswordCannotBeEmpty));
        else
        {
            // Validate password
            List<string> passwordErrors;
            if (!mPasswordValidator.ValidatePassword(newPassword, out passwordErrors))
            {
                mLogger.LogWarning("Password validation failed for password reset: {Email}", email);
                if (passwordErrors != null && passwordErrors.Count > 0)
                    errors.Add(new ValidationError("password", passwordErrors));
                else
                    errors.Add(new ValidationError("password", "Password validation failed"));
            }
        }

        if (errors.Count > 0)
            throw new ValidationException(errors.ToArray());

        try
        {
            mLogger.LogInformation("Resetting password for user {Email} with token", email);

            // Validate token and remove it to prevent reuse
            if (!mTokenService.ValidateToken(token, email, true))
            {
                mLogger.LogWarning("Password reset failed: invalid token for user {Email}", email);
                return false;
            }

            // Update password
            string passwordHash = mHashProvider.ComputeHash(newPassword);
            bool updated = mUserDao.UpdatePasswordByEmail(email, passwordHash);

            if (!updated)
            {
                mLogger.LogWarning("Password reset failed: user {Email} not found", email);
                return false;
            }

            mLogger.LogInformation("Successfully reset password for user {Email}", email);
            return true;
        }
        catch (Exception ex) when (ex is not ValidationException)
        {
            mLogger.LogError(ex, "Failed to reset password for user {Email}", email);
            throw;
        }
    }

    /// <inheritdoc/>
    public bool IsEmailAvailable(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        // Validate email format first
        if (!IsValidEmail(email))
            return false;

        try
        {
            // Check if email is already in use
            bool isUsed = mUserDao.IsEmailUsed(email, null);
            return !isUsed;
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Failed to check email availability for: {Email}", email);
            return false;
        }
    }
}
