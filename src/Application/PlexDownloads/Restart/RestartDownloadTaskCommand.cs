namespace Reaparr.Application;

/// <summary>
/// Restart the <see cref="DownloadTaskGeneric"/> by deleting the PlexDownloadClient and starting a new one.
/// </summary>
/// <param name="DownloadTaskGuid">The id of the <see cref="DownloadTaskGeneric"/> to restart.</param>
/// <returns>Is successful.</returns>
public record RestartDownloadTaskCommand(Guid DownloadTaskGuid) : ICommand<Result>;

public class RestartDownloadTaskCommandValidator : AbstractValidator<RestartDownloadTaskCommand>
{
    public RestartDownloadTaskCommandValidator()
    {
        RuleFor(x => x.DownloadTaskGuid).NotEmpty();
    }
}

public class RestartDownloadTaskCommandHandler : ICommandHandler<RestartDownloadTaskCommand, Result>
{
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IDownloadTaskUpdateDispatcher _downloadTaskUpdateDispatcher;
    private readonly IEventPublisher _eventPublisher;

    public RestartDownloadTaskCommandHandler(
        IReaparrDbContext dbContext,
        ICommandExecutor commandExecutor,
        IDownloadTaskUpdateDispatcher downloadTaskUpdateDispatcher,
        IEventPublisher eventPublisher
    )
    {
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
        _downloadTaskUpdateDispatcher = downloadTaskUpdateDispatcher;
        _eventPublisher = eventPublisher;
    }

    public async Task<Result> ExecuteAsync(RestartDownloadTaskCommand command, CancellationToken cancellationToken)
    {
        var downloadTaskKey = await _dbContext.GetDownloadTaskKeyAsync(command.DownloadTaskGuid, cancellationToken);
        if (downloadTaskKey is null)
            return ResultExtensions.EntityNotFound(nameof(DownloadTaskGeneric), command.DownloadTaskGuid).LogWarning();

        var childKeys = await _dbContext.GetDownloadableChildTaskKeys(downloadTaskKey, cancellationToken);

        await _downloadTaskUpdateDispatcher.OnStatusChangedAsync(
            downloadTaskKey,
            DownloadStatus.Restarting,
            cancellationToken
        );

        await _dbContext.CreateDownloadClientLog(
            downloadTaskKey,
            NotificationLevel.Information,
            DownloadStatus.Restarting,
            $"Restart requested for download task group {downloadTaskKey.Id}. {childKeys.Count} child task(s) will be processed."
        );

        foreach (var childKey in childKeys)
        {
            var downloadTask = await _dbContext.GetDownloadTaskAsync(childKey, cancellationToken);
            if (downloadTask is null)
            {
                ResultExtensions.EntityNotFound(nameof(DownloadTaskGeneric), childKey.Id).LogError();
                continue;
            }

            var stopResult = await _commandExecutor.Send(new StopDownloadTaskCommand(childKey.Id), cancellationToken);

            if (stopResult.IsFailed)
                return stopResult.LogError();

            await _downloadTaskUpdateDispatcher.OnStatusChangedAsync(
                childKey,
                DownloadStatus.Restarting,
                cancellationToken
            );

            await _dbContext.CreateDownloadClientLog(
                childKey,
                NotificationLevel.Information,
                DownloadStatus.Restarting,
                $"Restart workflow: stop completed for child task {childKey.Id} ({downloadTask.FileName}), preparing to queue."
            );

            switch (downloadTask.DownloadTaskType)
            {
                case DownloadTaskType.MovieData:
                    var refreshResult = await RefreshMovieDownloadTask(childKey, cancellationToken);
                    if (refreshResult.IsFailed)
                        continue;

                    break;

                case DownloadTaskType.EpisodeData:
                    var refreshEpisodeResult = await RefreshEpisodeMovieDownloadTask(childKey, cancellationToken);
                    if (refreshEpisodeResult.IsFailed)
                        continue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"The {downloadTask.DownloadTaskType} is unsupported");
            }

            await _dbContext.CreateDownloadClientLog(
                childKey,
                NotificationLevel.Information,
                DownloadStatus.Queued,
                $"Restart workflow: child task {childKey.Id} ({downloadTask.FileName}) queued again."
            );
        }

