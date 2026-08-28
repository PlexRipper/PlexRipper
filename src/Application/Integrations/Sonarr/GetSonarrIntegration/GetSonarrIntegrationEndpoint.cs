namespace Reaparr.Application;

public record GetSonarrIntegrationRequest
{
    [RouteParam]
    public Guid IntegrationId { get; init; }
}

public class GetSonarrIntegrationEndpoint : Endpoint<GetSonarrIntegrationRequest, SonarrIntegrationDetail>
{
    private readonly IReaparrDbContext _dbContext;

    public GetSonarrIntegrationEndpoint(IReaparrDbContext dbContext) => _dbContext = dbContext;

    public override void Configure()
    {
        Get(ApiRoutes.IntegrationController + "/Sonarr/{integrationId:guid}");
        Roles(DefaultUserAppCredentials.DefaultAdminRole);
    }

    public override async Task HandleAsync(GetSonarrIntegrationRequest req, CancellationToken ct)
    {
        var integration = await _dbContext
            .SonarrIntegrations.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == req.IntegrationId, ct);
        if (integration is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        HttpContext.Response.Headers.CacheControl = "no-store";
        await Send.OkAsync(ConfigureSonarrIntegrationEndpoint.ToDetail(integration), ct);
    }
}
