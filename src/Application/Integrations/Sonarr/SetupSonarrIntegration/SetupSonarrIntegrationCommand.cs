namespace Reaparr.Application;

public record SetupSonarrIntegrationCommand(Guid IntegrationId) : ICommand<Result<SonarrIntegration>>;

public class SetupSonarrIntegrationCommandValidator : AbstractValidator<SetupSonarrIntegrationCommand>
{
    public SetupSonarrIntegrationCommandValidator()
    {
        RuleFor(x => x).NotNull();
        RuleFor(x => x.IntegrationId).NotEmpty();
    }
}

public class SetupSonarrIntegrationCommandHandler
    : ICommandHandler<SetupSonarrIntegrationCommand, Result<SonarrIntegration>>
{
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IProgressHubService _progressHubService;

    public SetupSonarrIntegrationCommandHandler(
        IReaparrDbContext dbContext,
        ICommandExecutor commandExecutor,
        IProgressHubService progressHubService
    )
    {
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
        _progressHubService = progressHubService;
    }

    public async Task<Result<SonarrIntegration>> ExecuteAsync(
        SetupSonarrIntegrationCommand command,
        CancellationToken ct
    )
    {
        var integration = await _dbContext
            .SonarrIntegrations.AsTracking()
            .SingleOrDefaultAsync(x => x.Id == command.IntegrationId, ct);
        if (integration is null)
            return ResultExtensions.EntityNotFound(nameof(SonarrIntegration), command.IntegrationId);

        var downloadClientResult = await _commandExecutor.Send(
            new SetupSonarrDownloadClientCommand { IntegrationId = integration.Id },
            ct
        );
        if (downloadClientResult.IsCancelled)
        {
            var error = string.Join("; ", downloadClientResult.Errors.Select(x => x.Message));
            await SendProgress(integration.Id, IntegrationSetupProgressStage.DownloadClient, false, error: error);
            return downloadClientResult.ToResult<SonarrIntegration>().LogWarning();
        }

        if (downloadClientResult.IsFailed)
        {
            var error = string.Join("; ", downloadClientResult.Errors.Select(x => x.Message));
            await SendProgress(integration.Id, IntegrationSetupProgressStage.DownloadClient, false, error: error);
            return downloadClientResult.ToResult<SonarrIntegration>().LogError();
        }

        await SendProgress(integration.Id, IntegrationSetupProgressStage.DownloadClient, true);
        integration.ExternalDownloadClientId = downloadClientResult.Value.DownloadClientId;
        await _dbContext.SaveChangesAsync(ct);

        await SendProgress(integration.Id, IntegrationSetupProgressStage.Indexer, false, true);
        var indexerResult = await _commandExecutor.Send(
            new SetupSonarrIndexerCommand
            {
                IntegrationId = integration.Id,
                DownloadClientId = downloadClientResult.Value.DownloadClientId,
            },
            ct
        );
        if (indexerResult.IsCancelled)
        {
            await SendProgress(
                integration.Id,
                IntegrationSetupProgressStage.Indexer,
                false,
                error: string.Join("; ", indexerResult.Errors.Select(x => x.Message))
            );
            return indexerResult.ToResult<SonarrIntegration>().LogWarning();
        }

        if (indexerResult.IsFailed)
        {
            await SendProgress(
                integration.Id,
                IntegrationSetupProgressStage.Indexer,
                false,
                error: string.Join("; ", indexerResult.Errors.Select(x => x.Message))
            );
            return indexerResult.ToResult<SonarrIntegration>().LogError();
        }

        await SendProgress(integration.Id, IntegrationSetupProgressStage.Indexer, true);
        integration.ExternalIndexerId = indexerResult.Value.IndexerId;
        await _dbContext.SaveChangesAsync(ct);

        await SendProgress(integration.Id, IntegrationSetupProgressStage.Validation, false, true);
        var validationResult = await _commandExecutor.Send(
            new ValidateSonarrIntegrationCommand(
                integration.Id,
                downloadClientResult.Value.Resource,
                indexerResult.Value.Resource
            ),
            ct
        );
        if (validationResult.IsCancelled)
        {
            await SendProgress(
                integration.Id,
                IntegrationSetupProgressStage.Validation,
                false,
                error: string.Join("; ", validationResult.Errors.Select(x => x.Message))
            );
            return validationResult.ToResult<SonarrIntegration>().LogWarning();
        }

        if (validationResult.IsFailed)
        {
            await SendProgress(
                integration.Id,
                IntegrationSetupProgressStage.Validation,
                false,
                error: string.Join("; ", validationResult.Errors.Select(x => x.Message))
            );
            return validationResult.ToResult<SonarrIntegration>().LogError();
        }

        await SendProgress(integration.Id, IntegrationSetupProgressStage.Validation, true);
        integration.ProvisioningState = IntegrationProvisioningState.Configured;
        await _dbContext.SaveChangesAsync(ct);
        await SendProgress(integration.Id, IntegrationSetupProgressStage.Done, true);
        return Result.Ok(integration);
    }

    private Task SendProgress(
        Guid integrationId,
        IntegrationSetupProgressStage stage,
        bool isSuccess,
        bool isRunning = false,
        string? error = null
    ) =>
        _progressHubService.SendIntegrationSetupProgressAsync(
            new IntegrationSetupProgressDTO
            {
                IntegrationId = integrationId,
                Stage = stage,
                IsRunning = isRunning,
                IsSuccess = isSuccess,
                Error = error,
            }
        );
}
