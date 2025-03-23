using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Logging.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public record RefreshPlexAccountAccessEndpointRequest(int PlexAccountId);

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

            var serverAccessRapport = serverAccessResult.Value;
            var libraryAccessRapport = libraryAccessResult.Value;

            _list.Add(
                new RefreshPlexAccountAccessRapportDTO(
                    serverAccessRapport.PlexAccountId,
                    serverAccessRapport.PlexAccountName
                )
                {
                    Access = serverAccessResult
                        .Value.Access.Select(x => new PlexServerAccessRapportDTO()
                        {
                            State = x.State,
                            PlexServerId = x.PlexServerId,
                            PlexServerName = x.PlexServerName,
                            IsServerOffline = libraryAccessRapport.OfflineServers.Contains(x.PlexServerId),
                            LibraryAccess =
                                libraryAccessRapport
                                    .Reports.Find(y => y.PlexServerId == x.PlexServerId)
                                    ?.Data.Select(y => new PlexLibraryAccessRapportDTO
                                    {
                                        PlexLibraryName = y.PlexLibraryName,
                                        PlexServerId = y.PlexServerId,
                                        State = y.State,
                                        PlexLibraryId = y.PlexLibraryId,
                                    })
                                    .ToList() ?? [],
                        })
                        .ToList(),
                }
            );
        }

        // Send notifications to the client to refresh the PlexServerConnection data
        await _signalRService.SendRefreshNotificationAsync(
            [DataType.PlexAccount, DataType.PlexServer, DataType.PlexServerConnection],
            CancellationToken.None
        );

        await SendFluentResult(Result.Ok(_list), ct);
    }
}
