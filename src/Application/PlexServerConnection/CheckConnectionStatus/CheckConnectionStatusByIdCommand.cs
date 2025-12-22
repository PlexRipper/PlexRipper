using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.PlexApi.Contracts;

namespace Reaparr.Application;

public record CheckConnectionStatusByIdCommand(int PlexServerConnectionId) : ICommand<Result<PlexServerStatus>>;

public class CheckConnectionStatusByIdCommandValidator : AbstractValidator<CheckConnectionStatusByIdCommand>
{
    public CheckConnectionStatusByIdCommandValidator()
    {
        RuleFor(x => x.PlexServerConnectionId).GreaterThan(0);
    }
}

public class CheckConnectionStatusByIdCommandHandler
    : ICommandHandler<CheckConnectionStatusByIdCommand, Result<PlexServerStatus>>
{
    private readonly ISignalRService _signalRService;
    private readonly ICommandExecutor _commandDispatcher;
    private readonly IReaparrDbContext _dbContext;
    private readonly ILogger _log;
    private PlexServerConnection? _plexServerConnection;

    public CheckConnectionStatusByIdCommandHandler(
        IReaparrDbContext dbContext,
        ISignalRService signalRService,
        ICommandExecutor commandDispatcher,
        ILogger log
    )
    {
        _dbContext = dbContext;
        _signalRService = signalRService;
        _commandDispatcher = commandDispatcher;
        _log = log.ForContext<CheckConnectionStatusByIdCommandHandler>();
    }

    public async Task<Result<PlexServerStatus>> ExecuteAsync(
        CheckConnectionStatusByIdCommand command,
        CancellationToken cancellationToken
    )
    {
        var plexServerConnection = await _dbContext.PlexServerConnections.GetAsync(
            command.PlexServerConnectionId,
            cancellationToken
        );

        if (plexServerConnection is null)
        {
            return ResultExtensions
                .EntityNotFound(nameof(PlexServerConnection), command.PlexServerConnectionId)
                .LogError();
        }

        _plexServerConnection = plexServerConnection;

        // Request status
        var serverStatusResult = await _commandDispatcher.Send(
            new GetServerStatusCommand
            {
                PlexServerConnectionId = command.PlexServerConnectionId,
                ProgressAction = progress =>
                {
                    if (_plexServerConnection is not null)
                    {
                        _ = Task.Run(
                            async () =>
                            {
                                try
                                {
                                    var checkStatusProgress = progress.ToServerConnectionCheckStatusProgress(
                                        _plexServerConnection
                                    );
                                    await _signalRService.SendServerConnectionCheckStatusProgressAsync(
                                        checkStatusProgress
                                    );
                                }
                                catch (Exception ex)
                                {
                                    _log.Here()
                                        .ErrorResult(
                                            ex,
                                            "Error sending server connection check status progress update"
                                        );
                                }
                            },
                            CancellationToken.None
                        );
                    }
                },
            },
            cancellationToken
        );

        if (serverStatusResult.IsFailed)
            return serverStatusResult.LogError();

        // Add plexServer status to DB, the PlexServerStatus table functions as a server log.
        var plexServerStatus = serverStatusResult.Value;

        var upsertResult = await Result.Try(() =>
            _dbContext
                .PlexServerStatuses.Upsert(plexServerStatus)
                .On(x => new { x.PlexServerConnectionId })
                .RunAsync(cancellationToken)
        );

        if (upsertResult.IsFailed)
        {
            return upsertResult.LogError();
        }

        return serverStatusResult.Value;
    }
}
