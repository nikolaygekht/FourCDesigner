using Serilog;

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

            using var app =
                Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                    {
                        config.Sources.Clear();
                        config.AddConfiguration(mConfiguration);
                    })
                .ConfigureWebHostDefaults(webHostBuilder
                        => webHostBuilder
                        .UseKestrel()
                        .UseStartup<Startup>())
                .UseSerilog(
                    (hostingContext, loggerConfig) =>
                        loggerConfig
                            .ReadFrom.Configuration(hostingContext.Configuration)
                            .Enrich.FromLogContext(),
                    writeToProviders: true)
                .Build();

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

    }
}
