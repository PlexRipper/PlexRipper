using Application.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace PlexRipper.Application;

public record RestartDownloadTaskEndpointRequest(Guid DownloadTaskGuid);

public class RestartDownloadTaskEndpointRequestValidator : Validator<RestartDownloadTaskEndpointRequest>
{
    public RestartDownloadTaskEndpointRequestValidator()
    {
        RuleFor(x => x.DownloadTaskGuid).NotEmpty();
    }
}

public class RestartDownloadTaskEndpoint(ICommandExecutor commandExecutor)
    : BaseEndpoint<RestartDownloadTaskEndpointRequest>
{
    public override string EndpointPath => ApiRoutes.DownloadController + "/restart/{DownloadTaskGuid}";

    public override void Configure()
    {
        // TODO state is changed - use POST / PUT
        Get(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(RestartDownloadTaskEndpointRequest req, CancellationToken ct)
    {
        var restartResult = await commandExecutor.Send(new RestartDownloadTaskCommand(req.DownloadTaskGuid), ct);

        await SendFluentResult(restartResult, ct);
    }
}
