# Architecture Guide - .NET Implementation

## SOLID Principles

The code must follow SOLID principles without cutting edges.

### Single Responsibility

Each class must have one and only one purpose. The best way to obey this principle is to use ECB pattern.

## Single Responsibility: Always use the ECB Pattern

ECB pattern defines three possible roles for each class:

**Entity**: The class which only keeps data about an entity and provides methods to get and set these data and absolutely nothing else. No other operations on the entity allowed!

In this project, Gehtsoft.EF entities serve as ECB Entities.

**Boundary**: The facade class that provides the access to other logical components or the layers of the application.

Local components boundaries are typically drawn by business processes, use cases or usage scenarios.

The layers typically are responsible for different levels of abstraction: e.g. user interaction, file system interaction, database/persistent storage, configuration, logging and so on.

**Controller**: The class that implements business operation and keeps no state inside (use an entity to keep state if needed). Controller interacts with entities, boundaries and other controllers.

**Important Terminology**:
- **ECB Controllers** = Business logic classes (stateless, orchestrate operations)
- **API Controllers/Endpoints** = ASP.NET Web API controllers (thin HTTP routing layer)

### ECB Pattern Example

```csharp
// Entity (Gehtsoft.EF entity - pure data holder)
public class ChatSession
{
    public string SessionId { get; set; }
    public List<string> Messages { get; set; }

    public ChatSession()
    {
        Messages = new List<string>();
    }

    public void AddMessage(string message)
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));

        Messages.Add(message);
    }

    public List<string> GetHistory()
    {
        return new List<string>(Messages);
    }
}

// Boundary (facade for external service)
public interface IClaudeApiGateway
{
    Task<string> SendPromptAsync(string prompt, int timeoutSeconds);
}

public class ClaudeApiGateway : IClaudeApiGateway
{
    private readonly HttpClient mHttpClient;
    private readonly ILogger<ClaudeApiGateway> mLogger;

    public ClaudeApiGateway(HttpClient httpClient, ILogger<ClaudeApiGateway> logger)
    {
        if (httpClient == null)
            throw new ArgumentNullException(nameof(httpClient));
        if (logger == null)
            throw new ArgumentNullException(nameof(logger));

        mHttpClient = httpClient;
        mLogger = logger;
    }

    public async Task<string> SendPromptAsync(string prompt, int timeoutSeconds)
    {
        if (prompt == null)
            throw new ArgumentNullException(nameof(prompt));

        // Handles timeout, API formatting, retries
        using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds)))
        {
            try
            {
                HttpResponseMessage response = await mHttpClient.PostAsJsonAsync("/api/prompt", new { prompt }, cts.Token);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (TaskCanceledException ex)
            {
                mLogger.LogError(ex, "Claude API request timed out after {Timeout} seconds", timeoutSeconds);
                throw new TimeoutException($"Request timed out after {timeoutSeconds} seconds", ex);
            }
            catch (HttpRequestException ex)
            {
                mLogger.LogError(ex, "Claude API request failed");
                throw;
            }
        }
    }
}

// ECB Controller (business logic, stateless)
public interface ISummarizationController
{
    Task<string> SummarizeLast10Async(ChatSession session);
}

public class SummarizationController : ISummarizationController
{
    private readonly IClaudeApiGateway mClaudeGateway;
    private readonly ILogger<SummarizationController> mLogger;

    public SummarizationController(IClaudeApiGateway claudeGateway, ILogger<SummarizationController> logger)
    {
        if (claudeGateway == null)
            throw new ArgumentNullException(nameof(claudeGateway));
        if (logger == null)
            throw new ArgumentNullException(nameof(logger));

        mClaudeGateway = claudeGateway;
        mLogger = logger;
    }

    public async Task<string> SummarizeLast10Async(ChatSession session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        List<string> history = session.GetHistory();
        List<string> last10 = history.Skip(Math.Max(0, history.Count - 10)).ToList();
        string prompt = string.Join("\n", last10);

        mLogger.LogInformation("Summarizing last {Count} messages from session {SessionId}",
            last10.Count, session.SessionId);

        return await mClaudeGateway.SendPromptAsync(prompt, 30);
    }
}

// API Controller/Endpoint (thin HTTP layer)
[ApiController]
[Route("api/[controller]")]
public class SummarizationEndpoint : ControllerBase
{
    private readonly ISummarizationController mController;
    private readonly IMapper mMapper;
    private readonly ILogger<SummarizationEndpoint> mLogger;

    public SummarizationEndpoint(
        ISummarizationController controller,
        IMapper mapper,
        ILogger<SummarizationEndpoint> logger)
    {
        mController = controller;
        mMapper = mapper;
        mLogger = logger;
    }

    /// <summary>
    /// Summarizes the last 10 messages in a chat session.
    /// </summary>
    /// <param name="request">The summarization request containing session ID.</param>
    /// <returns>The summary text.</returns>
    [HttpPost("last10")]
    public async Task<IActionResult> SummarizeLast10([FromBody] SummarizationRequest request)
    {
        // Format validation happens automatically via model validation
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Map DTO to entity
        ChatSession session = mMapper.Map<ChatSession>(request);

        // Call business logic
        string summary = await mController.SummarizeLast10Async(session);

        // Map result to DTO and return
        SummarizationResponse response = new SummarizationResponse { Summary = summary };
        return Ok(response);
    }
}
```

