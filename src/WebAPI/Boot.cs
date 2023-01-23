using System.Net;
using DownloadManager.Contracts;
using Environment;
using PlexRipper.Application;

namespace PlexRipper.WebAPI;

/// <summary>
/// The Boot class is used to sequentially start various processes needed to start PlexRipper.
/// </summary>
public class Boot : IBoot
{
    #region Fields

    private readonly IHostApplicationLifetime _appLifetime;

    private readonly IConfigManager _configManager;

    private readonly ISchedulerService _schedulerService;

    private readonly IDownloadQueue _downloadQueue;

    #endregion

    #region Constructor

    public Boot(
        IHostApplicationLifetime appLifetime,
        IConfigManager configManager,
        ISchedulerService schedulerService,
        IDownloadQueue downloadQueue)
    {
        _appLifetime = appLifetime;
        _configManager = configManager;
        _schedulerService = schedulerService;
        _downloadQueue = downloadQueue;
    }

    #endregion

    #region Public Methods

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Log.Information("Shutting down the container");
        await _schedulerService.StopAsync();
    }

    public async Task WaitForStartAsync(CancellationToken cancellationToken)
    {
        Log.Information("Initiating boot process");
        ServicePointManager.DefaultConnectionLimit = 1000;

        // First await the finishing off all these
        if (!EnvironmentExtensions.IsIntegrationTestMode())
            Log.SetupLogging();

        _configManager.Setup();
        _downloadQueue.Setup();
        await _schedulerService.SetupAsync();

        _appLifetime.ApplicationStarted.Register(OnStarted);
        _appLifetime.ApplicationStopping.Register(OnStopping);
        _appLifetime.ApplicationStopped.Register(OnStopped);

        Log.Information("Finished Initiating boot process");
    }

    #endregion

    #region Private Methods

    private void OnStarted()
    {
        Log.Information("Boot.OnStarted has been called.");

        // Perform post-startup activities here
    }

    private void OnStopped()
    {
        Log.Information("Boot.OnStopped has been called.");

        // Perform post-stopped activities here
    }

    private void OnStopping()
    {
        Log.Information("Boot.OnStopping has been called.");

        // Perform on-stopping activities here
    }

    #endregion
}