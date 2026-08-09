namespace Reaparr.Application;

public record RefreshPlexAccountAccessEndpointRequest(int PlexAccountId = 0);

public class RefreshPlexAccountAccessEndpointRequestValidator : Validator<RefreshPlexAccountAccessEndpointRequest>
{
    public RefreshPlexAccountAccessEndpointRequestValidator()
    {
        RuleFor(x => x.PlexAccountId).GreaterThanOrEqualTo(0);
    }
}

public class RefreshPlexAccountAccessEndpoint
    : Endpoint<RefreshPlexAccountAccessEndpointRequest, ResultDTO<List<RefreshPlexAccountAccessRapportDTO>>>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;
    private readonly INotificationHubService _notificationHubService;
    private readonly IMediaQueryCache _mediaQueryCache;
    private List<RefreshPlexAccountAccessRapportDTO> _list = new();

    public RefreshPlexAccountAccessEndpoint(
        ILogger log,
        IReaparrDbContext dbContext,
        ICommandExecutor commandExecutor,
        INotificationHubService notificationHubService,
        IMediaQueryCache mediaQueryCache
    )
    {
        _log = log.ForContext<RefreshPlexAccountAccessEndpoint>();
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
        _notificationHubService = notificationHubService;
        _mediaQueryCache = mediaQueryCache;
    }

    public override void Configure()
    {
        Get(ApiRoutes.PlexAccountController + "/refresh/{PlexAccountId}");

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<List<RefreshPlexAccountAccessRapportDTO>>))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(RefreshPlexAccountAccessEndpointRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, req);
        var plexAccountIds = new List<int>();
        if (req.PlexAccountId > 0)
        {
            plexAccountIds.Add(req.PlexAccountId);
        }
        else
        {
            var enabledAccounts = await _dbContext.PlexAccounts.Where(x => x.IsEnabled).ToListAsync(ct);
            if (!enabledAccounts.Any())
            {
                _log.Here().Warning("No enabled Plex accounts found to start the refresh PlexServer access job");
                await Send.FluentResult(Result.Ok(new List<RefreshPlexAccountAccessRapportDTO>()), ct);
                return;
            }

            plexAccountIds.AddRange(enabledAccounts.Select(x => x.Id));
        }

        // Execute
        foreach (var plexAccountId in plexAccountIds)
        {
            var serverAccessResult = await _commandExecutor.Send(new RefreshPlexServerAccessCommand(plexAccountId), ct);

            if (serverAccessResult.IsCancelled)
            {
                serverAccessResult.LogWarning();
                await Send.FluentResult(serverAccessResult.ToResult(), CancellationToken.None);
                return;
            }

            if (serverAccessResult.IsFailed)
            {
                serverAccessResult.LogError();
                continue;
            }

            var plexAccountName = await _dbContext.GetPlexAccountDisplayName(plexAccountId, ct);
            var serverAccessRapport = serverAccessResult.Value;

            // If the Plex API returns a 401 Unauthorized error, remove the PlexAccount and PlexServerAccess
            if (serverAccessRapport.Access.All(x => x.State == PlexAccessState.Revoked))
            {
                var lostServerAccess = serverAccessRapport
                    .Access.Where(x => x.State == PlexAccessState.Revoked)
                    .Select(x => x.PlexServerId)
                    .ToList();

                var lostLibraryAccess = await _dbContext
                    .PlexAccountLibraries.Include(x => x.PlexLibrary)
                    .Include(x => x.PlexServer)
                    .Where(x => x.PlexAccountId == plexAccountId && lostServerAccess.Contains(x.PlexServerId))
                    .ToListAsync(cancellationToken: ct);

                var libraryAccessRapport = new PlexLibraryAccessRefreshResponse
                {
                    Reports = lostLibraryAccess
                        .Select(x =>
                            new PlexLibraryAccessRapport(
                                plexAccountName,
                                x.PlexServerId,
                                x.PlexServer!.Name
                            ).AddRevoked(x.PlexLibraryId, x.PlexLibrary!.Name)
                        )
                        .ToList(),
                    OfflineServers = [],
                };

                _list.Add(ToDTO(serverAccessRapport, libraryAccessRapport));

                // Remove LibraryAccess for the given PlexAccount
                var affectedLibraryIds = lostLibraryAccess.Select(x => x.PlexLibraryId).Distinct().ToList();
                await _dbContext
                    .PlexAccountLibraries.Where(x =>
                        x.PlexAccountId == plexAccountId && lostServerAccess.Contains(x.PlexServerId)
                    )
                    .ExecuteDeleteAsync(cancellationToken: ct);
                _mediaQueryCache.InvalidateLibraries(affectedLibraryIds, "Plex account library access revoked");
            }
            else
            {
                // Update library access
                var libraryAccessResult = await _commandExecutor.Send(
                    new RefreshLibraryAccessCommand(plexAccountId),
                    ct
                );

                if (libraryAccessResult.IsCancelled)
                {
                    libraryAccessResult.LogWarning();
                    await Send.FluentResult(libraryAccessResult.ToResult(), CancellationToken.None);
                    return;
                }

                if (libraryAccessResult.IsFailed)
                {
                    libraryAccessResult.LogError();
                    continue;
                }

                var libraryAccessRapport = libraryAccessResult.Value;

                _list.Add(ToDTO(serverAccessRapport, libraryAccessRapport));
            }
        }

        // Send notifications to the client to refresh the PlexServerConnection data
        await _notificationHubService.SendRefreshNotificationAsync(
            [RefreshDataType.PlexAccount, RefreshDataType.PlexServer, RefreshDataType.PlexServerConnection]
        );

        await Send.FluentResult(Result.Ok(_list), ct);
    }

    private RefreshPlexAccountAccessRapportDTO ToDTO(
        RefreshPlexServerAccessRapport serverAccessRapport,
        PlexLibraryAccessRefreshResponse libraryAccessRapport
    )
    {
        return new RefreshPlexAccountAccessRapportDTO(
            serverAccessRapport.PlexAccountId,
            serverAccessRapport.PlexAccountName
        )
        {
            Access = serverAccessRapport
                .Access.Select(x => new PlexServerAccessRapportDTO
                {
                    State = x.State,
                    PlexServerId = x.PlexServerId,
                    PlexServerName = x.PlexServerName,
                    IsServerOffline = libraryAccessRapport.OfflineServers.Contains(x.PlexServerId),
                    LibraryAccess = libraryAccessRapport
                        .Reports.Where(y => y.PlexServerId == x.PlexServerId)
                        .SelectMany(y => y.Data)
                        .Select(y => new PlexLibraryAccessRapportDTO
                        {
                            PlexLibraryName = y.PlexLibraryName,
                            PlexServerId = y.PlexServerId,
                            State = y.State,
                            PlexLibraryId = y.PlexLibraryId,
                        })
                        .ToList(),
                })
                .ToList(),
        };
    }
}