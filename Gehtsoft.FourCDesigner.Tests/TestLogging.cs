using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;

namespace Gehtsoft.FourCDesigner.Tests;

/// <summary>
/// Provides logging infrastructure for tests.
/// </summary>
public static class TestLogging
{
    private static readonly object sLock = new object();
    private static SerilogLoggerFactory? sLoggerFactory;

    /// <summary>
    /// Initializes Serilog with file output for tests.
    /// Only initializes once per test run.
    /// </summary>
    public static void Initialize()
    {
        lock (sLock)
        {
            if (sLoggerFactory != null)
                return;

            // Configure Serilog to log to file
            var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(
                    path: "Logs/test-.log",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            Log.Logger = logger;
            sLoggerFactory = new SerilogLoggerFactory(logger, dispose: true);
        }
    }

    /// <summary>
    /// Creates a logger for the specified type.
    /// </summary>
    /// <typeparam name="T">The type to create logger for.</typeparam>
    /// <returns>A logger instance.</returns>
    public static ILogger<T> CreateLogger<T>()
    {
        Initialize();
        return sLoggerFactory!.CreateLogger<T>();
    }

    /// <summary>
    /// Disposes the logger factory and flushes logs.
    /// </summary>
    public static void Shutdown()
    {
        lock (sLock)
        {
            sLoggerFactory?.Dispose();
            sLoggerFactory = null;
            Log.CloseAndFlush();
        }
    }
}
