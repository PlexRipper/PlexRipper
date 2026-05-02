using Reaparr.FluentResultExtensions;
using Velopack;

namespace Reaparr.AppHost;

/// <summary>
///  The main class entry point for the application.
/// </summary>
public class Program
{
    private static readonly AppRuntimeInfo _appRuntimeInfo = new();
    private static readonly AppBuildInfo _appBuildInfo = new();
    private static PathProvider? _pathProvider;
    private static DesktopStartupFailureDialog? _failureDialog;

    // ReSharper disable once InconsistentNaming
    private static Serilog.ILogger _log => Log.ForContext(typeof(Program));

    /// <summary>
    ///  The main method entry point for the application.
    /// </summary>
    /// <param name="args">Command-line arguments passed to the application.</param>
    [STAThread]
    public static async Task Main(string[] args)
    {
        // This should be run at the very start before anything is initiated
        var velopackResult = Result.Try(() => VelopackApp.Build().Run());
        if (velopackResult.IsFailed)
            FailedToStart(velopackResult);

        var testExceptionResult = Result.Fail(
            new ExceptionalError(new AbandonedMutexException("Manual startup failure dialog test"))
        );
        if (testExceptionResult.IsFailed)
            FailedToStart(testExceptionResult);

        try
        {
            _pathProvider = new PathProvider(_appBuildInfo, _appRuntimeInfo);
            var logBuffer = new LogBufferService();
            var signalRLogConfig = new SignalRLogConfig(_appRuntimeInfo, _pathProvider, logBuffer);
            _failureDialog = new DesktopStartupFailureDialog(_appBuildInfo, _pathProvider, logBuffer);
                
            // Skip logger setup in integration test mode to preserve test logger
            if (!_appRuntimeInfo.IsIntegrationTestMode)
                LogFactory.SetupLogging(signalRLogConfig, _appRuntimeInfo, _appRuntimeInfo.LogLevel);

            _log.Here().Information("Initiating Reaparr boot process");

            _log.Here()
                .Information(
                    "Starting Reaparr {Version} ({Channel}) in {RuntimeMode} mode on {CurrentOS} ({RuntimeIdentifier})",
                    _appBuildInfo.InformationalVersion,
                    _appBuildInfo.IsDevRelease ? "DEVELOPMENT" : "STABLE",
                    _appBuildInfo.RuntimeMode,
                    _appBuildInfo.CurrentOS,
                    _appBuildInfo.RuntimeIdentifier
                );

            AppExtensions.LogIdentity(_appBuildInfo, _appRuntimeInfo);

            FluentResultConfiguration.Setup();

            var builder = WebApplication.CreateBuilder(args);

            builder.Host.ConfigureAutofacBuilder(logBuffer);
            builder.Services.ConfigureServices(builder.Environment, _appRuntimeInfo);
            var app = builder.Build();

            if (!_appRuntimeInfo.IsIntegrationTestMode)
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

            app.ConfigureApplication(app.Environment, _appBuildInfo, _appRuntimeInfo);

            if (_appBuildInfo.IsDesktopMode && !_appRuntimeInfo.IsIntegrationTestMode)
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
            FailedToStart(Result.Fail(new ExceptionalError(e)));
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

        if (_appBuildInfo.IsDesktopMode)
        {
            try
            {
                _failureDialog?.Show(result);
            }
            catch (Exception dialogException)
            {
                _log.Here().Error(dialogException, "Failed to show startup failure dialog");
            }
        }

        _log.Here().Fatal("Reaparr has been shutdown! R.I.P.");

        System.Environment.Exit(1);
    }
}
