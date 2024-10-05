using System.Net;
using System.Runtime.InteropServices;
using Application.Contracts;
using Data.Contracts;
using Logging.Interface;
using Settings.Contracts;

namespace PlexRipper.WebAPI;

/// <summary>
/// The Boot class is used to sequentially start various processes needed to start PlexRipper.
/// </summary>
public class Boot : IHostedService
{
    #region Fields

    private readonly ILog _log;

    private readonly IPlexRipperDbContextManager _dbContextManager;

    private readonly IConfigManager _configManager;

    private readonly ISchedulerService _schedulerService;

    private readonly IDownloadQueue _downloadQueue;

    [DllImport("libc")]
    public static extern uint getuid();

    [DllImport("libc")]
    public static extern uint getgid();

    #endregion

    #region Constructor

    public Boot(
        ILog log,
        IPlexRipperDbContextManager dbContextManagerManager,
        IHostApplicationLifetime appLifetime,
        IConfigManager configManager,
        ISchedulerService schedulerService,
        IDownloadQueue downloadQueue
    )
    {
        _log = log;
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

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _log.InformationLine("Initiating boot process");
        ServicePointManager.DefaultConnectionLimit = 1000;

        _dbContextManager.Setup();

        _configManager.Setup();

        _downloadQueue.Setup();

        await _schedulerService.SetupAsync();

        _log.InformationLine("Finished Initiating boot process");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _log.InformationLine("Shutting down the container");
        await _schedulerService.StopAsync();
    }

    #endregion

    #region Private Methods

    private void OnStarted()
    {
        _log.InformationLine("Boot.OnStarted has been called");

        // Retrieve PUID and PGID from environment variables
        var puid = System.Environment.GetEnvironmentVariable("PUID");
        var pgid = System.Environment.GetEnvironmentVariable("PGID");
        Console.WriteLine($"PUID from env: {puid}");
        Console.WriteLine($"PGID from env: {pgid}");

        // Get the current user's UID and GID from the system
        var uid = getuid();
        var gid = getgid();
        Console.WriteLine($"Current UID: {uid}");
        Console.WriteLine($"Current GID: {gid}");

        // Get current username
        Console.WriteLine($"Current Username: {System.Environment.UserName}");

        // Perform post-startup activities here
    }

    private void OnStopping()
    {
        _log.InformationLine("Boot.OnStopping has been called");

        // Perform on-stopping activities here
    }

    private void OnStopped()
    {
        _log.InformationLine("Boot.OnStopped has been called");

        // Perform post-stopped activities here
    }

    #endregion
}
