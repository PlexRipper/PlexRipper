namespace Reaparr.BackgroundJobs;

/// <summary>
/// Checks Plex library section metadata and queues full syncs for libraries marked as outdated after Plex access refresh.
/// </summary>
public record CheckPlexLibrariesForUpdatesCommand : ICommand<Result>;

public class CheckPlexLibrariesForUpdatesCommandValidator : AbstractValidator<CheckPlexLibrariesForUpdatesCommand>
{
    public CheckPlexLibrariesForUpdatesCommandValidator()
    {
        RuleFor(x => x).NotNull();
    }
}

public class CheckPlexLibrariesForUpdatesCommandHandler
    : ICommandHandler<CheckPlexLibrariesForUpdatesCommand, Result>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;

    public CheckPlexLibrariesForUpdatesCommandHandler(
        ILogger log,
        IReaparrDbContext dbContext,
        ICommandExecutor commandExecutor
    )
    {
        _log = log.ForContext<CheckPlexLibrariesForUpdatesCommandHandler>();
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
    }

    public async Task<Result> ExecuteAsync(
        CheckPlexLibrariesForUpdatesCommand command,
        CancellationToken cancellationToken)
    {
        var enabledServerIds = await _dbContext
            .PlexServers
            .AsNoTracking()
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (!enabledServerIds.Any())
        {
            _log.Here().Debug("No enabled Plex servers found for automatic library sync");
            return Result.Ok();
        }

        var accountMappings = await _dbContext
            .PlexAccountServers
            .AsNoTracking()
            .Where(x => enabledServerIds.Contains(x.PlexServerId))
            .GroupBy(x => x.PlexServerId)
            .Select(x => new
            {
                PlexServerId = x.Key,
                PlexAccountId = x.Min(y => y.PlexAccountId),
            })
            .ToDictionaryAsync(x => x.PlexServerId, x => x.PlexAccountId, cancellationToken);

        var serversWithTokenMappings = new List<int>();
        foreach (var serverId in enabledServerIds)
        {
            if (!accountMappings.TryGetValue(serverId, out var accountId) || accountId <= 0)
            {
                _log.Here()
                    .Warning(
                        "No Plex account-server token mapping found for PlexServer with id {PlexServerId}; skipping refresh",
                        serverId
                    );
                continue;
            }

            var refreshResult = await _commandExecutor.Send(
                new RefreshLibraryAccessCommand(accountId, serverId),
                cancellationToken
            );

            if (refreshResult.IsFailed)
            {
                refreshResult.ToResult().LogError();
                continue;
            }

            serversWithTokenMappings.Add(serverId);
        }

        if (!serversWithTokenMappings.Any())
        {
            _log.Here().Debug("No Plex servers with token mappings found for automatic library sync");
            return Result.Ok();
        }

        var outdatedLibraryIds = await _dbContext
            .PlexLibraries.AsNoTracking()
            .Where(x => serversWithTokenMappings.Contains(x.PlexServerId))
            .Where(x => x.Type == PlexMediaType.Movie || x.Type == PlexMediaType.TvShow)
            .Where(x => x.Outdated)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (!outdatedLibraryIds.Any())
        {
            _log.Here().Debug("No outdated Plex libraries found for automatic sync");
            return Result.Ok();
        }

        _log.Here()
            .Information("Queueing {Count} outdated Plex libraries for automatic sync", outdatedLibraryIds.Count);

        var queueResult =
            await _commandExecutor.Send(new QueueLibrarySyncJobCommand(outdatedLibraryIds), cancellationToken);
        if (queueResult.IsCancelled)
            return queueResult;

        if (queueResult.IsFailed)
            return queueResult.LogError();

        return Result.Ok();
    }
}