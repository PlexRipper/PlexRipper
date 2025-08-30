using Reaparr.Application;
using Reaparr.Environment;
using Reaparr.FluentResultExtensions;
using Reaparr.Logging;
using ILog = Reaparr.Logging.ILog;

namespace Reaparr.WebAPI;

/// <summary>
///  The main class entry point for the application.
/// </summary>
public class Program
{
    private static readonly ILog _log = new LogConfig().CreateLogInstance(typeof(Program));

    /// <summary>
    ///  The main method entry point for the application.
    /// </summary>
    /// <param name="args"></param>
    public static void Main(string[] args)
    {
        try
        {
            _log.InformationLine("Starting Reaparr!");

            LogManager.SetupLogging(EnvironmentExtensions.GetLogLevel());
            FluentResultConfiguration.Setup();

            _log.Information(
                "Currently running {Channel} version {Version} on {CurrentOS}",
                EnvironmentExtensions.IsDevRelease() ? "DEVELOPMENT" : "STABLE",
                EnvironmentExtensions.GetVersion(),
                OsInfo.CurrentOS
            );

            AppExtensions.LogIdentity();

            _log.InformationLine("Initiating boot process");

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
            _log.FatalLine("Reaparr crashed due to exception!");
            Result.Fail(new ExceptionalError(e)).LogFatal();
            System.Environment.Exit(2);
        }
        finally
        {
            // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
            LogManager.CloseAndFlush();
        }
    }

    private static void FailedToStart(Result result)
    {
        _log.FatalLine("Reaparr failed to start!");

        result.LogFatal();

        System.Environment.Exit(1);
    }
}
