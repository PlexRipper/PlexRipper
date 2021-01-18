using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using PlexRipper.Application.Common;
using PlexRipper.Data;
using PlexRipper.Domain;

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

        private readonly PlexRipperDbContext _dbContext;

        public Boot(IHostApplicationLifetime appLifetime, IUserSettings userSettings, IFileSystem fileSystem, IFileMerger fileMerger, IDownloadManager downloadManager,
            PlexRipperDbContext dbContext)
        {
            _appLifetime = appLifetime;
            _userSettings = userSettings;
            _fileSystem = fileSystem;
            _fileMerger = fileMerger;
            _downloadManager = downloadManager;
            _dbContext = dbContext;
        }

        public Task WaitForStartAsync(CancellationToken cancellationToken)
        {
            Log.Information("Initiating boot process");
            ServicePointManager.DefaultConnectionLimit = 1000;

            var fileSystemSetup = _fileSystem.Setup();
            if (fileSystemSetup.IsFailed)
            {
                return Task.CompletedTask;
            }

            _dbContext.Setup();
            _userSettings.Setup();
            _downloadManager.Setup();
            _fileMerger.Setup();
            return Task.CompletedTask;
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