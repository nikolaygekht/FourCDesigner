namespace Gehtsoft.FourCDesigner.Dao;

/// <summary>
/// Interface for password hashing operations.
/// </summary>
public interface IHashProvider
{
    /// <summary>
    /// Computes the SHA512 hash of the specified password with salt.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <returns>The Base64 string representation of the SHA512 hash.</returns>
    string ComputeHash(string password);

    /// <summary>
    /// Validates a password against a stored hash.
    /// </summary>
    /// <param name="password">The password to validate.</param>
    /// <param name="storedHash">The stored hash to compare against.</param>
    /// <returns>True if the password matches the hash; otherwise, false.</returns>
    bool ValidatePassword(string password, string storedHash);
}
