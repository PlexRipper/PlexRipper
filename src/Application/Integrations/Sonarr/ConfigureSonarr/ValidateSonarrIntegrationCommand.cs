namespace Reaparr.Application;

public record ValidateSonarrIntegrationCommand(
    Guid IntegrationId,
    SonarrDownloadContractDTO DownloadClient,
    SonarrIndexerContractDTO Indexer
) : ICommand<Result>;

public class ValidateSonarrIntegrationCommandValidator : AbstractValidator<ValidateSonarrIntegrationCommand>
{
    public ValidateSonarrIntegrationCommandValidator()
    {
        RuleFor(x => x.IntegrationId).NotEmpty();
        RuleFor(x => x.DownloadClient).NotNull();
        RuleFor(x => x.Indexer).NotNull();
    }
}

public class ValidateSonarrIntegrationCommandHandler : ICommandHandler<ValidateSonarrIntegrationCommand, Result>
{
    private readonly ISonarrHttpClientFactory _sonarrHttpClientFactory;

    public ValidateSonarrIntegrationCommandHandler(ISonarrHttpClientFactory sonarrHttpClientFactory)
    {
        _sonarrHttpClientFactory = sonarrHttpClientFactory;
    }

    public async Task<Result> ExecuteAsync(ValidateSonarrIntegrationCommand command, CancellationToken ct)
    {
        var clientResult = await _sonarrHttpClientFactory.CreateAsync(command.IntegrationId);
        if (clientResult.IsFailed)
            return clientResult.ToResult().LogIfFailed();

        using var client = clientResult.Value;
        var results = await Task.WhenAll(
            client.TestSonarrDownloadClientAsync(command.DownloadClient, ct),
            client.TestSonarrIndexerAsync(command.Indexer, ct)
        );
        var result = results.Merge();
        return result.LogIfFailed();
    }
}
