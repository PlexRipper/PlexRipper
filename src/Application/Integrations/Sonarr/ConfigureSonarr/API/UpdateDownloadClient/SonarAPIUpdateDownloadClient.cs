namespace Reaparr.Application;

public record SonarApiUpdateDownloadClientCommand : ICommand<Result<SonarrDownloadContractDTO>>
{
    public required Guid IntegrationId { get; init; }
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
    private readonly ISonarrHttpClientFactory _sonarrHttpClientFactory;

    public SonarApiUpdateDownloadClientCommandHandler(ISonarrHttpClientFactory sonarrHttpClientFactory)
    {
        _sonarrHttpClientFactory = sonarrHttpClientFactory;
    }

    public async Task<Result<SonarrDownloadContractDTO>> ExecuteAsync(
        SonarApiUpdateDownloadClientCommand command,
        CancellationToken cancellationToken
    )
    {
        var clientResult = await _sonarrHttpClientFactory.CreateAsync(command.IntegrationId);
        if (clientResult.IsFailed)
            return clientResult.ToResult<SonarrDownloadContractDTO>();

        using var client = clientResult.Value;
        return await client.UpdateSonarrDownloadClientAsync(
            command.Id,
            command.ForceSave,
            command.Resource,
            cancellationToken
        );
    }
}
