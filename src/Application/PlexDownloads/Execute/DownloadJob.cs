using Application.Contracts;
using Data.Contracts;
using DownloadManager.Contracts;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexRipper.DownloadManager;
using Quartz;

namespace PlexRipper.Application;

public class DownloadJob : IJob, IDisposable
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IDownloadTaskFactory _downloadTaskFactory;
    private readonly INotificationsService _notificationsService;
    private readonly PlexDownloadClient _plexDownloadClient;

    public DownloadJob(
        ILog log,
        IPlexRipperDbContext dbContext,
        IDownloadTaskFactory downloadTaskFactory,
        INotificationsService notificationsService,
        PlexDownloadClient plexDownloadClient)
    {
        _log = log;
        _dbContext = dbContext;
        _downloadTaskFactory = downloadTaskFactory;
        _notificationsService = notificationsService;
        _plexDownloadClient = plexDownloadClient;
    }

    public static string DownloadTaskIdParameter => "DownloadTaskId";

    public static string PlexServerIdParameter => "PlexServerId";

    public static JobKey GetJobKey(int id) => new($"{DownloadTaskIdParameter}_{id}", nameof(DownloadJob));

    public async Task Execute(IJobExecutionContext context)
    {
        var dataMap = context.JobDetail.JobDataMap;
        var downloadTaskId = dataMap.GetIntValue(DownloadTaskIdParameter);
        var plexServerId = dataMap.GetIntValue(PlexServerIdParameter);

        var token = context.CancellationToken;
        _log.Debug("Executing job: {DownloadJobName} for {DownloadTaskIdName} with id: {DownloadTaskId}", nameof(DownloadJob), nameof(downloadTaskId),
            downloadTaskId);

        // Jobs should swallow exceptions as otherwise Quartz will keep re-executing it
        // https://www.quartz-scheduler.net/documentation/best-practices.html#throwing-exceptions
        try
        {
            // Create the multiple download worker tasks which will split up the work
            var downloadTask = await _dbContext.DownloadTasks.IncludeDownloadTasks().Include(x => x.PlexServer).GetAsync(downloadTaskId, token);
            if (downloadTask is null)
            {
                ResultExtensions.EntityNotFound(nameof(DownloadTask), downloadTaskId).LogError();
                return;
            }

            _log.Debug("Creating Download client for {DownloadTaskFullTitle}", downloadTask.FullTitle);

            if (!downloadTask.DownloadWorkerTasks.Any())
            {
                var generateResult = await GenerateDownloadWorkerTasks(downloadTask, token);
                if (generateResult.IsFailed)
                    return;

                downloadTask.DownloadWorkerTasks = generateResult.Value;
                _log.Debug("Generated DownloadWorkerTasks for {DownloadTaskFullTitle}", downloadTask.FullTitle);
            }

            var downloadClientResult = _plexDownloadClient.Setup(downloadTask);
            if (downloadClientResult.IsFailed)
            {
                downloadClientResult.LogError();
                return;
            }

            var startResult = _plexDownloadClient.Start();
            if (startResult.IsFailed)
                await _notificationsService.SendResult(startResult);

            try
            {
                await _plexDownloadClient.DownloadProcessTask.WaitAsync(token);
            }
            catch (TaskCanceledException)
            {
                _log.Information("{DownloadJobName} with {DownloadTaskIdName}: {DownloadTaskId} has been requested to be stopped", nameof(DownloadJob),
                    nameof(downloadTaskId), downloadTaskId);

                await _plexDownloadClient.StopAsync();
            }
        }
        catch (TaskCanceledException) { }
        catch (Exception e)
        {
            _log.Error(e);
        }
        finally
        {
            _log.Debug("Exiting job: {DownloadJobName} for {DownloadTaskName} with id: {DownloadTaskId}", nameof(DownloadJob), nameof(DownloadTask),
                downloadTaskId);
        }
    }

    public void Dispose()
    {
        _log.Here().Warning("Disposing job: {DownloadJobName} for {DownloadTaskName}", nameof(DownloadJob), nameof(DownloadTask));
    }

    private async Task<Result<List<DownloadWorkerTask>>> GenerateDownloadWorkerTasks(DownloadTask downloadTask, CancellationToken cancellationToken = default)
    {
        var downloadWorkerTasksResult = _downloadTaskFactory.GenerateDownloadWorkerTasks(downloadTask);
        if (downloadWorkerTasksResult.IsFailed)
            return downloadWorkerTasksResult.LogError();

        var downloadWorkerTasks = downloadWorkerTasksResult.Value;

        // Insert DownloadWorkerTasks into the database
        downloadWorkerTasks.ForEach(x => x.DownloadTask = null);
        await _dbContext.DownloadWorkerTasks.AddRangeAsync(downloadWorkerTasks, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        downloadWorkerTasks = await _dbContext.DownloadWorkerTasks
            .Where(x => x.DownloadTaskId == downloadTask.Id)
            .ToListAsync(cancellationToken);

        return Result.Ok(downloadWorkerTasks);
    }
}