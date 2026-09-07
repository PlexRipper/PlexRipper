namespace Reaparr.Application;

public record DeleteRadarrIntegrationRequest
{
    [RouteParam]
    public Guid IntegrationId { get; init; }

    [QueryParam]
    public bool Force { get; init; }
}

public class DeleteRadarrIntegrationEndpoint : Endpoint<DeleteRadarrIntegrationRequest>
{
    private readonly IReaparrDbContext _dbContext;
    private readonly IRadarrHttpClientFactory _radarrHttpClientFactory;

    public DeleteRadarrIntegrationEndpoint(
        IReaparrDbContext dbContext,
        IRadarrHttpClientFactory radarrHttpClientFactory
    )
    {
        _dbContext = dbContext;
        _radarrHttpClientFactory = radarrHttpClientFactory;
    }

    public override void Configure()
    {
        Delete(ApiRoutes.IntegrationController + "/Radarr/{integrationId:guid}");
    }

    public override async Task HandleAsync(DeleteRadarrIntegrationRequest req, CancellationToken ct)
    {
        var integration = await _dbContext
            .RadarrIntegrations.AsTracking()
            .SingleOrDefaultAsync(x => x.Id == req.IntegrationId, ct);
        if (integration is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var clientResult = await _radarrHttpClientFactory.CreateAsync(integration.Id);
        if (clientResult.IsFailed && !req.Force)
        {
            await Send.FluentResult(clientResult.ToResult(), ct);
            return;
        }

        if (clientResult.IsSuccess)
        {
            using var client = clientResult.Value;
            var cleanupResult = await client.DeleteRadarrResourcesAsync(
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

        _dbContext.RadarrIntegrations.Remove(integration);
        await _dbContext.SaveChangesAsync(ct);
        await Send.NoContentAsync(ct);
    }
}
