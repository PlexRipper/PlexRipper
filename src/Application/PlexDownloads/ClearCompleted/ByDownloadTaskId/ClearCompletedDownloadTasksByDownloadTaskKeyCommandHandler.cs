using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

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
        var byType = keys.ToLookup(k => k.Type, k => k.Id);

        // Confirm which of the requested keys actually exist in a Completed state.
        // Keys are already typed, so we only query the tables that are relevant.
        var results = await Task.WhenAll(
            ConfirmCompleted(_dbContext.DownloadTaskMovie, byType[DownloadTaskType.Movie], ct),
            ConfirmCompleted(
                _dbContext.DownloadTaskMovieFile,
                byType[DownloadTaskType.MovieData].Concat(byType[DownloadTaskType.MoviePart]),
                ct
            ),
            ConfirmCompleted(_dbContext.DownloadTaskTvShow, byType[DownloadTaskType.TvShow], ct),
            ConfirmCompleted(_dbContext.DownloadTaskTvShowSeason, byType[DownloadTaskType.Season], ct),
            ConfirmCompleted(_dbContext.DownloadTaskTvShowEpisode, byType[DownloadTaskType.Episode], ct),
            ConfirmCompleted(
                _dbContext.DownloadTaskTvShowEpisodeFile,
                byType[DownloadTaskType.EpisodeData].Concat(byType[DownloadTaskType.EpisodePart]),
                ct
            )
        );

        var confirmedIds = results.SelectMany(x => x).ToHashSet();
        var completedKeys = keys.Where(k => confirmedIds.Contains(k.Id)).ToList();

        if (completedKeys.Count == 0)
            return Result.Ok(0);

        var deleteResult = await _commandExecutor.Send(new DeleteDownloadTasksByKeyCommand(completedKeys), ct);
        if (deleteResult.IsFailed)
            return deleteResult.ToResult<int>();

        return Result.Ok(completedKeys.Count);
    }

    private static Task<List<Guid>> ConfirmCompleted<T>(IQueryable<T> set, IEnumerable<Guid> ids, CancellationToken ct)
        where T : DownloadTaskBase
    {
        var idList = ids.ToList();
        if (idList.Count == 0)
            return Task.FromResult(new List<Guid>());

        return set.Where(x => idList.Contains(x.Id) && x.DownloadStatus == DownloadStatus.Completed)
            .Select(x => x.Id)
            .ToListAsync(ct);
    }
}