## Open-Close Principle

If the method is tested and on production we do NOT change behavior of any method. If change is required we always introduce the new method and then refactor all related controllers and boundaries to interact with the new method.

## Liskov Substitution Principle

Derived classes must be usable through the base class interface without the user needing to know the difference. This avoids surprises or broken assumptions.

## Interface Segregation

Avoid creation of mega-classes and mega-interfaces that provide methods for all possible scenarios. Each controller or boundary must have access only to the operations they need to use and nothing else.

Create focused, client-specific interfaces:
- ❌ BAD: `IUserRepository` with 15 methods
- ✅ GOOD: `IUserReader`, `IUserWriter`, `IUserEmailLookup`, `IUserPasswordUpdater`

## Dependency Inversion

Any class that uses other classes must NOT depend on the implementation of the classes it uses.

The best way to implement this principle:
- Define class name and interface by the purpose of this class, not by the behavior
- Keep everything behind strictly defined interfaces
- Code against the interface, not the class
- Use ASP.NET Core built-in dependency injection

### Dependency Injection Example

```csharp
// Program.cs or Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    // Register boundaries
    services.AddScoped<IClaudeApiGateway, ClaudeApiGateway>();
    services.AddScoped<IUserDataBoundary, UserDatabaseBoundary>();

    // Register controllers
    services.AddScoped<ISummarizationController, SummarizationController>();
    services.AddScoped<IUserRegistrationController, UserRegistrationController>();

    // Register validators
    services.AddScoped<IUserValidator, UserValidator>();

    // Configure AutoMapper
    services.AddAutoMapper(typeof(Program));
}
```

## Resilience

All operations related to external services (REST APIs, databases):
- **Must be asynchronous** (use `async`/`await`)
- **Must have timeouts** (use `CancellationToken` or `TimeSpan`)
- **Must have clear error handling** (catch specific exceptions)

Mission critical scenarios may require:
- **Implementation of retry strategy** (using Polly library)
- **Implementation of circuit breaker pattern** (using Polly library)
- **Alternative flow** when part of functionality is unavailable

### Resilience Example with Polly

```csharp
using Polly;
using Polly.Timeout;

public class ResilientClaudeApiGateway : IClaudeApiGateway
{
    private readonly HttpClient mHttpClient;
    private readonly ILogger<ResilientClaudeApiGateway> mLogger;
    private readonly IAsyncPolicy<HttpResponseMessage> mRetryPolicy;

    public ResilientClaudeApiGateway(HttpClient httpClient, ILogger<ResilientClaudeApiGateway> logger)
    {
        mHttpClient = httpClient;
        mLogger = logger;

        // Configure retry policy: 3 retries with exponential backoff
        mRetryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TimeoutRejectedException>()
            .WaitAndRetryAsync(
                3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryCount, context) =>
                {
                    mLogger.LogWarning(exception,
                        "Retry {RetryCount} after {Delay}s", retryCount, timeSpan.TotalSeconds);
                });
    }

    public async Task<string> SendPromptAsync(string prompt, int timeoutSeconds)
    {
        if (prompt == null)
            throw new ArgumentNullException(nameof(prompt));

        HttpResponseMessage response = await mRetryPolicy.ExecuteAsync(async () =>
        {
            using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds)))
            {
                HttpResponseMessage result = await mHttpClient.PostAsJsonAsync("/api/prompt", new { prompt }, cts.Token);
                result.EnsureSuccessStatusCode();
                return result;
            }
        });

        return await response.Content.ReadAsStringAsync();
    }
}
```

