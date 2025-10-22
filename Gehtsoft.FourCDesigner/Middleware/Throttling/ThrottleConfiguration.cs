using Microsoft.Extensions.Configuration;

namespace Gehtsoft.FourCDesigner.Middleware.Throttling
{
    /// <summary>
    /// Implementation of throttle configuration that reads from IConfiguration.
    /// Supports separate limits for authorized and non-authorized users.
    /// </summary>
    internal class ThrottleConfiguration : IThrottleConfiguration
    {
        private readonly IConfiguration _configuration;

        public ThrottleConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <inheritdoc/>
        public bool ThrottlingEnabled =>
            _configuration.GetValue<bool>("application:throttle:enabled", true);

        /// <inheritdoc/>
        public int DefaultRequestsPerPeriod =>
            _configuration.GetValue<int>("application:throttle:defaultRequestsPerPeriod", 1);

        /// <inheritdoc/>
        public int CheckEmailRequestsPerPeriod =>
            _configuration.GetValue<int>("application:throttle:checkEmailRequestsPerPeriod", 10);

        /// <inheritdoc/>
        public double PeriodInSeconds =>
            _configuration.GetValue<double>("application:throttle:periodInSeconds", 60.0);

        /// <inheritdoc/>
        public bool AuthorizedThrottlingEnabled =>
            _configuration.GetValue<bool>("application:throttle:authorized:enabled", false);

        /// <inheritdoc/>
        public int AuthorizedRequestsPerPeriod =>
            _configuration.GetValue<int>("application:throttle:authorized:requestsPerPeriod", 100);
    }
}
