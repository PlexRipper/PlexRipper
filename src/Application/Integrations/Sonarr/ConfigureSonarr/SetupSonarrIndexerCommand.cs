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
    private readonly ISonarrSettings _settings;

    private readonly string IndexerName = "Reaparr";

    public SetupSonarrIndexerCommandHandler(ILogger log, ICommandExecutor commandExecutor, ISonarrSettings settings)
    {
        _log = log.ForContext<SetupSonarrIndexerCommandHandler>();
        _commandExecutor = commandExecutor;
        _settings = settings;
    }

    public async Task<Result<SetupSonarrIndexerCommandResult>> ExecuteAsync(
        SetupSonarrIndexerCommand command,
        CancellationToken ct
    )
    {
        if (!_settings.IsValidApiKey())
            return Result.Fail("Sonarr settings are invalid: ApiKey is invalid.").LogError();

        if (!_settings.IsValidUrl())
            return Result.Fail("Sonarr settings are invalid: BaseUrl is invalid.").LogError();

        try
        {
            var getResult = await _commandExecutor.Send(new SonarrApiGetIndexersCommand(), ct);
            if (getResult.IsFailed)
                return Result
                    .Fail("Failed to retrieve existing indexers from Sonarr.")
                    .WithErrors(getResult.Errors)
                    .LogError();

            var existing = getResult.Value.FirstOrDefault(d =>
                string.Equals(d.Name, IndexerName, StringComparison.OrdinalIgnoreCase)
            );

            var reaparrBaseUrl = command.ReaparrBaseUri.AbsoluteUri.TrimEnd('/') + "/api/public/indexer/";

            if (existing is not null)
            {
                var updateResult = await _commandExecutor.Send(
                    new SonarrApiUpdateIndexerCommand
                    {
                        Id = existing.Id,
                        ForceSave = true,
                        Resource = new SonarrUpdateIndexerDTO
                        {
                            EnableRss = true,
                            EnableAutomaticSearch = true,
                            EnableInteractiveSearch = true,
                            SupportsRss = true,
                            SupportsSearch = true,
                            Protocol = "torrent",
                            Priority = 25,
                            SeasonSearchMaximumSingleEpisodeAge = 0,
                            DownloadClientId = command.DownloadClientId,
                            Name = "Reaparr",
                            Fields =
                            [
                                new() { Name = "baseUrl", Value = reaparrBaseUrl },
                                new() { Name = "apiPath", Value = "/api" },
                                new() { Name = "apiKey", Value = "********" },
                                new()
                                {
                                    Name = "categories",
                                    Value = new List<int> { 5030, 5040 },
                                },
                                new() { Name = "animeCategories", Value = new List<int>() },
                                new() { Name = "animeStandardFormatSearch", Value = false },
                                new() { Name = "additionalParameters" },
                                new() { Name = "multiLanguages", Value = new List<string>() },
                                new() { Name = "failDownloads", Value = new List<string>() },
                                new() { Name = "minimumSeeders", Value = 1 },
                                new() { Name = "seedCriteria.seedRatio" },
                                new() { Name = "seedCriteria.seedTime" },
                                new() { Name = "seedCriteria.seasonPackSeedTime" },
                                new() { Name = "rejectBlocklistedTorrentHashesWhileGrabbing", Value = false },
                            ],
                            ImplementationName = "Torznab",
                            Implementation = "Torznab",
                            ConfigContract = "TorznabSettings",
                            InfoLink = "https://wiki.servarr.com/sonarr/supported#torznab",
                            Tags = [],
                        },
                    },
                    ct
                );

                if (updateResult.IsFailed)
                    return updateResult.LogError();

                return Result.Ok(new SetupSonarrIndexerCommandResult { IndexerId = updateResult.Value.Id });
            }
            else
            {
                var createResult = await _commandExecutor.Send(
                    new SonarrApiCreateIndexerCommand
                    {
                        ForceSave = true,
                        Resource = new SonarrCreateIndexerDTO
                        {
                            EnableRss = true,
                            EnableAutomaticSearch = true,
                            EnableInteractiveSearch = true,
                            SupportsRss = true,
                            SupportsSearch = true,
                            Protocol = "torrent",
                            Priority = 25,
                            SeasonSearchMaximumSingleEpisodeAge = 0,
                            DownloadClientId = command.DownloadClientId,
                            Name = "Reaparr",
                            Fields =
                            [
                                new() { Name = "baseUrl", Value = reaparrBaseUrl },
                                new() { Name = "apiPath", Value = "/api" },
                                new() { Name = "apiKey", Value = "********" },
                                new()
                                {
                                    Name = "categories",
                                    Value = new List<int> { 5030, 5040 },
                                },
                                new() { Name = "animeCategories", Value = new List<int>() },
                                new() { Name = "animeStandardFormatSearch", Value = false },
                                new() { Name = "additionalParameters" },
                                new() { Name = "multiLanguages", Value = new List<string>() },
                                new() { Name = "failDownloads", Value = new List<string>() },
                                new() { Name = "minimumSeeders", Value = 1 },
                                new() { Name = "seedCriteria.seedRatio" },
                                new() { Name = "seedCriteria.seedTime" },
                                new() { Name = "seedCriteria.seasonPackSeedTime" },
                                new() { Name = "rejectBlocklistedTorrentHashesWhileGrabbing", Value = false },
                            ],
                            ImplementationName = "Torznab",
                            Implementation = "Torznab",
                            ConfigContract = "TorznabSettings",
                            InfoLink = "https://wiki.servarr.com/sonarr/supported#torznab",
                            Tags = [],
                        },
                    },
                    ct
                );

                if (createResult.IsFailed)
                    return createResult.LogError();

                return Result.Ok(new SetupSonarrIndexerCommandResult { IndexerId = createResult.Value.Id });
            }
        }
        catch (TaskCanceledException e)
        {
            _log.Here().Error(e, "Timeout while communicating with Sonarr.");
            return Result.Fail("Timeout while communicating with Sonarr.").LogError();
        }
        catch (HttpRequestException e)
        {
            _log.Here().Error(e, "HTTP error while communicating with Sonarr.");
            return Result.Fail("HTTP error while communicating with Sonarr.").LogError();
        }
    }
}
