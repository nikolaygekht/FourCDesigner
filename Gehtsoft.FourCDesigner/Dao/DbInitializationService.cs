using Gehtsoft.EF.Db.SqlDb;
using Gehtsoft.EF.Db.SqlDb.EntityQueries;
using Gehtsoft.EF.Entities;
using Gehtsoft.FourCDesigner.Entities;

namespace Gehtsoft.FourCDesigner.Dao;

/// <summary>
/// Implementation of database initialization service.
/// This service is responsible for creating database schema and test data.
/// </summary>
public class DbInitializationService : IDbInitializationService
{
    private readonly IDbConfiguration mDbConfiguration;
    private readonly IHashProvider mHashProvider;
    private readonly ILogger<DbInitializationService> mLogger;
    private readonly object mLock = new object();
    private volatile bool mIsInitialized = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbInitializationService"/> class.
    /// </summary>
    /// <param name="dbConfiguration">The database configuration.</param>
    /// <param name="hashProvider">The hash provider for password hashing.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public DbInitializationService(
        IDbConfiguration dbConfiguration,
        IHashProvider hashProvider,
        ILogger<DbInitializationService> logger)
    {
        mDbConfiguration = dbConfiguration ?? throw new ArgumentNullException(nameof(dbConfiguration));
        mHashProvider = hashProvider ?? throw new ArgumentNullException(nameof(hashProvider));
        mLogger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public bool IsInitialized => mIsInitialized;

    /// <inheritdoc/>
    public void InitializeDatabase(SqlDbConnection connection)
    {
        if (connection == null)
            throw new ArgumentNullException(nameof(connection));

        if (mIsInitialized)
            return;

        lock (mLock)
        {
            if (mIsInitialized)
                return;

            try
            {
                mLogger.LogInformation("Starting database initialization");

                CreateTables(connection);

                if (mDbConfiguration.CreateTestUser)
                {
                    CreateTestUser(connection);
                }

                mIsInitialized = true;
                mLogger.LogInformation("Database initialization completed successfully");
            }
            catch (Exception ex)
            {
                mLogger.LogError(ex, "Failed to initialize database");
                throw;
            }
        }
    }

    /// <summary>
    /// Creates all database tables.
    /// </summary>
    /// <param name="connection">The database connection.</param>
    private void CreateTables(SqlDbConnection connection)
    {
        if (connection == null)
            throw new ArgumentNullException(nameof(connection));

        mLogger.LogInformation("Creating database tables");

        CreateEntityController controller = new CreateEntityController(
            new[] { typeof(User).Assembly },
            null);

        bool oldProtection = SqlInjectionProtectionPolicy.Instance.ProtectFromScalarsInQueries;
        SqlInjectionProtectionPolicy.Instance.ProtectFromScalarsInQueries = false;

        try
        {
            controller.UpdateTables(connection, CreateEntityController.UpdateMode.Update);
            mLogger.LogInformation("Database tables created successfully");
        }
        finally
        {
            SqlInjectionProtectionPolicy.Instance.ProtectFromScalarsInQueries = oldProtection;
        }
    }

    /// <summary>
    /// Creates a test user if it doesn't already exist.
    /// </summary>
    /// <param name="connection">The database connection.</param>
    private void CreateTestUser(SqlDbConnection connection)
    {
        if (connection == null)
            throw new ArgumentNullException(nameof(connection));

        const string testEmail = "user@fourcdesign.com";
        string testPassword = mDbConfiguration.TestUserPassword;

        mLogger.LogInformation("Checking if test user exists: {Email}", testEmail);

        User existingUser;
        using (var query = connection.GetSelectEntitiesQuery<User>())
        {
            query.Where.Property(nameof(User.Email)).Eq(testEmail);
            query.Execute();
            existingUser = query.ReadOne<User>();
        }

        if (existingUser != null)
        {
            mLogger.LogInformation("Test user already exists: {Email}", testEmail);
            return;
        }

        mLogger.LogInformation("Creating test user: {Email}", testEmail);

        string passwordHash = mHashProvider.ComputeHash(testPassword);

        User testUser = new User
        {
            Email = testEmail,
            PasswordHash = passwordHash,
            Role = "user",
            ActiveUser = true
        };

        using (var query = connection.GetInsertEntityQuery<User>())
        {
            query.Execute(testUser);
        }

        mLogger.LogInformation("Test user created successfully: {Email} with ID: {Id}", testEmail, testUser.Id);
    }
}
