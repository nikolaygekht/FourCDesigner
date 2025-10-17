using FluentAssertions;
using Gehtsoft.FourCDesigner.Middleware.Throttling;
using Microsoft.Extensions.Configuration;

namespace Gehtsoft.FourCDesigner.Tests.Middleware.Throttling
{
    public class ThrottleConfigurationTests
    {
        [Fact]
        public void ThrottlingEnabled_ShouldReadFromConfiguration()
        {
            // Arrange
            var configData = new Dictionary<string, string?>
            {
                { "application:throttle:enabled", "false" }
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            // Act
            var throttleConfig = new ThrottleConfiguration(configuration);

            // Assert
            throttleConfig.ThrottlingEnabled.Should().BeFalse();
        }

        [Fact]
        public void ThrottlingEnabled_ShouldDefaultToTrue_WhenNotConfigured()
        {
            // Arrange
            var configuration = new ConfigurationBuilder().Build();

            // Act
            var throttleConfig = new ThrottleConfiguration(configuration);

            // Assert
            throttleConfig.ThrottlingEnabled.Should().BeTrue();
        }

        [Fact]
        public void DefaultRequestsPerPeriod_ShouldReadFromConfiguration()
        {
            // Arrange
            var configData = new Dictionary<string, string?>
            {
                { "application:throttle:defaultRequestsPerPeriod", "5" }
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            // Act
            var throttleConfig = new ThrottleConfiguration(configuration);

            // Assert
            throttleConfig.DefaultRequestsPerPeriod.Should().Be(5);
        }

        [Fact]
        public void DefaultRequestsPerPeriod_ShouldDefaultToOne_WhenNotConfigured()
        {
            // Arrange
            var configuration = new ConfigurationBuilder().Build();

            // Act
            var throttleConfig = new ThrottleConfiguration(configuration);

            // Assert
            throttleConfig.DefaultRequestsPerPeriod.Should().Be(1);
        }

        [Fact]
        public void PeriodInSeconds_ShouldReadFromConfiguration()
        {
            // Arrange
            var configData = new Dictionary<string, string?>
            {
                { "application:throttle:periodInSeconds", "10.5" }
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            // Act
            var throttleConfig = new ThrottleConfiguration(configuration);

            // Assert
            throttleConfig.PeriodInSeconds.Should().Be(10.5);
        }

        [Fact]
        public void PeriodInSeconds_ShouldDefaultToOne_WhenNotConfigured()
        {
            // Arrange
            var configuration = new ConfigurationBuilder().Build();

            // Act
            var throttleConfig = new ThrottleConfiguration(configuration);

            // Assert
            throttleConfig.PeriodInSeconds.Should().Be(1.0);
        }

        [Fact]
        public void Configuration_ShouldReadAllSettings_WhenAllProvided()
        {
            // Arrange
            var configData = new Dictionary<string, string?>
            {
                { "application:throttle:enabled", "true" },
                { "application:throttle:defaultRequestsPerPeriod", "1" },
                { "application:throttle:periodInSeconds", "1.0" }
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            // Act
            var throttleConfig = new ThrottleConfiguration(configuration);

            // Assert
            throttleConfig.ThrottlingEnabled.Should().BeTrue();
            throttleConfig.DefaultRequestsPerPeriod.Should().Be(1);
            throttleConfig.PeriodInSeconds.Should().Be(1.0);
        }

        [Fact]
        public void AuthorizedThrottlingEnabled_ShouldReadFromConfiguration()
        {
            // Arrange
            var configData = new Dictionary<string, string?>
            {
                { "application:throttle:authorized:enabled", "true" }
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            // Act
            var throttleConfig = new ThrottleConfiguration(configuration);

            // Assert
            throttleConfig.AuthorizedThrottlingEnabled.Should().BeTrue();
        }

        [Fact]
        public void AuthorizedThrottlingEnabled_ShouldDefaultToFalse_WhenNotConfigured()
        {
            // Arrange
            var configuration = new ConfigurationBuilder().Build();

            // Act
            var throttleConfig = new ThrottleConfiguration(configuration);

            // Assert
            throttleConfig.AuthorizedThrottlingEnabled.Should().BeFalse();
        }

        [Fact]
        public void AuthorizedRequestsPerPeriod_ShouldReadFromConfiguration()
        {
            // Arrange
            var configData = new Dictionary<string, string?>
            {
                { "application:throttle:authorized:requestsPerPeriod", "50" }
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            // Act
            var throttleConfig = new ThrottleConfiguration(configuration);

            // Assert
            throttleConfig.AuthorizedRequestsPerPeriod.Should().Be(50);
        }

        [Fact]
        public void AuthorizedRequestsPerPeriod_ShouldDefaultTo100_WhenNotConfigured()
        {
            // Arrange
            var configuration = new ConfigurationBuilder().Build();

            // Act
            var throttleConfig = new ThrottleConfiguration(configuration);

            // Assert
            throttleConfig.AuthorizedRequestsPerPeriod.Should().Be(100);
        }

        [Fact]
        public void AuthorizedPeriodInSeconds_ShouldReadFromConfiguration()
        {
            // Arrange
            var configData = new Dictionary<string, string?>
            {
                { "application:throttle:authorized:periodInSeconds", "5.5" }
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            // Act
            var throttleConfig = new ThrottleConfiguration(configuration);

            // Assert
            throttleConfig.AuthorizedPeriodInSeconds.Should().Be(5.5);
        }

        [Fact]
        public void AuthorizedPeriodInSeconds_ShouldDefaultToOne_WhenNotConfigured()
        {
            // Arrange
            var configuration = new ConfigurationBuilder().Build();

            // Act
            var throttleConfig = new ThrottleConfiguration(configuration);

            // Assert
            throttleConfig.AuthorizedPeriodInSeconds.Should().Be(1.0);
        }

        [Fact]
        public void Configuration_ShouldReadAllAuthorizedSettings_WhenAllProvided()
        {
            // Arrange
            var configData = new Dictionary<string, string?>
            {
                { "application:throttle:enabled", "true" },
                { "application:throttle:defaultRequestsPerPeriod", "10" },
                { "application:throttle:periodInSeconds", "2.0" },
                { "application:throttle:authorized:enabled", "true" },
                { "application:throttle:authorized:requestsPerPeriod", "200" },
                { "application:throttle:authorized:periodInSeconds", "3.0" }
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            // Act
            var throttleConfig = new ThrottleConfiguration(configuration);

            // Assert
            throttleConfig.ThrottlingEnabled.Should().BeTrue();
            throttleConfig.DefaultRequestsPerPeriod.Should().Be(10);
            throttleConfig.PeriodInSeconds.Should().Be(2.0);
            throttleConfig.AuthorizedThrottlingEnabled.Should().BeTrue();
            throttleConfig.AuthorizedRequestsPerPeriod.Should().Be(200);
            throttleConfig.AuthorizedPeriodInSeconds.Should().Be(3.0);
        }
    }
}