### AI Specific Resilience

- Provide configuration to limit token input size
- Provide retry and circuit breaker pattern for AI services
- Implement graceful degradation when AI is unavailable
- Log all AI interactions for debugging and audit

## Database

All controllers must NOT operate with database or ORM directly. All database access must be implemented as a boundary (facade) class with interfaces which reflect the needs of controllers, not capabilities of the database.

This ensures it's possible to:
- Transparently replace implementation from one database to another
- Have multiple implementations and switch between them via configuration
- Use SQLite implementation for integration and end-to-end tests

### Database Boundary Pattern

```csharp
// Interface defined by controller needs, not database capabilities
public interface IUserDataBoundary
{
    Task<User> FindUserByEmailAsync(string email);
    Task<bool> SaveUserAsync(User user);
}

// Implementation using Gehtsoft.EF
public class UserDatabaseBoundary : IUserDataBoundary
{
    private readonly IDbConnectionFactory mConnectionFactory;
    private readonly ILogger<UserDatabaseBoundary> mLogger;

    public UserDatabaseBoundary(IDbConnectionFactory connectionFactory, ILogger<UserDatabaseBoundary> logger)
    {
        if (connectionFactory == null)
            throw new ArgumentNullException(nameof(connectionFactory));
        if (logger == null)
            throw new ArgumentNullException(nameof(logger));

        mConnectionFactory = connectionFactory;
        mLogger = logger;
    }

    public async Task<User> FindUserByEmailAsync(string email)
    {
        if (email == null)
            throw new ArgumentNullException(nameof(email));

        try
        {
            using (IDbConnection connection = mConnectionFactory.CreateConnection())
            {
                // Use Gehtsoft.EF query builder (see gehtsoft-ef-reference.md)
                // Protection against SQL injection is handled by parameterized queries
                User user = await connection.QuerySingleOrDefaultAsync<User>(
                    "SELECT * FROM Users WHERE Email = @Email",
                    new { Email = email });

                return user;
            }
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Failed to find user by email: {Email}", email);
            throw;
        }
    }

    public async Task<bool> SaveUserAsync(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        try
        {
            using (IDbConnection connection = mConnectionFactory.CreateConnection())
            {
                // Use parameterized queries to prevent SQL injection
                int rowsAffected = await connection.ExecuteAsync(
                    "INSERT INTO Users (Email, Name) VALUES (@Email, @Name)",
                    user);

                return rowsAffected > 0;
            }
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Failed to save user");
            throw;
        }
    }
}

// SQLite implementation for testing
public class SqliteUserDataBoundary : IUserDataBoundary
{
    // Same interface, different implementation
    // Used in integration tests
}
```

### SQL Injection Protection

**CRITICAL**: Always use parameterized queries. Never concatenate user input into SQL strings.

```csharp
// ❌ VULNERABLE - DO NOT DO THIS
string sql = $"SELECT * FROM Users WHERE Email = '{email}'";

// ✅ SAFE - Always use parameters
string sql = "SELECT * FROM Users WHERE Email = @Email";
var parameters = new { Email = email };
```

## AI Integration

AI interoperation must be hidden behind a facade that isolates the rest of the application from AI-specific operations.

The facade must be:
- **AI engine agnostic** (possible to support Claude, ChatGPT, Grok without refactoring)
- **Prompt builder abstraction** (prompts tailored to different AI engines)
- **User input isolation** (user input treated as content, never as instructions)

### AI Boundary Pattern

