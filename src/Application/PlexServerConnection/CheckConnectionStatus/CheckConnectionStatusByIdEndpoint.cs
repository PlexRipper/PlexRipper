using FastEndpoints;
using FluentValidation;
using Reaparr.Application.Contracts;

namespace Reaparr.Application;

public record CheckConnectionStatusByIdRequest(int PlexServerConnectionId);

public class CheckConnectionStatusByIdRequestValidator : Validator<CheckConnectionStatusByIdRequest>
{
    public CheckConnectionStatusByIdRequestValidator()
    {
        RuleFor(x => x.PlexServerConnectionId).GreaterThan(0);
    }
}

public class CheckConnectionStatusByIdEndpoint : BaseEndpoint<CheckConnectionStatusByIdRequest, PlexServerStatusDTO>
{
    private readonly ILogger _log;
    private readonly ICommandExecutor _commandExecutor;

    public override string EndpointPath => ApiRoutes.PlexServerConnectionController + "/check/{PlexServerConnectionId}";

    public CheckConnectionStatusByIdEndpoint(ILogger log, ICommandExecutor commandExecutor)
    {
        _log = log.ForContext<CheckConnectionStatusByIdEndpoint>();
        _commandExecutor = commandExecutor;
    }

    public override void Configure()
    {
        Get(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<PlexServerStatusDTO>))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status404NotFound, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(CheckConnectionStatusByIdRequest req, CancellationToken ct)
    {
        _log.DebugApiCall(HttpContext, req);
        var result = await _commandExecutor.Send(new CheckConnectionStatusByIdCommand(req.PlexServerConnectionId), ct);
        if (result.IsFailed)
            await SendFluentResult(result.ToResult(), ct);
        else
            await SendFluentResult(result, x => x.ToDTO(), ct);
    }
}
