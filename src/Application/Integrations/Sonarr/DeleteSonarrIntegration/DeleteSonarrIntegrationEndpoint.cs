namespace Reaparr.Application;

public record DeleteSonarrIntegrationRequest
{
    [RouteParam]
    public Guid IntegrationId { get; init; }

    [QueryParam]
    public bool Force { get; init; }
}

public class DeleteSonarrIntegrationEndpoint : Endpoint<DeleteSonarrIntegrationRequest>
{
    private readonly IReaparrDbContext _dbContext;
    private readonly ISonarrHttpClientFactory _sonarrHttpClientFactory;

    public DeleteSonarrIntegrationEndpoint(
        IReaparrDbContext dbContext,
        ISonarrHttpClientFactory sonarrHttpClientFactory
    )
    {
        _dbContext = dbContext;
        _sonarrHttpClientFactory = sonarrHttpClientFactory;
    }

    public override void Configure()
    {
        Delete(ApiRoutes.IntegrationController + "/Sonarr/{integrationId:guid}");
    }

    public override async Task HandleAsync(DeleteSonarrIntegrationRequest req, CancellationToken ct)
    {
        var integration = await _dbContext
            .SonarrIntegrations.AsTracking()
            .SingleOrDefaultAsync(x => x.Id == req.IntegrationId, ct);
        if (integration is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var clientResult = await _sonarrHttpClientFactory.CreateAsync(integration.Id);
        if (clientResult.IsFailed && !req.Force)
        {
            await Send.FluentResult(clientResult.ToResult(), ct);
            return;
        }

        if (clientResult.IsSuccess)
        {
            using var client = clientResult.Value;
            var cleanupResult = await client.DeleteSonarrResourcesAsync(
                integration.ExternalIndexerId,
                integration.ExternalDownloadClientId,
                ct
            );
            if (cleanupResult.IsFailed && !req.Force)
            {
                await Send.FluentResult(cleanupResult, ct);
                return;
            }
        }

        _dbContext.SonarrIntegrations.Remove(integration);
        await _dbContext.SaveChangesAsync(ct);
        await Send.NoContentAsync(ct);
    }
}
