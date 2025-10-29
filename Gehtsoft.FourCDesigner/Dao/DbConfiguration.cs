namespace Gehtsoft.FourCDesigner.Dao;

/// <summary>
/// Implementation of database configuration that reads from IConfiguration.
/// </summary>
public class DbConfiguration : IDbConfiguration
{
    private readonly IConfiguration mConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbConfiguration"/> class.
    /// </summary>
    /// <param name="configuration">The configuration provider.</param>
    /// <exception cref="ArgumentNullException">Thrown when configuration is null.</exception>
    public DbConfiguration(IConfiguration configuration)
    {
        mConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <inheritdoc/>
    public string Driver =>
        mConfiguration["db:driver"] ??
        throw new InvalidOperationException("Database driver not configured. Set 'db:driver' in appsettings.json");

    /// <inheritdoc/>
    public string ConnectionString =>
        mConfiguration["db:connectionString"] ??
        throw new InvalidOperationException("Database connection string not configured. Set 'db:connectionString' in appsettings.json");

    /// <inheritdoc/>
    public bool CreateTestUser
    {
        get
        {
            string? value = mConfiguration["db:createTestUser"];
            if (string.IsNullOrEmpty(value))
                return false;

            if (!bool.TryParse(value, out bool result))
                throw new InvalidOperationException($"Invalid value for 'db:createTestUser': {value}. Expected 'true' or 'false'.");

            return result;
        }
    }

    /// <inheritdoc/>
    public string TestUserPassword =>
        mConfiguration["db:testUserPassword"] ??
        throw new InvalidOperationException("Test user password not configured. Set 'db:testUserPassword' in appsettings.json");
}
