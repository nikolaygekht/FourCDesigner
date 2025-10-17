using Gehtsoft.EF.Db.SqlDb.EntityQueries;
using Gehtsoft.FourCDesigner.Entities;

namespace Gehtsoft.FourCDesigner.Dao;

/// <summary>
/// Implementation of User data access operations.
/// </summary>
public class UserDao : IUserDao
{
    private readonly IDbConnectionFactory mFactory;
    private readonly ILogger<UserDao> mLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserDao"/> class.
    /// </summary>
    /// <param name="factory">The database connection factory.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public UserDao(
        IDbConnectionFactory factory,
        ILogger<UserDao> logger)
    {
        mFactory = factory ?? throw new ArgumentNullException(nameof(factory));
        mLogger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public User? GetUserByEmail(string email)
    {
        if (email == null)
            throw new ArgumentNullException(nameof(email));

        try
        {
            using var connection = mFactory.GetConnection();

            using (var query = connection.GetSelectEntitiesQuery<User>())
            {
                query.Where.Property(nameof(User.Email)).Eq(email);
                query.Execute();
                return query.ReadOne<User>();
            }
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Failed to get user by email: {Email}", email);
            throw;
        }
    }

    /// <inheritdoc/>
    public User? GetUserById(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Id must be positive", nameof(id));

        try
        {
            using var connection = mFactory.GetConnection();

            using (var query = connection.GetSelectEntitiesQuery<User>())
            {
                query.Where.Property(nameof(User.Id)).Eq(id);
                query.Execute();
                return query.ReadOne<User>();
            }
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Failed to get user by id: {Id}", id);
            throw;
        }
    }

    /// <inheritdoc/>
    public void SaveUser(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        try
        {
            using var connection = mFactory.GetConnection();

            bool isUpdate = user.Id > 0;
            mLogger.LogDebug("{Operation} user", isUpdate ? "Updating" : "Inserting");

            using (var query = isUpdate
                ? connection.GetUpdateEntityQuery<User>()
                : connection.GetInsertEntityQuery<User>())
            {
                query.Execute(user);
            }

            mLogger.LogInformation("Successfully {Operation} user {UserId}",
                isUpdate ? "updated" : "inserted", user.Id);
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Failed to save user");
            throw;
        }
    }

    /// <inheritdoc/>
    public void DeleteUser(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (user.Id <= 0)
            throw new ArgumentException("User must have valid Id", nameof(user));

        try
        {
            mLogger.LogInformation("Deleting user {UserId}", user.Id);

            using var connection = mFactory.GetConnection();

            using (var query = connection.GetDeleteEntityQuery<User>())
            {
                query.Execute(user);
            }

            mLogger.LogInformation("Successfully deleted user {UserId}", user.Id);
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Failed to delete user {UserId}", user.Id);
            throw;
        }
    }

    /// <inheritdoc/>
    public bool DeleteUserByEmail(string email)
    {
        if (email == null)
            throw new ArgumentNullException(nameof(email));

        try
        {
            mLogger.LogInformation("Deleting user by email: {Email}", email);

            var user = GetUserByEmail(email);
            if (user == null)
            {
                mLogger.LogWarning("User not found for deletion: {Email}", email);
                return false;
            }

            using var connection = mFactory.GetConnection();

            using (var query = connection.GetDeleteEntityQuery<User>())
            {
                query.Execute(user);
            }

            mLogger.LogInformation("Successfully deleted user by email: {Email}", email);
            return true;
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Failed to delete user by email: {Email}", email);
            throw;
        }
    }

    /// <inheritdoc/>
    public bool UpdatePasswordByEmail(string email, string passwordHash)
    {
        if (email == null)
            throw new ArgumentNullException(nameof(email));
        if (passwordHash == null)
            throw new ArgumentNullException(nameof(passwordHash));

        try
        {
            mLogger.LogInformation("Updating password for user: {Email}", email);

            using var connection = mFactory.GetConnection();

            using (var query = connection.GetMultiUpdateEntityQuery<User>())
            {
                query.AddUpdateColumn(nameof(User.PasswordHash), passwordHash);
                query.Where.Property(nameof(User.Email)).Eq(email);
                query.Execute();

                bool updated = query.RowsAffected > 0;
                if (!updated)
                    mLogger.LogWarning("No user found to update password: {Email}", email);
                else
                    mLogger.LogInformation("Successfully updated password for user: {Email}", email);

                return updated;
            }
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Failed to update password for user: {Email}", email);
            throw;
        }
    }

    /// <inheritdoc/>
    public bool ActivateUserByEmail(string email)
    {
        if (email == null)
            throw new ArgumentNullException(nameof(email));

        try
        {
            mLogger.LogInformation("Activating user: {Email}", email);

            using var connection = mFactory.GetConnection();

            using (var query = connection.GetMultiUpdateEntityQuery<User>())
            {
                query.AddUpdateColumn(nameof(User.ActiveUser), true);
                query.Where.Property(nameof(User.Email)).Eq(email);
                query.Execute();

                bool updated = query.RowsAffected > 0;
                if (!updated)
                    mLogger.LogWarning("No user found to activate: {Email}", email);
                else
                    mLogger.LogInformation("Successfully activated user: {Email}", email);

                return updated;
            }
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Failed to activate user: {Email}", email);
            throw;
        }
    }

    /// <inheritdoc/>
    public bool DeactivateUserByEmail(string email)
    {
        if (email == null)
            throw new ArgumentNullException(nameof(email));

        try
        {
            mLogger.LogInformation("Deactivating user: {Email}", email);

            using var connection = mFactory.GetConnection();

            using (var query = connection.GetMultiUpdateEntityQuery<User>())
            {
                query.AddUpdateColumn(nameof(User.ActiveUser), false);
                query.Where.Property(nameof(User.Email)).Eq(email);
                query.Execute();

                bool updated = query.RowsAffected > 0;
                if (!updated)
                    mLogger.LogWarning("No user found to deactivate: {Email}", email);
                else
                    mLogger.LogInformation("Successfully deactivated user: {Email}", email);

                return updated;
            }
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Failed to deactivate user: {Email}", email);
            throw;
        }
    }

    /// <inheritdoc/>
    public bool IsEmailUsed(string email, int? excludeUserId = null)
    {
        if (email == null)
            throw new ArgumentNullException(nameof(email));

        try
        {
            using var connection = mFactory.GetConnection();

            using (var query = connection.GetSelectEntitiesCountQuery<User>())
            {
                query.Where.Property(nameof(User.Email)).Eq(email);
                if (excludeUserId.HasValue)
                    query.Where.Property(nameof(User.Id)).Neq(excludeUserId.Value);

                query.Execute();
                int count = query.RowCount;

                mLogger.LogDebug("Email {Email} usage check (excluding userId {ExcludeUserId}): count = {Count}",
                    email, excludeUserId, count);

                return count > 0;
            }
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Failed to check email usage: {Email}", email);
            throw;
        }
    }
}
