namespace Reaparr.Application;

public record SonarrApiCreateIndexerCommand : ICommand<Result<SonarrIndexerContractDTO>>
{
    public required Guid IntegrationId { get; init; }
    public required bool ForceSave { get; init; }
    public required SonarrIndexerContractDTO Resource { get; init; }
}

public class SonarrApiCreateIndexerCommandHandler
    : ICommandHandler<SonarrApiCreateIndexerCommand, Result<SonarrIndexerContractDTO>>
{
    private readonly ISonarrHttpClientFactory _sonarrHttpClientFactory;

    public SonarrApiCreateIndexerCommandHandler(ISonarrHttpClientFactory sonarrHttpClientFactory)
    {
        _sonarrHttpClientFactory = sonarrHttpClientFactory;
    }

    public async Task<Result<SonarrIndexerContractDTO>> ExecuteAsync(
        SonarrApiCreateIndexerCommand command,
        CancellationToken cancellationToken
    )
    {
        var clientResult = await _sonarrHttpClientFactory.CreateAsync(command.IntegrationId);
        if (clientResult.IsFailed)
            return clientResult.ToResult<SonarrIndexerContractDTO>();

        using var client = clientResult.Value;
        return await client.CreateSonarrIndexerAsync(command.ForceSave, command.Resource, cancellationToken);
    }
}
