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
        /// Header name for client identifier.
        /// </summary>
        private const string ClientIdHeaderName = "X-ClientId";

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
            // 1. Try X-ClientId header first (explicit client identification)
            if (context.Request.Headers.TryGetValue(ClientIdHeaderName, out var clientIdValues))
            {
                var clientId = clientIdValues.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(clientId))
                {
                    return $"client:{clientId}";
                }
            }

            // 2. Try X-Forwarded-For header (proxy/reverse proxy scenarios)
            // X-Forwarded-For format: "client, proxy1, proxy2"
            // We want the original client IP (first in the chain)
            if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedForValues))
            {
                var forwardedFor = forwardedForValues.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(forwardedFor))
                {
                    // Take the first IP in the chain (original client)
                    var clientIp = forwardedFor.Split(',')[0].Trim();
                    if (!string.IsNullOrWhiteSpace(clientIp))
                    {
                        return $"ip:{clientIp}";
                    }
                }
            }

            // 3. Fallback to RemoteIpAddress (direct connection)
            var remoteIp = context.Connection.RemoteIpAddress?.ToString();
            if (!string.IsNullOrWhiteSpace(remoteIp))
            {
                return $"ip:{remoteIp}";
            }

            // 4. Ultimate fallback (should rarely happen)
            return "unknown";
        }

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

            // Register reset service as singleton
            services.AddSingleton<IThrottleResetService, ThrottleResetService>();

            // Add ASP.NET Core rate limiting
            services.AddRateLimiter(options =>
            {
                // Configure default throttle policy using FixedWindow strategy
                // Configuration is read dynamically per request to support hot-reload
                options.AddPolicy(DefaultThrottlePolicyName, context =>
                {
                    // Get configuration, reset service, and session controller from DI
                    var config = context.RequestServices.GetRequiredService<IThrottleConfiguration>();
                    var resetService = context.RequestServices.GetRequiredService<IThrottleResetService>();
                    var sessionController = context.RequestServices.GetService<ISessionController>();

                    // Get current reset generation to invalidate old partitions
                    var generation = resetService.Generation;

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

                        // Use email with generation as partition key for authorized users
                        var authorizedPartitionKey = $"{userEmail}:g{generation}";
                        return RateLimitPartition.GetFixedWindowLimiter(authorizedPartitionKey, key =>
                            new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = config.AuthorizedRequestsPerPeriod,
                                Window = TimeSpan.FromSeconds(config.PeriodInSeconds),
                                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                                QueueLimit = 0,
                                AutoReplenishment = true // Automatically replenish permits after window expires
                            });
                    }

                    // Handle non-authorized users
                    // If throttling is disabled for non-authorized users, don't limit
                    if (!config.ThrottlingEnabled)
                    {
                        return RateLimitPartition.GetNoLimiter<string>("no-limit");
                    }

                    // Extract client identifier using fallback chain: X-ClientId → X-Forwarded-For → RemoteIpAddress
                    var clientId = GetClientIdentifier(context);

                    // Append generation to partition key to invalidate old partitions on reset
                    var partitionKey = $"{clientId}:g{generation}";

                    // Use fixed window rate limiter with current configuration values
                    return RateLimitPartition.GetFixedWindowLimiter(partitionKey, key =>
                        new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = config.DefaultRequestsPerPeriod,
                            Window = TimeSpan.FromSeconds(config.PeriodInSeconds),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0, // No queuing - reject immediately when limit is reached
                            AutoReplenishment = true // Automatically replenish permits after window expires
                        });
                });

                // Configure stricter email check policy to prevent enumeration attacks
                options.AddPolicy(EmailCheckThrottlePolicyName, context =>
                {
                    // Get configuration and reset service from DI
                    var config = context.RequestServices.GetRequiredService<IThrottleConfiguration>();
                    var resetService = context.RequestServices.GetRequiredService<IThrottleResetService>();

                    // Get current reset generation to invalidate old partitions
                    var generation = resetService.Generation;

                    // Extract client identifier using fallback chain: X-ClientId → X-Forwarded-For → RemoteIpAddress
                    var clientId = GetClientIdentifier(context);

                    // Append generation to partition key to invalidate old partitions on reset
                    var partitionKey = $"{clientId}:g{generation}";

                    // Use configuration-based limits
                    return RateLimitPartition.GetFixedWindowLimiter(partitionKey, key =>
                        new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = config.CheckEmailRequestsPerPeriod,
                            Window = TimeSpan.FromSeconds(config.PeriodInSeconds),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0,
                            AutoReplenishment = true // Automatically replenish permits after window expires
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
                    // Extract client identifier using fallback chain: X-ClientId → X-Forwarded-For → RemoteIpAddress
                    var clientId = GetClientIdentifier(context);

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
