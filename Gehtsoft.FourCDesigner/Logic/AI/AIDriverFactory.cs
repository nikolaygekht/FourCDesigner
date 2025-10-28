using Gehtsoft.FourCDesigner.Logic.AI.Testing;
using Gehtsoft.FourCDesigner.Logic.AI.Ollama;
using Gehtsoft.FourCDesigner.Logic.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Gehtsoft.FourCDesigner.Logic.AI;

/// <summary>
/// Factory for creating AI driver instances based on configuration.
/// </summary>
public class AIDriverFactory
{
    private readonly IAIConfiguration mConfiguration;
    private readonly IServiceProvider mServiceProvider;
    private readonly ILogger<AIDriverFactory> mLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AIDriverFactory"/> class.
    /// </summary>
    /// <param name="configuration">The AI configuration.</param>
    /// <param name="serviceProvider">The service provider for resolving dependencies.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when configuration, serviceProvider, or logger is null.
    /// </exception>
    public AIDriverFactory(
        IAIConfiguration configuration,
        IServiceProvider serviceProvider,
        ILogger<AIDriverFactory> logger)
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        if (serviceProvider == null)
            throw new ArgumentNullException(nameof(serviceProvider));

        if (logger == null)
            throw new ArgumentNullException(nameof(logger));

        mConfiguration = configuration;
        mServiceProvider = serviceProvider;
        mLogger = logger;
    }

    /// <summary>
    /// Creates an AI driver instance based on the configured driver type.
    /// </summary>
    /// <returns>An instance of IAIDriver.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the configured driver type is not supported.
    /// </exception>
    public IAIDriver CreateDriver()
    {
        string driverType = mConfiguration.Driver.ToLowerInvariant();

        mLogger.LogDebug("Creating AI driver of type: {DriverType}", driverType);

        switch (driverType)
        {
            case "mock":
                return CreateTestingDriver();

            case "ollama":
                return CreateOllamaDriver();

            case "openai":
                return CreateOpenAIDriver();

            default:
                string errorMessage =
                    $"Unsupported AI driver type: {driverType}. " +
                    "Supported types are: mock, ollama, openai";
                mLogger.LogError(errorMessage);
                throw new InvalidOperationException(errorMessage);
        }
    }

    /// <summary>
    /// Creates a testing/mock driver instance.
    /// </summary>
    /// <returns>An instance of AITestingDriver.</returns>
    private IAIDriver CreateTestingDriver()
    {
        IConfiguration configuration =
            mServiceProvider.GetRequiredService<IConfiguration>();

        string configName = mConfiguration.Config;

        IAITestingConfiguration config =
            new AITestingConfiguration(configuration, configName);

        ILogger<AITestingDriver> logger =
            mServiceProvider.GetRequiredService<ILogger<AITestingDriver>>();

        return new AITestingDriver(config, logger);
    }

    /// <summary>
    /// Creates an Ollama driver instance.
    /// </summary>
    /// <returns>An instance of AIOllamaDriver.</returns>
    private IAIDriver CreateOllamaDriver()
    {
        IConfiguration configuration =
            mServiceProvider.GetRequiredService<IConfiguration>();

        string configName = mConfiguration.Config;

        IAIOllamaConfiguration config =
            new AIOllamaConfiguration(configuration, configName);

        HttpClient httpClient = new HttpClient();

        ILogger<AIOllamaDriver> logger =
            mServiceProvider.GetRequiredService<ILogger<AIOllamaDriver>>();

        return new AIOllamaDriver(config, httpClient, logger);
    }

    /// <summary>
    /// Creates an OpenAI driver instance.
    /// </summary>
    /// <returns>An instance of AIOpenAIDriver.</returns>
    private IAIDriver CreateOpenAIDriver()
    {
        IConfiguration configuration =
            mServiceProvider.GetRequiredService<IConfiguration>();

        string configName = mConfiguration.Config;

        IAIOpenAIConfiguration config =
            new AIOpenAIConfiguration(configuration, configName);

        HttpClient httpClient = new HttpClient();

        ILogger<AIOpenAIDriver> logger =
            mServiceProvider.GetRequiredService<ILogger<AIOpenAIDriver>>();

        return new AIOpenAIDriver(config, httpClient, logger);
    }
}
