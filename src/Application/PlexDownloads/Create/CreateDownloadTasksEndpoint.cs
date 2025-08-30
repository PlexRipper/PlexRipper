using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Reaparr.Application.Contracts;
using ILog = Reaparr.Logging.ILog;

namespace Reaparr.Application;

public record CreateDownloadTasksEndpointRequest
{
    [FromBody]
    public required CreateDownloadTasksRequest Request { get; set; }
}

public class CreateDownloadTasksEndpointRequestValidator : Validator<CreateDownloadTasksEndpointRequest>
{
    public CreateDownloadTasksEndpointRequestValidator()
    {
        RuleFor(x => x.Request).NotNull();
    }
}

public class CreateDownloadTasksEndpoint : BaseEndpoint<CreateDownloadTasksEndpointRequest>
{
    private readonly ILog _log;
    private readonly ICommandExecutor _commandExecutor;

    public override string EndpointPath => ApiRoutes.DownloadController + "/create";

    public CreateDownloadTasksEndpoint(ILog log, ICommandExecutor commandExecutor)
    {
        _log = log;
        _commandExecutor = commandExecutor;
    }

    public override void Configure()
    {
        Post(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(CreateDownloadTasksEndpointRequest req, CancellationToken ct)
    {
        _log.DebugLine("Attempting to add download task orders: ");
        foreach (var downloadMediaDto in req.Request.DownloadMedias)
            _log.Debug("DownloadMediaDTO: {@DownloadMediaDto} ", downloadMediaDto);

        var result = await _commandExecutor.Send(new CreateDownloadTasksCommand(req.Request), ct);

        await SendFluentResult(result, ct);
    }
}
