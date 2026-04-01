using Flurl;
using Reaparr.PublicAPI.Contracts;

namespace Reaparr.Application;

public record SetupSonarrDownloadClientCommand : ICommand<Result<SetupSonarrDownloadClientCommandResult>>;

public record SetupSonarrDownloadClientCommandResult
{
    public int DownloadClientId { get; init; }
}

public class SetupSonarrDownloadClientCommandHandler
    : ICommandHandler<SetupSonarrDownloadClientCommand, Result<SetupSonarrDownloadClientCommandResult>>
{
    private readonly ILogger _log;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IIntegrationsSettings _integrationsSettings;
    private readonly ISonarrSettings _settings;
    private readonly INetworkSettings _networkSettings;

    private const string DOWNLOAD_CLIENT_NAME = "Reaparr DownloadClient";

    private const string PUBLIC_URL_HINT =
        "If Sonarr cannot reach Reaparr, set the 'Public URL' in Advanced → Network settings to an address reachable from Sonarr (e.g. http://reaparr:5000 in Docker).";

    public SetupSonarrDownloadClientCommandHandler(
        ILogger log,
        ICommandExecutor commandExecutor,
        IIntegrationsSettings integrationsSettings,
        ISonarrSettings settings,
        INetworkSettings networkSettings
    )
    {
        _log = log.ForContext<SetupSonarrDownloadClientCommandHandler>();
        _commandExecutor = commandExecutor;
        _integrationsSettings = integrationsSettings;
        _settings = settings;
        _networkSettings = networkSettings;
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

        _log.Here()
            .Debug(
                "Setting up Sonarr download client. SonarrBaseUrl: {SonarrBaseUrl}, ReaparrBaseUrl: {ReaparrBaseUrl}",
                sonarrBaseUri,
                _networkSettings.Url
            );

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
                string.Equals(d.Name, DOWNLOAD_CLIENT_NAME, StringComparison.OrdinalIgnoreCase)
            );

            if (currentDownloadClient != null)
            {
                var updateResult = await _commandExecutor.Send(
                    new SonarApiUpdateDownloadClientCommand
                    {
                        Id = currentDownloadClient.Id,
                        ForceSave = true,
                        Resource = BuildDownloadClientResource(_networkSettings.Uri),
                    },
                    ct
                );

                if (updateResult.IsFailed)
                    return updateResult.WithError(PUBLIC_URL_HINT).LogError();

                return Result.Ok(
                    new SetupSonarrDownloadClientCommandResult { DownloadClientId = updateResult.Value.Id }
                );
            }

            var createResult = await _commandExecutor.Send(
                new SonarrApiCreateDownloadClientCommand
                {
                    ForceSave = false,
                    Resource = BuildDownloadClientResource(_networkSettings.Uri),
                },
                ct
            );

            if (createResult.IsFailed)
                return createResult.WithError(PUBLIC_URL_HINT).LogError();

            return Result.Ok(new SetupSonarrDownloadClientCommandResult { DownloadClientId = createResult.Value.Id });
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

    private SonarrDownloadContractDTO BuildDownloadClientResource(Uri reaparrBaseUri)
    {
        var useSsl = string.Equals(reaparrBaseUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
        string urlBase = _networkSettings.BasePath.AppendPathSegment("api/public/download-client");

        return new SonarrDownloadContractDTO
        {
            Enable = true,
            Protocol = "torrent",
            Priority = 1,
            RemoveCompletedDownloads = true,
            RemoveFailedDownloads = true,
            Name = DOWNLOAD_CLIENT_NAME,
            Fields =
            [
                new() { Name = "host", Value = reaparrBaseUri.Host },
                new() { Name = "port", Value = reaparrBaseUri.Port },
                new() { Name = "useSsl", Value = useSsl },
                new() { Name = "urlBase", Value = urlBase },
                new() { Name = "username", Value = _integrationsSettings.DownloadClientUsername },
                new() { Name = "password", Value = _integrationsSettings.DownloadClientPassword },
                new() { Name = "tvCategory", Value = IntegrationDefinitions.SONARR_DEFAULT_CATEGORY },
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
            Tags = [],
        };
    }
}
