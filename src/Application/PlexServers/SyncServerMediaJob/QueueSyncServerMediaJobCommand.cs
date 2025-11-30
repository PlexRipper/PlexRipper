using FastEndpoints;
using FluentValidation;

namespace Reaparr.Application;

public record QueueSyncServerMediaJobCommand(int PlexServerId, bool ForceSync = false) : ICommand<Result>;

public class QueueSyncServerMediaJobCommandValidator : Validator<QueueSyncServerMediaJobCommand>
{
    public QueueSyncServerMediaJobCommandValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
    }
}

public class QueueSyncServerMediaJobCommandHandler : ICommandHandler<QueueSyncServerMediaJobCommand, Result>
{
    private readonly ILogger _log;
    private readonly ICommandExecutor _commandExecutor;

    public QueueSyncServerMediaJobCommandHandler(ILogger log, ICommandExecutor commandExecutor)
    {
        _log = log.ForContext<QueueSyncServerMediaJobCommandHandler>();
        _commandExecutor = commandExecutor;
    }

    public async Task<Result> ExecuteAsync(QueueSyncServerMediaJobCommand command, CancellationToken cancellationToken)
    {
        _log.Here()
            .Information("Queueing sync server media job for server with id {PlexServerId}", command.PlexServerId);

        return await _commandExecutor.Send(
            new SyncServerMediaJobCommand(command.PlexServerId, command.ForceSync),
            cancellationToken
        );
    }
}
