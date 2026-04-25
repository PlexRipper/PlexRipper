using Reaparr.FluentResultExtensions;
using Velopack;

namespace Reaparr.AppHost;

/// <summary>
///  The main class entry point for the application.
/// </summary>
public class Program
{
    private static readonly Serilog.ILogger _log = Log.Logger.ForContext(typeof(Program));

    /// <summary>
    ///  The main method entry point for the application.
    /// </summary>
    /// <param name="args">Command-line arguments passed to the application.</param>
    [STAThread]
    public static async Task Main(string[] args)
    {
        try
        {
            var appBuildInfo = new AppBuildInfo();
            var pathProvider = new PathProvider();
            var logBuffer = new LogBufferService();
            var signalRLogConfig = new SignalRLogConfig(pathProvider, logBuffer);

            // Skip logger setup in integration test mode to preserve test logger
            if (!EnvironmentExtensions.IsIntegrationTestMode())
                LogFactory.SetupLogging(signalRLogConfig, EnvironmentExtensions.GetLogLevel());

            _log.Here().Information("Initiating boot process");

            _log.Here()
                .Information(
                    "Starting Reaparr {Version} ({Channel}) in {RuntimeMode} mode on {CurrentOS} ({RuntimeIdentifier})",
                    appBuildInfo.InformationalVersion,
                    appBuildInfo.IsDevRelease ? "DEVELOPMENT" : "STABLE",
                    appBuildInfo.RuntimeMode,
                    appBuildInfo.CurrentOS,
                    appBuildInfo.RuntimeIdentifier
                );

            AppExtensions.LogIdentity();

            // Must be first after logging: handles installer hooks (install, uninstall, update) and exits early when invoked by the Velopack installer.
            VelopackApp.Build().Run();

            FluentResultConfiguration.Setup();

            var builder = WebApplication.CreateBuilder(args);

            builder.Host.ConfigureAutofacBuilder(logBuffer);
            builder.Services.ConfigureServices(builder.Environment);
            var app = builder.Build();

            if (!EnvironmentExtensions.IsIntegrationTestMode())
                signalRLogConfig.AttachSignalR(app, LogFactory.MinimumLogLevel);

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

            app.ConfigureApplication(app.Environment, appBuildInfo);

            if (appBuildInfo.IsDesktopMode && !EnvironmentExtensions.IsIntegrationTestMode())
            {
                var desktopLifecycleResult = await RunDesktopLifecycleAsync(app);
                if (desktopLifecycleResult.IsFailed)
                    FailedToStart(desktopLifecycleResult);
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

    internal static async Task<Result> RunDesktopLifecycleAsync(
        WebApplication app,
        CancellationToken cancellationToken = default
    )
    {
        var services = app.Services;
        var singleInstanceCoordinator = services.GetRequiredService<IDesktopSingleInstanceCoordinator>();

        if (!singleInstanceCoordinator.TryAcquirePrimaryOwnership())
        {
            _log.Here().Debug("Signaling the existing Reaparr desktop instance");
            return await singleInstanceCoordinator.SignalPrimaryInstanceAsync(cancellationToken);
        }

        await app.StartAsync(cancellationToken);
        try
        {
            _log.Here().Debug("Starting the Reaparr desktop single-instance listener");
            var desktopMode = services.GetRequiredService<IDesktopMode>();
            var listenerResult = singleInstanceCoordinator.StartListener(
                ct =>
                {
                    _log.Here().Debug("Showing the Reaparr desktop window");
                    return desktopMode.ShowMainWindowAsync(ct);
                },
                cancellationToken
            );
            if (listenerResult.IsFailed)
                return listenerResult;

            var desktopModeResult = await desktopMode.StartAsync(cancellationToken);
            if (desktopModeResult.IsFailed)
                return desktopModeResult;

            await desktopMode.WaitForExitAsync(cancellationToken);
            return Result.Ok();
        }
        finally
        {
            await app.StopAsync(cancellationToken);
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
