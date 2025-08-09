using Application.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace PlexRipper.Application;

public record StopDownloadTaskEndpointRequest(Guid DownloadTaskGuid);

public class StopDownloadTaskEndpointRequestValidator : Validator<StopDownloadTaskEndpointRequest>
{
    public StopDownloadTaskEndpointRequestValidator()
    {
        RuleFor(x => x.DownloadTaskGuid).NotEmpty();
    }
}

public class StopDownloadTaskEndpoint : BaseEndpoint<StopDownloadTaskEndpointRequest>
{
    private readonly ICommandExecutor _commandExecutor;

    public override string EndpointPath => ApiRoutes.DownloadController + "/stop/{DownloadTaskGuid}";

    public StopDownloadTaskEndpoint(ICommandExecutor commandExecutor)
    {
        _commandExecutor = commandExecutor;
    }

    public override void Configure()
    {
        Get(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(StopDownloadTaskEndpointRequest req, CancellationToken ct)
    {
        var stopResult = await _commandExecutor.Send(new StopDownloadTaskCommand(req.DownloadTaskGuid), ct);

        await SendFluentResult(stopResult, ct);
    }
}
