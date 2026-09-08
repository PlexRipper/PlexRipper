namespace Reaparr.Application;

public record SonarrApiUpdateIndexerCommand : ICommand<Result<SonarrIndexerContractDTO>>
{
    public required Guid IntegrationId { get; init; }
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
    private readonly ISonarrHttpClientFactory _sonarrHttpClientFactory;

    public SonarrApiUpdateIndexerCommandHandler(ISonarrHttpClientFactory sonarrHttpClientFactory)
    {
        _sonarrHttpClientFactory = sonarrHttpClientFactory;
    }

    public async Task<Result<SonarrIndexerContractDTO>> ExecuteAsync(
        SonarrApiUpdateIndexerCommand command,
        CancellationToken cancellationToken
    )
    {
        var clientResult = await _sonarrHttpClientFactory.CreateAsync(command.IntegrationId);
        if (clientResult.IsFailed)
            return clientResult.ToResult<SonarrIndexerContractDTO>();

        using var client = clientResult.Value;
        return await client.UpdateSonarrIndexerAsync(
            command.Id,
            command.ForceSave,
            command.Resource,
            cancellationToken
        );
    }
}
