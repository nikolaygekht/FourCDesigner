using Gehtsoft.FourCDesigner.Logic.User;
using Gehtsoft.FourCDesigner.Middleware.Throttling;
using Microsoft.AspNetCore.Mvc;

namespace Gehtsoft.FourCDesigner.Controllers;

/// <summary>
/// Controller for account-related HTTP actions that redirect to pages.
/// These are not REST API endpoints but regular HTTP endpoints that work like traditional web pages.
/// Routes are at the application root level to match email-generated URLs.
/// </summary>
[Controller]
[Route("")]
public class AccountActionController : ControllerBase
{
    private readonly IUserController mUserController;
    private readonly ILogger<AccountActionController> mLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountActionController"/> class.
    /// </summary>
    /// <param name="userController">The user controller.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public AccountActionController(
        IUserController userController,
        ILogger<AccountActionController> logger)
    {
        mUserController = userController ?? throw new ArgumentNullException(nameof(userController));
        mLogger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// HTTP GET endpoint to validate password reset token and forward to reset password page.
    /// URL: {prefix}/request-reset-password?email=...&amp;token=...
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="token">The reset token.</param>
    /// <returns>Redirect to reset password page if valid, or login page if invalid.</returns>
    [HttpGet("request-reset-password")]
    [Throttle(60000, 10, true)]
    public IActionResult RequestResetPassword([FromQuery] string email, [FromQuery] string token)
    {
        mLogger.LogInformation("Validating reset token for email: {Email}", email);

        // Secure flag only for HTTPS requests (allows HTTP in development/testing)
        bool isHttps = Request.IsHttps;

        bool isValid = mUserController.ValidateToken(email, token);

        if (isValid)
        {
            mLogger.LogInformation("Reset token valid for email: {Email}, forwarding to reset password page", email);

            // Set email and token in cookies for reset password page

            Response.Cookies.Append("reset_email", email, new CookieOptions
            {
                HttpOnly = false,
                Secure = isHttps,
                SameSite = SameSiteMode.Strict,
                MaxAge = TimeSpan.FromMinutes(5)
            });

            Response.Cookies.Append("reset_token", token, new CookieOptions
            {
                HttpOnly = false,
                Secure = isHttps,
                SameSite = SameSiteMode.Strict,
                MaxAge = TimeSpan.FromMinutes(5)
            });

            return Redirect("/resetpassword.html");
        }

        mLogger.LogWarning("Reset token invalid or expired for email: {Email}, forwarding to login", email);

        // Set error message in cookie
        Response.Cookies.Append("login_message", "invalid_token", new CookieOptions
        {
            HttpOnly = false, // Must be false so JavaScript can read it
            Secure = isHttps,
            SameSite = SameSiteMode.Strict,
            MaxAge = TimeSpan.FromMinutes(5)
        });

        Response.Cookies.Append("login_message_type", "error", new CookieOptions
        {
            HttpOnly = false,
            Secure = isHttps,
            SameSite = SameSiteMode.Strict,
            MaxAge = TimeSpan.FromMinutes(5)
        });

        return Redirect("/login.html");
    }

    /// <summary>
    /// HTTP GET endpoint to activate account with token and forward to login page.
    /// URL: {prefix}/activate-account?email=...&amp;token=...
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="token">The activation token.</param>
    /// <returns>Redirect to login page with appropriate status message.</returns>
    [HttpGet("activate-account")]
    [Throttle(60000, 10, true)]
    public IActionResult ActivateAccount([FromQuery] string email, [FromQuery] string token)
    {
        mLogger.LogInformation("Activating account for email: {Email}", email);

        // Secure flag only for HTTPS requests (allows HTTP in development/testing)
        bool isHttps = Request.IsHttps;

        try
        {
            bool success = mUserController.ActivateUser(email, token);

            if (success)
            {
                mLogger.LogInformation("Account activated successfully for email: {Email}", email);

                // Set success message in cookie
                Response.Cookies.Append("login_message", "account_activated", new CookieOptions
                {
                    HttpOnly = false, // Must be false so JavaScript can read it
                    Secure = isHttps,
                    SameSite = SameSiteMode.Strict,
                    MaxAge = TimeSpan.FromMinutes(5)
                });

                Response.Cookies.Append("login_message_type", "success", new CookieOptions
                {
                    HttpOnly = false,
                    Secure = isHttps,
                    SameSite = SameSiteMode.Strict,
                    MaxAge = TimeSpan.FromMinutes(5)
                });

                return Redirect("/login.html");
            }

            mLogger.LogWarning("Account activation failed for email: {Email} - invalid or expired token", email);

            // Set error message in cookie
            Response.Cookies.Append("login_message", "invalid_activation_token", new CookieOptions
            {
                HttpOnly = false,
                Secure = isHttps,
                SameSite = SameSiteMode.Strict,
                MaxAge = TimeSpan.FromMinutes(5)
            });

            Response.Cookies.Append("login_message_type", "error", new CookieOptions
            {
                HttpOnly = false,
                Secure = isHttps,
                SameSite = SameSiteMode.Strict,
                MaxAge = TimeSpan.FromMinutes(5)
            });

            return Redirect("/login.html");
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Account activation error for email: {Email}", email);

            // Set error message in cookie
            Response.Cookies.Append("login_message", "activation_failed", new CookieOptions
            {
                HttpOnly = false,
                Secure = isHttps,
                SameSite = SameSiteMode.Strict,
                MaxAge = TimeSpan.FromMinutes(5)
            });

            Response.Cookies.Append("login_message_type", "error", new CookieOptions
            {
                HttpOnly = false,
                Secure = isHttps,
                SameSite = SameSiteMode.Strict,
                MaxAge = TimeSpan.FromMinutes(5)
            });

            return Redirect("/login.html");
        }
    }
}
