# Solution Structure

The solution must be located in the main folder of the project and be named Gehtsoft.FourCDesigner.sln

The main server project must be located in Gehtsoft.FourCDesigner subfolder.

The unit and integration tests project be located in Gehtsoft.FourCDesigner.Tests subfolder.

The API test must be located project in Gehtsoft.FourCDesigner.ApiTests subfolder.

The E2E test must be located project in Gehtsoft.FourCDesigner.UITests subfolder.

# Gehtsoft.FourCDesigner Project Structure

The project is ASP.NET Core Web API server.

The root folder of the project contains:
- `Program.cs` - Application entry point
- `Startup.cs` - Service configuration and middleware pipeline
- `GlobalSuppressions.cs` - Code analysis suppressions

The rest must be organized in the following folders:

## Current Folder Structure

```
Gehtsoft.FourCDesigner/
├── Program.cs              - Application entry point
├── Startup.cs              - Service and middleware configuration
├── GlobalSuppressions.cs   - Code analysis suppressions
├── Config/                 - Configuration classes (not yet implemented)
├── Controllers/            - MVC/API controllers (not yet implemented)
│   └── Data/              - DTOs for requests and responses
├── Entities/               - Database entities (Gehtsoft.EF entities)
├── Logic/                  - ECB Controllers (business logic)
│   ├── User/              - User-related controllers and configuration
│   └── Session/           - Session management controllers and configuration
├── Boundary/               - Facades for external services (not yet implemented)
├── Dao/                    - Data access interfaces and implementations
├── Middleware/             - Custom middleware, attributes, and filters
│   ├── Authorization/     - Authorization attributes and filters
│   └── Throttling/        - Rate limiting configuration and extensions
├── Utils/                  - Common utilities and helpers
├── wwwroot/                - Static web content
│   ├── css/               - Stylesheets
│   ├── js/                - JavaScript files
│   └── images/            - Static images
└── Logs/                   - Application logs (runtime generated)
```

## Folder Descriptions

- **Config/** - Configuration facade classes implementing configuration interfaces (e.g., `IDbConfiguration`, `ISessionSettings`)
- **Controllers/** - ASP.NET Core API Controllers/Endpoints (thin HTTP routing layer)
- **Controllers/Data/** - DTOs (Data Transfer Objects) for API requests and responses
- **Entities/** - Gehtsoft.EF database entities (pure data holders)
- **Logic/** - ECB Controllers containing business logic (stateless, orchestrate operations)
  - Organized by feature/domain (e.g., User, Session)
  - Each subfolder contains related controllers, interfaces, and configuration
- **Boundary/** - Boundary facades for external services (e.g., SMTP, AI engines, file system)
- **Dao/** - Data Access Objects - interfaces and implementations for database operations
- **Middleware/** - Custom ASP.NET Core middleware, action filters, and attributes
  - **Authorization/** - Session-based authorization attributes and filters
  - **Throttling/** - Rate limiting middleware and configuration
- **Utils/** - Shared utilities, helpers, and extension methods
- **wwwroot/** - Static files served by the web server (HTML, CSS, JavaScript, images)
- **Logs/** - Runtime-generated log files (not checked into source control)

## Naming Conventions for Subfolders

When organizing code within Logic, Middleware, or other folders:
- Group by **feature/domain** (e.g., `Logic/User/`, `Logic/Session/`)
- Each feature folder should contain:
  - Interface definitions (e.g., `IUserController.cs`, `IUserConfiguration.cs`)
  - Implementation classes (e.g., `UserController.cs`, `UserConfiguration.cs`)
  - Related configuration classes
  - Feature-specific data structures if needed




