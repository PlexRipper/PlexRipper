using System.Net;
using Application.Contracts;
using Environment;
using Logging.Interface;
using PlexRipper.Application;

namespace PlexRipper.WebAPI;

/// <summary>
/// The Boot class is used to sequentially start various processes needed to start PlexRipper.
/// </summary>
public class Boot : IHostedService
{
    #region Fields

    private readonly ILog _log;
    private readonly IMediator _mediator;

    private readonly IHostApplicationLifetime _appLifetime;

    private readonly ISchedulerService _schedulerService;

    private readonly IDownloadQueue _downloadQueue;

    #endregion

    #region Constructor

    /// <summary>
    /// The Boot class is used to sequentially start various processes needed to start PlexRipper.
    /// </summary>
    public Boot(
        ILog log,
        IMediator mediator,
        IHostApplicationLifetime appLifetime,
        ISchedulerService schedulerService,
        IDownloadQueue downloadQueue
    )
    {
        _log = log;
        _mediator = mediator;
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
        ServicePointManager.DefaultConnectionLimit = 1000;

        if (EnvironmentExtensions.GetPuid() == 911 && EnvironmentExtensions.GetPgid() == 1001)
        {
            _log.ErrorLine(
                "PlexRipper has invalid PUID and PGID values and thus has defaulted to root, this is not allowed"
            );
            TerminateApplication();
            return;
        }

        var defaultUser = await CreateDefaultAppUser();
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
        _log.InformationLine("PlexRipper has been shutdown! R.I.P.");
    }

    private async Task<Result> CreateDefaultAppUser() => await _mediator.Send(new CreateDefaultAppUserCommand());

    #endregion
}
