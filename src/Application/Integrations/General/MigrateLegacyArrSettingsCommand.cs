namespace Reaparr.Application;

public record MigrateLegacyArrSettingsCommand : ICommand<Result>;

public class MigrateLegacyArrSettingsCommandValidator : AbstractValidator<MigrateLegacyArrSettingsCommand>
{
    public MigrateLegacyArrSettingsCommandValidator() => RuleFor(x => x).NotNull();
}

public class MigrateLegacyArrSettingsCommandHandler : ICommandHandler<MigrateLegacyArrSettingsCommand, Result>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IIntegrationsSettings _integrationsSettings;
    private readonly IConfigManager _configManager;

    public MigrateLegacyArrSettingsCommandHandler(
        ILogger log,
        IReaparrDbContext dbContext,
        ICommandExecutor commandExecutor,
        IIntegrationsSettings integrationsSettings,
        IConfigManager configManager
    )
    {
        _log = log.ForContext<MigrateLegacyArrSettingsCommandHandler>();
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
        _integrationsSettings = integrationsSettings;
        _configManager = configManager;
    }

    public async Task<Result> ExecuteAsync(MigrateLegacyArrSettingsCommand command, CancellationToken ct)
    {
        var radarrSettings = _integrationsSettings.Radarr;
        var sonarrSettings = _integrationsSettings.Sonarr;
        var hasLegacyRadarr = !string.IsNullOrWhiteSpace(radarrSettings.RadarrApiKey);
        var hasLegacySonarr = !string.IsNullOrWhiteSpace(sonarrSettings.SonarrApiKey);
        if (!hasLegacyRadarr && !hasLegacySonarr)
            return Result.Ok();

        var transactionResult = await _dbContext.ExecuteTransactionAsync(
            async (dbContext, transactionToken) =>
            {
                var defaultDownloadFolder = await dbContext.GetDownloadFolder();
                var imported = new List<IntegrationIdentity>(2);

                if (hasLegacyRadarr)
                {
                    var url = radarrSettings.RadarrBaseUrl.Trim().TrimEnd('/');
                    if (url.IsValidHttpUrl())
                    {
                        var integration = new RadarrIntegration
                        {
                            Id = Guid.NewGuid(),
                            DisplayName = "Radarr",
                            BaseUrl = url,
                            RadarrApiKey = radarrSettings.RadarrApiKey.Trim(),
                            QBittorrentApiKey = IntegrationApiKeyGenerator.GenerateQBittorrentApiKey(),
                            TorznabApiKey = IntegrationApiKeyGenerator.GenerateTorznabApiKey(),
                            Category = "reaparr-radarr",
                            DownloadFolderId = defaultDownloadFolder.Id,
                            ProvisioningState = IntegrationProvisioningState.Unconfigured,
                        };
                        dbContext.RadarrIntegrations.Add(integration);
                        imported.Add(new IntegrationIdentity(IntegrationType.Radarr, integration.Id));
                    }
                    else
                    {
                        _log.Here()
                            .Warning(
                                "Skipped legacy {IntegrationType} integration: URL is invalid",
                                IntegrationType.Radarr
                            );
                    }
                }

                if (hasLegacySonarr)
                {
                    var url = sonarrSettings.SonarrBaseUrl.Trim().TrimEnd('/');
                    if (url.IsValidHttpUrl())
                    {
                        var integration = new SonarrIntegration
                        {
                            Id = Guid.NewGuid(),
                            DisplayName = "Sonarr",
                            BaseUrl = url,
                            SonarrApiKey = sonarrSettings.SonarrApiKey.Trim(),
                            QBittorrentApiKey = IntegrationApiKeyGenerator.GenerateQBittorrentApiKey(),
                            TorznabApiKey = IntegrationApiKeyGenerator.GenerateTorznabApiKey(),
                            Category = "reaparr-sonarr",
                            DownloadFolderId = defaultDownloadFolder.Id,
                            ProvisioningState = IntegrationProvisioningState.Unconfigured,
                        };
                        dbContext.SonarrIntegrations.Add(integration);
                        imported.Add(new IntegrationIdentity(IntegrationType.Sonarr, integration.Id));
                    }
                    else
                    {
                        _log.Here()
                            .Warning(
                                "Skipped legacy {IntegrationType} integration: URL is invalid",
                                IntegrationType.Sonarr
                            );
                    }
                }

                await dbContext.SaveChangesAsync(transactionToken);
                return imported;
            },
            ct
        );
        if (transactionResult.IsCancelled)
            return transactionResult.ToResult().LogWarning();

        if (transactionResult.IsFailed)
        {
            if (hasLegacyRadarr)
                _log.Here().Error("Failed to import legacy {IntegrationType} integration", IntegrationType.Radarr);
            if (hasLegacySonarr)
                _log.Here().Error("Failed to import legacy {IntegrationType} integration", IntegrationType.Sonarr);
            return transactionResult.ToResult().LogError();
        }

        foreach (var integration in transactionResult.Value)
            _log.Here()
                .Information(
                    "Imported legacy {IntegrationType} integration {IntegrationId}",
                    integration.Type,
                    integration.Id
                );

        radarrSettings.Update(RadarrSettings.Create());
        sonarrSettings.Update(SonarrSettings.Create());
        var saveConfigResult = _configManager.SaveConfig();
        if (saveConfigResult.IsFailed)
            saveConfigResult.LogError();

        foreach (var integration in transactionResult.Value)
        {
            var setupResult = integration.Type switch
            {
                IntegrationType.Radarr => (
                    await _commandExecutor.Send(
                        new SetupRadarrIntegrationCommand(integration.Id),
                        CancellationToken.None
                    )
                ).ToResult(),
                IntegrationType.Sonarr => (
                    await _commandExecutor.Send(
                        new SetupSonarrIntegrationCommand(integration.Id),
                        CancellationToken.None
                    )
                ).ToResult(),
                _ => Result.Fail("Unsupported integration type: {IntegrationType}", integration.Type),
            };
            if (setupResult.IsFailed)
            {
                _log.Here()
                    .Error(
                        "Failed to set up imported {IntegrationType} integration {IntegrationId}: {Reasons}",
                        integration.Type,
                        integration.Id,
                        string.Join("; ", setupResult.Errors.Select(x => x.Message))
                    );
                setupResult.LogError();
            }
        }

        return saveConfigResult.IsFailed ? saveConfigResult : Result.Ok();
    }
}
