using Environment;
using Logging.Interface;
using PlexRipper.Application;

namespace PlexRipper.WebAPI;

/// <summary>
///  The main class entry point for the application.
/// </summary>
public class Program
{
    private static readonly ILog _log = LogManager.CreateLogInstance(typeof(Program));

    /// <summary>
    ///  The main method entry point for the application.
    /// </summary>
    /// <param name="args"></param>
    public static void Main(string[] args)
    {
        try
        {
            _log.InformationLine("Starting PlexRipper!");

            LogManager.SetupLogging(EnvironmentExtensions.GetLogLevel());

            var version = EnvironmentExtensions.GetVersion();
            _log.Information(
                "Currently running {Channel} version {Version} on {CurrentOS}",
                version.Contains("dev") ? "DEVELOPMENT" : "STABLE",
                version,
                OsInfo.CurrentOS
            );

            AppExtensions.LogIdentity();

            _log.InformationLine("Initiating boot process");

            var builder = WebApplication.CreateBuilder(args);

            builder.Host.ConfigureAutofacBuilder();

            builder.Services.ConfigureServices(builder.Environment);

            var app = builder.Build();

            var configResult = app.ConfigureConfigFile();
            if (configResult.IsFailed)
            {
                FailedToStart(configResult);
                return;
            }

            var configureDatabase = app.ConfigureDatabase();
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
            _log.FatalLine("PlexRipper crashed due to exception!");
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
        _log.FatalLine("PlexRipper failed to start!");

        result.LogFatal();

        System.Environment.Exit(1);
    }
}
