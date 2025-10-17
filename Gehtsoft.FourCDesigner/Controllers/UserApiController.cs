using Gehtsoft.FourCDesigner.Controllers.Data;
using Gehtsoft.FourCDesigner.Logic;
using Gehtsoft.FourCDesigner.Logic.Session;
using Gehtsoft.FourCDesigner.Logic.User;
using Gehtsoft.FourCDesigner.Utils;
using Microsoft.AspNetCore.Mvc;

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
    private readonly ILogger<UserApiController> mLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserApiController"/> class.
    /// </summary>
    /// <param name="sessionController">The session controller.</param>
    /// <param name="userController">The user controller.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public UserApiController(
        ISessionController sessionController,
        IUserController userController,
        ILogger<UserApiController> logger)
    {
        mSessionController = sessionController ?? throw new ArgumentNullException(nameof(sessionController));
        mUserController = userController ?? throw new ArgumentNullException(nameof(userController));
        mLogger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Authenticates a user and creates a new session.
    /// </summary>
    /// <param name="request">The login request containing email and password.</param>
    /// <returns>A session ID if authentication is successful; otherwise, Unauthorized.</returns>
    [HttpPost("login")]
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
    /// HTTP GET endpoint to validate password reset token and forward to reset password page.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="token">The reset token.</param>
    /// <returns>Redirect to reset password page if valid, or login page if invalid.</returns>
    [HttpGet("request-reset-password")]
    public IActionResult ValidateResetToken([FromQuery] string email, [FromQuery] string token)
    {
        mLogger.LogInformation("Validating reset token for email: {Email}", email);

        bool isValid = mUserController.ValidateToken(email, token);

        if (isValid)
        {
            mLogger.LogInformation("Reset token valid for email: {Email}, forwarding to reset password page", email);
            // Forward to reset password page with email and token as query parameters
            return Redirect($"/resetpassword.html?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}");
        }

        mLogger.LogWarning("Reset token invalid or expired for email: {Email}, forwarding to login", email);
        // Forward to login page if token is invalid
        return Redirect("/login.html?error=invalid_token");
    }

    /// <summary>
    /// HTTP GET endpoint to activate account with token and forward to login page.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="token">The activation token.</param>
    /// <returns>Redirect to login page with appropriate status message.</returns>
    [HttpGet("activate-account")]
    public IActionResult ActivateAccount([FromQuery] string email, [FromQuery] string token)
    {
        mLogger.LogInformation("Activating account for email: {Email}", email);

        try
        {
            bool success = mUserController.ActivateUser(email, token);

            if (success)
            {
                mLogger.LogInformation("Account activated successfully for email: {Email}", email);
                return Redirect("/login.html?activated=true");
            }

            mLogger.LogWarning("Account activation failed for email: {Email} - invalid or expired token", email);
            return Redirect("/login.html?error=invalid_activation_token");
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Account activation error for email: {Email}", email);
            return Redirect("/login.html?error=activation_failed");
        }
    }
}
