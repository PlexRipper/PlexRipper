using Application.Contracts;
using Data.Contracts;
using FluentValidation;
using Logging.Interface;
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
    private readonly ILog _log;
    private readonly ISignalRService _signalRService;
    private readonly IPlexApiService _plexApiService;
    private readonly IPlexRipperDbContext _dbContext;
    private PlexServerConnection? _plexServerConnection;

    public CheckConnectionStatusByIdCommandHandler(
        ILog log,
        IPlexRipperDbContext dbContext,
        ISignalRService signalRService,
        IPlexApiService plexApiService
    )
    {
        _log = log;
        _dbContext = dbContext;
        _signalRService = signalRService;
        _plexApiService = plexApiService;
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
        var serverStatusResult = await _plexApiService.GetPlexServerStatusAsync(command.PlexServerConnectionId, Action);
        if (serverStatusResult.IsFailed)
            return serverStatusResult.LogError();

        // Add plexServer status to DB, the PlexServerStatus table functions as a server log.
        var plexServerStatus = serverStatusResult.Value;
        _log.Debug(
            "Creating a new PlexServerStatus {@PlexServerStatus} in the database for {ServerId} ",
            plexServerStatus,
            plexServerStatus.PlexServerId
        );

        await _dbContext
            .PlexServerStatuses.Upsert(plexServerStatus)
            .On(x => new { x.PlexServerConnectionId })
            .RunAsync(cancellationToken);

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
