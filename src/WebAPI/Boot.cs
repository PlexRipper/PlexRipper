using System.Net;
using Application.Contracts;
using Logging.Interface;
using Settings.Contracts;

namespace PlexRipper.WebAPI;

/// <summary>
/// The Boot class is used to sequentially start various processes needed to start PlexRipper.
/// </summary>
public class Boot : IBoot
{
    #region Fields

    private readonly ILog _log;
    private readonly IHostApplicationLifetime _appLifetime;

    private readonly IConfigManager _configManager;

    private readonly ISchedulerService _schedulerService;

    private readonly IDownloadQueue _downloadQueue;

    #endregion

    #region Constructor

    public Boot(
        ILog log,
        IHostApplicationLifetime appLifetime,
        IConfigManager configManager,
        ISchedulerService schedulerService,
        IDownloadQueue downloadQueue)
    {
        _log = log;
        _appLifetime = appLifetime;
        _configManager = configManager;
        _schedulerService = schedulerService;
        _downloadQueue = downloadQueue;
    }

    #endregion

    #region Public Methods

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _log.InformationLine("Shutting down the container");
        await _schedulerService.StopAsync();
    }

    public async Task WaitForStartAsync(CancellationToken cancellationToken)
    {
        _log.InformationLine("Initiating boot process");
        ServicePointManager.DefaultConnectionLimit = 1000;

        _configManager.Setup();
        _downloadQueue.Setup();
        await _schedulerService.SetupAsync();

        _appLifetime.ApplicationStarted.Register(OnStarted);
        _appLifetime.ApplicationStopping.Register(OnStopping);
        _appLifetime.ApplicationStopped.Register(OnStopped);

        _log.InformationLine("Finished Initiating boot process");
    }

    #endregion

    #region Private Methods

    private void OnStarted()
    {
        _log.InformationLine("Boot.OnStarted has been called");

        // Perform post-startup activities here
    }

    private void OnStopped()
    {
        _log.InformationLine("Boot.OnStopped has been called");

        // Perform post-stopped activities here
    }

    private void OnStopping()
    {
        _log.InformationLine("Boot.OnStopping has been called");

        // Perform on-stopping activities here
    }

    #endregion
}