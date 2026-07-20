namespace Reaparr.Application;

/// <summary>
/// Dispatches comparison state projection to type- and ownership-specific sub-handlers
/// via <see cref="ICommandExecutor"/>. Determines whether the target library is owned or remote,
/// then sends the appropriate sub-command.
/// </summary>
public class ApplyComparisonStateCommandHandler : ICommandHandler<ApplyComparisonStateCommand, Result>
{
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;
    private readonly ILogger _log;

    public ApplyComparisonStateCommandHandler(
        IReaparrDbContext dbContext,
        ICommandExecutor commandExecutor,
        ILogger log)
    {
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
        _log = log.ForContext<ApplyComparisonStateCommandHandler>();
    }

    public async Task<Result> ExecuteAsync(ApplyComparisonStateCommand command, CancellationToken ct)
    {
        var items = command.Items;
        if (items.Count == 0 || command.PlexLibraryId == 0)
            return Result.Ok();

        var isOwned = await _dbContext.PlexLibraries
            .WhereIsOwned()
            .AnyAsync(x => x.Id == command.PlexLibraryId, ct);

        if (isOwned)
        {
            if (command.MediaType == PlexMediaType.Movie)
                return await _commandExecutor.Send(
                    new ApplyOwnedMovieComparisonStateCommand(items, command.PlexLibraryId), ct);
            if (command.MediaType == PlexMediaType.TvShow)
                return await _commandExecutor.Send(
                    new ApplyOwnedTvShowComparisonStateCommand(items, command.PlexLibraryId), ct);
        }
        else
        {
            if (command.MediaType == PlexMediaType.Movie)
                return await _commandExecutor.Send(
                    new ApplyRemoteMovieComparisonStateCommand(items, command.PlexLibraryId), ct);
            if (command.MediaType == PlexMediaType.TvShow)
                return await _commandExecutor.Send(
                    new ApplyRemoteTvShowComparisonStateCommand(items, command.PlexLibraryId), ct);
        }

        return Result.Ok();
    }
}
