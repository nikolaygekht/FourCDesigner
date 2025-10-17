using Gehtsoft.EF.Db.SqlDb;
using Gehtsoft.EF.Db.SqlDb.EntityQueries;
using Gehtsoft.FourCDesigner.Dao;
using Gehtsoft.FourCDesigner.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace Gehtsoft.FourCDesigner.Tests.Dao;

public class UserDaoTests : IDisposable
{
    private readonly string mConnectionString;
    private readonly SqlDbConnection mSchemaConnection;
    private readonly IDbConnectionFactory mFactory;
    private readonly UserDao mUserDao;

    public UserDaoTests()
    {
        // Create named in-memory SQLite connection that persists data between operations
        mConnectionString = "Data Source=file:UserDaoTestsDb?mode=memory&cache=shared";

        // Keep one connection open for the lifetime of the test to maintain the in-memory database
        mSchemaConnection = UniversalSqlDbFactory.Create("sqlite", mConnectionString);

        // Create schema
        var controller = new CreateEntityController(new[] { typeof(User).Assembly }, null);
        controller.UpdateTables(mSchemaConnection, CreateEntityController.UpdateMode.Update);

        // Create mock factory that creates a new connection each time
        var mockFactory = new Mock<IDbConnectionFactory>();
        mockFactory.Setup(f => f.GetConnection()).Returns(() => UniversalSqlDbFactory.Create("sqlite", mConnectionString));
        mFactory = mockFactory.Object;

        // Create UserDao with mock logger
        var mockLogger = new Mock<ILogger<UserDao>>();
        mUserDao = new UserDao(mFactory, mockLogger.Object);
    }

    public void Dispose()
    {
        mSchemaConnection?.Dispose();
    }

    [Fact]
    public void Constructor_WithNullFactory_ThrowsArgumentNullException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<UserDao>>();

        // Act
        Action act = () => new UserDao(null!, mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("factory");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new UserDao(mFactory, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void SaveUser_WithNewUser_InsertsUserAndSetsId()
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            Role = "user",
            ActiveUser = true
        };

        // Act
        mUserDao.SaveUser(user);

        // Assert
        user.Id.Should().BeGreaterThan(0);

        var retrievedUser = mUserDao.GetUserById(user.Id);
        retrievedUser.Should().NotBeNull();
        retrievedUser!.Email.Should().Be("test@example.com");
        retrievedUser.PasswordHash.Should().Be("hashedpassword");
        retrievedUser.Role.Should().Be("user");
        retrievedUser.ActiveUser.Should().BeTrue();
    }

    [Fact]
    public void SaveUser_WithExistingUser_UpdatesUser()
    {
        // Arrange
        var user = new User
        {
            Email = "original@example.com",
            PasswordHash = "originalHash",
            Role = "user",
            ActiveUser = true
        };
        mUserDao.SaveUser(user);

        // Modify user
        user.Email = "updated@example.com";
        user.Role = "admin";

        // Act
        mUserDao.SaveUser(user);

        // Assert
        var retrievedUser = mUserDao.GetUserById(user.Id);
        retrievedUser.Should().NotBeNull();
        retrievedUser!.Email.Should().Be("updated@example.com");
        retrievedUser.Role.Should().Be("admin");
    }

    [Fact]
    public void SaveUser_WithNullUser_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => mUserDao.SaveUser(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("user");
    }

