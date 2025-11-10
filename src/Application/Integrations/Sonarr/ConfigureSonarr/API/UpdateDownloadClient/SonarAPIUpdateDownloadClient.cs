using System.Text;
using System.Text.Json;
using FastEndpoints;
using FluentValidation;

namespace Reaparr.Application;

public record SonarApiUpdateDownloadClientCommand : ICommand<Result<SonarrDownloadContractDTO>>
{
    public required int Id { get; init; }
    public required bool ForceSave { get; init; }

    public required SonarrDownloadContractDTO Resource { get; init; }
}

public class SonarApiUpdateDownloadClientCommandValidator : Validator<SonarApiUpdateDownloadClientCommand>
{
    public SonarApiUpdateDownloadClientCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Resource).NotNull();
    }
}

public class SonarApiUpdateDownloadClientCommandHandler
    : ICommandHandler<SonarApiUpdateDownloadClientCommand, Result<SonarrDownloadContractDTO>>
{
    private readonly HttpClient _client;

    public SonarApiUpdateDownloadClientCommandHandler(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateSonarrHttpClient();
    }

    public async Task<Result<SonarrDownloadContractDTO>> ExecuteAsync(
        SonarApiUpdateDownloadClientCommand command,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var forceSave = command.ForceSave ? "true" : "false";
            var requestUri = new Uri($"/api/v3/downloadclient/{command.Id}?forceSave={forceSave}", UriKind.Relative);
            var json = JsonSerializer.Serialize(command.Resource, DefaultJsonSerializerOptions.ConfigStandard);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Put, requestUri)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            };

            var response = await _client.SendAsync(httpRequest, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return Result
                    .Fail($"Failed to update download client in Sonarr. StatusCode: {response.StatusCode}")
                    .WithError(body)
                    .LogError();
            }

            var updated = JsonSerializer.Deserialize<SonarrDownloadContractDTO>(
                body,
                DefaultJsonSerializerOptions.ConfigStandard
            );

            return Result.Ok(updated ?? new SonarrDownloadContractDTO());
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }
}
