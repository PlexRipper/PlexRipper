using FastEndpoints;
using FluentValidation;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.Application;

public record DeleteDownloadTaskEndpointRequest
{
    [FromBody]
    public required List<Guid> DownloadTaskIds { get; init; }
}

public class DeleteDownloadTaskEndpointRequestValidator : Validator<DeleteDownloadTaskEndpointRequest>
{
    public DeleteDownloadTaskEndpointRequestValidator()
    {
        RuleFor(x => x.DownloadTaskIds).NotEmpty();
    }
}

public class DeleteDownloadTaskEndpoint : BaseEndpoint<DeleteDownloadTaskEndpointRequest>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;

    public override string EndpointPath => ApiRoutes.DownloadController + "/delete";

    public DeleteDownloadTaskEndpoint(ILogger log, IReaparrDbContext dbContext, ICommandExecutor commandExecutor)
    {
        _log = log.ForContext<DeleteDownloadTaskEndpoint>();
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
    }

    public override void Configure()
    {
        Delete(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(DeleteDownloadTaskEndpointRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, req);

        // Resolve keys upfront for stop handling and to avoid sending an empty delete command.
        var keys = await _dbContext.GetDownloadTaskKeysAsync(req.DownloadTaskIds, ct);
        if (keys.Count == 0)
        {
            await SendFluentResult(Result.Ok(), ct);
            return;
        }

        foreach (var key in keys)
        {
            var stopResult = await _commandExecutor.Send(new StopDownloadTaskCommand(key.Id), ct);
            if (stopResult.IsFailed)
            {
                await SendFluentResult(stopResult, ct);
                return;
            }
        }

        var deleteResult = await _commandExecutor.Send(new DeleteDownloadTasksByKeyCommand(keys), ct);

        await SendFluentResult(deleteResult, ct);
    }
}
