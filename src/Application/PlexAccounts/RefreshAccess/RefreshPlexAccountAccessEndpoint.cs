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
        RuleFor(x => x.PlexAccountId).GreaterThan(0);
    }
}

public class RefreshPlexAccountAccessEndpoint : BaseCustomEndpoint<RefreshPlexAccountAccessEndpointRequest, ResultDTO>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IMediator _mediator;

    public override string EndpointPath => ApiRoutes.PlexAccountController + $"/refresh/{nameof(RefreshPlexAccountAccessEndpointRequest.PlexAccountId)}";

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
        Description(x => x
            .Produces(StatusCodes.Status200OK, typeof(ResultDTO))
            .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO)));
    }

    public override async Task HandleAsync(RefreshPlexAccountAccessEndpointRequest req, CancellationToken ct)
    {
        var plexAccountIds = new List<int>();

        var enabledAccounts = await _dbContext.PlexAccounts
            .Where(x => x.IsEnabled)
            .ToListAsync(ct);

        if (!enabledAccounts.Any())
        {
            _log.WarningLine("No enabled Plex accounts found to start the refresh PlexServer access job");
            await SendFluentResult(Result.Ok(), ct);
            return;
        }

        plexAccountIds.AddRange(enabledAccounts.Select(x => x.Id));

        foreach (var id in plexAccountIds)
            await _mediator.Send(new QueueRefreshPlexServerAccessJobCommand(id), ct);

        await SendFluentResult(Result.Ok(), ct);
    }
}