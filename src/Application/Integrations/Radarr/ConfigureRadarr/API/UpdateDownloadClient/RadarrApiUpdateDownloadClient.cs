namespace Reaparr.Application;

public record RadarrApiUpdateDownloadClientCommand : ICommand<Result<RadarrDownloadClientResourceDTO>>
{
    public required Guid IntegrationId { get; init; }
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
    : ICommandHandler<RadarrApiUpdateDownloadClientCommand, Result<RadarrDownloadClientResourceDTO>>
{
    private readonly IRadarrHttpClientFactory _radarrHttpClientFactory;

    public RadarrApiUpdateDownloadClientCommandHandler(IRadarrHttpClientFactory radarrHttpClientFactory)
    {
        _radarrHttpClientFactory = radarrHttpClientFactory;
    }

    public async Task<Result<RadarrDownloadClientResourceDTO>> ExecuteAsync(
        RadarrApiUpdateDownloadClientCommand command,
        CancellationToken cancellationToken
    )
    {
        var clientResult = await _radarrHttpClientFactory.CreateAsync(command.IntegrationId);
        if (clientResult.IsFailed)
            return clientResult.ToResult<RadarrDownloadClientResourceDTO>();

        using var client = clientResult.Value;
        return await client.UpdateRadarrDownloadClientAsync(
            command.Id,
            command.ForceSave,
            command.Resource,
            cancellationToken
        );
    }
}
