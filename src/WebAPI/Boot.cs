using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Environment;
using FluentResultExtensions.lib;
using Logging;
using Microsoft.Extensions.Hosting;
using PlexRipper.Application;
using PlexRipper.Application.Common;
using PlexRipper.Data;
using PlexRipper.Domain;
using PlexRipper.FileSystem;

namespace PlexRipper.WebAPI
{
    /// <summary>
    /// The Boot class is used to sequentially start various processes needed to start PlexRipper.
    /// </summary>
    internal class Boot : IHostLifetime, IHostedService
    {
        private readonly IHostApplicationLifetime _appLifetime;

        private readonly IUserSettings _userSettings;

        private readonly IFileSystem _fileSystem;

        private readonly IFileMerger _fileMerger;

        private readonly IDownloadManager _downloadManager;

        private readonly IPlexRipperDatabaseService _plexRipperDatabaseService;

        private readonly ISchedulerService _schedulerService;

        public Boot(IHostApplicationLifetime appLifetime, IUserSettings userSettings, IFileSystem fileSystem, IFileMerger fileMerger,
            IDownloadManager downloadManager, IPlexRipperDatabaseService plexRipperDatabaseService, ISchedulerService schedulerService)
        {
            _appLifetime = appLifetime;
            _userSettings = userSettings;
            _fileSystem = fileSystem;
            _fileMerger = fileMerger;
            _downloadManager = downloadManager;
            _plexRipperDatabaseService = plexRipperDatabaseService;
            _schedulerService = schedulerService;
        }

        public async Task WaitForStartAsync(CancellationToken cancellationToken)
        {
            Log.Information("Initiating boot process");
            ServicePointManager.DefaultConnectionLimit = 1000;

            // First await the finishing off all these
            _fileSystem.Setup();
            Log.SetupLogging();
            _userSettings.Setup();
            await _plexRipperDatabaseService.SetupAsync();

            // Keep running the following
            if (!EnviromentExtensions.IsIntegrationTestMode())
            {
                var fileMergerSetup = Task.Factory.StartNew(() => _fileMerger.SetupAsync(), TaskCreationOptions.LongRunning);
                await Task.WhenAll(fileMergerSetup);
                await _schedulerService.SetupAsync();
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _appLifetime.ApplicationStarted.Register(OnStarted);
            _appLifetime.ApplicationStopping.Register(OnStopping);
            _appLifetime.ApplicationStopped.Register(OnStopped);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private void OnStarted()
        {
            Log.Information("Boot.OnStarted has been called.");

            // Perform post-startup activities here
        }

        private void OnStopping()
        {
            Log.Information("Boot.OnStopping has been called.");

            // Perform on-stopping activities here
        }

        private void OnStopped()
        {
            Log.Information("Boot.OnStopped has been called.");

            // Perform post-stopped activities here
        }
    }
}