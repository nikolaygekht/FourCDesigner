using FluentAssertions;
using Gehtsoft.FourCDesigner.Middleware.Throttling;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gehtsoft.FourCDesigner.Tests.Middleware.Throttling
{
    public class ThrottlingIntegrationTests
    {
        [Fact]
        public void AddThrottling_ShouldRegisterConfiguration()
        {
            // Arrange
            var services = new ServiceCollection();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "application:throttle:enabled", "true" },
                    { "application:throttle:defaultRequestsPerPeriod", "10" },
                    { "application:throttle:periodInSeconds", "1.0" }
                })
                .Build();
            services.AddSingleton<IConfiguration>(config);

            // Act
            services.AddThrottling();
            var serviceProvider = services.BuildServiceProvider();
            var throttleConfig = serviceProvider.GetService<IThrottleConfiguration>();

            // Assert
            throttleConfig.Should().NotBeNull();
            throttleConfig!.ThrottlingEnabled.Should().BeTrue();
            throttleConfig.DefaultRequestsPerPeriod.Should().Be(10);
            throttleConfig.PeriodInSeconds.Should().Be(1.0);
        }

        [Fact]
        public void AddThrottling_ShouldRegisterRateLimiter()
        {
            // Arrange
            var services = new ServiceCollection();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "application:throttle:enabled", "true" },
                    { "application:throttle:defaultRequestsPerPeriod", "5" },
                    { "application:throttle:periodInSeconds", "0.1" }
                })
                .Build();
            services.AddSingleton<IConfiguration>(config);

            // Act
            services.AddThrottling();
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var rateLimiterOptions = serviceProvider.GetService<Microsoft.Extensions.Options.IOptions<RateLimiterOptions>>();
            rateLimiterOptions.Should().NotBeNull("rate limiter should be registered");
        }

        [Fact]
        public void AddThrottlePolicy_ShouldRegisterCustomPolicy()
        {
            // Arrange
            var services = new ServiceCollection();
            var config = new ConfigurationBuilder().Build();
            services.AddSingleton<IConfiguration>(config);

            // Act
            services.AddThrottlePolicy("CustomPolicy", permitLimit: 100, windowSeconds: 60);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var rateLimiterOptions = serviceProvider.GetService<Microsoft.Extensions.Options.IOptions<RateLimiterOptions>>();
            rateLimiterOptions.Should().NotBeNull("custom policy should be registered");
        }

        [Fact]
        public void DefaultThrottlePolicyName_ShouldBeAccessible()
        {
            // Assert
            ThrottlingServiceExtensions.DefaultThrottlePolicyName.Should().Be("DefaultThrottle");
        }

        [Fact]
        public void EnableRateLimitingAttribute_CanBeUsedWithDefaultPolicy()
        {
            // Arrange & Act
            var attribute = new EnableRateLimitingAttribute(ThrottlingServiceExtensions.DefaultThrottlePolicyName);

            // Assert
            attribute.PolicyName.Should().Be("DefaultThrottle");
        }

        [Fact]
        public void AddThrottling_WithAuthorizedSettings_ShouldRegisterConfiguration()
        {
            // Arrange
            var services = new ServiceCollection();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "application:throttle:enabled", "true" },
                    { "application:throttle:defaultRequestsPerPeriod", "10" },
                    { "application:throttle:periodInSeconds", "1.0" },
                    { "application:throttle:authorized:enabled", "true" },
                    { "application:throttle:authorized:requestsPerPeriod", "100" },
                    { "application:throttle:authorized:periodInSeconds", "2.0" }
                })
                .Build();
            services.AddSingleton<IConfiguration>(config);

            // Act
            services.AddThrottling();
            var serviceProvider = services.BuildServiceProvider();
            var throttleConfig = serviceProvider.GetService<IThrottleConfiguration>();

            // Assert
            throttleConfig.Should().NotBeNull();
            throttleConfig!.ThrottlingEnabled.Should().BeTrue();
            throttleConfig.DefaultRequestsPerPeriod.Should().Be(10);
            throttleConfig.PeriodInSeconds.Should().Be(1.0);
            throttleConfig.AuthorizedThrottlingEnabled.Should().BeTrue();
            throttleConfig.AuthorizedRequestsPerPeriod.Should().Be(100);
        }

        [Fact]
        public void AddThrottling_WithAuthorizedDisabled_ShouldUseDefaults()
        {
            // Arrange
            var services = new ServiceCollection();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "application:throttle:enabled", "true" },
                    { "application:throttle:defaultRequestsPerPeriod", "5" },
                    { "application:throttle:periodInSeconds", "1.0" }
                })
                .Build();
            services.AddSingleton<IConfiguration>(config);

            // Act
            services.AddThrottling();
            var serviceProvider = services.BuildServiceProvider();
            var throttleConfig = serviceProvider.GetService<IThrottleConfiguration>();

            // Assert
            throttleConfig.Should().NotBeNull();
            throttleConfig!.AuthorizedThrottlingEnabled.Should().BeFalse("authorized throttling should be disabled by default");
            throttleConfig.AuthorizedRequestsPerPeriod.Should().Be(100, "should use default value");
        }
    }
}
