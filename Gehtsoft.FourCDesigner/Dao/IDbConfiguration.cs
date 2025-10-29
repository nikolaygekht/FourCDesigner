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

    /// <summary>
    /// Gets a value indicating whether to create a test user on database initialization.
    /// </summary>
    bool CreateTestUser { get; }

    /// <summary>
    /// Gets the test user password.
    /// Used when CreateTestUser is true.
    /// </summary>
    string TestUserPassword { get; }
}
