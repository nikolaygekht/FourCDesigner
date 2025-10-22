using Gehtsoft.FourCDesigner.Controllers.Data;
using Gehtsoft.FourCDesigner.Dao;
using Gehtsoft.FourCDesigner.Logic.Email;
using Gehtsoft.FourCDesigner.Logic.Email.Model;
using Gehtsoft.FourCDesigner.Logic.Email.Queue;
using Gehtsoft.FourCDesigner.Logic.User;
using Gehtsoft.FourCDesigner.Middleware.Throttling;
using Microsoft.AspNetCore.Mvc;

namespace Gehtsoft.FourCDesigner.Controllers;

/// <summary>
/// Test controller for Development and Testing environments only.
/// Provides utility endpoints for testing email functionality.
/// </summary>
#if DEBUG
[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
    private readonly IEmailQueue mEmailQueue;
    private readonly IEmailService mEmailService;
    private readonly IWebHostEnvironment mEnvironment;
    private readonly ILogger<TestController> mLogger;
    private readonly ITestDao mTestDao;
    private readonly IUserDao mUserDao;
    private readonly IUserController mUserController;
    private readonly IThrottleCache mThrottleCache;
    private readonly IHashProvider mHashProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestController"/> class.
    /// </summary>
    /// <param name="emailQueue">The email queue.</param>
    /// <param name="emailService">The email service.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="testDao">The test data access object.</param>
    /// <param name="userDao">The user data access object.</param>
    /// <param name="userController">The user controller.</param>
    /// <param name="throttleCache">The throttle cache.</param>
    /// <param name="hashProvider">The hash provider.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public TestController(
        IEmailQueue emailQueue,
        IEmailService emailService,
        IWebHostEnvironment environment,
        ILogger<TestController> logger,
        ITestDao testDao,
        IUserDao userDao,
        IUserController userController,
        IThrottleCache throttleCache,
        IHashProvider hashProvider)
    {
        mEmailQueue = emailQueue ?? throw new ArgumentNullException(nameof(emailQueue));
        mEmailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        mEnvironment = environment ?? throw new ArgumentNullException(nameof(environment));
        mLogger = logger ?? throw new ArgumentNullException(nameof(logger));
        mTestDao = testDao ?? throw new ArgumentNullException(nameof(testDao));
        mUserDao = userDao ?? throw new ArgumentNullException(nameof(userDao));
        mUserController = userController ?? throw new ArgumentNullException(nameof(userController));
        mThrottleCache = throttleCache ?? throw new ArgumentNullException(nameof(throttleCache));
        mHashProvider = hashProvider ?? throw new ArgumentNullException(nameof(hashProvider));
    }

    /// <summary>
    /// Checks if the controller is available (only in Development or Testing environments).
    /// </summary>
    /// <returns>True if available.</returns>
    private bool IsAvailable()
    {
        return mEnvironment.IsDevelopment() ||
               mEnvironment.EnvironmentName.Equals("Testing", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the next email from the queue without sending it.
    /// Available only in Development and Testing environments.
    /// </summary>
    /// <returns>The next email message or 404 if queue is empty.</returns>
    [HttpGet("email/dequeue")]
    public IActionResult DequeueEmail()
    {
        if (!IsAvailable())
        {
            mLogger.LogWarning("Test: Attempt to access test endpoint in non-development environment");
            return NotFound();
        }

        mLogger.LogDebug("Test: Dequeuing email from queue");

        if (mEmailQueue.TryDequeue(out EmailMessage? message) && message != null)
        {
            var response = new DequeueEmailResponse
            {
                Id = message.Id.ToString(),
                To = message.To,
                Subject = message.Subject,
                Body = message.Body,
                IsHtml = message.HtmlContent,
                Priority = message.Priority,
                CreatedAt = message.Created,
                RetryCount = message.RetryCount
            };

            mLogger.LogInformation("Test: Dequeued email {Id} to {Recipients}", message.Id, string.Join(", ", message.To));
            return Ok(response);
        }

        mLogger.LogDebug("Test: Queue is empty");
        return NotFound(new { message = "Queue is empty" });
    }

    /// <summary>
    /// Gets the current queue size.
    /// Available only in Development and Testing environments.
    /// </summary>
    /// <returns>Queue size information.</returns>
    [HttpGet("email/queue-size")]
    public IActionResult GetQueueSize()
    {
        if (!IsAvailable())
        {
            mLogger.LogWarning("Test: Attempt to access test endpoint in non-development environment");
            return NotFound();
        }

        var response = new QueueSizeResponse
        {
            QueueSize = mEmailService.QueueSize,
            IsSenderActive = mEmailService.IsSenderActive
        };

        mLogger.LogDebug("Test: Queue size requested - {Size} messages", response.QueueSize);
        return Ok(response);
    }

    /// <summary>
    /// Resets the database by dropping and recreating all tables.
    /// Available only in Development and Testing environments.
    /// </summary>
    /// <returns>Success message.</returns>
    [HttpPost("db/reset")]
    public IActionResult ResetDatabase()
    {
        if (!IsAvailable())
        {
            mLogger.LogWarning("Test: Attempt to access test endpoint in non-development environment");
            return NotFound();
        }

        mLogger.LogInformation("Test: Resetting database");

        mTestDao.ResetDatabase();

        mLogger.LogInformation("Test: Database reset completed");
        return Ok(new { message = "Database reset successfully" });
    }

    /// <summary>
    /// Adds a test user to the database directly without sending emails.
    /// Available only in Development and Testing environments.
    /// </summary>
    /// <param name="request">The user creation request.</param>
    /// <returns>The created user information.</returns>
    [HttpPost("db/add-user")]
    public IActionResult AddUser([FromBody] AddUserRequest request)
    {
        if (!IsAvailable())
        {
            mLogger.LogWarning("Test: Attempt to access test endpoint in non-development environment");
            return NotFound();
        }

        if (request == null)
            return BadRequest(new { message = "Request body is required" });

        if (string.IsNullOrEmpty(request.Email))
            return BadRequest(new { message = "Email is required" });

        if (string.IsNullOrEmpty(request.Password))
            return BadRequest(new { message = "Password is required" });

        mLogger.LogInformation("Test: Adding user {Email}", request.Email);

        // Check if user already exists
        var existingUser = mUserDao.GetUserByEmail(request.Email);
        if (existingUser != null)
        {
            mLogger.LogWarning("Test: User {Email} already exists", request.Email);
            return BadRequest(new { message = "User already exists" });
        }

        // Create user directly without sending emails
        var user = new Entities.User
        {
            Email = request.Email,
            PasswordHash = mHashProvider.ComputeHash(request.Password),
            Role = "user",
            ActiveUser = request.Activate  // Set active state based on request
        };

        try
        {
            mUserDao.SaveUser(user);
            mLogger.LogInformation("Test: User {Email} added successfully (Active: {Active})", request.Email, request.Activate);
            return Ok(new { message = "User added successfully", email = request.Email, active = request.Activate });
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Test: Failed to add user {Email}", request.Email);
            return BadRequest(new { message = "Failed to add user", error = ex.Message });
        }
    }

    /// <summary>
    /// Gets user information by email address.
    /// Available only in Development and Testing environments.
    /// </summary>
    /// <param name="email">The email address to look up.</param>
    /// <returns>User information or 404 if not found.</returns>
    [HttpGet("user")]
    public IActionResult GetUser([FromQuery] string email)
    {
        if (!IsAvailable())
        {
            mLogger.LogWarning("Test: Attempt to access test endpoint in non-development environment");
            return NotFound();
        }

        if (string.IsNullOrEmpty(email))
            return BadRequest(new { message = "Email is required" });

        mLogger.LogDebug("Test: Getting user: {Email}", email);

        var user = mUserDao.GetUserByEmail(email);

        if (user == null)
        {
            return NotFound(new { message = "User not found", email });
        }

        return Ok(new
        {
            email = user.Email,
            role = user.Role,
            active = user.ActiveUser
        });
    }

    /// <summary>
    /// Requests a password reset for testing purposes.
    /// Available only in Development and Testing environments.
    /// Bypasses UI form and directly triggers password reset flow.
    /// </summary>
    /// <param name="email">The email address to request password reset for.</param>
    /// <returns>Success message.</returns>
    [HttpPost("request-password-reset")]
    public async Task<IActionResult> RequestPasswordReset([FromQuery] string email)
    {
        if (!IsAvailable())
        {
            mLogger.LogWarning("Test: Attempt to access test endpoint in non-development environment");
            return NotFound();
        }

        if (string.IsNullOrEmpty(email))
            return BadRequest(new { message = "Email is required" });

        mLogger.LogInformation("Test: Requesting password reset for {Email}", email);

        try
        {
            await mUserController.RequestPasswordReset(email, CancellationToken.None);
            mLogger.LogInformation("Test: Password reset requested for {Email}", email);
            return Ok(new { message = "Password reset requested successfully", email });
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Test: Failed to request password reset for {Email}", email);
            return StatusCode(500, new { message = "Failed to request password reset", error = ex.Message });
        }
    }

    /// <summary>
    /// Resets throttling state by clearing the entire cache.
    /// Available only in Development and Testing environments.
    /// </summary>
    /// <returns>Success message.</returns>
    [HttpPost("reset-throttling")]
    public IActionResult ResetThrottling()
    {
        if (!IsAvailable())
        {
            mLogger.LogWarning("Test: Attempt to access test endpoint in non-development environment");
            return NotFound();
        }

        mLogger.LogInformation("Test: Resetting throttling state");

        try
        {
            mThrottleCache.Reset();

            mLogger.LogInformation("Test: Throttling state reset completed");

            return Ok(new { message = "Throttling state reset successfully" });
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Test: Failed to reset throttling state");
            return StatusCode(500, new { message = "Failed to reset throttling state", error = ex.Message });
        }
    }

    /// <summary>
    /// Test endpoint with short throttle limit for testing throttling functionality.
    /// Allows only 3 requests per 5 seconds per client.
    /// Available only in Development and Testing environments.
    /// </summary>
    /// <returns>Success message with request count.</returns>
    [HttpGet("throttle-test")]
    [Throttle(5000, 3, true)]
    public IActionResult ThrottleTest()
    {
        if (!IsAvailable())
        {
            mLogger.LogWarning("Test: Attempt to access test endpoint in non-development environment");
            return NotFound();
        }

        return Ok(new
        {
            message = "Throttle test endpoint - 3 requests per 5 seconds per client",
            timestamp = DateTime.UtcNow
        });
    }
}

/// <summary>
/// Request model for adding a test user.
/// </summary>
public class AddUserRequest
{
    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's password.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether to activate the user immediately.
    /// </summary>
    public bool Activate { get; set; } = false;
}
#endif
