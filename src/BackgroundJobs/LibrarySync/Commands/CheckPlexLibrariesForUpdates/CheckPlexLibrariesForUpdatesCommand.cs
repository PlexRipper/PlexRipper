namespace Reaparr.BackgroundJobs;

/// <summary>
/// Checks Plex library section metadata and queues full syncs for libraries whose Plex UpdatedAt value is newer than Reaparr's SyncedAt value.
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
        var servers = await _dbContext
            .PlexServers.Select(x => new
            {
                x.Id,
            })
            .ToListAsync(cancellationToken);

        if (!servers.Any())
        {
            _log.Here().Debug("No enabled Plex servers found for automatic library sync");
            return Result.Ok();
        }

        var accountId = await _dbContext.PlexAccounts.Select(x => x.Id).FirstOrDefaultAsync(cancellationToken);

        if (accountId <= 0)
        {
            _log.Here().Warning("No Plex account found for automatic library sync");
            return Result.Ok();
        }

        var enabledServerIds = new List<int>();
        foreach (var server in servers)
        {
            enabledServerIds.Add(server.Id);

            var refreshResult = await _commandExecutor.Send(
                new RefreshLibraryAccessCommand(accountId, server.Id),
                cancellationToken
            );

            if (refreshResult.IsFailed)
                refreshResult.ToResult().LogError();
        }

        if (!enabledServerIds.Any())
        {
            _log.Here().Debug("No Plex servers are enabled for automatic library sync");
            return Result.Ok();
        }

        var outdatedLibraryIds = await _dbContext
            .PlexLibraries.AsNoTracking()
            .Where(x => enabledServerIds.Contains(x.PlexServerId))
            .Where(x => x.Type == PlexMediaType.Movie || x.Type == PlexMediaType.TvShow)
            .Where(x => x.UpdatedAt != null && (x.SyncedAt == null || x.SyncedAt < x.UpdatedAt))
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
        if (queueResult.IsFailed)
            return queueResult.LogError();

        return Result.Ok();
    }
}