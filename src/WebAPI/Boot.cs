using System.Net;
using System.Runtime.InteropServices;
using Application.Contracts;
using Data.Contracts;
using Logging.Interface;
using PlexRipper.Application;
using Settings.Contracts;

namespace PlexRipper.WebAPI;

/// <summary>
/// The Boot class is used to sequentially start various processes needed to start PlexRipper.
/// </summary>
public class Boot : IHostedService
{
    #region Fields

    private readonly ILog _log;
    private readonly IMediator _mediator;

    private readonly IPlexRipperDbContextManager _dbContextManager;

    private readonly IConfigManager _configManager;

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
        IPlexRipperDbContextManager dbContextManagerManager,
        IHostApplicationLifetime appLifetime,
        IConfigManager configManager,
        ISchedulerService schedulerService,
        IDownloadQueue downloadQueue
    )
    {
        _log = log;
        _mediator = mediator;
        _dbContextManager = dbContextManagerManager;
        _configManager = configManager;
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
        _log.InformationLine("Initiating boot process");
        ServicePointManager.DefaultConnectionLimit = 1000;

        var configSetupResult = _configManager.Setup();
        if (configSetupResult.IsFailed)
        {
            await StopAsync(cancellationToken);
            return;
        }

        var databaseSetupResult = _dbContextManager.Setup();
        if (databaseSetupResult.IsFailed)
        {
            await StopAsync(cancellationToken);
            return;
        }

        await CreateDefaultAppUser();

        _downloadQueue.Setup();

        await _schedulerService.SetupAsync();

        _log.InformationLine("Finished Initiating boot process");
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

    private async Task CreateDefaultAppUser()
    {
        await _mediator.Send(new CreateDefaultAppUserCommand());
    }

    #endregion
}
