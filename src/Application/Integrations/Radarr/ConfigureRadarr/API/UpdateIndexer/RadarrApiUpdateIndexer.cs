namespace Reaparr.Application;

public record RadarrApiUpdateIndexerCommand : ICommand<Result<RadarrIndexerResourceDTO>>
{
    public required Guid IntegrationId { get; init; }
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
    : ICommandHandler<RadarrApiUpdateIndexerCommand, Result<RadarrIndexerResourceDTO>>
{
    private readonly IRadarrHttpClientFactory _radarrHttpClientFactory;

    public RadarrApiUpdateIndexerCommandHandler(IRadarrHttpClientFactory radarrHttpClientFactory)
    {
        _radarrHttpClientFactory = radarrHttpClientFactory;
    }

    public async Task<Result<RadarrIndexerResourceDTO>> ExecuteAsync(
        RadarrApiUpdateIndexerCommand command,
        CancellationToken cancellationToken
    )
    {
        var clientResult = await _radarrHttpClientFactory.CreateAsync(command.IntegrationId);
        if (clientResult.IsFailed)
            return clientResult.ToResult<RadarrIndexerResourceDTO>();

        using var client = clientResult.Value;
        return await client.UpdateRadarrIndexerAsync(
            command.Id,
            command.ForceSave,
            command.Resource,
            cancellationToken
        );
    }
}
