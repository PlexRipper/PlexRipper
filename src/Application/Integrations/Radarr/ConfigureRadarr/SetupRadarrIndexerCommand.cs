using FastEndpoints;
using Reaparr.Settings.Contracts;

namespace Reaparr.Application;

public record SetupRadarrIndexerCommand : ICommand<Result<SetupRadarrIndexerCommandResult>>
{
    public required Uri ReaparrBaseUri { get; init; }

    public required int DownloadClientId { get; init; }
}

public record SetupRadarrIndexerCommandResult
{
    public int IndexerId { get; set; }
}

public class SetupRadarrIndexerCommandHandler
    : ICommandHandler<SetupRadarrIndexerCommand, Result<SetupRadarrIndexerCommandResult>>
{
    private readonly ILogger _log;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IRadarrSettings _radarrSettings;
    private readonly IIntegrationsSettings _integrationsSettings;

    private readonly string _indexerName = "Reaparr";

    public SetupRadarrIndexerCommandHandler(
        ILogger log,
        ICommandExecutor commandExecutor,
        IRadarrSettings radarrSettings,
        IIntegrationsSettings integrationsSettings
    )
    {
        _log = log.ForContext<SetupRadarrIndexerCommandHandler>();
        _commandExecutor = commandExecutor;
        _radarrSettings = radarrSettings;
        _integrationsSettings = integrationsSettings;
    }

    public async Task<Result<SetupRadarrIndexerCommandResult>> ExecuteAsync(
        SetupRadarrIndexerCommand command,
        CancellationToken ct
    )
    {
        if (!_radarrSettings.IsValidApiKey())
            return Result.Fail("Radarr settings are invalid: ApiKey is invalid.").LogError();

        if (!_radarrSettings.IsValidUrl())
            return Result.Fail("Radarr settings are invalid: BaseUrl is invalid.").LogError();

        _log.Information("Setting up Radarr indexer '{IndexerName}'...", _indexerName);

        // Check for existing indexers
        var getResult = await _commandExecutor.Send(new RadarrApiGetIndexersCommand(), ct);
        if (getResult.IsFailed)
            return Result
                .Fail("Failed to retrieve existing indexers from Radarr.")
                .WithErrors(getResult.Errors)
                .LogError();

        var existing = getResult.Value.FirstOrDefault(d =>
            string.Equals(d.Name, _indexerName, StringComparison.OrdinalIgnoreCase)
        );

        if (existing is not null)
        {
            _log.Information("Indexer '{IndexerName}' already exists in Radarr. Updating...", _indexerName);
            // Update existing indexer
            var updateResult = await _commandExecutor.Send(
                new RadarrApiUpdateIndexerCommand
                {
                    Id = existing.Id,
                    ForceSave = true,
                    Resource = BuildIndexerResource(command.ReaparrBaseUri, command.DownloadClientId),
                },
                ct
            );

            if (updateResult.IsFailed)
                return updateResult.LogError();

            _log.Information("Successfully updated indexer '{IndexerName}' in Radarr.", _indexerName);
            return Result.Ok(new SetupRadarrIndexerCommandResult { IndexerId = updateResult.Value.Id });
        }

        // Create a new indexer
        _log.Information("Creating new indexer '{IndexerName}' in Radarr...", _indexerName);
        var createResult = await _commandExecutor.Send(
            new RadarrApiCreateIndexerCommand
            {
                ForceSave = true,
                Resource = BuildIndexerResource(command.ReaparrBaseUri, command.DownloadClientId),
            },
            ct
        );

        if (createResult.IsFailed)
            return createResult.LogError();

        _log.Information("Successfully created indexer '{IndexerName}' in Radarr.", _indexerName);
        return Result.Ok(new SetupRadarrIndexerCommandResult { IndexerId = createResult.Value.Id });
    }

    private RadarrIndexerContractDTO BuildIndexerResource(Uri reaparrBaseUri, int downloadClientId)
    {
        var baseUrl = reaparrBaseUri.AbsoluteUri.TrimEnd('/') + "/api/public/indexer/";

        return new RadarrIndexerContractDTO
        {
            Name = _indexerName,
            EnableRss = true,
            EnableAutomaticSearch = true,
            EnableInteractiveSearch = true,
            SupportsRss = true,
            SupportsSearch = true,
            Protocol = "torrent",
            Priority = 25,
            DownloadClientId = downloadClientId,
            Fields =
            [
                new RadarrIndexerContractFieldDTO { Name = "baseUrl", Value = baseUrl },
                new RadarrIndexerContractFieldDTO { Name = "apiPath", Value = "/api" },
                new RadarrIndexerContractFieldDTO { Name = "apiKey", Value = _integrationsSettings.ReaparrApiKey },
                new RadarrIndexerContractFieldDTO
                {
                    Name = "categories",
                    Value = new List<int> { 2000, 2010, 2020, 2030, 2040, 2045, 2050, 2060, 2070 },
                },
                new RadarrIndexerContractFieldDTO { Name = "minimumSeeders", Value = 1 },
                new RadarrIndexerContractFieldDTO { Name = "seedCriteria.seedRatio", Value = null },
                new RadarrIndexerContractFieldDTO { Name = "seedCriteria.seedTime", Value = null },
            ],
            ImplementationName = "Torznab",
            Implementation = "Torznab",
            ConfigContract = "TorznabSettings",
            InfoLink = "https://wiki.servarr.com/radarr/supported#torznab",
            Tags = [],
        };
    }
}
