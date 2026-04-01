using Reaparr.Data.Contracts;
using Reaparr.PlexApi.Contracts;
using Reaparr.Settings.Contracts;

namespace Reaparr.Application;

public record DeterminePlexDownloadClientCommand(int PlexServerId, DownloadTaskKey DownloadTaskKey, string MetaDataPath)
    : ICommand<Result<PlexDownloadClientType>>;

public class DeterminePlexDownloadClientCommandValidator : AbstractValidator<DeterminePlexDownloadClientCommand>
{
    public DeterminePlexDownloadClientCommandValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
        RuleFor(x => x.DownloadTaskKey).NotNull();
        RuleFor(x => x.MetaDataPath)
            .NotEmpty()
            .Must(path => path.Contains("/library/metadata/"))
            .WithMessage("MetaDataPath must be in format '/library/metadata/{ratingKey}'");
    }
}

public class DeterminePlexDownloadClientCommandHandler
    : ICommandHandler<DeterminePlexDownloadClientCommand, Result<PlexDownloadClientType>>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly IServerSettingsModule _serverSettingsModule;
    private readonly ICommandExecutor _commandExecutor;

    public DeterminePlexDownloadClientCommandHandler(
        ILogger log,
        IReaparrDbContext dbContext,
        IServerSettingsModule serverSettingsModule,
        ICommandExecutor commandExecutor
    )
    {
        _log = log.ForContext<DeterminePlexDownloadClientCommandHandler>();
        _dbContext = dbContext;
        _serverSettingsModule = serverSettingsModule;
        _commandExecutor = commandExecutor;
    }

    public async Task<Result<PlexDownloadClientType>> ExecuteAsync(
        DeterminePlexDownloadClientCommand command,
        CancellationToken cancellationToken
    )
    {
        var machineId = await _dbContext.GetPlexServerMachineIdentifierById(command.PlexServerId, cancellationToken);
        if (string.IsNullOrWhiteSpace(machineId))
        {
            return Result
                .Fail($"Unable to resolve machine identifier for Plex server {command.PlexServerId}")
                .LogError();
        }

        if (!_serverSettingsModule.GetAllowStreamDownloader(machineId))
            return Result.Ok(PlexDownloadClientType.Direct);

        var decisionRequest = new TranscodeDecisionRequest(command.MetaDataPath);

        var decisionResult = await _commandExecutor.Send(
            new GetDashTranscodeDecisionCommand(command.PlexServerId, decisionRequest),
            cancellationToken
        );

        if (decisionResult.IsFailed)
            return decisionResult.ToResult();

        var clientType = decisionResult.Value.SuggestedClientType;
        _log.Debug(
            "Determined Plex download client type {ClientType} for Plex server {PlexServerId} and media {MetaDataPath}",
            clientType,
            command.PlexServerId,
            command.MetaDataPath
        );
        return Result.Ok(clientType);
    }
}
