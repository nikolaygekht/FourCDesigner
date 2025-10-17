namespace Gehtsoft.FourCDesigner.Logic.Session;

/// <summary>
/// ECB Controller interface for session operations.
/// </summary>
public interface ISessionController
{
    /// <summary>
    /// Authorizes a user and creates a new session.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="password">The user's password.</param>
    /// <param name="sessionId">The created session ID if authorization is successful.</param>
    /// <returns>True if the user exists, is active, and the password is correct; otherwise, false.</returns>
    bool Authorize(string email, string password, out string sessionId);

    /// <summary>
    /// Checks if a session exists and is valid, and extends its lifetime.
    /// </summary>
    /// <param name="sessionId">The session ID to check.</param>
    /// <param name="email">The user's email address if the session is valid.</param>
    /// <param name="role">The user's role if the session is valid.</param>
    /// <returns>True if the session exists and is valid; otherwise, false.</returns>
    bool CheckSession(string sessionId, out string email, out string role);

    /// <summary>
    /// Closes a session.
    /// </summary>
    /// <param name="sessionId">The session ID to close.</param>
    void CloseSession(string sessionId);
}
