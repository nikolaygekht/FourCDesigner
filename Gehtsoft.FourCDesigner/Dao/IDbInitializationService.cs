using Gehtsoft.EF.Db.SqlDb;

namespace Gehtsoft.FourCDesigner.Dao;

/// <summary>
/// Service interface for database initialization operations.
/// </summary>
public interface IDbInitializationService
{
    /// <summary>
    /// Initializes the database schema and creates test user if configured.
    /// This method should be called once when the first database connection is created.
    /// </summary>
    /// <param name="connection">The database connection to use for initialization.</param>
    /// <exception cref="ArgumentNullException">Thrown when connection is null.</exception>
    void InitializeDatabase(SqlDbConnection connection);

    /// <summary>
    /// Gets a value indicating whether the database has been initialized.
    /// </summary>
    bool IsInitialized { get; }
}
