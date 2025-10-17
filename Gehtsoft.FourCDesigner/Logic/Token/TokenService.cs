using Microsoft.Extensions.Caching.Memory;

namespace Gehtsoft.FourCDesigner.Logic.Token;

/// <summary>
/// Service for managing email verification and password reset tokens.
/// </summary>
public class TokenService : ITokenService, IDisposable
{
    private readonly ITokenServiceConfiguration mConfiguration;
    private readonly MemoryCache mCache;
    private readonly Random mRandom;

    /// <summary>
    /// Token data stored in cache.
    /// </summary>
    private class TokenData
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenService"/> class.
    /// </summary>
    /// <param name="configuration">The token service configuration.</param>
    /// <exception cref="ArgumentNullException">Thrown when configuration is null.</exception>
    public TokenService(ITokenServiceConfiguration configuration)
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        mConfiguration = configuration;
        mCache = new MemoryCache(new MemoryCacheOptions());
        mRandom = new Random();
    }

    /// <summary>
    /// Disposes the memory cache.
    /// </summary>
    public void Dispose()
    {
        mCache?.Dispose();
    }

    /// <inheritdoc/>
    public double ExpirationInSeconds => mConfiguration.ExpirationInSeconds;

    /// <inheritdoc/>
    public string GenerateToken(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentNullException(nameof(email));

        // Generate a random 6-digit token between 100000 and 999999
        int tokenNumber = mRandom.Next(100000, 1000000);
        string token = tokenNumber.ToString();

        // Create cache key using email to allow multiple tokens per email
        string cacheKey = $"token:{email}:{token}";

        // Store token data in cache with absolute expiration
        var tokenData = new TokenData
        {
            Email = email,
            Token = token
        };

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(mConfiguration.ExpirationInSeconds)
        };

        mCache.Set(cacheKey, tokenData, cacheOptions);

        return token;
    }

    /// <inheritdoc/>
    public bool ValidateToken(string token, string email, bool remove)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(email))
            return false;

        // Construct the cache key
        string cacheKey = $"token:{email}:{token}";

        // Try to get token data from cache
        bool isValid = false;
        if (mCache.TryGetValue<TokenData>(cacheKey, out var tokenData) && tokenData != null)
        {
            // Validate that the token matches and email matches
            isValid = tokenData.Token == token && tokenData.Email == email;

            // Remove token if requested, regardless of validity
            if (remove)
                mCache.Remove(cacheKey);
        }

        return isValid;
    }
}
