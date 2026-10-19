using Microsoft.Extensions.Hosting;

namespace Reaparr.Application;

public class MoveDownloadFileJobQueue : IMoveDownloadFileQueue
{
    private readonly ILogger _log;
    private readonly IReaparrDbContextFactory _dbContextFactory;
    private readonly IMoveDownloadFileScheduler _moveDownloadFileScheduler;
    private readonly IHostApplicationLifetime _appLifetime;

    public MoveDownloadFileJobQueue(
        ILogger log,
        IReaparrDbContextFactory dbContextFactory,
        IMoveDownloadFileScheduler moveDownloadFileScheduler,
        IHostApplicationLifetime appLifetime
    )
    {
        _log = log.ForContext<MoveDownloadFileJobQueue>();
        _dbContextFactory = dbContextFactory;
        _moveDownloadFileScheduler = moveDownloadFileScheduler;
        _appLifetime = appLifetime;
    }

    /// <inheritdoc/>
    public Task<Result> CheckMoveDownloadFileJobQueue() => CheckMoveDownloadFileJobQueue(CancellationToken.None);

    /// <inheritdoc/>
    public async Task<Result> CheckMoveDownloadFileJobQueue(CancellationToken cancellationToken)
    {
        if (_appLifetime.ApplicationStopping.IsCancellationRequested)
            return ResultExtensions.TaskIsCancelled(nameof(CheckMoveDownloadFileJobQueue));

        // Create a new DbContext for this operation to avoid threading issues
        using var dbContext = await _dbContextFactory.CreateAsync();

        // Find the oldest ready-to-move task.
        var movieCandidates = dbContext
            .DownloadTaskMovieFile.Where(x => x.DownloadStatus == DownloadStatus.DownloadFinished)
            .Select(x => new
            {
                x.Id,
                x.PlexServerId,
                x.PlexLibraryId,
                Type = DownloadTaskType.MovieData,
                x.CreatedAt,
            });

        var episodeCandidates = dbContext
            .DownloadTaskTvShowEpisodeFile.Where(x => x.DownloadStatus == DownloadStatus.DownloadFinished)
            .Select(x => new
            {
                x.Id,
                x.PlexServerId,
                x.PlexLibraryId,
                Type = DownloadTaskType.EpisodeData,
                x.CreatedAt,
            });

        var photoCandidates = dbContext
            .DownloadTaskPhotoImageFiles.Where(x => x.DownloadStatus == DownloadStatus.DownloadFinished)
            .Select(x => new
            {
                x.Id,
                x.PlexServerId,
                x.PlexLibraryId,
                Type = DownloadTaskType.PhotoData,
                x.CreatedAt,
            });

        var musicCandidates = dbContext
            .DownloadTaskMusicTrackFiles.Where(x => x.DownloadStatus == DownloadStatus.DownloadFinished)
            .Select(x => new
            {
                x.Id,
                x.PlexServerId,
                x.PlexLibraryId,
                Type = DownloadTaskType.MusicTrackData,
                x.CreatedAt,
            });

        var otherVideoCandidates = dbContext
            .DownloadTaskOtherVideoFiles.Where(x => x.DownloadStatus == DownloadStatus.DownloadFinished)
            .Select(x => new
            {
                x.Id,
                x.PlexServerId,
                x.PlexLibraryId,
                Type = DownloadTaskType.OtherVideoData,
                x.CreatedAt,
            });
        var key = await movieCandidates
            .Concat(episodeCandidates)
            .Concat(photoCandidates)
            .Concat(musicCandidates)
            .Concat(otherVideoCandidates)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new DownloadTaskKey
            {
                Id = x.Id,
                PlexServerId = x.PlexServerId,
                PlexLibraryId = x.PlexLibraryId,
                Type = x.Type,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (key is null)
        {
            _log.Here().Debug("No DownloadTask with status DownloadFinished found, nothing to move");
            return Result.Ok();
        }

        if (_appLifetime.ApplicationStopping.IsCancellationRequested)
            return ResultExtensions.TaskIsCancelled(nameof(CheckMoveDownloadFileJobQueue));

        var startResult = await _moveDownloadFileScheduler.StartMoveDownloadFileJob(key, cancellationToken);
        return startResult.IsSuccess ? Result.Ok() : startResult;
    }
}
