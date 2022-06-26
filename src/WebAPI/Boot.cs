using System.Net;
using Environment;
using PlexRipper.Application;
using PlexRipper.DownloadManager;

namespace PlexRipper.WebAPI;

/// <summary>
/// The Boot class is used to sequentially start various processes needed to start PlexRipper.
/// </summary>
public class Boot : IBoot
{
    #region Fields

    private readonly IHostApplicationLifetime _appLifetime;

    private readonly IConfigManager _configManager;

    private readonly IFileMerger _fileMerger;

    private readonly IPlexRipperDatabaseService _plexRipperDatabaseService;

    private readonly ISchedulerService _schedulerService;

    private readonly IMigrationService _migrationService;

    private readonly IDownloadSubscriptions _downloadSubscriptions;

    private readonly IDownloadQueue _downloadQueue;

    private readonly IDownloadTracker _downloadTracker;

    #endregion

    #region Constructor

    public Boot(IHostApplicationLifetime appLifetime,
        IConfigManager configManager,
        IFileMerger fileMerger,
        IPlexRipperDatabaseService plexRipperDatabaseService,
        ISchedulerService schedulerService,
        IMigrationService migrationService,
        IDownloadSubscriptions downloadSubscriptions,
        IDownloadQueue downloadQueue,
        IDownloadTracker downloadTracker)
    {
        _appLifetime = appLifetime;
        _configManager = configManager;
        _fileMerger = fileMerger;
        _plexRipperDatabaseService = plexRipperDatabaseService;
        _schedulerService = schedulerService;
        _migrationService = migrationService;
        _downloadSubscriptions = downloadSubscriptions;
        _downloadQueue = downloadQueue;
        _downloadTracker = downloadTracker;
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
        Log.SetupLogging();
        _configManager.Setup();
        await _plexRipperDatabaseService.SetupAsync();
        await _migrationService.SetupAsync();

        _downloadSubscriptions.Setup();
        _downloadQueue.Setup();
        _downloadTracker.Setup();
        await _fileMerger.SetupAsync();

        // TODO Remove this once the plexServer sync has been compatible for the integration test
        if (!EnvironmentExtensions.IsIntegrationTestMode())
        {
            await _schedulerService.SetupAsync();
        }

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