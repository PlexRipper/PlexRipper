using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.FileSystem.Contracts;

namespace Reaparr.Application;

public class MoveDownloadFileJobQueue : IMoveDownloadFileQueue
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly IMoveDownloadFileScheduler _moveDownloadFileScheduler;

    public MoveDownloadFileJobQueue(
        ILogger log,
        IReaparrDbContext dbContext,
        IMoveDownloadFileScheduler moveDownloadFileScheduler
    )
    {
        _log = log.ForContext<MoveDownloadFileJobQueue>();
        _dbContext = dbContext;
        _moveDownloadFileScheduler = moveDownloadFileScheduler;
    }

    /// <inheritdoc/>
    public async Task<Result> CheckMoveDownloadFileJobQueue()
    {
        if (await _moveDownloadFileScheduler.IsAnyMoveDownloadFileJobRunning())
            return Result.Fail("A MoveDownloadFileJob is already running, skipping queue check").LogInformation();

        // Find the first finished task (movie preferred, then episode)
        var key =
            await _dbContext
                .DownloadTaskMovieFile.Where(x => x.DownloadStatus == DownloadStatus.DownloadFinished)
                .Select(x => x.ToKey())
                .FirstOrDefaultAsync()
            ?? await _dbContext
                .DownloadTaskTvShowEpisodeFile.Where(x => x.DownloadStatus == DownloadStatus.DownloadFinished)
                .Select(x => x.ToKey())
                .FirstOrDefaultAsync();

        if (key is null)
            return _log.Here().ErrorResult("No DownloadTask found to either merge or move");

        var startResult = await _moveDownloadFileScheduler.StartMoveDownloadFileJob(key);
        return startResult.IsSuccess ? Result.Ok() : startResult;
    }
}
