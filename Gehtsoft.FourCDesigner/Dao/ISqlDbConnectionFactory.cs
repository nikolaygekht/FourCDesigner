using Gehtsoft.EF.Db.SqlDb;

namespace Gehtsoft.FourCDesigner.Dao;

/// <summary>
/// Factory interface for creating database connections.
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    /// Creates and returns a new database connection.
    /// </summary>
    /// <returns>A new SQL database connection.</returns>
    SqlDbConnection GetConnection();

    /// <summary>
    /// Creates and returns a new database connection asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation that returns a new SQL database connection.</returns>
    Task<SqlDbConnection> GetConnectionAsync(CancellationToken cancellationToken = default);
}
