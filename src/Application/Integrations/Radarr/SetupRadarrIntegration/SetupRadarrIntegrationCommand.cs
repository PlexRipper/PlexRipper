namespace Reaparr.Application;

public record SetupRadarrIntegrationCommand(Guid IntegrationId) : ICommand<Result<RadarrIntegration>>;

public class SetupRadarrIntegrationCommandValidator : AbstractValidator<SetupRadarrIntegrationCommand>
{
    public SetupRadarrIntegrationCommandValidator()
    {
        RuleFor(x => x).NotNull();
        RuleFor(x => x.IntegrationId).NotEmpty();
    }
}

public class SetupRadarrIntegrationCommandHandler
    : ICommandHandler<SetupRadarrIntegrationCommand, Result<RadarrIntegration>>
{
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IProgressHubService _progressHubService;

    public SetupRadarrIntegrationCommandHandler(
        IReaparrDbContext dbContext,
        ICommandExecutor commandExecutor,
        IProgressHubService progressHubService
    )
    {
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
        _progressHubService = progressHubService;
    }

    public async Task<Result<RadarrIntegration>> ExecuteAsync(
        SetupRadarrIntegrationCommand command,
        CancellationToken ct
    )
    {
        var integration = await _dbContext
            .RadarrIntegrations.AsTracking()
            .SingleOrDefaultAsync(x => x.Id == command.IntegrationId, ct);
        if (integration is null)
            return ResultExtensions.EntityNotFound(nameof(RadarrIntegration), command.IntegrationId);

        var downloadClientResult = await _commandExecutor.Send(
            new SetupRadarrDownloadClientCommand { IntegrationId = integration.Id },
            ct
        );
        if (downloadClientResult.IsCancelled)
        {
            var error = string.Join("; ", downloadClientResult.Errors.Select(x => x.Message));
            await SendProgress(integration.Id, IntegrationSetupProgressStage.Connecting, false, error: error);
            await SendProgress(integration.Id, IntegrationSetupProgressStage.DownloadClient, false, error: error);
            return downloadClientResult.ToResult<RadarrIntegration>().LogWarning();
        }

        if (downloadClientResult.IsFailed)
        {
            var error = string.Join("; ", downloadClientResult.Errors.Select(x => x.Message));
            await SendProgress(integration.Id, IntegrationSetupProgressStage.Connecting, false, error: error);
            await SendProgress(integration.Id, IntegrationSetupProgressStage.DownloadClient, false, error: error);
            return downloadClientResult.ToResult<RadarrIntegration>().LogError();
        }

        await SendProgress(integration.Id, IntegrationSetupProgressStage.DownloadClient, true);
        integration.ExternalDownloadClientId = downloadClientResult.Value.DownloadClientId;
        await _dbContext.SaveChangesAsync(ct);

        await SendProgress(integration.Id, IntegrationSetupProgressStage.Indexer, false, true);
        var indexerResult = await _commandExecutor.Send(
            new SetupRadarrIndexerCommand
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
            return indexerResult.ToResult<RadarrIntegration>().LogWarning();
        }

        if (indexerResult.IsFailed)
        {
            await SendProgress(
                integration.Id,
                IntegrationSetupProgressStage.Indexer,
                false,
                error: string.Join("; ", indexerResult.Errors.Select(x => x.Message))
            );
            return indexerResult.ToResult<RadarrIntegration>().LogError();
        }

        await SendProgress(integration.Id, IntegrationSetupProgressStage.Indexer, true);
        integration.ExternalIndexerId = indexerResult.Value.IndexerId;
        await _dbContext.SaveChangesAsync(ct);

        await SendProgress(integration.Id, IntegrationSetupProgressStage.Validation, false, true);
        var validationResult = await _commandExecutor.Send(
            new ValidateRadarrIntegrationCommand(
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
            return validationResult.ToResult<RadarrIntegration>().LogWarning();
        }

        if (validationResult.IsFailed)
        {
            await SendProgress(
                integration.Id,
                IntegrationSetupProgressStage.Validation,
                false,
                error: string.Join("; ", validationResult.Errors.Select(x => x.Message))
            );
            return validationResult.ToResult<RadarrIntegration>().LogError();
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
