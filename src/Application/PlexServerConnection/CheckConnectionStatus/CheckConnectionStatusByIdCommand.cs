using Application.Contracts;
using Data.Contracts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PlexApi.Contracts;

namespace PlexRipper.Application;

public record CheckConnectionStatusByIdCommand(int PlexServerConnectionId) : IRequest<Result<PlexServerStatus>>;

public class CheckConnectionStatusByIdCommandValidator : AbstractValidator<CheckConnectionStatusByIdCommand>
{
    public CheckConnectionStatusByIdCommandValidator()
    {
        RuleFor(x => x.PlexServerConnectionId).GreaterThan(0);
    }
}

public class CheckConnectionStatusByIdCommandHandler
    : IRequestHandler<CheckConnectionStatusByIdCommand, Result<PlexServerStatus>>
{
    private readonly ISignalRService _signalRService;
    private readonly ICommandExecutor _commandDispatcher;
    private readonly IPlexRipperDbContext _dbContext;
    private PlexServerConnection? _plexServerConnection;

    public CheckConnectionStatusByIdCommandHandler(
        IPlexRipperDbContext dbContext,
        ISignalRService signalRService,
        ICommandExecutor commandDispatcher
    )
    {
        _dbContext = dbContext;
        _signalRService = signalRService;
        _commandDispatcher = commandDispatcher;
    }

    public async Task<Result<PlexServerStatus>> Handle(
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
                ProgressAction = Action,
            },
            cancellationToken
        );

        if (serverStatusResult.IsFailed)
            return serverStatusResult.LogError();

        // Add plexServer status to DB, the PlexServerStatus table functions as a server log.
        var plexServerStatus = serverStatusResult.Value;

        var upsertResult = await Result.Try(
            () =>
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

    /// <summary>
    ///  The call-back action from the httpClient
    /// </summary>
    /// <param name="progress"></param>
    private async void Action(PlexApiClientProgress progress)
    {
        if (_plexServerConnection is not null)
        {
            var checkStatusProgress = progress.ToServerConnectionCheckStatusProgress(_plexServerConnection);
            await _signalRService.SendServerConnectionCheckStatusProgressAsync(checkStatusProgress);
        }
    }
}