        await _eventPublisher.PublishAsync(
            new CheckDownloadQueueEvent(downloadTaskKey.PlexServerId),
            cancellationToken
        );

        await _dbContext.CreateDownloadClientLog(
            downloadTaskKey,
            NotificationLevel.Information,
            DownloadStatus.Queued,
            $"Restart workflow complete for group {downloadTaskKey.Id}; queue check published."
        );

        return Result.Ok();
    }

    private async Task<Result> RefreshMovieDownloadTask(
        DownloadTaskKey downloadTaskKey,
        CancellationToken cancellationToken
    )
    {
        var downloadTask = await _dbContext.DownloadTaskMovieFile.FirstOrDefaultAsync(
            x => x.Id == downloadTaskKey.Id,
            CancellationToken.None
        );
        if (downloadTask is null)
            return ResultExtensions.EntityNotFound(nameof(DownloadTaskGeneric), downloadTaskKey.Id).LogError();

        var newDownloadTask = await _dbContext.PlexMovieData
            .Include(x => x.PlexMovie)
            .Where(x =>
                x.PlexApiMediaId == downloadTask.PlexApiMediaId && x.PlexApiPartId == downloadTask.PlexApiPartId)
            .Select(x => new DownloadTaskMovieFile
            {
                Id = downloadTask.Id,
                Parent = downloadTask.Parent,
                ParentId = downloadTask.ParentId,
                Title = x.GetFileName,
                DownloadStatus = DownloadStatus.Queued,
                CreatedAt = DateTime.UtcNow,
                FullTitle = $"{x.PlexMovie!.FullTitle}/{x.GetFileName}",
                PlexServerId = downloadTask.PlexServerId,
                PlexLibraryId = downloadTask.PlexLibraryId,
                PlexApiRatingKey = downloadTask.PlexApiRatingKey,
                PlexApiMediaId = downloadTask.PlexApiMediaId,
                PlexApiPartId = downloadTask.PlexApiPartId,
                FileName = x.GetFileName,
                FileLocationUrl = x.Key,
                HashId = downloadTask.HashId,
                Quality = x.VideoResolution,
                DirectoryMeta = new DownloadTaskDirectory
                {
                    DownloadRootPath = string.Empty,
                    DestinationRootPath = downloadTask.DirectoryMeta.DestinationRootPath,
                    MovieFolder = x.PlexMovie.Title.SanitizeFolderName(),
                    TvShowFolder = string.Empty,
                    SeasonFolder = string.Empty,
                    KeepCompletedInDownloadFolder = downloadTask.DirectoryMeta.KeepCompletedInDownloadFolder,
                },
                DataReceived = 0,
                DataTotal = 0,
                DownloadSpeed = 0,
                DirectDownloadSnapshot = null,
                DownloadClientType = downloadTask.DownloadClientType,
                FileTransferSpeed = 0,
                FileDataTransferred = 0,
                TimeRemaining = 0,
                DestinationFolderPathId = downloadTask.DestinationFolderPathId,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (newDownloadTask is null)
        {
            await _downloadTaskUpdateDispatcher.OnStatusChangedAsync(
                downloadTask.ToKey(),
                DownloadStatus.SourceUnavailable
            );

            await _dbContext.CreateDownloadClientLog(
                downloadTask.ToKey(),
                NotificationLevel.Information,
                DownloadStatus.SourceUnavailable,
                $"Could not find the original source media for download task \"{downloadTaskKey}\" with title \"{downloadTask.FullTitle}\""
            );

            return Result.Fail(
                $"Could not find the original source media for download task \"{downloadTaskKey}\" with title \"{downloadTask.FullTitle}\"");
        }

        return await Result.Try(async Task () =>
        {
            _dbContext.DownloadTaskMovieFile.Update(newDownloadTask);
            await _dbContext.SaveChangesAsync(cancellationToken);
        });
    }

    private async Task<Result> RefreshEpisodeMovieDownloadTask(
        DownloadTaskKey downloadTaskKey,
        CancellationToken cancellationToken
    )
    {
        var downloadTask =
            await _dbContext.DownloadTaskTvShowEpisodeFile.FirstOrDefaultAsync(
                x => x.Id == downloadTaskKey.Id,
                CancellationToken.None
            );
        if (downloadTask is null)
            return ResultExtensions.EntityNotFound(nameof(DownloadTaskGeneric), downloadTaskKey.Id).LogError();

        var newDownloadTask = await _dbContext.PlexTvShowEpisodeData
            .Include(x => x.PlexTvShowEpisode)
            .ThenInclude(x => x!.TvShowSeason)
            .ThenInclude(x => x!.TvShow)
            .Where(x =>
                x.PlexApiMediaId == downloadTask.PlexApiMediaId && x.PlexApiPartId == downloadTask.PlexApiPartId)
            .Select(x => new DownloadTaskTvShowEpisodeFile
            {
                Id = downloadTask.Id,
                Parent = downloadTask.Parent,
                ParentId = downloadTask.ParentId,
                Title = x.GetFileName,
                DownloadStatus = DownloadStatus.Queued,
                CreatedAt = DateTime.UtcNow,
                FullTitle = $"{x.PlexTvShowEpisode!.FullTitle}/{x.GetFileName}",
                PlexServerId = downloadTask.PlexServerId,
                PlexLibraryId = downloadTask.PlexLibraryId,
                PlexApiRatingKey = downloadTask.PlexApiRatingKey,
                PlexApiMediaId = downloadTask.PlexApiMediaId,
                PlexApiPartId = downloadTask.PlexApiPartId,
                FileName = x.GetFileName,
                FileLocationUrl = x.Key,
                HashId = downloadTask.HashId,
                Quality = x.VideoResolution,
                DirectoryMeta = new DownloadTaskDirectory
                {
                    DownloadRootPath = string.Empty,
                    DestinationRootPath = downloadTask.DirectoryMeta.DestinationRootPath,
                    MovieFolder = string.Empty,
                    TvShowFolder = x.PlexTvShowEpisode!.TvShow!.Title.SanitizeFolderName(),
                    SeasonFolder = x.PlexTvShowEpisode!.TvShowSeason!.Title.SanitizeFolderName(),
                    KeepCompletedInDownloadFolder = downloadTask.DirectoryMeta.KeepCompletedInDownloadFolder,
                },
                DataReceived = 0,
                DataTotal = 0,
                DownloadSpeed = 0,
                DirectDownloadSnapshot = null,
                DownloadClientType = downloadTask.DownloadClientType,
                FileTransferSpeed = 0,
                FileDataTransferred = 0,
                TimeRemaining = 0,
                DestinationFolderPathId = downloadTask.DestinationFolderPathId,
            })
            .FirstOrDefaultAsync(CancellationToken.None);

        if (newDownloadTask is null)
        {
            await _downloadTaskUpdateDispatcher.OnStatusChangedAsync(
                downloadTask.ToKey(),
                DownloadStatus.SourceUnavailable,
                CancellationToken.None
            );

            await _dbContext.CreateDownloadClientLog(
                downloadTask.ToKey(),
                NotificationLevel.Information,
                DownloadStatus.SourceUnavailable,
                $"Could not find the original source media for download task \"{downloadTaskKey}\" with title \"{downloadTask.FullTitle}\"");

            return Result.Fail(
                $"Could not find the original source media for download task \"{downloadTaskKey}\" with title \"{downloadTask.FullTitle}\"");
        }

        return await Result.Try(async Task () =>
        {
            _dbContext.DownloadTaskTvShowEpisodeFile.Update(newDownloadTask);
            await _dbContext.SaveChangesAsync(cancellationToken);
        });
    }
}