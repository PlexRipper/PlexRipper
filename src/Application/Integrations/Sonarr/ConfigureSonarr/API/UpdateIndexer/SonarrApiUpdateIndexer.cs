using System.Text.Json;
using FastEndpoints;
using FluentValidation;

namespace Reaparr.Application;

public record SonarrApiUpdateIndexerCommand : ICommand<Result<SonarrIndexerContractDTO>>
{
    public required int Id { get; init; }
    public required bool ForceSave { get; init; }

    public required SonarrIndexerContractDTO Resource { get; init; }
}

public class SonarrApiUpdateIndexerCommandValidator : Validator<SonarrApiUpdateIndexerCommand>
{
    public SonarrApiUpdateIndexerCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Resource).NotNull();
    }
}

public class SonarrApiUpdateIndexerCommandHandler
    : ICommandHandler<SonarrApiUpdateIndexerCommand, Result<SonarrIndexerContractDTO>>
{
    private readonly HttpClient _client;

    public SonarrApiUpdateIndexerCommandHandler(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateSonarrHttpClient();
    }

    public async Task<Result<SonarrIndexerContractDTO>> ExecuteAsync(
        SonarrApiUpdateIndexerCommand command,
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
                    .Fail($"Failed to update indexer in Sonarr. StatusCode: {response.StatusCode}")
                    .WithError(body)
                    .LogError();
            }

            var updated = JsonSerializer.Deserialize<SonarrIndexerContractDTO>(
                body,
                DefaultJsonSerializerOptions.ConfigStandard
            );

            return Result.Ok(updated ?? new SonarrIndexerContractDTO());
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }
}
