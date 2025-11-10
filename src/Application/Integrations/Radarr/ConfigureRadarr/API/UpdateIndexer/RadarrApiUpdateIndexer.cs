using System.Text.Json;
using FastEndpoints;
using FluentValidation;

namespace Reaparr.Application;

public record RadarrApiUpdateIndexerCommand : ICommand<Result<RadarrIndexerContractDTO>>
{
    public required int Id { get; init; }
    public required bool ForceSave { get; init; }

    public required RadarrIndexerContractDTO Resource { get; init; }
}

public class RadarrApiUpdateIndexerCommandValidator : Validator<RadarrApiUpdateIndexerCommand>
{
    public RadarrApiUpdateIndexerCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Resource).NotNull();
    }
}

public class RadarrApiUpdateIndexerCommandHandler
    : ICommandHandler<RadarrApiUpdateIndexerCommand, Result<RadarrIndexerContractDTO>>
{
    private readonly HttpClient _client;

    public RadarrApiUpdateIndexerCommandHandler(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateRadarrHttpClient();
    }

    public async Task<Result<RadarrIndexerContractDTO>> ExecuteAsync(
        RadarrApiUpdateIndexerCommand command,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var forceSave = command.ForceSave ? "true" : "false";
            var requestUri = new Uri($"/api/v3/indexer/{command.Id}?forceSave={forceSave}", UriKind.Relative);
            var json = JsonSerializer.Serialize(command.Resource, DefaultJsonSerializerOptions.ConfigStandard);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Put, requestUri);
            httpRequest.Content = json.ToStringContent();

            var response = await _client.SendAsync(httpRequest, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Result
                    .Fail($"Failed to update indexer in Radarr. StatusCode: {response.StatusCode}")
                    .WithError(body)
                    .LogError();
            }

            var updated = JsonSerializer.Deserialize<RadarrIndexerContractDTO>(
                body,
                DefaultJsonSerializerOptions.ConfigStandard
            );

            return Result.Ok(updated ?? new RadarrIndexerContractDTO());
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }
}
