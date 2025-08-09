using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace PlexRipper.Application;

public record RefreshPlexServerConnectionsEndpointRequest(int PlexServerId);

public class RefreshPlexServerConnectionsEndpointRequestValidator
    : Validator<RefreshPlexServerConnectionsEndpointRequest>
{
    public RefreshPlexServerConnectionsEndpointRequestValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
    }
}

public class RefreshPlexServerConnectionsEndpoint : BaseEndpoint<RefreshPlexServerConnectionsEndpointRequest>
{
    private readonly IPlexRipperDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;
    public override string EndpointPath => ApiRoutes.PlexServerController + "/{PlexServerId}/refresh";

    public RefreshPlexServerConnectionsEndpoint(IPlexRipperDbContext dbContext, ICommandExecutor commandExecutor)
    {
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
    }

    public override void Configure()
    {
        Get(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<PlexServerDTO>))
                .Produces(StatusCodes.Status404NotFound, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(RefreshPlexServerConnectionsEndpointRequest req, CancellationToken ct)
    {
        // Pick an account that has access to the PlexServer to connect with
        var plexAccountResult = await _dbContext.ChoosePlexAccountToConnect(req.PlexServerId, ct);
        if (plexAccountResult.IsFailed)
        {
            await SendFluentResult(plexAccountResult.ToResult(), ct);
            return;
        }

        await _commandExecutor.Send(new RefreshPlexServerAccessCommand(plexAccountResult.Value.Id), ct);
    }
}
