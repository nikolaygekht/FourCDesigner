using System.Security.Cryptography;
using System.Text;
using Gehtsoft.FourCDesigner.Dao.Configuration;

namespace Gehtsoft.FourCDesigner.Dao;

/// <summary>
/// Implementation of password hashing using SHA512 algorithm with salt.
/// </summary>
public class HashProvider : IHashProvider
{
    private readonly IHashProviderConfiguration mConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="HashProvider"/> class.
    /// </summary>
    /// <param name="configuration">The hash provider configuration.</param>
    /// <exception cref="ArgumentNullException">Thrown when configuration is null.</exception>
    public HashProvider(IHashProviderConfiguration configuration)
    {
        mConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <inheritdoc/>
    public string ComputeHash(string password)
    {
        if (password == null)
            throw new ArgumentNullException(nameof(password));

        string saltedPassword = password + mConfiguration.Salt;
        byte[] passwordBytes = Encoding.UTF8.GetBytes(saltedPassword);
        byte[] hashBytes = SHA512.HashData(passwordBytes);

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
