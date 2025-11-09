using FastEndpoints;
using Reaparr.Settings.Contracts;

namespace Reaparr.Application;

public record SetupSonarrIndexerCommand : ICommand<Result<SetupSonarrIndexerCommandResult>>
{
    public required Uri ReaparrBaseUri { get; init; }

    public required int DownloadClientId { get; init; }
}

public record SetupSonarrIndexerCommandResult
{
    public int IndexerId { get; set; }
}

public class SetupSonarrIndexerCommandHandler
    : ICommandHandler<SetupSonarrIndexerCommand, Result<SetupSonarrIndexerCommandResult>>
{
    private readonly ILogger _log;
    private readonly ICommandExecutor _commandExecutor;
    private readonly ISonarrSettings _sonarrSettings;
    private readonly IIntegrationsSettings _integrationsSettings;

    private readonly string _indexerName = "Reaparr";

    public SetupSonarrIndexerCommandHandler(
        ILogger log,
        ICommandExecutor commandExecutor,
        ISonarrSettings sonarrSettings,
        IIntegrationsSettings integrationsSettings
    )
    {
        _log = log.ForContext<SetupSonarrIndexerCommandHandler>();
        _commandExecutor = commandExecutor;
        _sonarrSettings = sonarrSettings;
        _integrationsSettings = integrationsSettings;
    }

    public async Task<Result<SetupSonarrIndexerCommandResult>> ExecuteAsync(
        SetupSonarrIndexerCommand command,
        CancellationToken ct
    )
    {
        if (!_sonarrSettings.IsValidApiKey())
            return Result.Fail("Sonarr settings are invalid: ApiKey is invalid.").LogError();

        if (!_sonarrSettings.IsValidUrl())
            return Result.Fail("Sonarr settings are invalid: BaseUrl is invalid.").LogError();

        // Check for existing indexers
        var getResult = await _commandExecutor.Send(new SonarrApiGetIndexersCommand(), ct);
        if (getResult.IsFailed)
            return Result
                .Fail("Failed to retrieve existing indexers from Sonarr.")
                .WithErrors(getResult.Errors)
                .LogError();

        var existing = getResult.Value.FirstOrDefault(d =>
            string.Equals(d.Name, _indexerName, StringComparison.OrdinalIgnoreCase)
        );

        if (existing is not null)
        {
            // Update existing indexer
            var updateResult = await _commandExecutor.Send(
                new SonarrApiUpdateIndexerCommand
                {
                    Id = existing.Id,
                    ForceSave = true,
                    Resource = BuildIndexerResource(command.ReaparrBaseUri, command.DownloadClientId),
                },
                ct
            );

            if (updateResult.IsFailed)
                return updateResult.LogError();

            return Result.Ok(new SetupSonarrIndexerCommandResult { IndexerId = updateResult.Value.Id });
        }

        // Create a new indexer
        var createResult = await _commandExecutor.Send(
            new SonarrApiCreateIndexerCommand
            {
                ForceSave = true,
                Resource = BuildIndexerResource(command.ReaparrBaseUri, command.DownloadClientId),
            },
            ct
        );

        if (createResult.IsFailed)
            return createResult.LogError();

        return Result.Ok(new SetupSonarrIndexerCommandResult { IndexerId = createResult.Value.Id });
    }

    private SonarrIndexerContractDTO BuildIndexerResource(Uri reaparrBaseUri, int downloadClientId)
    {
        var baseUrl = reaparrBaseUri.AbsoluteUri.TrimEnd('/') + "/api/public/indexer/";

        return new SonarrIndexerContractDTO
        {
            Name = _indexerName,
            EnableRss = true,
            EnableAutomaticSearch = true,
            EnableInteractiveSearch = true,
            SupportsRss = true,
            SupportsSearch = true,
            Protocol = "torrent",
            Priority = 25,
            SeasonSearchMaximumSingleEpisodeAge = 0,
            DownloadClientId = downloadClientId,
            Fields =
            [
                new SonarrIndexerContractFieldDTO { Name = "baseUrl", Value = baseUrl },
                new SonarrIndexerContractFieldDTO { Name = "apiPath", Value = "/api" },
                new SonarrIndexerContractFieldDTO { Name = "apiKey", Value = _integrationsSettings.ReaparrApiKey },
                new SonarrIndexerContractFieldDTO
                {
                    Name = "categories",
                    Value = new List<int>
                    {
                        2010,
                        2020,
                        2030,
                        2040,
                        2045,
                        2050,
                        2060,
                        2070,
                        5000,
                        5030,
                        5040,
                        5050,
                        5070,
                        5080,
                        5090,
                        2000,
                    },
                },
                new SonarrIndexerContractFieldDTO
                {
                    Name = "animeCategories",
                    Value =
                        (List<int>)
                            [
                                2010,
                                2020,
                                2030,
                                2040,
                                2045,
                                2050,
                                2060,
                                2070,
                                5000,
                                5030,
                                5040,
                                5050,
                                5070,
                                5080,
                                5090,
                                2000,
                            ],
                },
                new SonarrIndexerContractFieldDTO { Name = "animeStandardFormatSearch", Value = false },
                new SonarrIndexerContractFieldDTO { Name = "additionalParameters", Value = (object?)null },
                new SonarrIndexerContractFieldDTO { Name = "multiLanguages", Value = new List<string>() },
                new SonarrIndexerContractFieldDTO { Name = "failDownloads", Value = new List<string>() },
                new SonarrIndexerContractFieldDTO { Name = "minimumSeeders", Value = 1 },
                new SonarrIndexerContractFieldDTO { Name = "seedCriteria.seedRatio", Value = (object?)null },
                new SonarrIndexerContractFieldDTO { Name = "seedCriteria.seedTime", Value = (object?)null },
                new SonarrIndexerContractFieldDTO { Name = "seedCriteria.seasonPackSeedTime", Value = (object?)null },
                new SonarrIndexerContractFieldDTO
                {
                    Name = "rejectBlocklistedTorrentHashesWhileGrabbing",
                    Value = false,
                },
            ],
            ImplementationName = "Torznab",
            Implementation = "Torznab",
            ConfigContract = "TorznabSettings",
            InfoLink = "https://wiki.servarr.com/sonarr/supported#torznab",
            Tags = [],
        };
    }
}
