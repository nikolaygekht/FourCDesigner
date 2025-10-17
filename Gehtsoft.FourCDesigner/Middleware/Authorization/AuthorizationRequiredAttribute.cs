using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Gehtsoft.FourCDesigner.Middleware.Authorization;

/// <summary>
/// Attribute that requires a valid session for accessing the action.
/// Checks the X-fourc-session header for a valid session ID.
/// Optionally can require a specific role.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class AuthorizationRequiredAttribute : Attribute, IFilterFactory
{
    /// <summary>
    /// Gets the required role. If null or empty, any valid session is sufficient.
    /// </summary>
    public string? RequiredRole { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationRequiredAttribute"/> class
    /// that allows any authenticated user.
    /// </summary>
    public AuthorizationRequiredAttribute()
    {
        RequiredRole = null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationRequiredAttribute"/> class
    /// that requires a specific role.
    /// </summary>
    /// <param name="requiredRole">The role required to access the action.</param>
    public AuthorizationRequiredAttribute(string requiredRole)
    {
        RequiredRole = requiredRole;
    }

    /// <inheritdoc/>
    public bool IsReusable => false;

    /// <inheritdoc/>
    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        return ActivatorUtilities.CreateInstance<AuthorizationRequiredFilter>(
            serviceProvider,
            RequiredRole ?? string.Empty);
    }
}
