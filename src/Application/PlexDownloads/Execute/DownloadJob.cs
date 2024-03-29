﻿using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Application.Contracts;
using Data.Contracts;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Settings.Contracts;

namespace PlexRipper.Application;

public class DownloadJob : IJob, IDisposable
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly INotificationsService _notificationsService;
    private readonly IDownloadManagerSettingsModule _downloadManagerSettings;
    private readonly IPlexDownloadClient _plexDownloadClient;

    public DownloadJob(
        ILog log,
        IPlexRipperDbContext dbContext,
        INotificationsService notificationsService,
        IDownloadManagerSettingsModule downloadManagerSettings,
        IPlexDownloadClient plexDownloadClient)
    {
        _log = log;
        _dbContext = dbContext;
        _notificationsService = notificationsService;
        _downloadManagerSettings = downloadManagerSettings;
        _plexDownloadClient = plexDownloadClient;
    }

    public static string DownloadTaskIdParameter => "DownloadTaskId";

    public static JobKey GetJobKey(Guid id) => new($"{DownloadTaskIdParameter}_{id}", nameof(DownloadJob));

    public async Task Execute(IJobExecutionContext context)
    {
        var dataMap = context.JobDetail.JobDataMap;
        var downloadTaskKey = dataMap.GetJsonValue<DownloadTaskKey>(DownloadTaskIdParameter);

        var token = context.CancellationToken;
        _log.Debug("Executing job: {DownloadJobName} for {DownloadTaskIdName} with id: {DownloadTaskId}", nameof(DownloadJob), nameof(downloadTaskKey),
            downloadTaskKey);

        // Jobs should swallow exceptions as otherwise Quartz will keep re-executing it
        // https://www.quartz-scheduler.net/documentation/best-practices.html#throwing-exceptions
        try
        {
            // Create the multiple download worker tasks which will split up the work
            var downloadTask = await _dbContext.GetDownloadTaskFileAsync(downloadTaskKey, token);
            if (downloadTask is null)
            {
                ResultExtensions.EntityNotFound(nameof(DownloadTaskFileBase), downloadTaskKey.Id).LogError();
                return;
            }

            if (!downloadTask.IsDownloadable)
            {
                _log.Warning("DownloadTask {DownloadTaskId} is not downloadable, aborting DownloadJob", downloadTaskKey);
                return;
            }

            // Create the multiple download worker tasks which will split up the work
            if (!downloadTask.DownloadWorkerTasks.Any())
            {
                var parts = _downloadManagerSettings.DownloadSegments;
                downloadTask.DownloadWorkerTasks = downloadTask.GenerateDownloadWorkerTasks(parts);
                await _dbContext.DownloadWorkerTasks.AddRangeAsync(downloadTask.DownloadWorkerTasks, token);
                await _dbContext.SaveChangesAsync(token);
                _log.Debug("Generated DownloadWorkerTasks for {DownloadTaskFullTitle}", downloadTask.FullTitle);
            }

            var result = await SetDownloadAndDestination(downloadTask);
            if (result.IsFailed)
            {
                result.LogError();
                return;
            }

            downloadTask = result.Value;

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
                    nameof(downloadTaskKey), downloadTaskKey);

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
                downloadTaskKey);
        }
    }

    public void Dispose()
    {
        _log.Here().Warning("Disposing job: {DownloadJobName} for {DownloadTaskName}", nameof(DownloadJob), nameof(DownloadTaskGeneric));
    }

    private void SetupSubscription(IPlexDownloadClient plexDownloadClient)
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

    private async Task<Result<DownloadTaskFileBase>> SetDownloadAndDestination(DownloadTaskFileBase downloadTask)
    {
        var downloadFolder = await _dbContext.GetDownloadFolder();
        var destinationFolder = await _dbContext.GetDestinationFolder(downloadTask.PlexLibraryId);

        downloadTask.DirectoryMeta.DownloadRootPath = downloadFolder.DirectoryPath;
        downloadTask.DirectoryMeta.DestinationRootPath = destinationFolder.DirectoryPath;

        switch (downloadTask.DownloadTaskType)
        {
            case DownloadTaskType.MovieData:
                await _dbContext.DownloadTaskMovieFile.Where(x => x.Id == downloadTask.Id)
                    .ExecuteUpdateAsync(p => p
                        .SetProperty(x => x.DirectoryMeta.DownloadRootPath, downloadFolder.DirectoryPath)
                        .SetProperty(x => x.DirectoryMeta.DestinationRootPath, destinationFolder.DirectoryPath));
                break;
            case DownloadTaskType.EpisodeData:
                await _dbContext.DownloadTaskTvShowEpisodeFile.Where(x => x.Id == downloadTask.Id)
                    .ExecuteUpdateAsync(p => p
                        .SetProperty(x => x.DirectoryMeta.DownloadRootPath, downloadFolder.DirectoryPath)
                        .SetProperty(x => x.DirectoryMeta.DestinationRootPath, destinationFolder.DirectoryPath));
                break;
            default:
                return Result.Fail($"DownloadTaskType {downloadTask.DownloadTaskType} is not supported");
        }

        return Result.Ok(downloadTask);
    }
}