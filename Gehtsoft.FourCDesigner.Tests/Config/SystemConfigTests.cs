using Gehtsoft.FourCDesigner.Logic.Config;
using Microsoft.Extensions.Configuration;
using Xunit;
using FluentAssertions;

namespace Gehtsoft.FourCDesigner.Tests.Config;

/// <summary>
/// Unit tests for SystemConfig class.
/// </summary>
public class SystemConfigTests
{
    [Fact]
    public void Constructor_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new SystemConfig(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("configuration");
    }

    [Fact]
    public void ExternalProtocol_WithDefaultConfiguration_ShouldReturnHttps()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        var systemConfig = new SystemConfig(configuration);

        // Act
        string protocol = systemConfig.ExternalProtocol;

        // Assert
        protocol.Should().Be("https");
    }

    [Fact]
    public void ExternalHost_WithDefaultConfiguration_ShouldReturnLocalhost()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        var systemConfig = new SystemConfig(configuration);

        // Act
        string host = systemConfig.ExternalHost;

        // Assert
        host.Should().Be("localhost");
    }

    [Fact]
    public void ExternalPort_WithDefaultConfiguration_ShouldReturn443()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        var systemConfig = new SystemConfig(configuration);

        // Act
        int port = systemConfig.ExternalPort;

        // Assert
        port.Should().Be(443);
    }

    [Fact]
    public void ExternalPrefix_WithDefaultConfiguration_ShouldReturnEmptyString()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        var systemConfig = new SystemConfig(configuration);

        // Act
        string prefix = systemConfig.ExternalPrefix;

        // Assert
        prefix.Should().Be(string.Empty);
    }

    [Fact]
    public void AllProperties_WithCustomConfiguration_ShouldReturnCustomValues()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            ["system:routing:externalProtocol"] = "http",
            ["system:routing:externalHost"] = "example.com",
            ["system:routing:externalPort"] = "8080",
            ["system:routing:externalPrefix"] = "/api"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();
        var systemConfig = new SystemConfig(configuration);

        // Act & Assert
        systemConfig.ExternalProtocol.Should().Be("http");
        systemConfig.ExternalHost.Should().Be("example.com");
        systemConfig.ExternalPort.Should().Be(8080);
        systemConfig.ExternalPrefix.Should().Be("/api");
    }

    [Fact]
    public void ExternalPort_WithInvalidValue_ShouldReturnDefault()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            ["system:routing:externalPort"] = "invalid"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();
        var systemConfig = new SystemConfig(configuration);

        // Act
        int port = systemConfig.ExternalPort;

        // Assert
        port.Should().Be(443);
    }

    [Fact]
    public void ExternalPort_WithZeroValue_ShouldReturnDefault()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            ["system:routing:externalPort"] = "0"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();
        var systemConfig = new SystemConfig(configuration);

        // Act
        int port = systemConfig.ExternalPort;

        // Assert
        port.Should().Be(443);
    }

    [Fact]
    public void ExternalPort_WithNegativeValue_ShouldReturnDefault()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            ["system:routing:externalPort"] = "-1"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();
        var systemConfig = new SystemConfig(configuration);

        // Act
        int port = systemConfig.ExternalPort;

        // Assert
        port.Should().Be(443);
    }

    [Fact]
    public void ExternalPort_WithValueAbove65535_ShouldReturnDefault()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            ["system:routing:externalPort"] = "65536"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();
        var systemConfig = new SystemConfig(configuration);

        // Act
        int port = systemConfig.ExternalPort;

        // Assert
        port.Should().Be(443);
    }

    [Fact]
    public void ExternalPort_WithValidEdgeCases_ShouldReturnValues()
    {
        // Test port 1 (minimum valid)
        var configData1 = new Dictionary<string, string>
        {
            ["system:routing:externalPort"] = "1"
        };
        var configuration1 = new ConfigurationBuilder()
            .AddInMemoryCollection(configData1!)
            .Build();
        var systemConfig1 = new SystemConfig(configuration1);
        systemConfig1.ExternalPort.Should().Be(1);

        // Test port 65535 (maximum valid)
        var configData2 = new Dictionary<string, string>
        {
            ["system:routing:externalPort"] = "65535"
        };
        var configuration2 = new ConfigurationBuilder()
            .AddInMemoryCollection(configData2!)
            .Build();
        var systemConfig2 = new SystemConfig(configuration2);
        systemConfig2.ExternalPort.Should().Be(65535);
    }

    [Fact]
    public void ExternalPrefix_WithoutLeadingSlash_ShouldAddSlash()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            ["system:routing:externalPrefix"] = "api"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();
        var systemConfig = new SystemConfig(configuration);

        // Act
        string prefix = systemConfig.ExternalPrefix;

        // Assert
        prefix.Should().Be("/api");
    }

    [Fact]
    public void ExternalPrefix_WithLeadingSlash_ShouldNotDoubleSlash()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            ["system:routing:externalPrefix"] = "/api"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();
        var systemConfig = new SystemConfig(configuration);

        // Act
        string prefix = systemConfig.ExternalPrefix;

        // Assert
        prefix.Should().Be("/api");
    }

    [Fact]
    public void ExternalPrefix_WithEmptyString_ShouldReturnEmptyString()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            ["system:routing:externalPrefix"] = ""
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();
        var systemConfig = new SystemConfig(configuration);

        // Act
        string prefix = systemConfig.ExternalPrefix;

        // Assert
        prefix.Should().Be(string.Empty);
    }

    [Fact]
    public void AllProperties_WithPartialConfiguration_ShouldReturnDefaultsForMissing()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            ["system:routing:externalHost"] = "example.com",
            ["system:routing:externalPort"] = "8080"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();
        var systemConfig = new SystemConfig(configuration);

        // Act & Assert
        systemConfig.ExternalProtocol.Should().Be("https"); // Default
        systemConfig.ExternalHost.Should().Be("example.com"); // Configured
        systemConfig.ExternalPort.Should().Be(8080); // Configured
        systemConfig.ExternalPrefix.Should().Be(string.Empty); // Default
    }

    [Fact]
    public void ExternalProtocol_WithHttpsConfiguration_ShouldReturnHttps()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            ["system:routing:externalProtocol"] = "https"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();
        var systemConfig = new SystemConfig(configuration);

        // Act
        string protocol = systemConfig.ExternalProtocol;

        // Assert
        protocol.Should().Be("https");
    }

    [Fact]
    public void ExternalPrefix_WithComplexPath_ShouldHandleCorrectly()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            ["system:routing:externalPrefix"] = "api/v1"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();
        var systemConfig = new SystemConfig(configuration);

        // Act
        string prefix = systemConfig.ExternalPrefix;

        // Assert
        prefix.Should().Be("/api/v1");
    }

    [Fact]
    public void UseUrls_WithDefaultConfiguration_ShouldReturnDefaultValue()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        var systemConfig = new SystemConfig(configuration);

        // Act
        string useUrls = systemConfig.UseUrls;

        // Assert
        useUrls.Should().Be("http://localhost:5000");
    }

    [Fact]
    public void UseUrls_WithCustomConfiguration_ShouldReturnCustomValue()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            ["system:routing:useUrls"] = "https://0.0.0.0:8080"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();
        var systemConfig = new SystemConfig(configuration);

        // Act
        string useUrls = systemConfig.UseUrls;

        // Assert
        useUrls.Should().Be("https://0.0.0.0:8080");
    }

    [Fact]
    public void UseUrls_WithMultipleUrls_ShouldReturnAllUrls()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            ["system:routing:useUrls"] = "http://localhost:5000;https://localhost:5001"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();
        var systemConfig = new SystemConfig(configuration);

        // Act
        string useUrls = systemConfig.UseUrls;

        // Assert
        useUrls.Should().Be("http://localhost:5000;https://localhost:5001");
    }

    [Fact]
    public void UseUrls_WithEmptyString_ShouldReturnDefault()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            ["system:routing:useUrls"] = ""
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();
        var systemConfig = new SystemConfig(configuration);

        // Act
        string useUrls = systemConfig.UseUrls;

        // Assert
        useUrls.Should().Be("http://localhost:5000");
    }
}
