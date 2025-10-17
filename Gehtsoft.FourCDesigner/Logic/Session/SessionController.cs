using System.Security.Cryptography;
using System.Text;
using Gehtsoft.FourCDesigner.Dao;
using Gehtsoft.FourCDesigner.Logic.User;
using Microsoft.Extensions.Caching.Memory;

namespace Gehtsoft.FourCDesigner.Logic.Session;

/// <summary>
/// ECB Controller for session operations.
/// </summary>
public class SessionController : ISessionController, IDisposable
{
    private readonly ISessionSettings mSettings;
    private readonly IUserController mUserController;
    private readonly MemoryCache mCache;

    /// <summary>
    /// Session data stored in cache.
    /// </summary>
    private class SessionData
    {
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionController"/> class.
    /// </summary>
    /// <param name="settings">The session settings.</param>
    /// <param name="userController">The user controller.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public SessionController(ISessionSettings settings, IUserController userController)
    {
        mSettings = settings ?? throw new ArgumentNullException(nameof(settings));
        mUserController = userController ?? throw new ArgumentNullException(nameof(userController));
        mCache = new MemoryCache(new MemoryCacheOptions());
    }

    /// <summary>
    /// Disposes the memory cache.
    /// </summary>
    public void Dispose()
    {
        mCache?.Dispose();
    }

    /// <inheritdoc/>
    public bool Authorize(string email, string password, out string sessionId)
    {
        sessionId = string.Empty;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            return false;

        // Validate user credentials
        UserInfo? userInfo = mUserController.ValidateUser(email, password);
        if (userInfo == null || !userInfo.ActiveUser)
            return false;

        // Generate a hard-to-guess session ID
        sessionId = GenerateSessionId(email);

        // Store session data in cache with sliding expiration
        var sessionData = new SessionData
        {
            Email = userInfo.Email,
            Role = userInfo.Role
        };

        var cacheOptions = new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromSeconds(mSettings.SessionTimeoutInSeconds)
        };

        mCache.Set(sessionId, sessionData, cacheOptions);

        return true;
    }

    /// <inheritdoc/>
    public bool CheckSession(string sessionId, out string email, out string role)
    {
        email = string.Empty;
        role = string.Empty;

        if (string.IsNullOrEmpty(sessionId))
            return false;

        // Try to get session data from cache
        // The Get operation automatically extends the sliding expiration
        if (mCache.TryGetValue<SessionData>(sessionId, out var sessionData) && sessionData != null)
        {
            email = sessionData.Email;
            role = sessionData.Role;
            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public void CloseSession(string sessionId)
    {
        if (string.IsNullOrEmpty(sessionId))
            return;

        mCache.Remove(sessionId);
    }

    /// <summary>
    /// Generates a hard-to-guess session ID using SHA256 hash.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <returns>A session ID as a base64 string.</returns>
    private static string GenerateSessionId(string email)
    {
        // Combine email with timestamp and random data for uniqueness
        string input = $"{email}:{DateTime.UtcNow.Ticks}:{Guid.NewGuid()}";
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = SHA256.HashData(inputBytes);

        // Convert to base64 string
        return Convert.ToBase64String(hashBytes);
    }
}
