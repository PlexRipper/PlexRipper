using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.FileSystem.Contracts;
using Reaparr.Logging;
using Serilog;

namespace Reaparr.Application;

public class FileMergeQueue : IFileMergeQueue
{
    private readonly Serilog.ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly IFileMergeScheduler _fileMergeScheduler;

    public FileMergeQueue(ILogger log, IReaparrDbContext dbContext, IFileMergeScheduler fileMergeScheduler)
    {
        _log = log.ForContext<FileMergeQueue>();
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
            return _log.Here().ErrorResult("No DownloadTask found to either merge or move");
        }

        var startResult = await _fileMergeScheduler.StartFileMergeJob(key);
        return startResult.IsSuccess ? Result.Ok(key) : startResult;
    }
}
