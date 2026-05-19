namespace Reaparr.Application;

public class GetPlexLibraryAccessTimelineEndpoint : BaseEndpointWithoutRequest<PlexLibraryAccessTimelineDTO>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.PlexLibraryController + "/access-timeline";

    public GetPlexLibraryAccessTimelineEndpoint(ILogger log, IReaparrDbContext dbContext)
    {
        _log = log.ForContext<GetPlexLibraryAccessTimelineEndpoint>();
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<PlexLibraryAccessTimelineDTO>))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext);

        var events = await _dbContext.PlexLibraryAccessHistoryEvents
            .GroupJoin(
                _dbContext.PlexAccounts,
                historyEvent => historyEvent.PlexAccountId,
                account => account.Id,
                (historyEvent, accounts) => new { HistoryEvent = historyEvent, Accounts = accounts }
            )
            .SelectMany(
                x => x.Accounts.DefaultIfEmpty(),
                (x, account) => new { x.HistoryEvent, Account = account }
            )
            .GroupJoin(
                _dbContext.PlexServers,
                x => x.HistoryEvent.PlexServerId,
                server => server.Id,
                (x, servers) => new { x.HistoryEvent, x.Account, Servers = servers }
            )
            .SelectMany(
                x => x.Servers.DefaultIfEmpty(),
                (x, server) => new { x.HistoryEvent, x.Account, Server = server }
            )
            .GroupJoin(
                _dbContext.PlexLibraries,
                x => x.HistoryEvent.PlexLibraryId,
                library => library.Id,
                (x, libraries) => new { x.HistoryEvent, x.Account, x.Server, Libraries = libraries }
            )
            .SelectMany(
                x => x.Libraries.DefaultIfEmpty(),
                (x, library) => new PlexLibraryAccessTimelineEventDTO
                {
                    Id = x.HistoryEvent.Id,
                    RefreshRunId = x.HistoryEvent.RefreshRunId,
                    PlexAccountId = x.HistoryEvent.PlexAccountId,
                    PlexAccountName = x.Account != null ? x.Account.DisplayName : x.HistoryEvent.PlexAccountNameSnapshot ?? string.Empty,
                    PlexServerId = x.HistoryEvent.PlexServerId,
                    PlexServerName = x.Server != null ? x.Server.Name : x.HistoryEvent.PlexServerNameSnapshot,
                    PlexLibraryId = x.HistoryEvent.PlexLibraryId,
                    PlexLibraryName = library != null ? library.Title : x.HistoryEvent.PlexLibraryNameSnapshot,
                    State = x.HistoryEvent.State,
                    OccurredAtUtc = x.HistoryEvent.OccurredAtUtc,
                }
            )
            .OrderBy(x => x.OccurredAtUtc)
            .ThenBy(x => x.PlexServerName)
            .ThenBy(x => x.PlexLibraryName)
            .ToListAsync(ct);

        var response = new PlexLibraryAccessTimelineDTO
        {
            Events = events,
            CurrentState = ToCurrentState(events),
        };

        await SendFluentResult(Result.Ok(response), x => x, ct);
    }

    private static List<PlexLibraryAccessCurrentStateDTO> ToCurrentState(List<PlexLibraryAccessTimelineEventDTO> events)
    {
        List<PlexLibraryAccessCurrentStateLibraryDTO> accessibleLibraries = [];
        foreach (var eventGroup in events.GroupBy(x => new { x.PlexAccountId, x.PlexServerId, x.PlexLibraryId }))
        {
            var accessibleLibrary = ToAccessibleLibraryOrDefault(
                eventGroup.OrderBy(x => x.OccurredAtUtc).ThenBy(x => x.Id).ToList()
            );
            if (accessibleLibrary is not null)
                accessibleLibraries.Add(accessibleLibrary);
        }

        return accessibleLibraries
            .GroupBy(x => new { x.PlexServerId, x.PlexServerName })
            .Select(x => new PlexLibraryAccessCurrentStateDTO
            {
                PlexServerId = x.Key.PlexServerId,
                PlexServerName = x.Key.PlexServerName,
                Libraries = x.OrderBy(y => y.PlexLibraryName).ThenBy(y => y.PlexAccountName).ToList(),
            })
            .OrderBy(x => x.PlexServerName)
            .ToList();
    }

    private static PlexLibraryAccessCurrentStateLibraryDTO? ToAccessibleLibraryOrDefault(
        List<PlexLibraryAccessTimelineEventDTO> events
    )
    {
        var latestEvent = events.Last();
        if (latestEvent.State != PlexAccessState.Granted)
            return null;

        return new PlexLibraryAccessCurrentStateLibraryDTO
        {
            PlexServerId = latestEvent.PlexServerId,
            PlexServerName = latestEvent.PlexServerName,
            PlexLibraryId = latestEvent.PlexLibraryId,
            PlexLibraryName = latestEvent.PlexLibraryName,
            PlexAccountId = latestEvent.PlexAccountId,
            PlexAccountName = latestEvent.PlexAccountName,
            GrantedAt = latestEvent.OccurredAtUtc,
            LastChangedAt = latestEvent.OccurredAtUtc,
        };
    }
}
