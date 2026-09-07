namespace Reaparr.Application;

/// <summary>
/// Tests every configured Radarr and Sonarr integration after Reaparr starts, persists each latest
/// connection result, and asks reachable Arr instances to retest their download clients.
/// </summary>
public record NotifyArrAppsOnStartupCommand : ICommand<Result>;

public class NotifyArrAppsOnStartupCommandValidator : AbstractValidator<NotifyArrAppsOnStartupCommand>
{
    public NotifyArrAppsOnStartupCommandValidator() => RuleFor(x => x).NotNull();
}

public class NotifyArrAppsOnStartupCommandHandler : ICommandHandler<NotifyArrAppsOnStartupCommand, Result>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IRadarrHttpClientFactory _radarrHttpClientFactory;
    private readonly ISonarrHttpClientFactory _sonarrHttpClientFactory;

    public NotifyArrAppsOnStartupCommandHandler(
        ILogger log,
        IReaparrDbContext dbContext,
        ICommandExecutor commandExecutor,
        IRadarrHttpClientFactory radarrHttpClientFactory,
        ISonarrHttpClientFactory sonarrHttpClientFactory
    )
    {
        _log = log.ForContext<NotifyArrAppsOnStartupCommandHandler>();
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
        _radarrHttpClientFactory = radarrHttpClientFactory;
        _sonarrHttpClientFactory = sonarrHttpClientFactory;
    }

    public async Task<Result> ExecuteAsync(NotifyArrAppsOnStartupCommand command, CancellationToken ct)
    {
        var radarrIds = await _dbContext
            .RadarrIntegrations.Where(x => x.ProvisioningState == IntegrationProvisioningState.Configured)
            .Select(x => x.Id)
            .ToListAsync(ct);
        var sonarrIds = await _dbContext
            .SonarrIntegrations.Where(x => x.ProvisioningState == IntegrationProvisioningState.Configured)
            .Select(x => x.Id)
            .ToListAsync(ct);

        _log.Here()
            .Debug(
                "Testing {RadarrIntegrationCount} Radarr and {SonarrIntegrationCount} Sonarr integrations on startup",
                radarrIds.Count,
                sonarrIds.Count
            );
        var results = await Task.WhenAll(
            radarrIds.Select(id => TestRadarrAsync(id, ct)).Concat(sonarrIds.Select(id => TestSonarrAsync(id, ct)))
        );

        var cancelledResult = results.FirstOrDefault(x => x.IsCancelled);
        if (cancelledResult is not null)
            return cancelledResult.LogWarning();

        foreach (var result in results.Where(x => x.IsFailed))
            result.LogError();

        return Result.Ok();
    }

    private async Task<Result> TestRadarrAsync(Guid integrationId, CancellationToken ct)
    {
        var connectionResult = await _commandExecutor.Send(
            new TestConnectionToRadarrCommand(integrationId, null, null),
            ct
        );
        if (connectionResult.IsCancelled)
            return connectionResult.ToResult();
        if (connectionResult.IsFailed || connectionResult.Value.Status != TestConnectionStatus.Success)
            return connectionResult.ToResult();

        var clientResult = await _radarrHttpClientFactory.CreateAsync(integrationId);
        if (clientResult.IsCancelled)
            return clientResult.ToResult();
        if (clientResult.IsFailed)
            return clientResult.ToResult();

        using var client = clientResult.Value;
        return await client.TestAllRadarrDownloadClientsAsync(ct);
    }

    private async Task<Result> TestSonarrAsync(Guid integrationId, CancellationToken ct)
    {
        var connectionResult = await _commandExecutor.Send(
            new TestConnectionToSonarrCommand(integrationId, null, null),
            ct
        );
        if (connectionResult.IsCancelled)
            return connectionResult.ToResult();
        if (connectionResult.IsFailed || connectionResult.Value.Status != TestConnectionStatus.Success)
            return connectionResult.ToResult();

        var clientResult = await _sonarrHttpClientFactory.CreateAsync(integrationId);
        if (clientResult.IsCancelled)
            return clientResult.ToResult();
        if (clientResult.IsFailed)
            return clientResult.ToResult();

        using var client = clientResult.Value;
        return await client.TestAllSonarrDownloadClientsAsync(ct);
    }
}
