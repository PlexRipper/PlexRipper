namespace Reaparr.Application;

public class ClearCompletedDownloadTasksByDownloadTaskKeyCommandValidator
    : Validator<ClearCompletedDownloadTasksByDownloadTaskKeyCommand>
{
    public ClearCompletedDownloadTasksByDownloadTaskKeyCommandValidator()
    {
        RuleFor(x => x.DownloadTaskKeys).NotEmpty();
    }
}

public class ClearCompletedDownloadTasksByDownloadTaskKeyCommandHandler
    : ICommandHandler<ClearCompletedDownloadTasksByDownloadTaskKeyCommand, Result<int>>
{
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;

    public ClearCompletedDownloadTasksByDownloadTaskKeyCommandHandler(
        IReaparrDbContext dbContext,
        ICommandExecutor commandExecutor
    )
    {
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
    }

    public async Task<Result<int>> ExecuteAsync(
        ClearCompletedDownloadTasksByDownloadTaskKeyCommand request,
        CancellationToken ct
    )
    {
        var keys = request.DownloadTaskKeys;
        var byType = keys.ToLookup(k => k.Type);

        // Confirm which of the requested keys actually exist in a Completed state.
        // Keys are already typed, so we only query the tables that are relevant.
        var confirmedKeys = new HashSet<DownloadTaskKey>();

        confirmedKeys.UnionWith(
            await ConfirmCompleted(_dbContext.DownloadTaskMovie, byType[DownloadTaskType.Movie].ToList(), ct)
        );
        confirmedKeys.UnionWith(
            await ConfirmCompleted(
                _dbContext.DownloadTaskMovieFile,
                byType[DownloadTaskType.MovieData].Concat(byType[DownloadTaskType.MoviePart]).ToList(),
                ct
            )
        );
        confirmedKeys.UnionWith(
            await ConfirmCompleted(_dbContext.DownloadTaskTvShow, byType[DownloadTaskType.TvShow].ToList(), ct)
        );
        confirmedKeys.UnionWith(
            await ConfirmCompleted(_dbContext.DownloadTaskTvShowSeason, byType[DownloadTaskType.Season].ToList(), ct)
        );
        confirmedKeys.UnionWith(
            await ConfirmCompleted(_dbContext.DownloadTaskTvShowEpisode, byType[DownloadTaskType.Episode].ToList(), ct)
        );
        confirmedKeys.UnionWith(
            await ConfirmCompleted(
                _dbContext.DownloadTaskTvShowEpisodeFile,
                byType[DownloadTaskType.EpisodeData].Concat(byType[DownloadTaskType.EpisodePart]).ToList(),
                ct
            )
        );

        var completedKeys = confirmedKeys.Distinct().ToList();

        if (completedKeys.Count == 0)
            return Result.Ok(0);

        var deleteResult = await _commandExecutor.Send(new DeleteDownloadTasksByKeyCommand(completedKeys), ct);

        return deleteResult.IsFailed ? deleteResult.LogIfFailed() : Result.Ok(completedKeys.Count);
    }

    private static async Task<List<DownloadTaskKey>> ConfirmCompleted<T>(
        IQueryable<T> set,
        IReadOnlyCollection<DownloadTaskKey> keys,
        CancellationToken ct
    )
        where T : DownloadTaskBase
    {
        var idList = keys.Select(x => x.Id).Distinct().ToList();
        if (idList.Count == 0)
            return [];

        return await set.Where(x => idList.Contains(x.Id) && x.DownloadStatus == DownloadStatus.Completed)
            .Select(x => new DownloadTaskKey
            {
                Id = x.Id,
                Type = x.DownloadTaskType,
                PlexServerId = x.PlexServerId,
                PlexLibraryId = x.PlexLibraryId,
            })
            .ToListAsync(ct);
    }
}
