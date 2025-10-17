using Gehtsoft.EF.Db.SqlDb;

namespace Gehtsoft.FourCDesigner.Dao;

/// <summary>
/// Implementation of SQL database connection factory using UniversalSqlDbFactory.
/// </summary>
public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly string mDriver;
    private readonly string mConnectionString;
    private readonly IDbInitializationService mInitializationService;
    private readonly ILogger<DbConnectionFactory> mLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbConnectionFactory"/> class.
    /// </summary>
    /// <param name="dbConfiguration">The database configuration.</param>
    /// <param name="initializationService">The database initialization service.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">Thrown when dbConfiguration, initializationService, or logger is null.</exception>
    public DbConnectionFactory(
        IDbConfiguration dbConfiguration,
        IDbInitializationService initializationService,
        ILogger<DbConnectionFactory> logger)
    {
        if (dbConfiguration == null)
            throw new ArgumentNullException(nameof(dbConfiguration));

        mInitializationService = initializationService ?? throw new ArgumentNullException(nameof(initializationService));
        mLogger = logger ?? throw new ArgumentNullException(nameof(logger));
        mDriver = dbConfiguration.Driver;
        mConnectionString = dbConfiguration.ConnectionString;

        mLogger.LogInformation("DbConnectionFactory initialized with driver: {Driver}", mDriver);
    }

    /// <inheritdoc/>
    public SqlDbConnection GetConnection()
    {
        try
        {
            mLogger.LogDebug("Creating database connection using driver: {Driver}", mDriver);
            SqlDbConnection connection = UniversalSqlDbFactory.Create(mDriver, mConnectionString);

            // Initialize database on first connection
            if (!mInitializationService.IsInitialized)
                mInitializationService.InitializeDatabase(connection);

            mLogger.LogDebug("Database connection created successfully");
            return connection;
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Failed to create database connection with driver: {Driver}", mDriver);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<SqlDbConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            mLogger.LogDebug("Creating database connection asynchronously using driver: {Driver}", mDriver);
            SqlDbConnection connection = await UniversalSqlDbFactory.CreateAsync(mDriver, mConnectionString, cancellationToken);

            // Initialize database on first connection
            if (!mInitializationService.IsInitialized)
                mInitializationService.InitializeDatabase(connection);

            mLogger.LogDebug("Database connection created successfully");
            return connection;
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Failed to create database connection asynchronously with driver: {Driver}", mDriver);
            throw;
        }
    }
}
