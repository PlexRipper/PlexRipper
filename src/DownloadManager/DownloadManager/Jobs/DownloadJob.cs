using PlexRipper.Application;
using PlexRipper.Application.BackgroundServices;
using PlexRipper.Application.DownloadWorkerTasks;
using PlexRipper.DownloadManager.DownloadClient;
using Quartz;

namespace PlexRipper.DownloadManager.Jobs;

public class DownloadJob : IBaseJob, IDisposable
{
    private readonly IMediator _mediator;
    private readonly IDownloadTaskFactory _downloadTaskFactory;
    private readonly INotificationsService _notificationsService;
    private readonly Func<PlexDownloadClient> _plexDownloadClientFactory;
    private List<IDisposable> _disposables = new();
    private PlexDownloadClient _downloadClient;

    #region Fields

    #endregion

    #region Constructors

    public DownloadJob(
        IMediator mediator,
        IDownloadTaskFactory downloadTaskFactory,
        INotificationsService notificationsService,
        Func<PlexDownloadClient> plexDownloadClientFactory)
    {
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
        var token = context.CancellationToken;
        var dataMap = context.JobDetail.JobDataMap;
        var downloadTaskId = dataMap.GetIntValue(DownloadTaskIdParameter);
        var plexServerId = dataMap.GetIntValue(PlexServerIdParameter);
        Log.Debug($"Executing job: {nameof(DownloadJob)} for {nameof(DownloadTask)}: {downloadTaskId}");

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

            Log.Debug($"Creating Download client for {downloadTask.FullTitle}");

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
                Log.Debug($"Generated DownloadWorkerTasks for {downloadTask.FullTitle}");
            }

            var downloadClientResult = _plexDownloadClientFactory().Setup(downloadTask);
            if (downloadClientResult.IsFailed)
            {
                downloadClientResult.LogError();
                return;
            }

            _downloadClient = downloadClientResult.Value;

            var startResult = _downloadClient.Start();
            if (startResult.IsFailed)
                await _notificationsService.SendResult(startResult);

            while (!_downloadClient.DownloadProcessTask.IsCompleted)
            {
                if (token.IsCancellationRequested)
                {
                    Log.Information($"{nameof(DownloadJob)} with {nameof(downloadTaskId)}: {downloadTaskId} has been requested to be stopped.");
                    await _downloadClient.StopAsync();

                    // ReSharper disable once MethodSupportsCancellation
                    await Task.Delay(2000);
                    return;
                }

                // Don't pass token as it will prevent a graceful stop of the downloadProcess
                // ReSharper disable once MethodSupportsCancellation
                await Task.Delay(500);
            }

            await _mediator.Publish(new DownloadTaskFinished(downloadTaskId, plexServerId), token);
            Log.Debug($"Exiting {nameof(DownloadJob)} for {nameof(DownloadTask)} with id: {downloadTaskId}");
        }
        catch (Exception e)
        {
            Log.Error(e);
        }
    }

    #endregion

    #endregion

    public void Dispose()
    {
        foreach (var disposable in _disposables)
            disposable.Dispose();

        _downloadClient.Dispose();
    }
}