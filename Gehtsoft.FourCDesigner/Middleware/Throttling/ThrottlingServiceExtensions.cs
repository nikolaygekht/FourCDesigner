using Gehtsoft.FourCDesigner.Logic.Session;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace Gehtsoft.FourCDesigner.Middleware.Throttling
{
    /// <summary>
    /// Extension methods for registering throttling services using ASP.NET Core's built-in rate limiting.
    /// Supports separate throttling limits for authorized and non-authorized users.
    /// </summary>
    public static class ThrottlingServiceExtensions
    {
        /// <summary>
        /// Policy name for the default throttling policy.
        /// </summary>
        public const string DefaultThrottlePolicyName = "DefaultThrottle";

        /// <summary>
        /// Policy name for email check throttling policy (stricter to prevent enumeration).
        /// </summary>
        public const string EmailCheckThrottlePolicyName = "EmailCheckThrottle";

        /// <summary>
        /// Header name for session ID.
        /// </summary>
        private const string SessionHeaderName = "X-fourc-session";

        /// <summary>
        /// Adds throttling services to the dependency injection container.
        /// Uses ASP.NET Core's built-in rate limiting with configuration-based settings.
        /// Supports separate limits for authorized and non-authorized users.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddThrottling(this IServiceCollection services)
        {
            // Register configuration as singleton
            services.AddSingleton<IThrottleConfiguration, ThrottleConfiguration>();

            // Add ASP.NET Core rate limiting
            services.AddRateLimiter(options =>
            {
                // Configure default throttle policy using FixedWindow strategy
                // Configuration is read dynamically per request to support hot-reload
                options.AddPolicy(DefaultThrottlePolicyName, context =>
                {
                    // Get configuration and session controller from DI
                    var config = context.RequestServices.GetRequiredService<IThrottleConfiguration>();
                    var sessionController = context.RequestServices.GetService<ISessionController>();

                    // Check if request has a valid session
                    string? sessionId = null;
                    string? userEmail = null;
                    bool isAuthorized = false;

                    if (sessionController != null &&
                        context.Request.Headers.TryGetValue(SessionHeaderName, out var sessionIdValues))
                    {
                        sessionId = sessionIdValues.FirstOrDefault();
                        if (!string.IsNullOrEmpty(sessionId))
                        {
                            isAuthorized = sessionController.CheckSession(sessionId, out userEmail, out _);
                        }
                    }

                    // Handle authorized users
                    if (isAuthorized && !string.IsNullOrEmpty(userEmail))
                    {
                        // If throttling is disabled for authorized users, don't limit
                        if (!config.AuthorizedThrottlingEnabled)
                        {
                            return RateLimitPartition.GetNoLimiter<string>("authorized-no-limit");
                        }

                        // Use email as partition key for authorized users
                        return RateLimitPartition.GetFixedWindowLimiter(userEmail, key =>
                            new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = config.AuthorizedRequestsPerPeriod,
                                Window = TimeSpan.FromSeconds(config.AuthorizedPeriodInSeconds),
                                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                                QueueLimit = 0
                            });
                    }

                    // Handle non-authorized users
                    // If throttling is disabled for non-authorized users, don't limit
                    if (!config.ThrottlingEnabled)
                    {
                        return RateLimitPartition.GetNoLimiter<string>("no-limit");
                    }

                    // Extract client identifier (IP address) for non-authorized users
                    var clientId = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                    // Use fixed window rate limiter with current configuration values
                    return RateLimitPartition.GetFixedWindowLimiter(clientId, key =>
                        new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = config.DefaultRequestsPerPeriod,
                            Window = TimeSpan.FromSeconds(config.PeriodInSeconds),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0 // No queuing - reject immediately when limit is reached
                        });
                });

                // Configure stricter email check policy to prevent enumeration attacks
                options.AddPolicy(EmailCheckThrottlePolicyName, context =>
                {
                    // Use IP address as partition key for email checking
                    var clientId = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                    // Stricter limits: 10 requests per 60 seconds
                    return RateLimitPartition.GetFixedWindowLimiter(clientId, key =>
                        new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 10,
                            Window = TimeSpan.FromSeconds(60),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        });
                });

                // Configure rejection behavior
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            });

            return services;
        }

        /// <summary>
        /// Adds a custom named throttle policy with specific settings.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="policyName">The name of the policy.</param>
        /// <param name="permitLimit">Maximum number of requests allowed in the time window.</param>
        /// <param name="windowSeconds">Time window in seconds.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddThrottlePolicy(this IServiceCollection services,
            string policyName,
            int permitLimit,
            double windowSeconds)
        {
            services.AddRateLimiter(options =>
            {
                options.AddPolicy(policyName, context =>
                {
                    var clientId = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                    return RateLimitPartition.GetFixedWindowLimiter(clientId, _ =>
                        new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = permitLimit,
                            Window = TimeSpan.FromSeconds(windowSeconds),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        });
                });
            });

            return services;
        }
    }
}