```csharp
// AI engine agnostic interface
public interface IAiServiceBoundary
{
    Task<string> GenerateResponseAsync(string prompt, AiRequestOptions options);
}

// Prompt builder abstraction
public interface IPromptBuilder
{
    string BuildPrompt(string userContent, string systemInstructions);
}

// Claude-specific implementation
public class ClaudePromptBuilder : IPromptBuilder
{
    public string BuildPrompt(string userContent, string systemInstructions)
    {
        // User content is NEVER treated as instructions
        // Always wrapped/escaped to be treated as content only
        return $"{systemInstructions}\n\nUser content to analyze:\n```\n{userContent}\n```";
    }
}

// ChatGPT-specific implementation
public class ChatGptPromptBuilder : IPromptBuilder
{
    public string BuildPrompt(string userContent, string systemInstructions)
    {
        // Different prompt format for ChatGPT
        return $"System: {systemInstructions}\n\nAnalyze the following user content:\n{userContent}";
    }
}

// Claude implementation
public class ClaudeAiServiceBoundary : IAiServiceBoundary
{
    private readonly HttpClient mHttpClient;
    private readonly IPromptBuilder mPromptBuilder;
    private readonly ILogger<ClaudeAiServiceBoundary> mLogger;

    public async Task<string> GenerateResponseAsync(string prompt, AiRequestOptions options)
    {
        // Implementation details...
    }
}

// Easy to add new AI providers
public class ChatGptAiServiceBoundary : IAiServiceBoundary
{
    // Different implementation, same interface
}
```

### User Input Safety

**CRITICAL**: User input must NEVER be treated as AI instructions.

```csharp
public class SafeAiController : IAiController
{
    private readonly IAiServiceBoundary mAiService;
    private readonly IPromptBuilder mPromptBuilder;

    public async Task<string> AnalyzeUserTextAsync(string userText)
    {
        if (userText == null)
            throw new ArgumentNullException(nameof(userText));

        // User text is wrapped/escaped - treated as content, not instructions
        string safePrompt = mPromptBuilder.BuildPrompt(
            userText,
            "Analyze the sentiment of the provided text");

        return await mAiService.GenerateResponseAsync(safePrompt, new AiRequestOptions());
    }
}
```

## Server Architecture

Use ASP.NET Core Web API.

One server must serve both:
- Static front-end content (HTML, CSS, client-side scripts)
- REST API endpoints (back-end logic)

### REST API Requirements

- **Stateless**: No server-side session state
- **Client Session State Pattern**: Use JWT tokens or signed session identifiers stored in client (localStorage or httpOnly cookies)
- **Authentication**: JWT tokens with proper validation
- **Authorization**: Role-based or claims-based

### Security Requirements

The system must implement:
- **HTML escaping** for all user-generated content displayed
- **CSRF tokens** for form submissions
- **CORS configuration** for API access
- **Security headers**: `Content-Security-Policy`, `X-Content-Type-Options`, `X-Frame-Options`, `Strict-Transport-Security`
- **Input validation** at API boundary (format validation in endpoints, logical validation in validators)

### Server Configuration Example

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(Program));

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://yourdomain.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Configure JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

var app = builder.Build();

// Configure middleware
app.UseHttpsRedirection();

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'");
    context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000");
    await next();
});

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

// Serve static files
app.UseStaticFiles();

// Map API endpoints
app.MapControllers();

// Fallback to index.html for SPA routing
app.MapFallbackToFile("index.html");

app.Run();
```

## API Endpoint Structure

API Controllers/Endpoints must be thin and only:
1. Deserialize JSON to DTOs
2. Validate request format (via model validation attributes)
3. Map DTOs to entities using AutoMapper
4. Call ECB Controllers for business logic
5. Map results back to DTOs
6. Serialize response to JSON

**Logical/business validation** must be done via separate Validator classes, not in endpoints.

### Endpoint Example

```csharp
[ApiController]
[Route("api/[controller]")]
public class UserEndpoint : ControllerBase
{
    private readonly IUserRegistrationController mController;
    private readonly IUserValidator mValidator;
    private readonly IMapper mMapper;
    private readonly ILogger<UserEndpoint> mLogger;

