using System.Security.Cryptography;
using System.Text;

namespace Gehtsoft.FourCDesigner.Dao;

/// <summary>
/// Implementation of password hashing using SHA256 algorithm.
/// </summary>
public class HashProvider : IHashProvider
{
    /// <inheritdoc/>
    public string ComputeHash(string password)
    {
        if (password == null)
            throw new ArgumentNullException(nameof(password));

        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        byte[] hashBytes = SHA256.HashData(passwordBytes);

        return Convert.ToBase64String(hashBytes);
    }

    /// <inheritdoc/>
    public bool ValidatePassword(string password, string storedHash)
    {
        if (password == null)
            throw new ArgumentNullException(nameof(password));

        if (storedHash == null)
            throw new ArgumentNullException(nameof(storedHash));

        string computedHash = ComputeHash(password);
        return string.Equals(computedHash, storedHash, StringComparison.Ordinal);
    }
}
