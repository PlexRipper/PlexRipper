using System.Text.Json;
using FastEndpoints;
using FluentValidation;

namespace Reaparr.Application;

public record RadarrApiUpdateDownloadClientCommand : ICommand<Result<RadarrDownloadContractDTO>>
{
    public required int Id { get; init; }
    public required bool ForceSave { get; init; }
    public required RadarrDownloadContractDTO Resource { get; init; }
}

public class RadarrApiUpdateDownloadClientCommandValidator : Validator<RadarrApiUpdateDownloadClientCommand>
{
    public RadarrApiUpdateDownloadClientCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Resource).NotNull();
    }
}

public class RadarrApiUpdateDownloadClientCommandHandler
    : ICommandHandler<RadarrApiUpdateDownloadClientCommand, Result<RadarrDownloadContractDTO>>
{
    private readonly HttpClient _client;

    public RadarrApiUpdateDownloadClientCommandHandler(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateRadarrHttpClient();
    }

    public async Task<Result<RadarrDownloadContractDTO>> ExecuteAsync(
        RadarrApiUpdateDownloadClientCommand command,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var forceSave = command.ForceSave ? "true" : "false";
            var requestUri = new Uri($"/api/v3/downloadclient/{command.Id}?forceSave={forceSave}", UriKind.Relative);
            var json = JsonSerializer.Serialize(command.Resource, DefaultJsonSerializerOptions.ConfigStandard);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Put, requestUri);
            httpRequest.Content = json.ToStringContent();

            var response = await _client.SendAsync(httpRequest, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Result
                    .Fail($"Failed to update download client in Radarr. StatusCode: {response.StatusCode}")
                    .WithError(body)
                    .LogError();
            }

            var updated = JsonSerializer.Deserialize<RadarrDownloadContractDTO>(
                body,
                DefaultJsonSerializerOptions.ConfigStandard
            );

            return Result.Ok(updated ?? new RadarrDownloadContractDTO());
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }
}
