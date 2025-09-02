using Reaparr.Application;
using Reaparr.Application.Contracts;
using Reaparr.Environment;

namespace Reaparr.WebAPI;

/// <summary>
/// The Boot class is used to sequentially start various processes needed to start Reaparr.
/// </summary>
public class Boot : IHostedService
{
    #region Fields

    private readonly Serilog.ILogger _log;
    private readonly ICommandExecutor _commandExecutor;

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
        IHostApplicationLifetime appLifetime,
        ISchedulerService schedulerService,
        IDownloadQueue downloadQueue
    )
    {
        _log = log.ForContext<Boot>();
        _commandExecutor = commandExecutor;
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
        if (EnvironmentExtensions.GetPuid() == 911 && EnvironmentExtensions.GetPgid() == 1001)
        {
            _log.Here()
                .Error("Reaparr has invalid PUID and PGID values and thus has defaulted to root, this is not allowed");
            TerminateApplication();
            return;
        }

        var defaultUser = await _commandExecutor.Send(new CreateDefaultAppUserCommand(), CancellationToken.None);
        if (defaultUser.IsFailed)
        {
            TerminateApplication();
            return;
        }

        _downloadQueue.Setup();

        await _schedulerService.SetupAsync();

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
        await _schedulerService.StopAsync();
    }

    #endregion

    #region Private Methods

    private void OnStarted()
    {
        _log.Here().Debug("Boot.OnStarted has been called");

        // Perform post-startup activities here
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
