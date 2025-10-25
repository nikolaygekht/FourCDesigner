using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Gehtsoft.FourCDesigner.Logic.Config;

namespace Gehtsoft.FourCDesigner.Tests.Config;

/// <summary>
/// Tests for UrlBuilder to ensure deployment flexibility across different configurations.
/// </summary>
public class UrlBuilderTests
{
    #region Test Helpers

    /// <summary>
    /// Creates a SystemConfig with default values for testing.
    /// </summary>
    private static SystemConfig CreateDefaultSystemConfig()
    {
        var configData = new Dictionary<string, string>
        {
            ["system:routing:externalProtocol"] = "https",
            ["system:routing:externalHost"] = "example.com",
            ["system:routing:externalPort"] = "443",
            ["system:routing:externalPrefix"] = "",
            ["system:routing:useUrls"] = "http://localhost:5000"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();
        return new SystemConfig(configuration);
    }

    /// <summary>
    /// Creates a SystemConfig with specific values for testing.
    /// </summary>
    private static SystemConfig CreateSystemConfig(string protocol, string host, int port, string prefix)
    {
        var configData = new Dictionary<string, string>
        {
            ["system:routing:externalProtocol"] = protocol,
            ["system:routing:externalHost"] = host,
            ["system:routing:externalPort"] = port.ToString(),
            ["system:routing:externalPrefix"] = prefix,
            ["system:routing:useUrls"] = "http://localhost:5000"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();
        return new SystemConfig(configuration);
    }

    #endregion

    #region 1.1 Basic URL Construction Tests

    [Theory]
    [InlineData("http", "localhost", 80, "", "/activate-account",
        "http://localhost/activate-account")]
    [InlineData("https", "localhost", 443, "", "/activate-account",
        "https://localhost/activate-account")]
    [InlineData("http", "example.com", 8080, "", "/activate-account",
        "http://example.com:8080/activate-account")]
    [InlineData("https", "example.com", 8443, "", "/activate-account",
        "https://example.com:8443/activate-account")]
    public void BuildUrl_WithVariousConfigurations_GeneratesCorrectUrl(
        string protocol, string host, int port, string prefix, string path, string expected)
    {
        // Arrange: Create mock SystemConfig with specific values
        var systemConfig = CreateSystemConfig(protocol, host, port, prefix);
        var urlBuilder = new UrlBuilder(systemConfig, NullLogger<UrlBuilder>.Instance);

        // Act
        var result = urlBuilder.BuildUrl(path);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region 1.2 Port Handling Tests

    [Theory]
    [InlineData("http", 80, false)]  // Default HTTP port - omit from URL
    [InlineData("http", 8080, true)] // Non-default HTTP port - include in URL
    [InlineData("https", 443, false)] // Default HTTPS port - omit from URL
    [InlineData("https", 8443, true)] // Non-default HTTPS port - include in URL
    public void BuildUrl_WithPortConfiguration_HandlesDefaultPortsCorrectly(
        string protocol, int port, bool shouldIncludePort)
    {
        // Arrange
        var systemConfig = CreateSystemConfig(protocol, "example.com", port, "");
        var urlBuilder = new UrlBuilder(systemConfig, NullLogger<UrlBuilder>.Instance);

        // Act
        var result = urlBuilder.BuildUrl("/test");

        // Assert
        if (shouldIncludePort)
        {
            result.Should().Contain($":{port}");
        }
        else
        {
            result.Should().NotContain($":{port}");
        }
    }

    #endregion

    #region 1.3 Prefix Handling Tests

    [Theory]
    [InlineData("", "/activate", "https://example.com/activate")]
    [InlineData("/api", "/activate", "https://example.com/api/activate")]
    [InlineData("/4c", "/activate", "https://example.com/4c/activate")]
    [InlineData("/api/v1", "/activate", "https://example.com/api/v1/activate")]
    public void BuildUrl_WithVariousPrefixes_AppendsCorrectly(
        string prefix, string path, string expected)
    {
        // Arrange
        var systemConfig = CreateSystemConfig("https", "example.com", 443, prefix);
        var urlBuilder = new UrlBuilder(systemConfig, NullLogger<UrlBuilder>.Instance);

        // Act
        var result = urlBuilder.BuildUrl(path);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region 1.4 Query Parameter Tests

    [Fact]
    public void BuildUrl_WithQueryParameters_EncodesCorrectly()
    {
        // Arrange
        var systemConfig = CreateDefaultSystemConfig();
        var urlBuilder = new UrlBuilder(systemConfig, NullLogger<UrlBuilder>.Instance);

        // Act
        var result = urlBuilder.BuildUrl("/activate", new { email = "user@example.com", token = "abc123" });

        // Assert
        result.Should().Contain("?");
        result.Should().Contain("email=user%40example.com");
        result.Should().Contain("token=abc123");
        result.Should().Contain("&");
    }

    [Fact]
    public void BuildUrl_WithSpecialCharactersInQuery_EncodesCorrectly()
    {
        // Arrange
        var systemConfig = CreateDefaultSystemConfig();
        var urlBuilder = new UrlBuilder(systemConfig, NullLogger<UrlBuilder>.Instance);

        // Act
        var result = urlBuilder.BuildUrl("/test", new { text = "Hello & goodbye", param = "a=b" });

        // Assert
        result.Should().Contain("text=Hello+%26+goodbye");
        result.Should().Contain("param=a%3db");
    }

    [Fact]
    public void BuildUrl_WithNullQueryParameters_GeneratesUrlWithoutQuery()
    {
        // Arrange
        var systemConfig = CreateDefaultSystemConfig();
        var urlBuilder = new UrlBuilder(systemConfig, NullLogger<UrlBuilder>.Instance);

        // Act
        var result = urlBuilder.BuildUrl("/activate", null);

        // Assert
        result.Should().NotContain("?");
    }

    #endregion

    #region 1.5 Path Handling Tests

    [Theory]
    [InlineData("/activate-account")]  // With leading slash
    [InlineData("activate-account")]   // Without leading slash
    public void BuildUrl_WithVariousPathFormats_NormalizesCorrectly(string path)
    {
        // Arrange
        var systemConfig = CreateDefaultSystemConfig();
        var urlBuilder = new UrlBuilder(systemConfig, NullLogger<UrlBuilder>.Instance);

        // Act
        var result = urlBuilder.BuildUrl(path);

        // Assert
        result.Should().Contain("/activate-account");
        result.Should().NotContain("//activate-account");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void BuildUrl_WithNullOrEmptyPath_ThrowsArgumentException(string? path)
    {
        // Arrange
        var systemConfig = CreateDefaultSystemConfig();
        var urlBuilder = new UrlBuilder(systemConfig, NullLogger<UrlBuilder>.Instance);

        // Act
        Action act = () => urlBuilder.BuildUrl(path!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("relativePath");
    }

    #endregion

    #region 1.6 Real-World Deployment Scenarios

    [Theory]
    [MemberData(nameof(GetDeploymentScenarios))]
    public void BuildUrl_ForRealDeploymentScenarios_GeneratesCorrectUrls(
        string protocol, string host, int port, string prefix,
        string path, object? query, string expectedUrl)
    {
        // Arrange
        var systemConfig = CreateSystemConfig(protocol, host, port, prefix);
        var urlBuilder = new UrlBuilder(systemConfig, NullLogger<UrlBuilder>.Instance);

        // Act
        var result = urlBuilder.BuildUrl(path, query);

        // Assert
        result.Should().Be(expectedUrl);
    }

    public static IEnumerable<object?[]> GetDeploymentScenarios() =>
        new List<object?[]>
        {
            // Local development
            new object?[] {
                "http", "localhost", 5000, "",
                "/activate-account",
                new { email = "user@test.com", token = "123456" },
                "http://localhost:5000/activate-account?email=user%40test.com&token=123456"
            },
            // Production
            new object?[] {
                "https", "4cdesigner.com", 443, "",
                "/reset-password",
                new { email = "user@test.com", token = "654321" },
                "https://4cdesigner.com/reset-password?email=user%40test.com&token=654321"
            },
            // Behind reverse proxy with prefix
            new object?[] {
                "https", "example.com", 443, "/4c",
                "/activate-account",
                new { email = "admin@test.com", token = "abc123" },
                "https://example.com/4c/activate-account?email=admin%40test.com&token=abc123"
            },
            // Custom port deployment
            new object?[] {
                "https", "server.local", 8443, "/api",
                "/activate-account",
                new { email = "dev@test.com", token = "xyz789" },
                "https://server.local:8443/api/activate-account?email=dev%40test.com&token=xyz789"
            },
            // Without query parameters
            new object?[] {
                "https", "example.com", 443, "",
                "/activate-account",
                null,
                "https://example.com/activate-account"
            }
        };

    #endregion
}
