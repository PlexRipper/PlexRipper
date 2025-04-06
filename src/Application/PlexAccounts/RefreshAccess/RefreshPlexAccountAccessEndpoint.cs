using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Logging.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public record RefreshPlexAccountAccessEndpointRequest(int PlexAccountId = 0);

public class RefreshPlexAccountAccessEndpointRequestValidator : Validator<RefreshPlexAccountAccessEndpointRequest>
{
    public RefreshPlexAccountAccessEndpointRequestValidator()
    {
        RuleFor(x => x.PlexAccountId).GreaterThanOrEqualTo(0);
    }
}

public class RefreshPlexAccountAccessEndpoint
    : BaseEndpoint<RefreshPlexAccountAccessEndpointRequest, ResultDTO<List<RefreshPlexAccountAccessRapportDTO>>>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IMediator _mediator;
    private readonly ISignalRService _signalRService;
    private List<RefreshPlexAccountAccessRapportDTO> _list = new();

    public override string EndpointPath => ApiRoutes.PlexAccountController + "/refresh/{PlexAccountId}";

    public RefreshPlexAccountAccessEndpoint(
        ILog log,
        IPlexRipperDbContext dbContext,
        IMediator mediator,
        ISignalRService signalRService
    )
    {
        _log = log;
        _dbContext = dbContext;
        _mediator = mediator;
        _signalRService = signalRService;
    }

    public override void Configure()
    {
        Get(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<List<RefreshPlexAccountAccessRapportDTO>>))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(RefreshPlexAccountAccessEndpointRequest req, CancellationToken ct)
    {
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
                _log.WarningLine("No enabled Plex accounts found to start the refresh PlexServer access job");
                await SendFluentResult(Result.Ok(), ct);
                return;
            }

            plexAccountIds.AddRange(enabledAccounts.Select(x => x.Id));
        }

        // Execute
        foreach (var plexAccountId in plexAccountIds)
        {
            var serverAccessResult = await _mediator.Send(new RefreshPlexServerAccessCommand(plexAccountId), ct);

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
                await _dbContext
                    .PlexAccountLibraries.Where(x =>
                        x.PlexAccountId == plexAccountId && lostServerAccess.Contains(x.PlexServerId)
                    )
                    .ExecuteDeleteAsync(cancellationToken: ct);
            }
            else
            {
                // Update library access
                var libraryAccessResult = await _mediator.Send(
                    new RefreshLibraryAccessCommand(plexAccountId),
                    CancellationToken.None
                );

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
        await _signalRService.SendRefreshNotificationAsync(
            [DataType.PlexAccount, DataType.PlexServer, DataType.PlexServerConnection],
            CancellationToken.None
        );

        await SendFluentResult(Result.Ok(_list), ct);
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
                .Access.Select(x => new PlexServerAccessRapportDTO()
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
