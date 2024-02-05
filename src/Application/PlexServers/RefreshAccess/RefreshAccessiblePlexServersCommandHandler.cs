using Application.Contracts;
using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using PlexApi.Contracts;
using Settings.Contracts;

namespace PlexRipper.Application;

/// <summary>
/// Retrieves the latest accessible <see cref="PlexServer">PlexServers</see> for this <see cref="PlexAccount"/> from the PlexAPI and stores it in the Database.
/// </summary>
/// <param name="plexAccountId">The id of the <see cref="PlexAccount"/> to check.</param>
public record RefreshAccessiblePlexServersCommand(int plexAccountId) : IRequest<Result<List<PlexServer>>>;

public class RefreshAccessiblePlexServersValidator : AbstractValidator<RefreshAccessiblePlexServersCommand>
{
    public RefreshAccessiblePlexServersValidator()
    {
        RuleFor(x => x).NotNull();
    }
}

public class RefreshAccessiblePlexServersCommandHandler : IRequestHandler<RefreshAccessiblePlexServersCommand, Result<List<PlexServer>>>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IServerSettingsModule _serverSettingsModule;
    private readonly IAddOrUpdatePlexServersCommand _addOrUpdatePlexServersCommand;
    private readonly IPlexApiService _plexServiceApi;

    public RefreshAccessiblePlexServersCommandHandler(
        ILog log,
        IPlexRipperDbContext dbContext,
        IServerSettingsModule serverSettingsModule,
        IAddOrUpdatePlexServersCommand addOrUpdatePlexServersCommand,
        IPlexApiService plexServiceApi
    )
    {
        _log = log;
        _dbContext = dbContext;
        _serverSettingsModule = serverSettingsModule;
        _addOrUpdatePlexServersCommand = addOrUpdatePlexServersCommand;
        _plexServiceApi = plexServiceApi;
    }

    public async Task<Result<List<PlexServer>>> Handle(RefreshAccessiblePlexServersCommand request, CancellationToken cancellationToken)
    {
        var plexAccountId = request.plexAccountId;
        if (plexAccountId <= 0)
            return ResultExtensions.IsInvalidId(nameof(plexAccountId)).LogWarning();

        _log.Debug("Refreshing Plex servers for PlexAccount: {PlexAccountId}", plexAccountId);
        var tupleResult = await _plexServiceApi.GetAccessiblePlexServersAsync(plexAccountId);
        var serversResult = tupleResult.servers;
        var tokensResult = tupleResult.tokens;

        if (serversResult.IsFailed || tokensResult.IsFailed)
            return serversResult;

        if (!serversResult.Value.Any())
            return Result.Ok(new List<PlexServer>());

        var serverList = serversResult.Value;
        var serverAccessTokens = tokensResult.Value;

        // Add PlexServers and their PlexServerConnections
        var updateResult = await _addOrUpdatePlexServersCommand.ExecuteAsync(serverList, cancellationToken);
        if (updateResult.IsFailed)
            return updateResult;

        // Check if every server has a settings entry
        _serverSettingsModule.EnsureAllServersHaveASettingsEntry(serverList);

        var plexAccountResult = await _mediator.Send(new GetPlexAccountByIdQuery(plexAccountId));
        if (plexAccountResult.IsFailed)
            return plexAccountResult.ToResult();

        var plexAccountTokensResult = await _mediator.Send(new AddOrUpdatePlexAccountServersCommand(plexAccountResult.Value, serverAccessTokens));
        if (plexAccountTokensResult.IsFailed)
            return plexAccountTokensResult;

        _log.Information("Successfully refreshed accessible Plex servers for account {PlexAccountDisplayName}", plexAccountResult.Value.DisplayName);
        return await _mediator.Send(new GetAllPlexServersByPlexAccountIdQuery(plexAccountId));
    }
}