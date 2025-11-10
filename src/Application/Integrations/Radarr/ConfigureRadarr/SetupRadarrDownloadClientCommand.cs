using FastEndpoints;
using Reaparr.PublicAPI.Contracts;
using Reaparr.Settings.Contracts;

namespace Reaparr.Application;

public record SetupRadarrDownloadClientCommand(Uri ReaparrBaseUri)
    : ICommand<Result<SetupRadarrDownloadClientCommandResult>>;

public record SetupRadarrDownloadClientCommandResult
{
    public int DownloadClientId { get; init; }
}

public class SetupRadarrDownloadClientCommandHandler
    : ICommandHandler<SetupRadarrDownloadClientCommand, Result<SetupRadarrDownloadClientCommandResult>>
{
    private readonly ILogger _log;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IIntegrationsSettings _integrationsSettings;
    private readonly IRadarrSettings _settings;

    private const string DOWNLOAD_CLIENT_NAME = "Reaparr DownloadClient";

    public SetupRadarrDownloadClientCommandHandler(
        ILogger log,
        ICommandExecutor commandExecutor,
        IIntegrationsSettings integrationsSettings,
        IRadarrSettings settings
    )
    {
        _log = log.ForContext<SetupRadarrDownloadClientCommandHandler>();
        _commandExecutor = commandExecutor;
        _integrationsSettings = integrationsSettings;
        _settings = settings;
    }

    public async Task<Result<SetupRadarrDownloadClientCommandResult>> ExecuteAsync(
        SetupRadarrDownloadClientCommand command,
        CancellationToken ct
    )
    {
        if (string.IsNullOrWhiteSpace(_settings.RadarrBaseUrl) || string.IsNullOrWhiteSpace(_settings.RadarrApiKey))
            return Result.Fail("Radarr settings are invalid: BaseUrl and ApiKey are required.").LogError();

        if (
            !Uri.TryCreate(_settings.RadarrBaseUrl.TrimEnd('/'), UriKind.Absolute, out var radarrBaseUri)
            || (radarrBaseUri.Scheme != Uri.UriSchemeHttp && radarrBaseUri.Scheme != Uri.UriSchemeHttps)
        )
            return Result.Fail("Radarr BaseUrl is invalid.").LogError();

        try
        {
            var result = await _commandExecutor.Send(new RadarrApiGetDownloadClientsCommand(), ct);
            if (result.IsFailed)
                return Result
                    .Fail("Failed to retrieve existing download clients from Radarr.")
                    .WithErrors(result.Errors)
                    .LogError();

            var list = result.Value;

            var currentDownloadClient = list.FirstOrDefault(d =>
                string.Equals(d.Name, DOWNLOAD_CLIENT_NAME, StringComparison.OrdinalIgnoreCase)
            );

            if (currentDownloadClient != null)
            {
                var updateResult = await _commandExecutor.Send(
                    new RadarrApiUpdateDownloadClientCommand
                    {
                        Id = currentDownloadClient.Id,
                        ForceSave = true,
                        Resource = BuildDownloadClientResource(command.ReaparrBaseUri),
                    },
                    ct
                );

                if (updateResult.IsFailed)
                    return updateResult.LogError();

                return Result.Ok(
                    new SetupRadarrDownloadClientCommandResult { DownloadClientId = updateResult.Value.Id }
                );
            }

            var createResult = await _commandExecutor.Send(
                new RadarrApiCreateDownloadClientCommand
                {
                    ForceSave = false,
                    Resource = BuildDownloadClientResource(command.ReaparrBaseUri),
                },
                ct
            );

            if (createResult.IsFailed)
                return createResult.LogError();

            return Result.Ok(new SetupRadarrDownloadClientCommandResult { DownloadClientId = createResult.Value.Id });
        }
        catch (TaskCanceledException e)
        {
            _log.Here().Error(e, "Timeout while communicating with Radarr.");
            return Result.Fail("Timeout while communicating with Radarr.").LogError();
        }
        catch (HttpRequestException e)
        {
            _log.Here().Error(e, "HTTP error while communicating with Radarr.");
            return Result.Fail("HTTP error while communicating with Radarr.").LogError();
        }
    }

    private RadarrDownloadContractDTO BuildDownloadClientResource(Uri reaparrBaseUri)
    {
        // Derive urlBase from Reaparr's base URI to include any PathBase and ensure correct trailing segment
        var basePath = string.IsNullOrEmpty(reaparrBaseUri.AbsolutePath) ? "/" : reaparrBaseUri.AbsolutePath;
        if (!basePath.EndsWith("/"))
            basePath += "/";
        var derivedUrlBase = $"{basePath}api/public/download-client/";

        var useSsl = string.Equals(reaparrBaseUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);

        return new RadarrDownloadContractDTO
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
                new() { Name = "urlBase", Value = derivedUrlBase },
                new() { Name = "username", Value = _integrationsSettings.DownloadClientUsername },
                new() { Name = "password", Value = _integrationsSettings.DownloadClientPassword },
                new() { Name = "movieCategory", Value = IntegrationDefinitions.RADARR_DEFAULT_CATEGORY },
                new() { Name = "recentMoviePriority", Value = 0 },
                new() { Name = "olderMoviePriority", Value = 0 },
                new() { Name = "initialState", Value = 0 },
                new() { Name = "sequentialOrder", Value = false },
                new() { Name = "firstAndLast", Value = false },
                new() { Name = "contentLayout", Value = 0 },
            ],
            ImplementationName = "qBittorrent",
            Implementation = "QBittorrent",
            ConfigContract = "QBittorrentSettings",
            InfoLink = "https://wiki.servarr.com/radarr/supported#qbittorrent",
            Tags = [],
        };
    }
}
