namespace Reaparr.Application;

public record SetupRadarrIntegrationRequest
{
    [RouteParam]
    public Guid IntegrationId { get; init; }
}

public class SetupRadarrIntegrationEndpoint : Endpoint<SetupRadarrIntegrationRequest, ResultDTO<RadarrIntegrationDTO>>
{
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;

    public SetupRadarrIntegrationEndpoint(IReaparrDbContext dbContext, ICommandExecutor commandExecutor)
    {
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
    }

    public override void Configure()
    {
        Post(ApiRoutes.IntegrationController + "/Radarr/{integrationId:guid}/Setup");
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<RadarrIntegrationDTO>))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status404NotFound, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(SetupRadarrIntegrationRequest req, CancellationToken ct)
    {
        var integration = await _dbContext
            .RadarrIntegrations.AsTracking()
            .SingleOrDefaultAsync(x => x.Id == req.IntegrationId, ct);
        if (integration is null)
        {
            await Send.FluentResult(ResultExtensions.EntityNotFound(nameof(RadarrIntegration), req.IntegrationId), ct);
            return;
        }

        var downloadClientResult = await _commandExecutor.Send(
            new SetupRadarrDownloadClientCommand { IntegrationId = integration.Id },
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
            new SetupRadarrIndexerCommand
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
        await Send.FluentResult(Result.Ok(integration), model => model.ToDTO(), ct);
    }
}
