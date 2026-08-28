using System.Net;

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
        Roles(DefaultUserAppCredentials.DefaultAdminRole);
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

        var clientResult = await _sonarrHttpClientFactory.CreateAsync(integration.Id, ct);
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

        _dbContext.SonarrIntegrations.Remove(integration);
        await _dbContext.SaveChangesAsync(ct);
        await Send.NoContentAsync(ct);
    }

    private static async Task<Result> DeleteRemoteResources(
        HttpClient client,
        SonarrIntegration integration,
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
                    $"Failed to delete Sonarr {resource.Name} {resource.Id.Value}: {response.StatusCode}."
                );
        }

        return Result.Ok();
    }
}
