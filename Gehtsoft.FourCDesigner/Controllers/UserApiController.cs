using Gehtsoft.FourCDesigner.Logic.Config;
using Gehtsoft.FourCDesigner.Controllers.Data;
using Gehtsoft.FourCDesigner.Logic;
using Gehtsoft.FourCDesigner.Logic.Email.Configuration;
using Gehtsoft.FourCDesigner.Logic.Session;
using Gehtsoft.FourCDesigner.Logic.User;
using Gehtsoft.FourCDesigner.Middleware.Throttling;
using Gehtsoft.FourCDesigner.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Gehtsoft.FourCDesigner.Controllers;

/// <summary>
/// API Controller for user authentication and session management.
/// </summary>
[ApiController]
[Route("api/user")]
public class UserApiController : ControllerBase
{
    private readonly ISessionController mSessionController;
    private readonly IUserController mUserController;
    private readonly IUserConfiguration mUserConfiguration;
    private readonly IEmailConfiguration mEmailConfiguration;
    private readonly ILogger<UserApiController> mLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserApiController"/> class.
    /// </summary>
    /// <param name="sessionController">The session controller.</param>
    /// <param name="userController">The user controller.</param>
    /// <param name="userConfiguration">The user configuration.</param>
    /// <param name="emailConfiguration">The email configuration.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public UserApiController(
        ISessionController sessionController,
        IUserController userController,
        IUserConfiguration userConfiguration,
        IEmailConfiguration emailConfiguration,
        ILogger<UserApiController> logger)
    {
        mSessionController = sessionController ?? throw new ArgumentNullException(nameof(sessionController));
        mUserController = userController ?? throw new ArgumentNullException(nameof(userController));
        mUserConfiguration = userConfiguration ?? throw new ArgumentNullException(nameof(userConfiguration));
        mEmailConfiguration = emailConfiguration ?? throw new ArgumentNullException(nameof(emailConfiguration));
        mLogger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Authenticates a user and creates a new session.
    /// </summary>
    /// <param name="request">The login request containing email and password.</param>
    /// <returns>A session ID if authentication is successful; otherwise, Unauthorized.</returns>
    [HttpPost("login")]
    [EnableRateLimiting(ThrottlingServiceExtensions.DefaultThrottlePolicyName)]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // Format validation happens automatically via model validation
        if (!ModelState.IsValid)
        {
            mLogger.LogWarning("Login request validation failed");
            return BadRequest(ModelState);
        }

        mLogger.LogInformation("Login attempt for email: {Email}", request.Email);

        // Call business logic
        bool success = mSessionController.Authorize(request.Email, request.Password, out string sessionId);

        if (!success)
        {
            mLogger.LogWarning("Login failed for email: {Email}", request.Email);
            return Unauthorized(new { error = "Invalid credentials or inactive user" });
        }

        mLogger.LogInformation("Login successful for email: {Email}", request.Email);

        // Map result to DTO and return
        var response = new LoginResponse { SessionId = sessionId };

        return Ok(response);
    }

    /// <summary>
    /// Logs out a user by closing their session.
    /// </summary>
    /// <param name="request">The logout request containing the session ID.</param>
    /// <returns>Success status.</returns>
    [HttpPost("logout")]
    public IActionResult Logout([FromBody] LogoutRequest request)
    {
        // Format validation happens automatically via model validation
        if (!ModelState.IsValid)
        {
            mLogger.LogWarning("Logout request validation failed");
            return BadRequest(ModelState);
        }

        mLogger.LogInformation("Logout request for session");

        // Call business logic
        mSessionController.CloseSession(request.SessionId);

        mLogger.LogInformation("Logout successful");

        return Ok(new { message = "Logged out successfully" });
    }

    /// <summary>
    /// Validates a session and returns whether it is valid.
    /// </summary>
    /// <param name="request">The validate session request containing the session ID.</param>
    /// <returns>Validation result indicating if the session is valid.</returns>
    [HttpPost("validate-session")]
    public IActionResult ValidateSession([FromBody] ValidateSessionRequest request)
    {
        // Format validation happens automatically via model validation
        if (!ModelState.IsValid)
        {
            mLogger.LogWarning("Validate session request validation failed");
            return BadRequest(ModelState);
        }

        mLogger.LogDebug("Validating session");

        // Call business logic
        bool isValid = mSessionController.CheckSession(request.SessionId, out string email, out string role);

        var response = new ValidateSessionResponse { Valid = isValid };

        if (isValid)
            mLogger.LogDebug("Session is valid for email: {Email}", email);
        else
            mLogger.LogDebug("Session is invalid or expired");

        return Ok(response);
    }

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="request">The registration request containing email and password.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Success status with validation errors if any.</returns>
    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterUserRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            mLogger.LogWarning("Register user request validation failed");
            return BadRequest(ModelState);
        }

        mLogger.LogInformation("Registration attempt for email: {Email}", request.Email);

        try
        {
            bool success = await mUserController.RegisterUser(request.Email, request.Password, cancellationToken);

            if (success)
            {
                mLogger.LogInformation("Registration successful for email: {Email}", request.Email);
                return Ok(new RegisterUserResponse
                {
                    Success = true,
                    Message = "Registration successful. Please check your email for activation code."
                });
            }

            mLogger.LogWarning("Registration failed for email: {Email}", request.Email);
            return BadRequest(new RegisterUserResponse
            {
                Success = false,
                Message = "Registration failed"
            });
        }
        catch (ValidationException ex)
        {
            mLogger.LogWarning("Registration validation failed for email: {Email}", request.Email);

            var errors = ex.Errors.Select(e => new FieldValidationError
            {
                Field = e.Field,
                Messages = e.Messages.ToList()
            }).ToList();

            return BadRequest(new RegisterUserResponse
            {
                Success = false,
                Errors = errors
            });
        }
    }

    /// <summary>
    /// Requests a password reset for the specified email.
    /// </summary>
    /// <param name="request">The password reset request containing email.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Success status (always returns success for security).</returns>
    [HttpPost("request-password-reset")]
    public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            mLogger.LogWarning("Request password reset validation failed");
            return BadRequest(ModelState);
        }

        mLogger.LogInformation("Password reset requested for email: {Email}", request.Email);

        // Call business logic - this is silent for security reasons
        await mUserController.RequestPasswordReset(request.Email, cancellationToken);

        // Always return success message for security (don't reveal if user exists)
        return Ok(new { message = "If the email exists, a password reset link has been sent." });
    }

    /// <summary>
    /// Resets the password using a token.
    /// </summary>
    /// <param name="request">The reset password request containing email, password, and token.</param>
    /// <returns>Success status with validation errors if any.</returns>
    [HttpPost("reset-password")]
    public IActionResult ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            mLogger.LogWarning("Reset password request validation failed");
            return BadRequest(ModelState);
        }

        mLogger.LogInformation("Password reset attempt for email: {Email}", request.Email);

        try
        {
            bool success = mUserController.ResetPassword(request.Email, request.Token, request.Password);

            if (success)
            {
                mLogger.LogInformation("Password reset successful for email: {Email}", request.Email);
                return Ok(new ResetPasswordResponse
                {
                    Success = true,
                    Message = "Password has been reset successfully. You can now log in with your new password."
                });
            }

            mLogger.LogWarning("Password reset failed for email: {Email} - invalid or expired token", request.Email);
            return BadRequest(new ResetPasswordResponse
            {
                Success = false,
                Message = "Invalid or expired token"
            });
        }
        catch (ValidationException ex)
        {
            mLogger.LogWarning("Password reset validation failed for email: {Email}", request.Email);

            var errors = ex.Errors.Select(e => new FieldValidationError
            {
                Field = e.Field,
                Messages = e.Messages.ToList()
            }).ToList();

            return BadRequest(new ResetPasswordResponse
            {
                Success = false,
                Errors = errors
            });
        }
    }

    /// <summary>
    /// Gets the password validation rules.
    /// </summary>
    /// <returns>Password validation rules.</returns>
    [HttpGet("password-rules")]
    public IActionResult GetPasswordRules()
    {
        mLogger.LogDebug("Getting password validation rules");

        var rules = mUserConfiguration.PasswordRules;
        var response = new PasswordRulesResponse
        {
            MinimumLength = rules.MinimumLength,
            RequireCapitalLetter = rules.RequireCapitalLetter,
            RequireSmallLetter = rules.RequireSmallLetter,
            RequireDigit = rules.RequireDigit,
            RequireSpecialSymbol = rules.RequireSpecialSymbol
        };

        return Ok(response);
    }

    /// <summary>
    /// Gets the system email address configuration.
    /// </summary>
    /// <returns>System email configuration.</returns>
    [HttpGet("system-email")]
    public IActionResult GetSystemEmail()
    {
        mLogger.LogDebug("Getting system email address");

        var response = new SystemEmailResponse
        {
            EmailFrom = mEmailConfiguration.MailAddressFrom
        };

        return Ok(response);
    }

    /// <summary>
    /// Checks if an email is available for registration.
    /// Applies stricter rate limiting to prevent email enumeration attacks.
    /// </summary>
    /// <param name="email">The email address to check.</param>
    /// <returns>True if the email is available; false if taken or invalid.</returns>
    [HttpGet("check-email")]
    [EnableRateLimiting(ThrottlingServiceExtensions.EmailCheckThrottlePolicyName)]
    public IActionResult CheckEmail([FromQuery] string email)
    {
        mLogger.LogDebug("Checking email availability");

        if (string.IsNullOrWhiteSpace(email))
        {
            mLogger.LogWarning("Email check called with empty email");
            return BadRequest();
        }

        bool available = mUserController.IsEmailAvailable(email);

        var response = new CheckEmailResponse
        {
            Available = available
        };

        return Ok(response);
    }
}