    public UserEndpoint(
        IUserRegistrationController controller,
        IUserValidator validator,
        IMapper mapper,
        ILogger<UserEndpoint> logger)
    {
        mController = controller;
        mValidator = validator;
        mMapper = mapper;
        mLogger = logger;
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="request">The user registration data.</param>
    /// <returns>The created user information.</returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegistrationRequest request)
    {
        // 1. Format validation (automatic via model validation)
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // 2. Map DTO to entity
        User user = mMapper.Map<User>(request);

        // 3. Logical validation via validator
        ValidationResult validationResult = mValidator.Validate(user);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        // 4. Call business logic
        User createdUser = await mController.RegisterUserAsync(user);

        // 5. Map entity back to DTO
        UserDto userDto = mMapper.Map<UserDto>(createdUser);

        // 6. Return response
        return CreatedAtAction(nameof(GetUser), new { id = userDto.Id }, userDto);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        User user = await mController.GetUserAsync(id);
        if (user == null)
            return NotFound();

        UserDto userDto = mMapper.Map<UserDto>(user);
        return Ok(userDto);
    }
}
```

## AutoMapper Configuration

Use AutoMapper to map between DTOs and Entities.

```csharp
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // DTO to Entity mappings
        CreateMap<UserRegistrationRequest, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        // Entity to DTO mappings
        CreateMap<User, UserDto>();

        CreateMap<ChatSession, ChatSessionDto>()
            .ForMember(dest => dest.MessageCount, opt => opt.MapFrom(src => src.Messages.Count));
    }
}
```

## Validation Pattern

Separate validators for business/logical validation.

```csharp
public interface IUserValidator
{
    ValidationResult Validate(User user);
}

public class UserValidator : IUserValidator
{
    private readonly IUserDataBoundary mUserDataBoundary;

    public UserValidator(IUserDataBoundary userDataBoundary)
    {
        if (userDataBoundary == null)
            throw new ArgumentNullException(nameof(userDataBoundary));

        mUserDataBoundary = userDataBoundary;
    }

    public ValidationResult Validate(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        ValidationResult result = new ValidationResult();

        // Business rule: email must be unique
        User existingUser = mUserDataBoundary.FindUserByEmailAsync(user.Email).Result;
        if (existingUser != null)
            result.AddError("Email", "Email already registered");

        // Business rule: name must not contain special characters
        if (user.Name != null && user.Name.Any(c => !char.IsLetterOrDigit(c) && c != ' '))
            result.AddError("Name", "Name must only contain letters and spaces");

        return result;
    }
}
```

## Configuration

Use `IConfiguration` behind semantic, type-safe configuration facades.

Each configuration section should have its own facade class that:
- Encapsulates configuration access
- Provides type-safe properties
- Handles parsing and validation
- Provides meaningful defaults

### Configuration Facade Pattern

```csharp
// Configuration facade interface
public interface IClaudeConfiguration
{
    string ApiKey { get; }
    string ApiUrl { get; }
    int TimeoutSeconds { get; }
    int MaxTokens { get; }
    int MaxRetries { get; }
}

// Configuration facade implementation
public class ClaudeConfiguration : IClaudeConfiguration
{
    private readonly IConfiguration mConfiguration;

    public ClaudeConfiguration(IConfiguration configuration)
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        mConfiguration = configuration;
    }

    public string ApiKey =>
        mConfiguration["Claude:ApiKey"] ??
        throw new InvalidOperationException("Claude API key not configured");

    public string ApiUrl =>
        mConfiguration["Claude:ApiUrl"] ?? "https://api.anthropic.com";

    public int TimeoutSeconds
    {
        get
        {
            string value = mConfiguration["Claude:TimeoutSeconds"];
            if (string.IsNullOrEmpty(value))
                return 30; // Default

            if (!int.TryParse(value, out int result))
                throw new InvalidOperationException($"Invalid timeout value: {value}");

            return result;
        }
    }

    public int MaxTokens
    {
        get
        {
            string value = mConfiguration["Claude:MaxTokens"];
            return string.IsNullOrEmpty(value) ? 4096 : int.Parse(value);
        }
    }

    public int MaxRetries
    {
        get
        {
            string value = mConfiguration["Claude:MaxRetries"];
            return string.IsNullOrEmpty(value) ? 3 : int.Parse(value);
        }
    }
}

// Registration in DI
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<IClaudeConfiguration, ClaudeConfiguration>();
}

// Usage in a boundary
public class ClaudeApiGateway : IClaudeApiGateway
{
    private readonly HttpClient mHttpClient;
    private readonly IClaudeConfiguration mConfig;

