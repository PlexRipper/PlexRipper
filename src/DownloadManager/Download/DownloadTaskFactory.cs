using Application.Contracts;
using AutoMapper;
using Data.Contracts;
using DownloadManager.Contracts;
using FileSystem.Contracts;
using Logging.Interface;
using Settings.Contracts;

namespace PlexRipper.DownloadManager;

public class DownloadTaskFactory : IDownloadTaskFactory
{
    #region Fields

    private readonly IPathSystem _pathSystem;

    private readonly IDownloadManagerSettingsModule _downloadManagerSettings;

    private readonly IMapper _mapper;

    private readonly IPlexRipperDbContext _dbContext;

    private readonly ILog _log;

    private readonly IMediator _mediator;

    private readonly INotificationsService _notificationsService;

    #endregion

    #region Constructor

    public DownloadTaskFactory(
        ILog log,
        IMediator mediator,
        IMapper mapper,
        IPlexRipperDbContext dbContext,
        INotificationsService notificationsService,
        IPathSystem pathSystem,
        IDownloadManagerSettingsModule downloadManagerSettings)
    {
        _log = log;
        _mediator = mediator;
        _mapper = mapper;
        _dbContext = dbContext;
        _notificationsService = notificationsService;
        _pathSystem = pathSystem;
        _downloadManagerSettings = downloadManagerSettings;
    }

    #endregion

    #region Public Methods

    /// <inheritdoc/>
    public Result<List<DownloadWorkerTask>> GenerateDownloadWorkerTasks(DownloadTaskFileBase downloadTask)
    {
        if (downloadTask is null)
            return ResultExtensions.IsNull(nameof(downloadTask)).LogWarning();

        var parts = _downloadManagerSettings.DownloadSegments;
        if (parts <= 0)
            return Result.Fail($"Parameter {nameof(parts)} was {parts}, prevented division by invalid value").LogWarning();

        // Create download worker tasks/segments/ranges
        var totalBytesToReceive = downloadTask.DataTotal;
        var partSize = totalBytesToReceive / parts;
        var remainder = totalBytesToReceive - partSize * parts;

        var downloadWorkerTasks = new List<DownloadWorkerTask>();

        for (var i = 0; i < parts; i++)
        {
            var start = partSize * i;
            var end = start + partSize;
            if (i == parts - 1 && remainder > 0)
            {
                // Add the remainder to the last download range
                end += remainder;
            }

            downloadWorkerTasks.Add(new DownloadWorkerTask(downloadTask, i + 1, start, end));
        }

        return Result.Ok(downloadWorkerTasks);
    }

    #endregion
}