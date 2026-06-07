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

public class DeleteDownloadTaskEndpoint : Endpoint<DeleteDownloadTaskEndpointRequest, BaseResultDTO>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;

    public DeleteDownloadTaskEndpoint(ILogger log, IReaparrDbContext dbContext, ICommandExecutor commandExecutor)
    {
        _log = log.ForContext<DeleteDownloadTaskEndpoint>();
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
    }

    public override void Configure()
    {
        Delete(ApiRoutes.DownloadController + "/delete");

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
            await Send.FluentResult(Result.Ok(), ct);
            return;
        }

        foreach (var key in keys)
        {
            var stopResult = await _commandExecutor.Send(new StopDownloadTaskCommand(key.Id), ct);
            if (stopResult.IsFailed)
            {
                await Send.FluentResult(stopResult, ct);
                return;
            }
        }

        var deleteResult = await _commandExecutor.Send(new DeleteDownloadTasksByKeyCommand(keys), ct);

        await Send.FluentResult(deleteResult, ct);
    }
}
