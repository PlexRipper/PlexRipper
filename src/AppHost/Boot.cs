using Reaparr.Application.Contracts;

namespace Reaparr.AppHost;

/// <summary>
/// The Boot class is used to sequentially start various processes needed to start Reaparr.
/// </summary>
public class Boot : IHostedService
{
    #region Fields

    private readonly Serilog.ILogger _log;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IAppRuntimeInfo _appRuntimeInfo;

    private readonly IHostApplicationLifetime _appLifetime;

    private readonly ISchedulerService _schedulerService;

    private readonly IDownloadQueue _downloadQueue;

    #endregion

    #region Constructor

    /// <summary>
    /// The Boot class is used to sequentially start various processes needed to start Reaparr.
    /// </summary>
    public Boot(
        Serilog.ILogger log,
        ICommandExecutor commandExecutor,
        IAppRuntimeInfo appRuntimeInfo,
        IHostApplicationLifetime appLifetime,
        ISchedulerService schedulerService,
        IDownloadQueue downloadQueue
    )
    {
        _log = log.ForContext<Boot>();
        _commandExecutor = commandExecutor;
        _appRuntimeInfo = appRuntimeInfo;
        _appLifetime = appLifetime;
        _schedulerService = schedulerService;
        _downloadQueue = downloadQueue;

        appLifetime.ApplicationStarted.Register(OnStarted);
        appLifetime.ApplicationStopping.Register(OnStopping);
        appLifetime.ApplicationStopped.Register(OnStopped);
    }

    #endregion

    #region Public Methods

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_appRuntimeInfo is { PUID: 911, PGID: 1001 })
        {
            _log.Here()
                .Error("Reaparr has invalid PUID and PGID values and thus has defaulted to root, this is not allowed");
            TerminateApplication();
            return;
        }

        var defaultUser = await _commandExecutor.Send(new CreateDefaultAppUserCommand(), cancellationToken);
        if (defaultUser.IsFailed)
        {
            TerminateApplication();
            return;
        }

        var downloadQueueSetup = _downloadQueue.Setup(_appLifetime.ApplicationStopping);
        if (downloadQueueSetup.IsFailed)
        {
            TerminateApplication();
            return;
        }

        var recoverResult = await _commandExecutor.Send(new RecoverInterruptedDownloadsCommand(), cancellationToken);
        if (recoverResult.IsFailed)
            recoverResult.LogError();

        await _schedulerService.SetupAsync(cancellationToken);

        if (!_appRuntimeInfo.IsIntegrationTestMode)
        {
            var bootQueueKickResult = await _downloadQueue.CheckDownloadQueueForAllServers(cancellationToken);
            if (bootQueueKickResult.IsFailed)
                bootQueueKickResult.LogError();
        }

        _log.Here().Information("Finished Initiating boot process");
    }

    private void TerminateApplication()
    {
        _log.Here().Fatal("An error occurred during the boot process, terminating application");
        _appLifetime.StopApplication();
    }

    /// <inheritdoc/>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _log.Here().Information("Shutting down the container");

        // Stop scheduler first so background jobs can't race with the auto-pause DB queries
        await _schedulerService.StopAsync(cancellationToken);

        var autoPauseResult = await _commandExecutor.Send(new AutoPauseActiveDownloadsCommand(), cancellationToken);
        if (autoPauseResult.IsFailed)
            autoPauseResult.LogError();
    }

    #endregion

    #region Private Methods

    private async void OnStarted()
    {
        _log.Here().Debug("Boot.OnStarted has been called");
        
        var result = await Result.Try(async Task () =>
        {
            await _commandExecutor.Send(new NotifyArrAppsOnStartupCommand(), _appLifetime.ApplicationStopping);
            await _commandExecutor.Send(new WarmupMediaQueryCacheCommand(), _appLifetime.ApplicationStopping);
        }, exception =>
        {
            if (exception is OperationCanceledException canceledException &&
                _appLifetime.ApplicationStopping.IsCancellationRequested)
            {
                _log.Here().Debug("Boot.OnStarted was cancelled because application shutdown was requested");
                return new ExceptionalError("Operation was cancelled", exception);
            }

            _log.Here().Error(exception, "Unexpected error while running post-startup tasks");
            return new ExceptionalError(exception);
        });

        result.LogIfFailed();
    }

    private void OnStopping()
    {
        _log.Here().Debug("Boot.OnStopping has been called");

        // Perform on-stopping activities here
    }

    private void OnStopped()
    {
        _log.Here().Debug("Boot.OnStopped has been called");

        // Perform post-stopped activities here
        _log.Here().Information("Reaparr has been shutdown! R.I.P.");
    }

    #endregion
}