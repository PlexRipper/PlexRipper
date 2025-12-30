using Reaparr.Application;
using Reaparr.Environment;
using Reaparr.FluentResultExtensions;

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
    /// <param name="args"></param>
    public static void Main(string[] args)
    {
        try
        {
            _log.Here().Information("Starting Reaparr!");

            // Skip logger setup in integration test mode to preserve test logger
            if (!EnvironmentExtensions.IsIntegrationTestMode())
                LogFactory.SetupLogging(EnvironmentExtensions.GetLogLevel());
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

            builder.Host.ConfigureAutofacBuilder();

            builder.Services.ConfigureServices(builder.Environment);

            var app = builder.Build();

            var configResult = app.SetupConfigFile();
            if (configResult.IsFailed)
            {
                FailedToStart(configResult);
                return;
            }

            var configureDatabase = app.SetupDatabase();
            if (configureDatabase.IsFailed)
            {
                FailedToStart(configureDatabase);
                return;
            }

            app.ConfigureApplication(app.Environment);

            app.Run();
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
