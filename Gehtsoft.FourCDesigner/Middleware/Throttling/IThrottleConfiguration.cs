namespace Gehtsoft.FourCDesigner.Middleware.Throttling
{
    /// <summary>
    /// Configuration interface for throttling settings.
    /// Supports separate limits for authorized and non-authorized users.
    /// </summary>
    public interface IThrottleConfiguration
    {
        /// <summary>
        /// Gets a value indicating whether throttling is enabled for non-authorized users.
        /// </summary>
        public bool ThrottlingEnabled { get; }

        /// <summary>
        /// Gets the default number of requests allowed per period for non-authorized users.
        /// </summary>
        public int DefaultRequestsPerPeriod { get; }

        /// <summary>
        /// Gets the throttling period in seconds for non-authorized users.
        /// </summary>
        public double PeriodInSeconds { get; }

        /// <summary>
        /// Gets a value indicating whether throttling is enabled for authorized users.
        /// If false, authorized users are not throttled.
        /// </summary>
        public bool AuthorizedThrottlingEnabled { get; }

        /// <summary>
        /// Gets the number of requests allowed per period for authorized users.
        /// </summary>
        public int AuthorizedRequestsPerPeriod { get; }

        /// <summary>
        /// Gets the throttling period in seconds for authorized users.
        /// </summary>
        public double AuthorizedPeriodInSeconds { get; }
    }
}
