using Gehtsoft.EF.Db.SqlDb.EntityQueries;
using Gehtsoft.EF.Entities;
using Gehtsoft.EF.Db.SqlDb;
using Gehtsoft.FourCDesigner.Entities;

namespace Gehtsoft.FourCDesigner.Dao;

/// <summary>
/// Data access implementation for test operations.
/// Available only in Development and Testing environments.
/// </summary>
public class TestDao : ITestDao
{
    private readonly IDbConnectionFactory mConnectionFactory;
    private readonly ILogger<TestDao> mLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestDao"/> class.
    /// </summary>
    /// <param name="connectionFactory">The database connection factory.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public TestDao(
        IDbConnectionFactory connectionFactory,
        ILogger<TestDao> logger)
    {
        mConnectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        mLogger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public void ResetDatabase()
    {
        using var connection = mConnectionFactory.GetConnection();

        var controller = new CreateEntityController(
            new[] { typeof(User).Assembly },
            null);

        bool oldProtection = SqlInjectionProtectionPolicy.Instance.ProtectFromScalarsInQueries;
        SqlInjectionProtectionPolicy.Instance.ProtectFromScalarsInQueries = false;

        try
        {
            mLogger.LogDebug("TestDao: Dropping database tables");
            controller.DropTables(connection);

            mLogger.LogDebug("TestDao: Creating database tables");
            controller.CreateTables(connection);

            mLogger.LogInformation("TestDao: Database reset completed");
        }
        finally
        {
            SqlInjectionProtectionPolicy.Instance.ProtectFromScalarsInQueries = oldProtection;
        }
    }
}
