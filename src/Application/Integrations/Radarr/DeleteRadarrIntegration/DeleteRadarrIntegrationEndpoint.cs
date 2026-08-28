using System.Net;

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
        Roles(DefaultUserAppCredentials.DefaultAdminRole);
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

        var clientResult = await _radarrHttpClientFactory.CreateAsync(integration.Id, ct);
        if (clientResult.IsFailed && !req.Force)
        {
            await Send.FluentResult(clientResult.ToResult(), ct);
            return;
        }

        if (clientResult.IsSuccess)
        {
            using var client = clientResult.Value;
            var cleanupResult = await DeleteRemoteResources(client, integration, ct);
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

    private static async Task<Result> DeleteRemoteResources(
        HttpClient client,
        RadarrIntegration integration,
        CancellationToken ct
    )
    {
        foreach (var resource in new[]
                 {
                     (Name: "indexer", Id: integration.ExternalIndexerId),
                     (Name: "downloadclient", Id: integration.ExternalDownloadClientId),
                 })
        {
            if (resource.Id is null)
                continue;

            using var response = await client.DeleteAsync($"api/v3/{resource.Name}/{resource.Id.Value}", ct);
            if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NotFound)
                return Result.Fail(
                    $"Failed to delete Radarr {resource.Name} {resource.Id.Value}: {response.StatusCode}."
                );
        }

        return Result.Ok();
    }
}
