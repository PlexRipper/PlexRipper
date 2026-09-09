namespace Reaparr.Application;

public record ValidateRadarrIntegrationCommand(
    Guid IntegrationId,
    RadarrDownloadContractDTO DownloadClient,
    RadarrIndexerContractDTO Indexer
) : ICommand<Result>;

public class ValidateRadarrIntegrationCommandValidator : AbstractValidator<ValidateRadarrIntegrationCommand>
{
    public ValidateRadarrIntegrationCommandValidator()
    {
        RuleFor(x => x.IntegrationId).NotEmpty();
        RuleFor(x => x.DownloadClient).NotNull();
        RuleFor(x => x.Indexer).NotNull();
    }
}

public class ValidateRadarrIntegrationCommandHandler : ICommandHandler<ValidateRadarrIntegrationCommand, Result>
{
    private readonly IRadarrHttpClientFactory _radarrHttpClientFactory;

    public ValidateRadarrIntegrationCommandHandler(IRadarrHttpClientFactory radarrHttpClientFactory)
    {
        _radarrHttpClientFactory = radarrHttpClientFactory;
    }

    public async Task<Result> ExecuteAsync(ValidateRadarrIntegrationCommand command, CancellationToken ct)
    {
        var clientResult = await _radarrHttpClientFactory.CreateAsync(command.IntegrationId);
        if (clientResult.IsFailed)
            return clientResult.ToResult().LogIfFailed();

        using var client = clientResult.Value;
        var results = await Task.WhenAll(
            client.TestRadarrDownloadClientAsync(command.DownloadClient, ct),
            client.TestRadarrIndexerAsync(command.Indexer, ct)
        );
        var result = results.Merge();
        return result.LogIfFailed();
    }
}
