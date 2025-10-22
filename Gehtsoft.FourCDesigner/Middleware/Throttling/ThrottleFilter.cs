using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Gehtsoft.FourCDesigner.Middleware.Throttling;

/// <summary>
/// Filter that enforces throttling by tracking request counts and timestamps.
/// Uses a memory cache to store throttling data with sliding expiration.
/// </summary>
internal class ThrottleFilter : IAsyncActionFilter
{
    private readonly int mTimeout;
    private readonly int mLimit;
    private readonly bool mPerClient;
    private readonly IThrottleConfiguration? mConfiguration;
    private readonly ILogger<ThrottleFilter>? mLogger;
    private readonly IThrottleCache? mCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThrottleFilter"/> class.
    /// </summary>
    /// <param name="timeout">The timeout in milliseconds for the throttling window.</param>
    /// <param name="limit">The maximum number of requests allowed within the timeout period.</param>
    /// <param name="perClient">If true, throttling is per client; if false, throttling is global.</param>
    /// <param name="configuration">The throttle configuration (optional, used to check if throttling is enabled).</param>
    /// <param name="logger">The logger (optional).</param>
    /// <param name="cache">The throttle cache (optional, if null throttling is disabled).</param>
    public ThrottleFilter(
        int timeout,
        int limit,
        bool perClient,
        IThrottleConfiguration? configuration,
        ILogger<ThrottleFilter>? logger,
        IThrottleCache? cache)
    {
        mTimeout = timeout;
        mLimit = limit;
        mPerClient = perClient;
        mConfiguration = configuration;
        mLogger = logger;
        mCache = cache;
    }

    /// <inheritdoc/>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        if (next == null)
            throw new ArgumentNullException(nameof(next));

        // Check if throttling is enabled in configuration
        bool enabled = mConfiguration?.ThrottlingEnabled ?? true;

        // If throttling is disabled or cache is not available, skip throttling
        if (!enabled || mCache == null)
        {
            await next();
            return;
        }

        // Build throttle key
        string path = $"{context.RouteData.Values["controller"]}.{context.RouteData.Values["action"]}";
        string key;

        if (mPerClient)
        {
            // Extract client identifier using fallback chain: X-ClientId → X-Forwarded-For → RemoteIpAddress
            string clientId = GetClientIdentifier(context.HttpContext);
            key = $"{path}:{clientId}";
        }
        else
        {
            // Global throttling - same key for all clients
            key = path;
        }

        // Get current throttle data from cache
        ThrottleData? data = mCache.Get(key);

        if (data != null)
        {
            // Update count and timestamp
            data.Count++;
            data.LastRequest = DateTime.UtcNow;
        }
        else
        {
            // First request in this window
            data = new ThrottleData
            {
                LastRequest = DateTime.UtcNow,
                Count = 1
            };
        }

        // Always update cache with new count (even if we're about to throttle)
        // This ensures the counter increases for repeated throttled requests
        mCache.Set(key, data, mTimeout);

        // Check if limit is exceeded
        if (data.Count > mLimit)
        {
            // Only log on the first throttled request to avoid log spam
            if (data.Count == mLimit + 1)
            {
                mLogger?.LogWarning(
                    "Throttled request to {Path} from {Address} (limit: {Limit})",
                    context.HttpContext.Request.Path,
                    context.HttpContext.Connection.RemoteIpAddress,
                    mLimit);
            }

            context.Result = new StatusCodeResult(429);
            return;
        }

        await next();
    }

    /// <summary>
    /// Extracts the client identifier from the HTTP context using a fallback chain:
    /// 1. X-ClientId header (for mobile apps, testing)
    /// 2. X-Forwarded-For header (for proxy/NAT scenarios)
    /// 3. RemoteIpAddress (direct connection)
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>The client identifier string.</returns>
    private static string GetClientIdentifier(HttpContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        // 1. Try X-ClientId header first (explicit client identification)
        if (context.Request.Headers.TryGetValue("X-ClientId", out var clientIdValues))
        {
            var clientId = clientIdValues.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(clientId))
                return $"client:{clientId}";
        }

        // 2. Try X-Forwarded-For header (proxy/reverse proxy scenarios)
        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedForValues))
        {
            var forwardedFor = forwardedForValues.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(forwardedFor))
            {
                // Take the first IP in the chain (original client)
                var clientIp = forwardedFor.Split(',')[0].Trim();
                if (!string.IsNullOrWhiteSpace(clientIp))
                    return $"ip:{clientIp}";
            }
        }

        // 3. Fallback to RemoteIpAddress (direct connection)
        var remoteIp = context.Connection.RemoteIpAddress?.ToString();
        if (!string.IsNullOrWhiteSpace(remoteIp))
            return $"ip:{remoteIp}";

        // 4. Ultimate fallback (should rarely happen)
        return "unknown";
    }
}
