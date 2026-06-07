namespace Reaparr.Application;

public record GetPlexServerConnectionByIdEndpointRequest(int PlexServerConnectionId);

public class GetPlexServerConnectionByIdEndpointRequestValidator : Validator<GetPlexServerConnectionByIdEndpointRequest>
{
    public GetPlexServerConnectionByIdEndpointRequestValidator()
    {
        RuleFor(x => x.PlexServerConnectionId).GreaterThan(0);
    }
}

public class GetPlexServerConnectionByIdEndpoint
    : Endpoint<GetPlexServerConnectionByIdEndpointRequest, PlexServerConnectionDTO>
{
    private readonly IReaparrDbContext _dbContext;

    public GetPlexServerConnectionByIdEndpoint(IReaparrDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(ApiRoutes.PlexServerConnectionController + "/{PlexServerConnectionId}");

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<PlexServerConnectionDTO>))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status404NotFound, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(GetPlexServerConnectionByIdEndpointRequest req, CancellationToken ct)
    {
        var plexServerConnection = await _dbContext
            .PlexServerConnections.Include(x => x.LatestConnectionStatus)
            .FirstOrDefaultAsync(x => x.Id == req.PlexServerConnectionId, ct);

        if (plexServerConnection is null)
        {
            await Send.FluentResult(
                ResultExtensions.EntityNotFound(nameof(PlexServerConnection), req.PlexServerConnectionId),
                ct
            );
        }
        else
            await Send.FluentResult(Result.Ok(plexServerConnection), x => x.ToDTO(), ct);
    }
}
