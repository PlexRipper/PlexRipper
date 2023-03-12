using Application.Contracts;
using Data.Contracts;
using DownloadManager.Contracts;
using Logging.Interface;
using Quartz;

namespace PlexRipper.DownloadManager.Jobs;

public class DownloadJob : IJob, IDisposable
{
    private readonly ILog _log;
    private readonly IMediator _mediator;
    private readonly IDownloadTaskFactory _downloadTaskFactory;
    private readonly INotificationsService _notificationsService;
    private readonly PlexDownloadClient _plexDownloadClient;

    #region Fields

    #endregion

    #region Constructors

    public DownloadJob(
        ILog log,
        IMediator mediator,
        IDownloadTaskFactory downloadTaskFactory,
        INotificationsService notificationsService,
        PlexDownloadClient plexDownloadClient)
    {
        _log = log;
        _mediator = mediator;
        _downloadTaskFactory = downloadTaskFactory;
        _notificationsService = notificationsService;
        _plexDownloadClient = plexDownloadClient;
    }

    #endregion

    #region Properties

    public static string DownloadTaskIdParameter => "DownloadTaskId";

    public static string PlexServerIdParameter => "PlexServerId";

    #endregion

    #region Methods

    #region Public

    public static JobKey GetJobKey(int id)
    {
        return new JobKey($"{DownloadTaskIdParameter}_{id}", nameof(DownloadJob));
    }

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
            var downloadTaskResult = await _mediator.Send(new GetDownloadTaskByIdQuery(downloadTaskId, true), token);
            if (downloadTaskResult.IsFailed)
            {
                downloadTaskResult.LogError();
                return;
            }

            // Create the multiple download worker tasks which will split up the work
            var downloadTask = downloadTaskResult.Value;

            _log.Debug("Creating Download client for {DownloadTaskFullTitle}", downloadTask.FullTitle);

            if (!downloadTask.DownloadWorkerTasks.Any())
            {
                var downloadWorkerTasksResult = _downloadTaskFactory.GenerateDownloadWorkerTasks(downloadTask);
                if (downloadWorkerTasksResult.IsFailed)
                {
                    downloadWorkerTasksResult.LogError();
                    return;
                }

                downloadTask.DownloadWorkerTasks = downloadWorkerTasksResult.Value;

                var addResult = await _mediator.Send(new AddDownloadWorkerTasksCommand(downloadWorkerTasksResult.Value), token);
                if (addResult.IsFailed)
                {
                    addResult.LogError();
                    return;
                }

                var getResult = await _mediator.Send(new GetAllDownloadWorkerTasksByDownloadTaskIdQuery(downloadTask.Id), token);
                if (getResult.IsFailed)
                {
                    getResult.LogError();
                    return;
                }

                downloadTask.DownloadWorkerTasks = getResult.Value;
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

    #endregion

    #endregion

    public void Dispose()
    {
        _log.Here().Warning("Disposing job: {DownloadJobName} for {DownloadTaskName}", nameof(DownloadJob), nameof(DownloadTask));
    }
}