using Microsoft.AspNetCore.Mvc.Filters;

namespace Gehtsoft.FourCDesigner.Middleware.Throttling;

/// <summary>
/// Attribute to apply throttling to a controller action.
/// Uses a memory cache-based approach to track and limit request rates.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class ThrottleAttribute : Attribute, IFilterFactory
{
    private readonly int mTimeout;
    private readonly int mLimit;
    private readonly bool mPerClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThrottleAttribute"/> class.
    /// </summary>
    /// <param name="timeout">The timeout in milliseconds for the throttling window.</param>
    /// <param name="limit">The maximum number of requests allowed within the timeout period.</param>
    /// <param name="perClient">If true, throttling is per client; if false, throttling is global.</param>
    public ThrottleAttribute(int timeout, int limit, bool perClient)
    {
        mTimeout = timeout;
        mLimit = limit;
        mPerClient = perClient;
    }

    /// <inheritdoc/>
    public bool IsReusable => true;

    /// <inheritdoc/>
    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        if (serviceProvider == null)
            throw new ArgumentNullException(nameof(serviceProvider));

        var configuration = serviceProvider.GetService<IThrottleConfiguration>();
        var logger = serviceProvider.GetService<ILogger<ThrottleFilter>>();
        var cache = serviceProvider.GetService<IThrottleCache>();

        return new ThrottleFilter(mTimeout, mLimit, mPerClient, configuration, logger, cache);
    }
}
