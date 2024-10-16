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

        var result = await _plexServiceApi.GetAccessiblePlexServersAsync(plexAccountId);

        if (result.Has401UnauthorizedError())
        {
            _log.Warning(
                "Plex API returned 401 Unauthorized for PlexAccount: {PlexAccountDisplayName}",
                plexAccountDisplayName
            );
            _log.Warning(
                "Removing PlexServerAccess and LibraryAccess for PlexAccount: {PlexAccountDisplayName}",
                plexAccountDisplayName
            );

            // TODO: Remove PlexServerAccess and LibraryAccess for this PlexAccount
        }

        if (result.IsFailed)
            return result.LogError();

        if (!result.Value.Any())
        {
            _log.Warning("No Plex servers found for PlexAccount: {plexAccountName}", plexAccountDisplayName);
            return Result.Ok();
        }

        var serverList = result.Value.Select(x => x.PlexServer).ToList();
        var serverAccessTokens = result.Value.Select(x => x.AccessToken).ToList();

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
