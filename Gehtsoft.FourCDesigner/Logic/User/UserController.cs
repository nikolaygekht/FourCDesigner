using AutoMapper;
using Gehtsoft.FourCDesigner.Dao;
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
    private readonly IMessages mMessages;
    private readonly IMapper mMapper;
    private readonly ILogger<UserController> mLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserController"/> class.
    /// </summary>
    /// <param name="userDao">The user data access object.</param>
    /// <param name="hashProvider">The password hash provider.</param>
    /// <param name="passwordValidator">The password validator.</param>
    /// <param name="messages">The localized messages provider.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public UserController(
        IUserDao userDao,
        IHashProvider hashProvider,
        IPasswordValidator passwordValidator,
        IMessages messages,
        IMapper mapper,
        ILogger<UserController> logger)
    {
        mUserDao = userDao ?? throw new ArgumentNullException(nameof(userDao));
        mHashProvider = hashProvider ?? throw new ArgumentNullException(nameof(hashProvider));
        mPasswordValidator = passwordValidator ?? throw new ArgumentNullException(nameof(passwordValidator));
        mMessages = messages ?? throw new ArgumentNullException(nameof(messages));
        mMapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        mLogger = logger ?? throw new ArgumentNullException(nameof(logger));
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
    public int RegisterUser(string email, string password)
    {
        ValidateUserData(email, password);

        try
        {
            mLogger.LogInformation("Registering new user with email: {Email}", email);

            // Create new user
            var user = new Entities.User
            {
                Email = email,
                PasswordHash = mHashProvider.ComputeHash(password),
                Role = "user",
                ActiveUser = true
            };

            mUserDao.SaveUser(user);

            mLogger.LogInformation("Successfully registered user {UserId} with email: {Email}", user.Id, email);
            return user.Id;
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
    public bool ActivateUser(string email)
    {
        try
        {
            mLogger.LogInformation("Activating user {Email}", email);

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
}
