using Gehtsoft.FourCDesigner.Logic.Session;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Gehtsoft.FourCDesigner.Middleware.Authorization;

/// <summary>
/// Filter that validates session and optionally checks user role.
/// </summary>
public class AuthorizationRequiredFilter : IActionFilter
{
    private readonly ISessionController mSessionController;
    private readonly ILogger<AuthorizationRequiredFilter> mLogger;
    private readonly string mRequiredRole;

    /// <summary>
    /// The header name containing the session ID.
    /// </summary>
    public const string SessionHeaderName = "X-fourc-session";

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationRequiredFilter"/> class.
    /// </summary>
    /// <param name="sessionController">The session controller.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="requiredRole">The required role (empty string if any authenticated user is allowed).</param>
    /// <exception cref="ArgumentNullException">Thrown when sessionController or logger is null.</exception>
    public AuthorizationRequiredFilter(
        ISessionController sessionController,
        ILogger<AuthorizationRequiredFilter> logger,
        string requiredRole)
    {
        mSessionController = sessionController ?? throw new ArgumentNullException(nameof(sessionController));
        mLogger = logger ?? throw new ArgumentNullException(nameof(logger));
        mRequiredRole = requiredRole ?? string.Empty;
    }

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Get session ID from header
        if (!context.HttpContext.Request.Headers.TryGetValue(SessionHeaderName, out var sessionIdValues))
        {
            mLogger.LogWarning("Authorization failed: {Header} header not found", SessionHeaderName);
            context.Result = new UnauthorizedObjectResult(new
            {
                error = "Unauthorized",
                message = $"Missing {SessionHeaderName} header"
            });
            return;
        }

        string sessionId = sessionIdValues.FirstOrDefault() ?? string.Empty;
        if (string.IsNullOrEmpty(sessionId))
        {
            mLogger.LogWarning("Authorization failed: {Header} header is empty", SessionHeaderName);
            context.Result = new UnauthorizedObjectResult(new
            {
                error = "Unauthorized",
                message = $"{SessionHeaderName} header is empty"
            });
            return;
        }

        // Check session validity
        if (!mSessionController.CheckSession(sessionId, out string email, out string role))
        {
            mLogger.LogWarning("Authorization failed: Invalid or expired session {SessionId}", sessionId);
            context.Result = new UnauthorizedObjectResult(new
            {
                error = "Unauthorized",
                message = "Invalid or expired session"
            });
            return;
        }

        // Check role if required
        if (!string.IsNullOrEmpty(mRequiredRole) && role != mRequiredRole)
        {
            mLogger.LogWarning(
                "Authorization failed: User {Email} with role {UserRole} attempted to access resource requiring role {RequiredRole}",
                email, role, mRequiredRole);
            context.Result = new ForbidResult();
            return;
        }

        // Store user info in HttpContext.Items for potential use in the action
        context.HttpContext.Items["UserEmail"] = email;
        context.HttpContext.Items["UserRole"] = role;

        mLogger.LogDebug("Authorization successful: User {Email} with role {Role}", email, role);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No action needed after execution
    }
}
