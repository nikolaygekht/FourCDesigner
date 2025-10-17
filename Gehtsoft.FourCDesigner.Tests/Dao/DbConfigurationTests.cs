using Gehtsoft.FourCDesigner.Dao;
using Microsoft.Extensions.Configuration;
using Xunit;
using FluentAssertions;

namespace Gehtsoft.FourCDesigner.Tests.Dao;

public class DbConfigurationTests
{
    [Fact]
    public void Constructor_WithNullConfiguration_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new DbConfiguration(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("configuration");
    }

    [Fact]
    public void Driver_WithConfiguredValue_ReturnsValue()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            ["db:driver"] = "sqlite"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();
        var dbConfig = new DbConfiguration(configuration);

        // Act
        string driver = dbConfig.Driver;

        // Assert
        driver.Should().Be("sqlite");
    }

    [Fact]
    public void Driver_WithMissingValue_ThrowsInvalidOperationException()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        var dbConfig = new DbConfiguration(configuration);

        // Act
        Action act = () => { var _ = dbConfig.Driver; };

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*driver not configured*");
    }

    [Fact]
    public void ConnectionString_WithConfiguredValue_ReturnsValue()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            ["db:connectionString"] = "Data Source=test.db"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();
        var dbConfig = new DbConfiguration(configuration);

        // Act
        string connectionString = dbConfig.ConnectionString;

        // Assert
        connectionString.Should().Be("Data Source=test.db");
    }

    [Fact]
    public void ConnectionString_WithMissingValue_ThrowsInvalidOperationException()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        var dbConfig = new DbConfiguration(configuration);

        // Act
        Action act = () => { var _ = dbConfig.ConnectionString; };

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*connection string not configured*");
    }

    [Fact]
    public void Configuration_WithBothValues_ReturnsCorrectValues()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            ["db:driver"] = "npgsql",
            ["db:connectionString"] = "Host=localhost;Database=testdb"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();
        var dbConfig = new DbConfiguration(configuration);

        // Act & Assert
        dbConfig.Driver.Should().Be("npgsql");
        dbConfig.ConnectionString.Should().Be("Host=localhost;Database=testdb");
    }
}
