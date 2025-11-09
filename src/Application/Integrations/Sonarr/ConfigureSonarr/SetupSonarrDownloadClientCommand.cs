using FastEndpoints;
using Reaparr.Settings.Contracts;

namespace Reaparr.Application;

public record SetupSonarrDownloadClientCommand(Uri ReaparrBaseUri)
    : ICommand<Result<SetupSonarrDownloadClientCommandResult>>;

public record SetupSonarrDownloadClientCommandResult
{
    public int DownloadClientId { get; set; }
}

public class SetupSonarrDownloadClientCommandHandler
    : ICommandHandler<SetupSonarrDownloadClientCommand, Result<SetupSonarrDownloadClientCommandResult>>
{
    private readonly ILogger _log;
    private readonly ICommandExecutor _commandExecutor;
    private readonly ISonarrSettings _settings;

    private readonly string DownloadClientName = "Reaparr DownloadClient";

    public SetupSonarrDownloadClientCommandHandler(
        ILogger log,
        ICommandExecutor commandExecutor,
        ISonarrSettings settings
    )
    {
        _log = log.ForContext<SetupSonarrDownloadClientCommandHandler>();
        _commandExecutor = commandExecutor;
        _settings = settings;
    }

    public async Task<Result<SetupSonarrDownloadClientCommandResult>> ExecuteAsync(
        SetupSonarrDownloadClientCommand command,
        CancellationToken ct
    )
    {
        if (string.IsNullOrWhiteSpace(_settings.SonarrBaseUrl) || string.IsNullOrWhiteSpace(_settings.SonarrApiKey))
            return Result.Fail("Sonarr settings are invalid: BaseUrl and ApiKey are required.").LogError();

        if (
            !Uri.TryCreate(_settings.SonarrBaseUrl.TrimEnd('/'), UriKind.Absolute, out var sonarrBaseUri)
            || (sonarrBaseUri.Scheme != Uri.UriSchemeHttp && sonarrBaseUri.Scheme != Uri.UriSchemeHttps)
        )
            return Result.Fail("Sonarr BaseUrl is invalid.").LogError();

        try
        {
            var result = await _commandExecutor.Send(new SonarApiGetDownloadClientsCommand(), ct);
            if (result.IsFailed)
                return Result
                    .Fail("Failed to retrieve existing download clients from Sonarr.")
                    .WithErrors(result.Errors)
                    .LogError();

            var list = result.Value;

            var currentDownloadClient = list.FirstOrDefault(d =>
                string.Equals(d.Name, DownloadClientName, StringComparison.OrdinalIgnoreCase)
            );

            if (currentDownloadClient != null)
            {
                var updateResult = await _commandExecutor.Send(
                    new SonarApiUpdateDownloadClientCommand
                    {
                        Id = currentDownloadClient?.Id ?? -1,
                        ForceSave = true,
                        Resource = new SonarrUpdateDownloadClientDTO
                        {
                            Enable = true,
                            Protocol = "torrent",
                            Priority = 1,
                            RemoveCompletedDownloads = true,
                            RemoveFailedDownloads = true,
                            Name = DownloadClientName,
                            Fields =
                            [
                                new() { Name = "host", Value = command.ReaparrBaseUri.Host },
                                new() { Name = "port", Value = command.ReaparrBaseUri.Port },
                                new() { Name = "useSsl", Value = false },
                                new() { Name = "urlBase", Value = "/api/public/download-client/" },
                                new() { Name = "username" },
                                new() { Name = "password" },
                                new() { Name = "tvCategory", Value = "tv-sonarr" },
                                new() { Name = "tvImportedCategory" },
                                new() { Name = "recentTvPriority", Value = 0 },
                                new() { Name = "olderTvPriority", Value = 0 },
                                new() { Name = "initialState", Value = 0 },
                                new() { Name = "sequentialOrder", Value = false },
                                new() { Name = "firstAndLast", Value = false },
                                new() { Name = "contentLayout", Value = 0 },
                            ],
                            ImplementationName = "qBittorrent",
                            Implementation = "QBittorrent",
                            ConfigContract = "QBittorrentSettings",
                            InfoLink = "https://wiki.servarr.com/sonarr/supported#qbittorrent",
                            Id = currentDownloadClient?.Id ?? -1,
                        },
                    },
                    ct
                );

                if (updateResult.IsFailed)
                    return updateResult.LogError();

                return Result.Ok(
                    new SetupSonarrDownloadClientCommandResult { DownloadClientId = updateResult.Value.Id }
                );
            }
            else
            {
                var createResult = await _commandExecutor.Send(
                    new SonarrApiCreateDownloadClientCommand
                    {
                        ForceSave = false,
                        Resource = new SonarrCreateDownloadClientDTO
                        {
                            Enable = true,
                            Protocol = "torrent",
                            Priority = 1,
                            RemoveCompletedDownloads = true,
                            RemoveFailedDownloads = true,
                            Name = DownloadClientName,
                            Fields =
                            [
                                new() { Name = "host", Value = command.ReaparrBaseUri.Host },
                                new() { Name = "port", Value = command.ReaparrBaseUri.Port },
                                new() { Name = "useSsl", Value = false },
                                new() { Name = "urlBase", Value = "/api/public/download-client/" },
                                new() { Name = "username" },
                                new() { Name = "password" },
                                new() { Name = "tvCategory", Value = "tv-sonarr" },
                                new() { Name = "tvImportedCategory" },
                                new() { Name = "recentTvPriority", Value = 0 },
                                new() { Name = "olderTvPriority", Value = 0 },
                                new() { Name = "initialState", Value = 0 },
                                new() { Name = "sequentialOrder", Value = false },
                                new() { Name = "firstAndLast", Value = false },
                                new() { Name = "contentLayout", Value = 0 },
                            ],
                            ImplementationName = "qBittorrent",
                            Implementation = "QBittorrent",
                            ConfigContract = "QBittorrentSettings",
                            InfoLink = "https://wiki.servarr.com/sonarr/supported#qbittorrent",
                            Tags = new List<int>(),
                        },
                    }
                );

                if (createResult.IsFailed)
                    return createResult.LogError();

                return Result.Ok(
                    new SetupSonarrDownloadClientCommandResult { DownloadClientId = createResult.Value.Id }
                );
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
