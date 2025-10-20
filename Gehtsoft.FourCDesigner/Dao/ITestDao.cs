namespace Gehtsoft.FourCDesigner.Dao;

/// <summary>
/// Data access interface for test operations.
/// Available only in Development and Testing environments.
/// </summary>
public interface ITestDao
{
    /// <summary>
    /// Resets the database by dropping and recreating all tables.
    /// </summary>
    void ResetDatabase();
}
