using Application.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace PlexRipper.Application;

public record CheckAllConnectionsStatusByPlexServerRequest(int PlexServerId);

public class CheckAllConnectionsStatusByPlexServerRequestValidator
    : Validator<CheckAllConnectionsStatusByPlexServerRequest>
{
    public CheckAllConnectionsStatusByPlexServerRequestValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
    }
}

public class CheckAllConnectionsStatusByPlexServerEndpoint
    : BaseEndpoint<CheckAllConnectionsStatusByPlexServerRequest, List<PlexServerStatusDTO>>
{
    private readonly ICommandExecutor _commandExecutor;

    public override string EndpointPath => ApiRoutes.PlexServerConnectionController + "/check/by-server/{PlexServerId}";

    public CheckAllConnectionsStatusByPlexServerEndpoint(ICommandExecutor commandExecutor)
    {
        _commandExecutor = commandExecutor;
    }

    public override void Configure()
    {
        Get(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<List<PlexServerStatusDTO>>))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status404NotFound, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(CheckAllConnectionsStatusByPlexServerRequest req, CancellationToken ct)
    {
        var result = await _commandExecutor.Send(
            new CheckAllConnectionsStatusByPlexServerCommand(req.PlexServerId),
            ct
        );
        if (result.IsFailed)
            await SendFluentResult(result.ToResult(), ct);
        else
            await SendFluentResult(result, x => x.ToDTO(), ct);
    }
}
