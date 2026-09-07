namespace Reaparr.Application;

public record SetupSonarrIntegrationRequest
{
    [RouteParam]
    public Guid IntegrationId { get; init; }
}

public class SetupSonarrIntegrationEndpoint : Endpoint<SetupSonarrIntegrationRequest, ResultDTO<SonarrIntegrationDTO>>
{
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IProgressHubService _progressHubService;

    public SetupSonarrIntegrationEndpoint(
        IReaparrDbContext dbContext,
        ICommandExecutor commandExecutor,
        IProgressHubService progressHubService
    )
    {
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
        _progressHubService = progressHubService;
    }

    public override void Configure()
    {
        Post(ApiRoutes.IntegrationController + "/Sonarr/{integrationId:guid}/Setup");
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<SonarrIntegrationDTO>))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status404NotFound, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(SetupSonarrIntegrationRequest req, CancellationToken ct)
    {
        var integration = await _dbContext
            .SonarrIntegrations.AsTracking()
            .SingleOrDefaultAsync(x => x.Id == req.IntegrationId, ct);
        if (integration is null)
        {
            await Send.FluentResult(ResultExtensions.EntityNotFound(nameof(SonarrIntegration), req.IntegrationId), ct);
            return;
        }

        var downloadClientResult = await _commandExecutor.Send(
            new SetupSonarrDownloadClientCommand { IntegrationId = integration.Id },
            ct
        );
        if (downloadClientResult.IsFailed)
        {
            var error = string.Join("; ", downloadClientResult.Errors.Select(x => x.Message));
            await _progressHubService.SendIntegrationSetupProgressAsync(
                new IntegrationSetupProgressDTO
                {
                    IntegrationId = integration.Id,
                    Stage = IntegrationSetupProgressStage.Connecting,
                    IsRunning = false,
                    IsSuccess = false,
                    Error = error,
                }
            );
            await _progressHubService.SendIntegrationSetupProgressAsync(
                new IntegrationSetupProgressDTO
                {
                    IntegrationId = integration.Id,
                    Stage = IntegrationSetupProgressStage.DownloadClient,
                    IsRunning = false,
                    IsSuccess = false,
                    Error = error,
                }
            );
            await Send.FluentResult(downloadClientResult.ToResult(), ct);
            return;
        }

        await _progressHubService.SendIntegrationSetupProgressAsync(
            new IntegrationSetupProgressDTO
            {
                IntegrationId = integration.Id,
                Stage = IntegrationSetupProgressStage.DownloadClient,
                IsRunning = false,
                IsSuccess = true,
            }
        );

        integration.ExternalDownloadClientId = downloadClientResult.Value.DownloadClientId;
        await _dbContext.SaveChangesAsync(ct);

        await _progressHubService.SendIntegrationSetupProgressAsync(
            new IntegrationSetupProgressDTO
            {
                IntegrationId = integration.Id,
                Stage = IntegrationSetupProgressStage.Indexer,
                IsRunning = true,
                IsSuccess = false,
            }
        );
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
            await _progressHubService.SendIntegrationSetupProgressAsync(
                new IntegrationSetupProgressDTO
                {
                    IntegrationId = integration.Id,
                    Stage = IntegrationSetupProgressStage.Indexer,
                    IsRunning = false,
                    IsSuccess = false,
                    Error = string.Join("; ", indexerResult.Errors.Select(x => x.Message)),
                }
            );
            await Send.FluentResult(indexerResult.ToResult(), ct);
            return;
        }

        await _progressHubService.SendIntegrationSetupProgressAsync(
            new IntegrationSetupProgressDTO
            {
                IntegrationId = integration.Id,
                Stage = IntegrationSetupProgressStage.Indexer,
                IsRunning = false,
                IsSuccess = true,
            }
        );

        integration.ExternalIndexerId = indexerResult.Value.IndexerId;
        integration.ProvisioningState = IntegrationProvisioningState.Configured;
        await _dbContext.SaveChangesAsync(ct);
        await _progressHubService.SendIntegrationSetupProgressAsync(
            new IntegrationSetupProgressDTO
            {
                IntegrationId = integration.Id,
                Stage = IntegrationSetupProgressStage.Done,
                IsRunning = false,
                IsSuccess = true,
            }
        );
        await Send.FluentResult(Result.Ok(integration), model => model.ToDTO(), ct);
    }
}
