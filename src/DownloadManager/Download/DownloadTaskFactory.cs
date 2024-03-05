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
    public async Task<Result<List<DownloadTask>>> RegenerateDownloadTask(List<int> downloadTaskIds)
    {
        if (!downloadTaskIds.Any())
            return ResultExtensions.IsEmpty(nameof(downloadTaskIds)).LogWarning();

        _log.Debug("Regenerating {DownloadTaskIdsCount} download tasks", downloadTaskIds.Count);

        var freshDownloadTasks = new List<DownloadTask>();

        foreach (var downloadTaskId in downloadTaskIds)
        {
            var downloadTask = await _dbContext.DownloadTasks.GetAsync(downloadTaskId);
            if (downloadTask is null)
            {
                ResultExtensions.EntityNotFound(nameof(DownloadTask), downloadTaskId).LogError();
                continue;
            }

            var mediaIdResult = await _dbContext.GetPlexMediaByMediaKeyAsync(downloadTask.Id, downloadTask.PlexServerId, downloadTask.MediaType);
            if (mediaIdResult.IsFailed)
            {
                var result = Result.Fail($"Could not recreate the download task for {downloadTask.FullTitle}");
                result.WithReasons(mediaIdResult.Reasons);
                await _notificationsService.SendResult(result);
                continue;
            }

            var list = new List<DownloadMediaDTO>
            {
                new()
                {
                    Type = downloadTask.MediaType,
                    MediaIds = new List<int> { mediaIdResult.Value },
                },
            };

            // var downloadTasksResult = await GenerateAsync(list);
            // if (downloadTasksResult.IsFailed)
            // {
            //     var result = Result.Fail($"Could not recreate the download task for {downloadTask.FullTitle}").WithReasons(mediaIdResult.Reasons);
            //     await _notificationsService.SendResult(result);
            //     continue;
            // }

            await _dbContext.DeleteDownloadWorkerTasksAsync(downloadTask.Id);

            // downloadTasksResult.Value[0].Id = downloadTask.Id;
            // downloadTasksResult.Value[0].Priority = downloadTask.Priority;

            //  freshDownloadTasks.AddRange(downloadTasksResult.Value);
        }

        _log.Debug("Successfully regenerated {FreshDownloadTasksCount} out of {DownloadTaskIdsCount} download tasks", freshDownloadTasks.Count,
            downloadTaskIds.Count);

        if (downloadTaskIds.Count - freshDownloadTasks.Count > 0)
            _log.ErrorLine("Failed to generate");

        return Result.Ok(freshDownloadTasks);
    }

    public Result<List<DownloadWorkerTask>> GenerateDownloadWorkerTasks(DownloadTask downloadTask)
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