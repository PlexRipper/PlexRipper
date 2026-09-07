namespace Reaparr.Application;

public record SonarrApiCreateDownloadClientCommand : ICommand<Result<SonarrDownloadContractDTO>>
{
    public required Guid IntegrationId { get; init; }
    public required bool ForceSave { get; init; }
    public required SonarrDownloadContractDTO Resource { get; init; }
}

public class SonarApiCreateDownloadClientCommandHandler
    : ICommandHandler<SonarrApiCreateDownloadClientCommand, Result<SonarrDownloadContractDTO>>
{
    private readonly ISonarrHttpClientFactory _sonarrHttpClientFactory;

    public SonarApiCreateDownloadClientCommandHandler(ISonarrHttpClientFactory sonarrHttpClientFactory)
    {
        _sonarrHttpClientFactory = sonarrHttpClientFactory;
    }

    public async Task<Result<SonarrDownloadContractDTO>> ExecuteAsync(
        SonarrApiCreateDownloadClientCommand command,
        CancellationToken cancellationToken
    )
    {
        var clientResult = await _sonarrHttpClientFactory.CreateAsync(command.IntegrationId);
        if (clientResult.IsFailed)
            return clientResult.ToResult<SonarrDownloadContractDTO>();

        using var client = clientResult.Value;
        return await client.CreateSonarrDownloadClientAsync(command.ForceSave, command.Resource, cancellationToken);
    }
}
