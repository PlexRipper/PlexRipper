using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using PlexRipper.Application.Common;
using PlexRipper.Domain;

namespace PlexRipper.WebAPI
{
    internal class Boot : IHostLifetime, IHostedService
    {
        private readonly IHostApplicationLifetime _appLifetime;

        private readonly IUserSettings _userSettings;

        private readonly IFileManager _fileManager;

        public Boot(IHostApplicationLifetime appLifetime, IUserSettings userSettings, IFileManager fileManager)
        {
            _appLifetime = appLifetime;
            _userSettings = userSettings;
            _fileManager = fileManager;
        }

        public async Task WaitForStartAsync(CancellationToken cancellationToken)
        {
            Log.Information("Initiating boot process");
            _userSettings.Setup();
            _fileManager.Setup();
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