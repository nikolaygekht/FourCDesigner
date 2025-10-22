namespace Gehtsoft.FourCDesigner.Middleware.Throttling;

/// <summary>
/// Interface for throttle cache operations.
/// Provides methods to store and retrieve throttling data.
/// </summary>
public interface IThrottleCache
{
    /// <summary>
    /// Gets throttle data from the cache.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <returns>The throttle data if found; otherwise, null.</returns>
    ThrottleData? Get(string key);

    /// <summary>
    /// Sets throttle data in the cache with sliding expiration.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="data">The throttle data to store.</param>
    /// <param name="timeout">The timeout in milliseconds for sliding expiration.</param>
    void Set(string key, ThrottleData data, int timeout);

    /// <summary>
    /// Removes an entry from the cache.
    /// </summary>
    /// <param name="key">The cache key.</param>
    void Remove(string key);

    /// <summary>
    /// Resets the entire cache.
    /// </summary>
    void Reset();
}

/// <summary>
/// Represents throttling data stored in the cache.
/// </summary>
public class ThrottleData
{
    /// <summary>
    /// Gets or sets the timestamp of the last request.
    /// </summary>
    public DateTime LastRequest { get; set; }

    /// <summary>
    /// Gets or sets the count of requests within the current window.
    /// </summary>
    public int Count { get; set; }
}
