using Reaparr.Application;
using Reaparr.Application.Contracts;
using Reaparr.Environment;
using Reaparr.Logging;

namespace Reaparr.WebAPI;

/// <summary>
/// The Boot class is used to sequentially start various processes needed to start Reaparr.
/// </summary>
public class Boot : IHostedService
{
    #region Fields

    private readonly ILog _log;
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
        ILog log,
        ICommandExecutor commandExecutor,
        IHostApplicationLifetime appLifetime,
        ISchedulerService schedulerService,
        IDownloadQueue downloadQueue
    )
    {
        _log = log;
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
            _log.ErrorLine(
                "Reaparr has invalid PUID and PGID values and thus has defaulted to root, this is not allowed"
            );
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

        _log.InformationLine("Finished Initiating boot process");
    }

    private void TerminateApplication()
    {
        _log.ErrorLine("An error occurred during the boot process, terminating application");
        _appLifetime.StopApplication();
    }

    /// <inheritdoc/>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _log.InformationLine("Shutting down the container");
        await _schedulerService.StopAsync();
    }

    #endregion

    #region Private Methods

    private void OnStarted()
    {
        _log.DebugLine("Boot.OnStarted has been called");

        // Perform post-startup activities here
    }

    private void OnStopping()
    {
        _log.DebugLine("Boot.OnStopping has been called");

        // Perform on-stopping activities here
    }

    private void OnStopped()
    {
        _log.DebugLine("Boot.OnStopped has been called");

        // Perform post-stopped activities here
        _log.InformationLine("Reaparr has been shutdown! R.I.P.");
    }

    #endregion
}
