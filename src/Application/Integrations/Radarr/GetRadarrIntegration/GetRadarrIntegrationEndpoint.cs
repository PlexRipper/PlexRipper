namespace Reaparr.Application;

public record GetRadarrIntegrationRequest
{
    [RouteParam]
    public Guid IntegrationId { get; init; }
}

public class GetRadarrIntegrationEndpoint : Endpoint<GetRadarrIntegrationRequest, ResultDTO<RadarrIntegrationDTO>>
{
    private readonly IReaparrDbContext _dbContext;

    public GetRadarrIntegrationEndpoint(IReaparrDbContext dbContext) => _dbContext = dbContext;

    public override void Configure()
    {
        Get(ApiRoutes.IntegrationController + "/Radarr/{integrationId:guid}");
        Roles(DefaultUserAppCredentials.DefaultAdminRole);
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<RadarrIntegrationDTO>))
                .Produces(StatusCodes.Status404NotFound, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(GetRadarrIntegrationRequest req, CancellationToken ct)
    {
        var integration = await _dbContext
            .RadarrIntegrations.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == req.IntegrationId, ct);
        if (integration is null)
        {
            await Send.FluentResult(ResultExtensions.EntityNotFound(nameof(RadarrIntegration), req.IntegrationId), ct);
            return;
        }

        HttpContext.Response.Headers.CacheControl = "no-store";
        await Send.FluentResult(Result.Ok(integration), model => model.ToDTO(), ct);
    }
}
