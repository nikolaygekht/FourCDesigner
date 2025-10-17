namespace Gehtsoft.FourCDesigner.Dao;

/// <summary>
/// Configuration interface for database connection settings.
/// </summary>
public interface IDbConfiguration
{
    /// <summary>
    /// Gets the database driver name (e.g., "sqlite", "mssql", "npgsql").
    /// </summary>
    string Driver { get; }

    /// <summary>
    /// Gets the database connection string.
    /// </summary>
    string ConnectionString { get; }
}
