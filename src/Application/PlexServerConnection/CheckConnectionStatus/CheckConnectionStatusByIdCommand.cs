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
    private PlexServerConnection _plexServerConnection;

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
        _plexServerConnection = await _dbContext.PlexServerConnections.GetAsync(
            command.PlexServerConnectionId,
            cancellationToken
        );

        // Request status
        var serverStatusResult = await _plexApiService.GetPlexServerStatusAsync(command.PlexServerConnectionId, Action);
        if (serverStatusResult.IsFailed)
            return serverStatusResult.LogError();

        // Add plexServer status to DB, the PlexServerStatus table functions as a server log.
        await CreateStatus(serverStatusResult.Value, cancellationToken);

        await StatusTrim(_plexServerConnection.PlexServerId, cancellationToken);

        return serverStatusResult.Value;
    }

    /// <summary>
    ///  The call-back action from the httpClient
    /// </summary>
    /// <param name="progress"></param>
    private async void Action(PlexApiClientProgress progress)
    {
        var checkStatusProgress = progress.ToServerConnectionCheckStatusProgress();
        checkStatusProgress.PlexServerConnection = _plexServerConnection;
        await _signalRService.SendServerConnectionCheckStatusProgressAsync(checkStatusProgress);
    }

    private async Task CreateStatus(PlexServerStatus plexServerStatus, CancellationToken cancellationToken)
    {
        _log.Debug(
            "Creating a new PlexServerStatus {@PlexServerStatus} in the database for {ServerId} ",
            plexServerStatus,
            plexServerStatus.PlexServerId
        );

        plexServerStatus.PlexServer = null;
        plexServerStatus.PlexServerConnection = null;
        await _dbContext.PlexServerStatuses.AddAsync(plexServerStatus, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Result> StatusTrim(int plexServerId, CancellationToken cancellationToken)
    {
        if (plexServerId > 0)
            return ResultExtensions.IsInvalidId(nameof(plexServerId), plexServerId);

        var serverStatusList = await _dbContext
            .PlexServerStatuses.AsTracking()
            .Where(x => x.PlexServerId == plexServerId)
            .ToListAsync(cancellationToken);

        if (serverStatusList.Count > 100)
        {
            // All server status are stored chronologically, which means instead of sorting by LastChecked we can do sort by Id.
            serverStatusList = serverStatusList.OrderByDescending(x => x.Id).ToList();
            _dbContext.PlexServerStatuses.RemoveRange(serverStatusList.Skip(100));
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return Result.Ok();
    }
}
