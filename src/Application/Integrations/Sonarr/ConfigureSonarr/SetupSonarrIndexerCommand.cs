using FastEndpoints;
using Reaparr.Settings.Contracts;

namespace Reaparr.Application;

public record SetupSonarrIndexerCommand(Uri ReaparrBaseUri, int DownloadClientId) : ICommand<Result>;

public class SetupSonarrIndexerCommandHandler : ICommandHandler<SetupSonarrIndexerCommand, Result>
{
    private readonly ILogger _log;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ISonarrSettings _settings;

    public SetupSonarrIndexerCommandHandler(
        ILogger log,
        IHttpClientFactory httpClientFactory,
        ISonarrSettings settings
    )
    {
        _log = log.ForContext<SetupSonarrIndexerCommandHandler>();
        _httpClientFactory = httpClientFactory;
        _settings = settings;
    }

    public async Task<Result> ExecuteAsync(SetupSonarrIndexerCommand command, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_settings.BaseUrl) || string.IsNullOrWhiteSpace(_settings.ApiKey))
            return Result.Fail("Sonarr settings are invalid: BaseUrl and ApiKey are required.").LogError();

        if (
            !Uri.TryCreate(_settings.BaseUrl.TrimEnd('/'), UriKind.Absolute, out var sonarrBaseUri)
            || (sonarrBaseUri.Scheme != Uri.UriSchemeHttp && sonarrBaseUri.Scheme != Uri.UriSchemeHttps)
        )
            return Result.Fail("Sonarr BaseUrl is invalid.").LogError();

        var client = _httpClientFactory.CreateSonarrHttpClient();

        try
        {
            // var list = await SonarrApi.GetDefinitionsAsync(client, "/api/v3/indexer", ct);
            // var existing = list.FirstOrDefault(d =>
            //     string.Equals(d.Name, "Reaparr", StringComparison.OrdinalIgnoreCase)
            // );
            //
            // var payload = BuildReaparrIndexer(command.ReaparrBaseUri, command.DownloadClientId);
            // SonarrDefinition? upserted;
            // var wasCreated = false;
            // if (existing is not null)
            // {
            //     payload.Id = existing.Id;
            //     upserted = await SonarrApi.CreateOrUpdateAsync(client, "/api/v3/indexer", payload, existing.Id, ct);
            // }
            // else
            // {
            //     upserted = await SonarrApi.CreateOrUpdateAsync(client, "/api/v3/indexer", payload, null, ct);
            //     wasCreated = true;
            // }
            //
            // if (upserted?.Id is null)
            //     return Result.Fail("Failed to create or update Reaparr indexer in Sonarr.").LogError();
            //
            // return Result.Ok(new UpsertResult(upserted.Id.Value, wasCreated));
            await Task.CompletedTask;
            return Result.Ok();
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
