using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Gehtsoft.FourCDesigner.Logic.AI.Testing;

/// <summary>
/// Testing implementation of AI driver that uses mock responses from a JSON file.
/// </summary>
public class AITestingDriver : IAIDriver
{
    private readonly IAITestingConfiguration mConfiguration;
    private readonly ILogger<AITestingDriver> mLogger;
    private readonly List<MockOperation> mMockOperations;

    /// <summary>
    /// Initializes a new instance of the <see cref="AITestingDriver"/> class.
    /// </summary>
    /// <param name="configuration">The testing configuration.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">Thrown when configuration or logger is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the mock file cannot be read or parsed.</exception>
    public AITestingDriver(
        IAITestingConfiguration configuration,
        ILogger<AITestingDriver> logger)
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        if (logger == null)
            throw new ArgumentNullException(nameof(logger));

        mConfiguration = configuration;
        mLogger = logger;
        mMockOperations = new List<MockOperation>();

        LoadMockOperations();
    }

    /// <summary>
    /// Loads mock operations from the JSON file.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the file cannot be read or parsed.</exception>
    private void LoadMockOperations()
    {
        try
        {
            string filePath = mConfiguration.MockFilePath;

            if (!File.Exists(filePath))
            {
                mLogger.LogWarning(
                    "Mock file not found at {FilePath}, using empty mock list",
                    filePath);
                return;
            }

            string jsonContent = File.ReadAllText(filePath);

            List<MockOperation>? operations =
                JsonSerializer.Deserialize<List<MockOperation>>(
                    jsonContent,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (operations != null)
            {
                mMockOperations.AddRange(operations);
                mLogger.LogInformation(
                    "Loaded {Count} mock operations from {FilePath}",
                    mMockOperations.Count,
                    filePath);
            }
        }
        catch (Exception ex)
        {
            mLogger.LogError(
                ex,
                "Failed to load mock operations from {FilePath}",
                mConfiguration.MockFilePath);
            throw new InvalidOperationException(
                $"Failed to load mock operations from {mConfiguration.MockFilePath}",
                ex);
        }
    }

    /// <inheritdoc/>
    public Task<AIResult> ValidateUserInputAsync(string userInput)
    {
        if (userInput == null)
            throw new ArgumentNullException(nameof(userInput));

        mLogger.LogDebug("Validating user input with testing driver");

        AIResult result = FindMatchingResponse("validate", string.Empty, userInput);
        return Task.FromResult(result);
    }

    /// <inheritdoc/>
    public Task<AIResult> GetSuggestionsAsync(string instructions, string userInput)
    {
        if (instructions == null)
            throw new ArgumentNullException(nameof(instructions));

        if (userInput == null)
            throw new ArgumentNullException(nameof(userInput));

        mLogger.LogDebug(
            "Getting suggestions with testing driver: instructions={Instructions}",
            instructions);

        AIResult result = FindMatchingResponse("process", instructions, userInput);
        return Task.FromResult(result);
    }

    /// <summary>
    /// Finds a matching mock response based on operation, request pattern, and user data pattern.
    /// </summary>
    /// <param name="operation">The operation type.</param>
    /// <param name="request">The request or instructions.</param>
    /// <param name="userData">The user data or input.</param>
    /// <returns>The matching AI result.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no matching mock operation is found.</exception>
    private AIResult FindMatchingResponse(
        string operation,
        string request,
        string userData)
    {
        for (int i = 0; i < mMockOperations.Count; i++)
        {
            MockOperation mockOp = mMockOperations[i];

            if (mockOp.Operation != operation)
                continue;

            try
            {
                bool requestMatches = Regex.IsMatch(
                    request,
                    mockOp.RequestPattern,
                    RegexOptions.IgnoreCase);

                bool userDataMatches = Regex.IsMatch(
                    userData,
                    mockOp.UserDataPattern,
                    RegexOptions.IgnoreCase);

                if (requestMatches && userDataMatches)
                {
                    mLogger.LogDebug(
                        "Found matching mock operation: operation={Operation}, " +
                        "requestPattern={RequestPattern}, userDataPattern={UserDataPattern}",
                        operation,
                        mockOp.RequestPattern,
                        mockOp.UserDataPattern);

                    return mockOp.Response;
                }
            }
            catch (Exception ex)
            {
                mLogger.LogWarning(
                    ex,
                    "Invalid regex pattern in mock operation: " +
                    "requestPattern={RequestPattern}, userDataPattern={UserDataPattern}",
                    mockOp.RequestPattern,
                    mockOp.UserDataPattern);
            }
        }

        string errorMessage =
            $"No matching mock operation found for operation={operation}, " +
            $"request={request}, userData={userData}";

        mLogger.LogError(errorMessage);

        throw new InvalidOperationException(errorMessage);
    }
}
