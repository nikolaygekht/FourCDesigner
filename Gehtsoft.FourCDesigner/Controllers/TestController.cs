using Gehtsoft.FourCDesigner.Controllers.Data;
using Gehtsoft.FourCDesigner.Dao;
using Gehtsoft.FourCDesigner.Logic.Email;
using Gehtsoft.FourCDesigner.Logic.Email.Model;
using Gehtsoft.FourCDesigner.Logic.Email.Queue;
using Gehtsoft.FourCDesigner.Logic.User;
using Gehtsoft.FourCDesigner.Middleware.Throttling;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

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
    private readonly IThrottleResetService mThrottleResetService;

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
    /// <param name="throttleResetService">The throttle reset service.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public TestController(
        IEmailQueue emailQueue,
        IEmailService emailService,
        IWebHostEnvironment environment,
        ILogger<TestController> logger,
        ITestDao testDao,
        IUserDao userDao,
        IUserController userController,
        IThrottleResetService throttleResetService)
    {
        mEmailQueue = emailQueue ?? throw new ArgumentNullException(nameof(emailQueue));
        mEmailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        mEnvironment = environment ?? throw new ArgumentNullException(nameof(environment));
        mLogger = logger ?? throw new ArgumentNullException(nameof(logger));
        mTestDao = testDao ?? throw new ArgumentNullException(nameof(testDao));
        mUserDao = userDao ?? throw new ArgumentNullException(nameof(userDao));
        mUserController = userController ?? throw new ArgumentNullException(nameof(userController));
        mThrottleResetService = throttleResetService ?? throw new ArgumentNullException(nameof(throttleResetService));
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
    /// Adds a test user to the database.
    /// Available only in Development and Testing environments.
    /// </summary>
    /// <param name="request">The user creation request.</param>
    /// <returns>The created user information.</returns>
    [HttpPost("db/add-user")]
    public async Task<IActionResult> AddUser([FromBody] AddUserRequest request)
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

        var success = await mUserController.RegisterUser(request.Email, request.Password);

        if (!success)
        {
            mLogger.LogWarning("Test: Failed to add user {Email}", request.Email);
            return BadRequest(new { message = "Failed to register user" });
        }

        // Optionally activate the user immediately if requested
        if (request.Activate)
        {
            bool activated = mUserDao.ActivateUserByEmail(request.Email);
            if (activated)
            {
                mLogger.LogInformation("Test: User {Email} activated", request.Email);
            }
            else
            {
                mLogger.LogWarning("Test: Failed to activate user {Email}", request.Email);
            }
        }

        mLogger.LogInformation("Test: User {Email} added successfully", request.Email);
        return Ok(new { message = "User added successfully", email = request.Email });
    }

    /// <summary>
    /// Resets throttling state by incrementing the generation counter.
    /// This invalidates all existing rate limiter partitions by changing partition keys.
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
            var oldGeneration = mThrottleResetService.Generation;
            mThrottleResetService.Reset();
            var newGeneration = mThrottleResetService.Generation;

            mLogger.LogInformation("Test: Throttling state reset completed (generation {OldGen} -> {NewGen})",
                oldGeneration, newGeneration);

            return Ok(new
            {
                message = "Throttling state reset successfully",
                oldGeneration,
                newGeneration
            });
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Test: Failed to reset throttling state");
            return StatusCode(500, new { message = "Failed to reset throttling state", error = ex.Message });
        }
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
