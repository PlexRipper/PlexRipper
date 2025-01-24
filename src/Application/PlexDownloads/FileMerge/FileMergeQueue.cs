using Application.Contracts;
using Data.Contracts;
using FileSystem.Contracts;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public class FileMergeQueue : IFileMergeQueue
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IFileMergeScheduler _fileMergeScheduler;

    public FileMergeQueue(ILog log, IPlexRipperDbContext dbContext, IFileMergeScheduler fileMergeScheduler)
    {
        _log = log;
        _dbContext = dbContext;
        _fileMergeScheduler = fileMergeScheduler;
    }

    /// <inheritdoc/>
    public async Task<Result<DownloadTaskKey>> CheckFileMergeQueue()
    {
        if (await _fileMergeScheduler.IsAnyFileMergeJobRunning())
        {
            return Result.Fail("Cannot run more than 1 fileMergeJob at the same time.").LogWarning();
        }

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
        {
            return _log.DebugLine("No DownloadTask found to either merge or move").ToResult();
        }

        var startResult = await _fileMergeScheduler.StartFileMergeJob(key);
        return startResult.IsSuccess ? Result.Ok(key) : startResult;
    }
}
