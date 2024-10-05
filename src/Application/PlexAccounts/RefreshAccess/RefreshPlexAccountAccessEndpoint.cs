using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using Logging.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public record RefreshPlexAccountAccessEndpointRequest(int PlexAccountId);

public class RefreshPlexAccountAccessEndpointRequestValidator : Validator<RefreshPlexAccountAccessEndpointRequest>
{
    public RefreshPlexAccountAccessEndpointRequestValidator() { }
}

public class RefreshPlexAccountAccessEndpoint : BaseEndpoint<RefreshPlexAccountAccessEndpointRequest, ResultDTO>
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
        AllowAnonymous();
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO))
        );
    }

    public override async Task HandleAsync(RefreshPlexAccountAccessEndpointRequest req, CancellationToken ct)
    {
        if (req.PlexAccountId > 0)
        {
            await _mediator.Send(new RefreshPlexServerAccessCommand(req.PlexAccountId), ct);
            await _mediator.Send(new RefreshLibraryAccessCommand(req.PlexAccountId), ct);
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

            var plexAccountIds = enabledAccounts.Select(x => x.Id).ToList();
            foreach (var plexAccountId in plexAccountIds)
            {
                await _mediator.Send(new RefreshPlexServerAccessCommand(plexAccountId), ct);
                await _mediator.Send(new RefreshLibraryAccessCommand(plexAccountId), ct);
            }
        }

        await SendFluentResult(Result.Ok(), ct);
    }
}