    public ClaudeApiGateway(HttpClient httpClient, IClaudeConfiguration config)
    {
        mHttpClient = httpClient;
        mConfig = config;

        mHttpClient.BaseAddress = new Uri(mConfig.ApiUrl);
        mHttpClient.DefaultRequestHeaders.Add("x-api-key", mConfig.ApiKey);
        mHttpClient.Timeout = TimeSpan.FromSeconds(mConfig.TimeoutSeconds);
    }
}
```

### appsettings.json Structure

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Claude": {
    "ApiKey": "your-api-key",
    "ApiUrl": "https://api.anthropic.com",
    "TimeoutSeconds": 30,
    "MaxTokens": 4096,
    "MaxRetries": 3
  },
  "Database": {
    "ConnectionString": "Data Source=app.db",
    "Provider": "SQLite",
    "CommandTimeout": 30
  },
  "Jwt": {
    "Key": "your-secret-key-here-min-32-chars",
    "Issuer": "YourApp",
    "Audience": "YourAppUsers",
    "ExpirationMinutes": 60
  }
}
```

## Logging

Use `ILogger<T>` interface with Serilog as implementation.

### Logging Configuration

```csharp
// Program.cs
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();
```

### Logging Usage

```csharp
public class UserRegistrationController : IUserRegistrationController
{
    private readonly IUserDataBoundary mUserData;
    private readonly ILogger<UserRegistrationController> mLogger;

    public UserRegistrationController(
        IUserDataBoundary userData,
        ILogger<UserRegistrationController> logger)
    {
        mUserData = userData;
        mLogger = logger;
    }

    public async Task<User> RegisterUserAsync(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        mLogger.LogInformation("Registering new user with email: {Email}", user.Email);

        try
        {
            bool success = await mUserData.SaveUserAsync(user);
            if (success)
            {
                mLogger.LogInformation("Successfully registered user {UserId}", user.Id);
                return user;
            }
            else
            {
                mLogger.LogWarning("Failed to register user with email: {Email}", user.Email);
                throw new InvalidOperationException("User registration failed");
            }
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Error during user registration for email: {Email}", user.Email);
            throw;
        }
    }
}
```

### Structured Logging Best Practices

- Use structured logging with named parameters: `{Email}`, `{UserId}`
- Log at appropriate levels: `Debug`, `Information`, `Warning`, `Error`, `Critical`
- Include context in log messages
- Log exceptions with full stack traces
- Avoid logging sensitive data (passwords, API keys, PII)

## Front-End Architecture

The front-end must be a set of static HTML pages using:
- HTML5/CSS3
- jQuery
- Bootstrap

One user form corresponds to one page for simplicity and maintainability.

### Front-End Security
- HTML escape all user-generated content before display
- Use CSRF tokens for all form submissions
- Validate all inputs client-side (as UX enhancement, not security)
- Store JWT tokens securely (httpOnly cookies preferred over localStorage)

### Example Front-End Structure

```
wwwroot/
├── index.html
├── login.html
├── register.html
├── dashboard.html
├── css/
│   ├── bootstrap.min.css
│   └── app.css
├── js/
│   ├── jquery.min.js
│   ├── bootstrap.min.js
│   └── app.js
└── images/
```

## Testing Strategy

### Integration Testing
- Use SQLite boundary implementation for database
- Test complete flows through API endpoints
- Mock external services (AI, third-party APIs)
- Test authentication and authorization

### Unit Testing
- Test ECB Controllers in isolation
- Test validators independently
- Test configuration facades
- Mock all boundaries

## Summary Checklist

For every new feature, ensure:

- [ ] ECB pattern applied (Entities, Boundaries, Controllers separated)
- [ ] API endpoints are thin (just routing and mapping)
- [ ] Business logic in ECB Controllers
- [ ] Validation in separate Validator classes
- [ ] AutoMapper for DTO ↔ Entity mapping
- [ ] All public method arguments validated
- [ ] Async operations with timeouts
- [ ] Error handling with specific exceptions
- [ ] Structured logging with ILogger
- [ ] Configuration behind semantic facades
- [ ] Database access through boundaries
- [ ] SQL injection protection (parameterized queries)
- [ ] User input never treated as AI instructions
- [ ] Security headers configured
- [ ] XML documentation on all public APIs
- [ ] Integration tests with SQLite

