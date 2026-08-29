namespace Reaparr.Application;

public record GetSonarrIntegrationRequest
{
    [RouteParam]
    public Guid IntegrationId { get; init; }
}

public class GetSonarrIntegrationEndpoint : Endpoint<GetSonarrIntegrationRequest, ResultDTO<SonarrIntegrationDTO>>
{
    private readonly IReaparrDbContext _dbContext;

    public GetSonarrIntegrationEndpoint(IReaparrDbContext dbContext) => _dbContext = dbContext;

    public override void Configure()
    {
        Get(ApiRoutes.IntegrationController + "/Sonarr/{integrationId:guid}");
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<SonarrIntegrationDTO>))
                .Produces(StatusCodes.Status404NotFound, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(GetSonarrIntegrationRequest req, CancellationToken ct)
    {
        var integration = await _dbContext
            .SonarrIntegrations.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == req.IntegrationId, ct);
        if (integration is null)
        {
            await Send.FluentResult(ResultExtensions.EntityNotFound(nameof(SonarrIntegration), req.IntegrationId), ct);
            return;
        }

        HttpContext.Response.Headers.CacheControl = "no-store";
        await Send.FluentResult(Result.Ok(integration), model => model.ToDTO(), ct);
    }
}
