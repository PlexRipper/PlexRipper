using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Environment;
using Logging;
using Microsoft.Extensions.Hosting;
using PlexRipper.Application;
using PlexRipper.DownloadManager;

namespace PlexRipper.WebAPI
{
    /// <summary>
    /// The Boot class is used to sequentially start various processes needed to start PlexRipper.
    /// </summary>
    internal class Boot : IHostLifetime, IHostedService
    {
        #region Fields

        private readonly IHostApplicationLifetime _appLifetime;

        private readonly IConfigManager _configManager;

        private readonly IFileMerger _fileMerger;

        private readonly IFileSystem _fileSystem;

        private readonly IPlexRipperDatabaseService _plexRipperDatabaseService;

        private readonly ISchedulerService _schedulerService;

        private readonly IMigrationService _migrationService;

        private readonly IDownloadSubscriptions _downloadSubscriptions;

        private readonly IDownloadQueue _downloadQueue;

        #endregion

        #region Constructor

        public Boot(IHostApplicationLifetime appLifetime, IConfigManager configManager, IFileSystem fileSystem, IFileMerger fileMerger, IPlexRipperDatabaseService plexRipperDatabaseService, ISchedulerService schedulerService,
            IMigrationService migrationService, IDownloadSubscriptions downloadSubscriptions, IDownloadQueue downloadQueue)
        {
            _appLifetime = appLifetime;
            _configManager = configManager;
            _fileSystem = fileSystem;
            _fileMerger = fileMerger;
            _plexRipperDatabaseService = plexRipperDatabaseService;
            _schedulerService = schedulerService;
            _migrationService = migrationService;
            _downloadSubscriptions = downloadSubscriptions;
            _downloadQueue = downloadQueue;
        }

        #endregion

        #region Public Methods

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _appLifetime.ApplicationStarted.Register(OnStarted);
            _appLifetime.ApplicationStopping.Register(OnStopping);
            _appLifetime.ApplicationStopped.Register(OnStopped);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Log.Information("Shutting down the container");
            return Task.CompletedTask;
        }

        public async Task WaitForStartAsync(CancellationToken cancellationToken)
        {
            Log.Information("Initiating boot process");
            ServicePointManager.DefaultConnectionLimit = 1000;

            // First await the finishing off all these
            _fileSystem.Setup();
            Log.SetupLogging();
            _configManager.Setup();
            await _plexRipperDatabaseService.SetupAsync();
            await _migrationService.SetupAsync();

            _downloadSubscriptions.Setup();
            _downloadQueue.Setup();

            // Keep running the following
            if (!EnvironmentExtensions.IsIntegrationTestMode())
            {
                var fileMergerSetup = Task.Factory.StartNew(() => _fileMerger.SetupAsync(), TaskCreationOptions.LongRunning);
                await Task.WhenAll(fileMergerSetup);
                await _schedulerService.SetupAsync();
            }
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
}