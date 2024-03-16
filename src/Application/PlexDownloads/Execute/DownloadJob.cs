using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Application.Contracts;
using Data.Contracts;
using Logging.Interface;
using Quartz;

namespace PlexRipper.Application;

public class DownloadJob : IJob, IDisposable
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IMediator _mediator;
    private readonly INotificationsService _notificationsService;
    private readonly PlexDownloadClient _plexDownloadClient;

    public DownloadJob(
        ILog log,
        IPlexRipperDbContext dbContext,
        IMediator mediator,
        INotificationsService notificationsService,
        PlexDownloadClient plexDownloadClient)
    {
        _log = log;
        _dbContext = dbContext;
        _mediator = mediator;
        _notificationsService = notificationsService;
        _plexDownloadClient = plexDownloadClient;
    }

    public static string DownloadTaskIdParameter => "DownloadTaskId";

    public static string PlexServerIdParameter => "PlexServerId";

    public static JobKey GetJobKey(Guid id) => new($"{DownloadTaskIdParameter}_{id}", nameof(DownloadJob));

    public async Task Execute(IJobExecutionContext context)
    {
        var dataMap = context.JobDetail.JobDataMap;
        var downloadTaskId = dataMap.GetGuidValue(DownloadTaskIdParameter);
        var plexServerId = dataMap.GetIntValue(PlexServerIdParameter);

        var token = context.CancellationToken;
        _log.Debug("Executing job: {DownloadJobName} for {DownloadTaskIdName} with id: {DownloadTaskId}", nameof(DownloadJob), nameof(downloadTaskId),
            downloadTaskId);

        // Jobs should swallow exceptions as otherwise Quartz will keep re-executing it
        // https://www.quartz-scheduler.net/documentation/best-practices.html#throwing-exceptions
        try
        {
            // Create the multiple download worker tasks which will split up the work
            var downloadTask = await _dbContext.GetDownloadTaskAsync(downloadTaskId, cancellationToken: token);
            if (downloadTask is null)
            {
                ResultExtensions.EntityNotFound(nameof(DownloadTaskGeneric), downloadTaskId).LogError();
                return;
            }

            if (!downloadTask.IsDownloadable)
            {
                _log.Warning("DownloadTask {DownloadTaskId} is not downloadable, aborting DownloadJob", downloadTaskId);
                return;
            }

            _log.Debug("Creating Download client for {DownloadTaskFullTitle}", downloadTask.FullTitle);
            var downloadClientResult = await _plexDownloadClient.Setup(downloadTask.ToKey(), token);
            if (downloadClientResult.IsFailed)
            {
                downloadClientResult.LogError();
                return;
            }

            SetupSubscription(_plexDownloadClient);

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
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _log.Error(ex);
        }
        finally
        {
            _log.Debug("Exiting job: {DownloadJobName} for {DownloadTaskName} with id: {DownloadTaskId}", nameof(DownloadJob), nameof(DownloadTaskGeneric),
                downloadTaskId);
        }
    }

    public void Dispose()
    {
        _log.Here().Warning("Disposing job: {DownloadJobName} for {DownloadTaskName}", nameof(DownloadJob), nameof(DownloadTaskGeneric));
    }

    private void SetupSubscription(PlexDownloadClient plexDownloadClient)
    {
        plexDownloadClient.ListenToDownloadWorkerLog
            .Select(logs => Observable.Defer(() => CreateLog(logs).ToObservable()))
            .Concat()
            .Subscribe();
    }

    private async Task CreateLog(IList<DownloadWorkerLog> logs)
    {
        if (logs is null || !logs.Any())
            return;

        try
        {
            await _dbContext.DownloadWorkerTasksLogs.AddRangeAsync(logs);
            await _dbContext.SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _log.Error(ex);
        }
    }
}