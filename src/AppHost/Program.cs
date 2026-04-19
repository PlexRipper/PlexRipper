using Reaparr.FluentResultExtensions;
using Velopack;

namespace Reaparr.AppHost;

/// <summary>
///  The main class entry point for the application.
/// </summary>
public class Program
{
    private static readonly Serilog.ILogger _log = LogFactory.Create<Program>();

    /// <summary>
    ///  The main method entry point for the application.
    /// </summary>
    /// <param name="args">Command-line arguments passed to the application.</param>
    [STAThread]
    public static async Task Main(string[] args)
    {
        try
        {
            var logBuffer = new LogBufferService();
            var signalRLogConfig = new SignalRLogConfig(logBuffer);

            // Skip logger setup in integration test mode to preserve test logger
            if (!EnvironmentExtensions.IsIntegrationTestMode())
                LogFactory.SetupLogging(EnvironmentExtensions.GetLogLevel(), signalRLogConfig);

            // Must be first: handles installer hooks (install, uninstall, update) and exits early when invoked by the Velopack installer.
            VelopackApp.Build().Run();

            FluentResultConfiguration.Setup();

            _log.Here()
                .Information(
                    "Currently running {Channel} version {Version} on {CurrentOS}",
                    EnvironmentExtensions.IsDevRelease() ? "DEVELOPMENT" : "STABLE",
                    EnvironmentExtensions.GetVersion(),
                    OsInfo.CurrentOS
                );

            AppExtensions.LogIdentity();

            _log.Here().Information("Initiating boot process");

            var builder = WebApplication.CreateBuilder(args);

            builder.Host.ConfigureAutofacBuilder(logBuffer);
            builder.Services.ConfigureServices(builder.Environment);
            var app = builder.Build();

            signalRLogConfig.AttachSignalR(app);

            var configResult = app.SetupConfigFile();
            if (configResult.IsFailed)
            {
                FailedToStart(configResult);
                return;
            }

            var configureDatabase = await app.SetupDatabase();
            if (configureDatabase.IsFailed)
            {
                FailedToStart(configureDatabase);
                return;
            }

            app.ApplyForwardedHeaders();

            app.ConfigureApplication(app.Environment);

            if (EnvironmentExtensions.IsDesktopMode())
            {
                await app.StartAsync();
                try
                {
                    var desktopModeResult = app.Services.GetRequiredService<IDesktopMode>().Setup();
                    if (desktopModeResult.IsFailed)
                        FailedToStart(desktopModeResult);
                }
                finally
                {
                    await app.StopAsync();
                }
            }
            else
            {
                await app.RunAsync();
            }
        }
        catch (Exception e)
        {
            _log.Here().Fatal("Reaparr crashed due to an exception!");
            Result.Fail(new ExceptionalError(e)).LogFatal();
            System.Environment.Exit(2);
        }
        finally
        {
            // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
            LogFactory.CloseAndFlush();
        }
    }

    private static void FailedToStart(Result result)
    {
        _log.Here().Fatal("Reaparr failed to start!");

        result.LogFatal();

        _log.Here().Fatal("Reaparr has been shutdown! R.I.P.");

        System.Environment.Exit(1);
    }
}
