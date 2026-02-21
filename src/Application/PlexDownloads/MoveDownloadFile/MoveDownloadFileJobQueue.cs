using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.FileSystem.Contracts;

namespace Reaparr.Application;

public class MoveDownloadFileJobQueue : IMoveDownloadFileQueue
{
    private readonly ILogger _log;
    private readonly IReaparrDbContextFactory _dbContextFactory;
    private readonly IMoveDownloadFileScheduler _moveDownloadFileScheduler;

    public MoveDownloadFileJobQueue(
        ILogger log,
        IReaparrDbContextFactory dbContextFactory,
        IMoveDownloadFileScheduler moveDownloadFileScheduler
    )
    {
        _log = log.ForContext<MoveDownloadFileJobQueue>();
        _dbContextFactory = dbContextFactory;
        _moveDownloadFileScheduler = moveDownloadFileScheduler;
    }

    /// <inheritdoc/>
    public async Task<Result> CheckMoveDownloadFileJobQueue()
    {
        if (await _moveDownloadFileScheduler.IsAnyMoveDownloadFileJobRunning())
        {
            _log.Here().Debug("A MoveDownloadFileJob is already running, skipping queue check");
            return Result.Ok();
        }

        // Create a new DbContext for this operation to avoid threading issues
        using var dbContext = await _dbContextFactory.CreateAsync();

        // Find the first ready-to-move task (DownloadFinished preferred, MoveError as retry; movies before episodes)
        var key =
            await dbContext
                .DownloadTaskMovieFile.Where(x =>
                    x.DownloadStatus == DownloadStatus.DownloadFinished || x.DownloadStatus == DownloadStatus.MoveError
                )
                .OrderByDescending(x => x.DownloadStatus == DownloadStatus.DownloadFinished)
                .Select(x => x.ToKey())
                .FirstOrDefaultAsync()
            ?? await dbContext
                .DownloadTaskTvShowEpisodeFile.Where(x =>
                    x.DownloadStatus == DownloadStatus.DownloadFinished || x.DownloadStatus == DownloadStatus.MoveError
                )
                .OrderByDescending(x => x.DownloadStatus == DownloadStatus.DownloadFinished)
                .Select(x => x.ToKey())
                .FirstOrDefaultAsync();

        if (key is null)
        {
            _log.Here().Debug("No DownloadTask with status DownloadFinished or MoveError found, nothing to move");
            return Result.Ok();
        }

        var startResult = await _moveDownloadFileScheduler.StartMoveDownloadFileJob(key);
        return startResult.IsSuccess ? Result.Ok() : startResult;
    }
}
