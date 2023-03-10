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
    private readonly Func<PlexDownloadClient> _plexDownloadClientFactory;
    private PlexDownloadClient _downloadClient;

    #region Fields

    #endregion

    #region Constructors

    public DownloadJob(
        ILog log,
        IMediator mediator,
        IDownloadTaskFactory downloadTaskFactory,
        INotificationsService notificationsService,
        Func<PlexDownloadClient> plexDownloadClientFactory)
    {
        _log = log;
        _mediator = mediator;
        _downloadTaskFactory = downloadTaskFactory;
        _notificationsService = notificationsService;
        _plexDownloadClientFactory = plexDownloadClientFactory;
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
        _log.Debug("Executing job: {DownloadJobName} for {DownloadTaskIdName} with id: {DownloadTaskId}", nameof(DownloadJob), nameof(downloadTaskId),
            downloadTaskId);

        // Jobs should swallow exceptions as otherwise Quartz will keep re-executing it
        // https://www.quartz-scheduler.net/documentation/best-practices.html#throwing-exceptions
        try
        {
            var downloadTaskResult = await _mediator.Send(new GetDownloadTaskByIdQuery(downloadTaskId, true));
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

                var addResult = await _mediator.Send(new AddDownloadWorkerTasksCommand(downloadWorkerTasksResult.Value));
                if (addResult.IsFailed)
                {
                    addResult.LogError();
                    return;
                }

                var getResult = await _mediator.Send(new GetAllDownloadWorkerTasksByDownloadTaskIdQuery(downloadTask.Id));
                if (getResult.IsFailed)
                {
                    getResult.LogError();
                    return;
                }

                downloadTask.DownloadWorkerTasks = getResult.Value;
                _log.Debug("Generated DownloadWorkerTasks for {DownloadTaskFullTitle}", downloadTask.FullTitle);
            }

            var downloadClientResult = _plexDownloadClientFactory().Setup(downloadTask);
            if (downloadClientResult.IsFailed)
            {
                downloadClientResult.LogError();
                return;
            }

            _downloadClient = downloadClientResult.Value;

            var startResult = await _downloadClient.Start();
            if (startResult.IsFailed)
                await _notificationsService.SendResult(startResult);

            while (!_downloadClient.DownloadProcessTask.IsCompleted)
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    _log.Information("{DownloadJobName} with {DownloadTaskIdName}: {DownloadTaskId} has been requested to be stopped", nameof(DownloadJob),
                        nameof(downloadTaskId), downloadTaskId);

                    await _downloadClient.StopAsync();

                    // ReSharper disable once MethodSupportsCancellation
                    await Task.Delay(2000);
                    return;
                }

                // Don't pass token as it will prevent a graceful stop of the downloadProcess
                // ReSharper disable once MethodSupportsCancellation
                await Task.Delay(1000);
            }

            _log.Debug("Exiting job: {DownloadJobName} for {DownloadTaskName} with id: {DownloadTaskId}", nameof(DownloadJob), nameof(DownloadTask),
                downloadTaskId);


        }
        catch (Exception e)
        {
            _log.Error(e);
        }
    }

    #endregion

    #endregion

    public void Dispose()
    {
        _log.Here().Warning("Disposing job: {DownloadJobName} for {DownloadTaskName}", nameof(DownloadJob), nameof(DownloadTask));

      //  _downloadClient.Dispose();
    }
}