    [Fact]
    public void GetUserById_WithExistingId_ReturnsUser()
    {
        // Arrange
        var user = new User
        {
            Email = "find@example.com",
            PasswordHash = "hash",
            Role = "user",
            ActiveUser = true
        };
        mUserDao.SaveUser(user);

        // Act
        var result = mUserDao.GetUserById(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Email.Should().Be("find@example.com");
    }

    [Fact]
    public void GetUserById_WithNonExistingId_ReturnsNull()
    {
        // Act
        var result = mUserDao.GetUserById(9999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetUserById_WithInvalidId_ThrowsArgumentException()
    {
        // Act
        Action act = () => mUserDao.GetUserById(0);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*must be positive*")
            .WithParameterName("id");
    }

    [Fact]
    public void GetUserByEmail_WithExistingEmail_ReturnsUser()
    {
        // Arrange
        var user = new User
        {
            Email = "search@example.com",
            PasswordHash = "hash",
            Role = "user",
            ActiveUser = true
        };
        mUserDao.SaveUser(user);

        // Act
        var result = mUserDao.GetUserByEmail("search@example.com");

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("search@example.com");
        result.Id.Should().Be(user.Id);
    }

    [Fact]
    public void GetUserByEmail_WithNonExistingEmail_ReturnsNull()
    {
        // Act
        var result = mUserDao.GetUserByEmail("nonexisting@example.com");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetUserByEmail_WithNullEmail_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => mUserDao.GetUserByEmail(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("email");
    }

    [Fact]
    public void DeleteUser_WithExistingUser_RemovesUser()
    {
        // Arrange
        var user = new User
        {
            Email = "delete@example.com",
            PasswordHash = "hash",
            Role = "user",
            ActiveUser = true
        };
        mUserDao.SaveUser(user);
        int userId = user.Id;

        // Act
        mUserDao.DeleteUser(user);

        // Assert
        var result = mUserDao.GetUserById(userId);
        result.Should().BeNull();
    }

    [Fact]
    public void DeleteUser_WithNullUser_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => mUserDao.DeleteUser(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("user");
    }

    [Fact]
    public void DeleteUser_WithInvalidUserId_ThrowsArgumentException()
    {
        // Arrange
        var user = new User
        {
            Id = 0,
            Email = "test@example.com",
            PasswordHash = "hash",
            Role = "user",
            ActiveUser = true
        };

        // Act
        Action act = () => mUserDao.DeleteUser(user);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*must have valid Id*")
            .WithParameterName("user");
    }

    [Fact]
    public void SaveUser_MultipleUsers_AllCanBeRetrieved()
    {
        // Arrange
        var user1 = new User { Email = "multiuser1@example.com", PasswordHash = "hash1", Role = "user", ActiveUser = true };
        var user2 = new User { Email = "multiuser2@example.com", PasswordHash = "hash2", Role = "admin", ActiveUser = false };
        var user3 = new User { Email = "multiuser3@example.com", PasswordHash = "hash3", Role = "user", ActiveUser = true };

        // Act
        mUserDao.SaveUser(user1);
        mUserDao.SaveUser(user2);
        mUserDao.SaveUser(user3);

        // Assert
        var retrieved1 = mUserDao.GetUserByEmail("multiuser1@example.com");
        var retrieved2 = mUserDao.GetUserByEmail("multiuser2@example.com");
        var retrieved3 = mUserDao.GetUserByEmail("multiuser3@example.com");

        retrieved1.Should().NotBeNull();
        retrieved2.Should().NotBeNull();
        retrieved3.Should().NotBeNull();

        retrieved1!.Role.Should().Be("user");
        retrieved2!.Role.Should().Be("admin");
        retrieved2.ActiveUser.Should().BeFalse();
    }

    [Fact]
    public void IsEmailUsed_WithNonExistingEmail_ReturnsFalse()
    {
        // Act
        bool result = mUserDao.IsEmailUsed("nonexisting@example.com");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsEmailUsed_WithExistingEmail_ReturnsTrue()
    {
        // Arrange
        var user = new User
        {
            Email = "existing@example.com",
            PasswordHash = "hash",
            Role = "user",
            ActiveUser = true
        };
        mUserDao.SaveUser(user);

        // Act
        bool result = mUserDao.IsEmailUsed("existing@example.com");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsEmailUsed_WithExistingEmailAndExcludingSameUser_ReturnsFalse()
    {
        // Arrange
        var user = new User
        {
            Email = "excludesame@example.com",
            PasswordHash = "hash",
            Role = "user",
            ActiveUser = true
        };
        mUserDao.SaveUser(user);

        // Act
        bool result = mUserDao.IsEmailUsed("excludesame@example.com", user.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsEmailUsed_WithExistingEmailAndExcludingDifferentUser_ReturnsTrue()
    {
        // Arrange
        var user1 = new User
        {
            Email = "user1@example.com",
            PasswordHash = "hash1",
            Role = "user",
            ActiveUser = true
        };
        var user2 = new User
        {
            Email = "user2@example.com",
            PasswordHash = "hash2",
            Role = "user",
            ActiveUser = true
        };
        mUserDao.SaveUser(user1);
        mUserDao.SaveUser(user2);

        // Act - Check if user1's email is used, excluding user2 (should still find user1)
        bool result = mUserDao.IsEmailUsed("user1@example.com", user2.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsEmailUsed_WithNullEmail_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => mUserDao.IsEmailUsed(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("email");
    }

    [Fact]
    public void IsEmailUsed_WithExcludeUserIdAndNoMatch_ReturnsFalse()
    {
        // Arrange
        var user = new User
        {
            Email = "onlyuser@example.com",
            PasswordHash = "hash",
            Role = "user",
            ActiveUser = true
        };
        mUserDao.SaveUser(user);

        // Act - Check same email but exclude the only user with that email
        bool result = mUserDao.IsEmailUsed("onlyuser@example.com", user.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void DeleteUserByEmail_WithExistingUser_RemovesUserAndReturnsTrue()
    {
        // Arrange
        var user = new User
        {
            Email = "deletebyemail@example.com",
            PasswordHash = "hash",
            Role = "user",
            ActiveUser = true
        };
        mUserDao.SaveUser(user);

        // Act
        bool result = mUserDao.DeleteUserByEmail("deletebyemail@example.com");

        // Assert
        result.Should().BeTrue();
        var deletedUser = mUserDao.GetUserByEmail("deletebyemail@example.com");
        deletedUser.Should().BeNull();
    }

    [Fact]
    public void DeleteUserByEmail_WithNonExistingUser_ReturnsFalse()
    {
        // Act
        bool result = mUserDao.DeleteUserByEmail("nonexistent@example.com");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void DeleteUserByEmail_WithNullEmail_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => mUserDao.DeleteUserByEmail(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("email");
    }

    [Fact]
    public void UpdatePasswordByEmail_WithExistingUser_UpdatesPasswordAndReturnsTrue()
    {
        // Arrange
        var user = new User
        {
            Email = "updatepwd@example.com",
            PasswordHash = "oldhash",
            Role = "user",
            ActiveUser = true
        };
        mUserDao.SaveUser(user);

        // Act
        bool result = mUserDao.UpdatePasswordByEmail("updatepwd@example.com", "newhash");

        // Assert
        result.Should().BeTrue();
        var updatedUser = mUserDao.GetUserByEmail("updatepwd@example.com");
        updatedUser.Should().NotBeNull();
        updatedUser!.PasswordHash.Should().Be("newhash");
    }

    [Fact]
    public void UpdatePasswordByEmail_WithNonExistingUser_ReturnsFalse()
    {
        // Act
        bool result = mUserDao.UpdatePasswordByEmail("nonexistent@example.com", "newhash");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void UpdatePasswordByEmail_WithNullEmail_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => mUserDao.UpdatePasswordByEmail(null!, "newhash");

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("email");
    }

    [Fact]
    public void UpdatePasswordByEmail_WithNullPasswordHash_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => mUserDao.UpdatePasswordByEmail("test@example.com", null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("passwordHash");
    }

    [Fact]
    public void ActivateUserByEmail_WithExistingInactiveUser_ActivatesUserAndReturnsTrue()
    {
        // Arrange
        var user = new User
        {
            Email = "activate@example.com",
            PasswordHash = "hash",
            Role = "user",
            ActiveUser = false
        };
        mUserDao.SaveUser(user);

        // Act
        bool result = mUserDao.ActivateUserByEmail("activate@example.com");

        // Assert
        result.Should().BeTrue();
        var activatedUser = mUserDao.GetUserByEmail("activate@example.com");
        activatedUser.Should().NotBeNull();
        activatedUser!.ActiveUser.Should().BeTrue();
    }

    [Fact]
    public void ActivateUserByEmail_WithExistingActiveUser_ReturnsTrue()
    {
        // Arrange
        var user = new User
        {
            Email = "alreadyactive@example.com",
            PasswordHash = "hash",
            Role = "user",
            ActiveUser = true
        };
        mUserDao.SaveUser(user);

        // Act
        bool result = mUserDao.ActivateUserByEmail("alreadyactive@example.com");

        // Assert
        result.Should().BeTrue();
        var retrievedUser = mUserDao.GetUserByEmail("alreadyactive@example.com");
        retrievedUser.Should().NotBeNull();
        retrievedUser!.ActiveUser.Should().BeTrue();
    }

    [Fact]
    public void ActivateUserByEmail_WithNonExistingUser_ReturnsFalse()
    {
        // Act
        bool result = mUserDao.ActivateUserByEmail("nonexistent@example.com");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ActivateUserByEmail_WithNullEmail_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => mUserDao.ActivateUserByEmail(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("email");
    }

    [Fact]
    public void DeactivateUserByEmail_WithExistingActiveUser_DeactivatesUserAndReturnsTrue()
    {
        // Arrange
        var user = new User
        {
            Email = "deactivate@example.com",
            PasswordHash = "hash",
            Role = "user",
            ActiveUser = true
        };
        mUserDao.SaveUser(user);

        // Act
        bool result = mUserDao.DeactivateUserByEmail("deactivate@example.com");

        // Assert
        result.Should().BeTrue();
        var deactivatedUser = mUserDao.GetUserByEmail("deactivate@example.com");
        deactivatedUser.Should().NotBeNull();
        deactivatedUser!.ActiveUser.Should().BeFalse();
    }

    [Fact]
    public void DeactivateUserByEmail_WithExistingInactiveUser_ReturnsTrue()
    {
        // Arrange
        var user = new User
        {
            Email = "alreadyinactive@example.com",
            PasswordHash = "hash",
            Role = "user",
            ActiveUser = false
        };
        mUserDao.SaveUser(user);

        // Act
        bool result = mUserDao.DeactivateUserByEmail("alreadyinactive@example.com");

        // Assert
        result.Should().BeTrue();
        var retrievedUser = mUserDao.GetUserByEmail("alreadyinactive@example.com");
        retrievedUser.Should().NotBeNull();
        retrievedUser!.ActiveUser.Should().BeFalse();
    }

    [Fact]
    public void DeactivateUserByEmail_WithNonExistingUser_ReturnsFalse()
    {
        // Act
        bool result = mUserDao.DeactivateUserByEmail("nonexistent@example.com");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void DeactivateUserByEmail_WithNullEmail_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => mUserDao.DeactivateUserByEmail(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("email");
    }
}
