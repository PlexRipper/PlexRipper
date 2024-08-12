using Application.Contracts;
using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using PlexApi.Contracts;

namespace PlexRipper.Application;

/// <summary>
/// Retrieve the latest accessible <see cref="PlexServer">PlexServers</see> for this <see cref="PlexAccount"/> from the PlexAPI and stores it in the Database.
/// </summary>
/// <param name="PlexAccountId">The id of the <see cref="PlexAccount"/> to check.</param>
public record RefreshPlexServerAccessCommand(int PlexAccountId) : IRequest<Result>;

public class RefreshPlexServerAccessCommandValidator : AbstractValidator<RefreshPlexServerAccessCommand>
{
    public RefreshPlexServerAccessCommandValidator()
    {
        RuleFor(x => x.PlexAccountId).GreaterThan(0);
    }
}

public class RefreshPlexServerAccessCommandHandler : IRequestHandler<RefreshPlexServerAccessCommand, Result>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IMediator _mediator;
    private readonly IPlexApiService _plexServiceApi;
    private readonly ISignalRService _signalRService;

    public RefreshPlexServerAccessCommandHandler(
        ILog log,
        IPlexRipperDbContext dbContext,
        IMediator mediator,
        IPlexApiService plexServiceApi,
        ISignalRService signalRService
    )
    {
        _log = log;
        _dbContext = dbContext;
        _mediator = mediator;
        _plexServiceApi = plexServiceApi;
        _signalRService = signalRService;
    }

    public async Task<Result> Handle(RefreshPlexServerAccessCommand command, CancellationToken cancellationToken)
    {
        var plexAccountId = command.PlexAccountId;

        var plexAccountDisplayName = await _dbContext.GetPlexAccountDisplayName(plexAccountId, cancellationToken);

        _log.Debug("Refreshing Plex servers for PlexAccount: {PlexAccountId}", plexAccountId);

        var tupleResult = await _plexServiceApi.GetAccessiblePlexServersAsync(plexAccountId);
        var serversResult = tupleResult.servers;
        var tokensResult = tupleResult.tokens;

        if (serversResult.IsFailed || tokensResult.IsFailed)
            return serversResult.LogError();

        if (!serversResult.Value.Any())
        {
            _log.Warning("No Plex servers found for PlexAccount: {PlexAccountId}", plexAccountId);
            return Result.Ok();
        }

        var serverList = serversResult.Value;
        var serverAccessTokens = tokensResult.Value;

        // Add PlexServers and their PlexServerConnections
        var updateResult = await _mediator.Send(new AddOrUpdatePlexServersCommand(serverList), cancellationToken);
        if (updateResult.IsFailed)
            return updateResult.LogError();

        // Add or update the PlexAccount and PlexServer relationships
        var plexAccountTokensResult = await _mediator.Send(
            new AddOrUpdatePlexAccountServersCommand(plexAccountId, serverAccessTokens),
            cancellationToken
        );

        if (plexAccountTokensResult.IsFailed)
            return plexAccountTokensResult.LogError();

        // Send notifications to the client to refresh the PlexServerConnection data
        await _signalRService.SendRefreshNotificationAsync(
            [DataType.PlexAccount, DataType.PlexServer, DataType.PlexServerConnection],
            cancellationToken
        );

        _log.Information(
            "Successfully refreshed accessible Plex servers for account {PlexAccountDisplayName}",
            plexAccountDisplayName
        );

        return Result.Ok();
    }
}
