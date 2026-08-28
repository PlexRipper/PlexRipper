namespace Reaparr.Application;

public record SetupSonarrIntegrationRequest
{
    [RouteParam]
    public Guid IntegrationId { get; init; }
}

public class SetupSonarrIntegrationEndpoint : Endpoint<SetupSonarrIntegrationRequest, SonarrIntegrationDetail>
{
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;

    public SetupSonarrIntegrationEndpoint(IReaparrDbContext dbContext, ICommandExecutor commandExecutor)
    {
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
    }

    public override void Configure()
    {
        Post(ApiRoutes.IntegrationController + "/Sonarr/{integrationId:guid}/Setup");
        Roles(DefaultUserAppCredentials.DefaultAdminRole);
    }

    public override async Task HandleAsync(SetupSonarrIntegrationRequest req, CancellationToken ct)
    {
        var integration = await _dbContext
            .SonarrIntegrations.AsTracking()
            .SingleOrDefaultAsync(x => x.Id == req.IntegrationId, ct);
        if (integration is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var downloadClientResult = await _commandExecutor.Send(
            new SetupSonarrDownloadClientCommand { IntegrationId = integration.Id },
            ct
        );
        if (downloadClientResult.IsFailed)
        {
            await Send.FluentResult(downloadClientResult.ToResult(), ct);
            return;
        }

        integration.ExternalDownloadClientId = downloadClientResult.Value.DownloadClientId;
        await _dbContext.SaveChangesAsync(ct);

        var indexerResult = await _commandExecutor.Send(
            new SetupSonarrIndexerCommand
            {
                IntegrationId = integration.Id,
                DownloadClientId = downloadClientResult.Value.DownloadClientId,
            },
            ct
        );
        if (indexerResult.IsFailed)
        {
            await Send.FluentResult(indexerResult.ToResult(), ct);
            return;
        }

        integration.ExternalIndexerId = indexerResult.Value.IndexerId;
        integration.ProvisioningState = IntegrationProvisioningState.Configured;
        await _dbContext.SaveChangesAsync(ct);
        await Send.OkAsync(ConfigureSonarrIntegrationEndpoint.ToDetail(integration), ct);
    }
}
