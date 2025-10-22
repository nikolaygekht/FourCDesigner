using Microsoft.Extensions.Caching.Memory;

namespace Gehtsoft.FourCDesigner.Middleware.Throttling;

/// <summary>
/// Implementation of throttle cache using MemoryCache.
/// Stores throttling data with sliding expiration.
/// </summary>
public class ThrottleCache : IThrottleCache
{
    private IMemoryCache mCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThrottleCache"/> class.
    /// </summary>
    /// <param name="cache">The memory cache instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when cache is null.</exception>
    public ThrottleCache()
    {
        mCache = new MemoryCache(new MemoryCacheOptions());
    }

    /// <inheritdoc/>
    public ThrottleData? Get(string key)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        return mCache.Get<ThrottleData>(key);
    }

    /// <inheritdoc/>
    public void Set(string key, ThrottleData data, int timeout)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        if (data == null)
            throw new ArgumentNullException(nameof(data));

        var options = new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMilliseconds(timeout)
        };

        mCache.Set(key, data, options);
    }

    /// <inheritdoc/>
    public void Remove(string key)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        mCache.Remove(key);
    }

    /// <inheritdoc/>
    public void Reset()
    {
        mCache = new MemoryCache(new MemoryCacheOptions());
    }
}
