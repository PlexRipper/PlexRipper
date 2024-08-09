using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using PlexApi.Contracts;

namespace PlexRipper.Application;

/// <summary>
/// Queue a job to retrieves the latest accessible <see cref="PlexServer">PlexServers</see> for this <see cref="PlexAccount"/> from the PlexAPI and stores it in the Database.
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

public class RefreshPlexServerAccessHandler : IRequestHandler<RefreshPlexServerAccessCommand, Result>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IMediator _mediator;
    private readonly IPlexApiService _plexServiceApi;

    public RefreshPlexServerAccessHandler(
        ILog log,
        IPlexRipperDbContext dbContext,
        IMediator mediator,
        IPlexApiService plexServiceApi
    )
    {
        _log = log;
        _dbContext = dbContext;
        _mediator = mediator;
        _plexServiceApi = plexServiceApi;
    }

    public async Task<Result> Handle(RefreshPlexServerAccessCommand command, CancellationToken cancellationToken)
    {
        var plexAccountId = command.PlexAccountId;

        var plexAccount = await _dbContext.PlexAccounts.GetAsync(plexAccountId, cancellationToken);
        if (plexAccount is null)
            return ResultExtensions.EntityNotFound(nameof(PlexAccount), plexAccountId).LogError();

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

        _log.Information(
            "Successfully refreshed accessible Plex servers for account {PlexAccountDisplayName}",
            plexAccount.DisplayName
        );

        return Result.Ok();
    }
}
