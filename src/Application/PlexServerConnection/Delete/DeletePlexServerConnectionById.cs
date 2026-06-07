namespace Reaparr.Application;

public record DeletePlexServerConnectionByIdRequest(int PlexServerConnectionId);

public class DeletePlexServerConnectionByIdRequestValidator : Validator<DeletePlexServerConnectionByIdRequest>
{
    public DeletePlexServerConnectionByIdRequestValidator()
    {
        RuleFor(x => x.PlexServerConnectionId).GreaterThan(0);
    }
}

public class DeletePlexServerConnectionById : Endpoint<DeletePlexServerConnectionByIdRequest>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;

    public DeletePlexServerConnectionById(ILogger log, IReaparrDbContext dbContext)
    {
        _log = log.ForContext<DeletePlexServerConnectionById>();
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Delete(ApiRoutes.PlexServerConnectionController + "/{PlexServerConnectionId}");

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status404NotFound, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(DeletePlexServerConnectionByIdRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, req);
        var deleteCount = await _dbContext
            .PlexServerConnections.Where(x => x.Id == req.PlexServerConnectionId)
            .ExecuteDeleteAsync(ct);

        if (deleteCount == 0)
        {
            await Send.FluentResult(
                ResultExtensions.EntityNotFound(nameof(PlexServerConnection), req.PlexServerConnectionId),
                ct
            );
            return;
        }

        await Send.FluentResult(Result.Ok(), ct);
    }
}
