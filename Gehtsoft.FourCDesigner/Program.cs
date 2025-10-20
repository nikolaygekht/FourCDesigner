using Serilog;
using Gehtsoft.FourCDesigner.Logic.Config;

namespace Gehtsoft.FourCDesigner
{
    public class Program
    {
        public static IConfiguration mConfiguration;
        public static IConfiguration Configuration => mConfiguration;

        public static void Main(string[] args)
        {
            mConfiguration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("Config/appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("Config/appsettings.local.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"Config/appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            try
            {
                Log.Information("Starting web application");

                using var app = CreateHostBuilder(args).Build();

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            // Create system config to read UseUrls setting (if configuration is available)
            string? useUrls = null;
            if (mConfiguration != null)
            {
                var systemConfig = new SystemConfig(mConfiguration);
                useUrls = systemConfig.UseUrls;
            }

            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    // If mConfiguration is not initialized (e.g., in tests), skip this
                    if (mConfiguration != null)
                    {
                        config.Sources.Clear();
                        config.AddConfiguration(mConfiguration);
                    }
                })
                .ConfigureWebHostDefaults(webHostBuilder =>
                {
                    webHostBuilder.UseKestrel();

                    // Apply UseUrls only if configuration is available
                    if (!string.IsNullOrEmpty(useUrls))
                        webHostBuilder.UseUrls(useUrls);

                    webHostBuilder.UseStartup<Startup>();
                })
                .UseSerilog(
                    (hostingContext, loggerConfig) =>
                        loggerConfig
                            .ReadFrom.Configuration(hostingContext.Configuration)
                            .Enrich.FromLogContext(),
                    writeToProviders: true);
        }

    }
}
