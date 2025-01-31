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

    public override string EndpointPath => ApiRoutes.PlexAccountController + "/refresh/{PlexAccountId}";

    public RefreshPlexAccountAccessEndpoint(ILog log, IPlexRipperDbContext dbContext, IMediator mediator)
    {
        _log = log;
        _dbContext = dbContext;
        _mediator = mediator;
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
        var list = new List<RefreshPlexAccountAccessRapportDTO>();
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
            var serverAccessRapportResult = await _mediator.Send(new RefreshPlexServerAccessCommand(plexAccountId), ct);
            var libraryAccessRapportResult = await _mediator.Send(new RefreshLibraryAccessCommand(plexAccountId), ct);

            var serverAccessRapport = serverAccessRapportResult.Value;
            var libraryAccessRapport = libraryAccessRapportResult.Value;

            list.Add(
                new RefreshPlexAccountAccessRapportDTO
                {
                    PlexAccountId = plexAccountId,
                    Access = serverAccessRapport
                        .Data.Select(x => new PlexServerAccessRapportDTO
                        {
                            IsServerOffline = libraryAccessRapport.OfflineServers.Contains(x.PlexServerId),
                            PlexServerId = x.PlexServerId,
                            State = x.State,
                            LibraryAccess =
                                libraryAccessRapport
                                    .Reports.Find(y => y.PlexServerId == x.PlexServerId)
                                    ?.Data.Select(y => new PlexLibraryAccessRapportDTO
                                    {
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

        await SendFluentResult(Result.Ok(list), ct);
    }
}
