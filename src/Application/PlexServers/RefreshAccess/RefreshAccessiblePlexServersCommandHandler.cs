using Application.Contracts;
using AutoMapper;
using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using PlexApi.Contracts;
using Settings.Contracts;

namespace PlexRipper.Application;

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
    private readonly IMapper _mapper;
    private readonly IServerSettingsModule _serverSettingsModule;
    private readonly IAddOrUpdatePlexServersCommand _addOrUpdatePlexServersCommand;
    private readonly IAddOrUpdatePlexAccountServersCommand _addOrUpdatePlexAccountServersCommand;
    private readonly IPlexApiService _plexServiceApi;

    public RefreshAccessiblePlexServersCommandHandler(
        ILog log,
        IPlexRipperDbContext dbContext,
        IMapper mapper,
        IServerSettingsModule serverSettingsModule,
        IAddOrUpdatePlexServersCommand addOrUpdatePlexServersCommand,
        IAddOrUpdatePlexAccountServersCommand addOrUpdatePlexAccountServersCommand,
        IPlexApiService plexServiceApi
    )
    {
        _log = log;
        _dbContext = dbContext;
        _mapper = mapper;
        _serverSettingsModule = serverSettingsModule;
        _addOrUpdatePlexServersCommand = addOrUpdatePlexServersCommand;
        _addOrUpdatePlexAccountServersCommand = addOrUpdatePlexAccountServersCommand;
        _plexServiceApi = plexServiceApi;
    }

    public async Task<Result<List<PlexServer>>> Handle(RefreshAccessiblePlexServersCommand request, CancellationToken cancellationToken)
    {
        var plexAccountId = request.plexAccountId;
        if (plexAccountId <= 0)
            return ResultExtensions.IsInvalidId(nameof(plexAccountId)).LogWarning();

        var plexAccount = await _dbContext.PlexAccounts.FindAsync(plexAccountId, cancellationToken);
        if (plexAccount is null)
            return ResultExtensions.EntityNotFound(nameof(PlexAccount), plexAccountId);

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

        // Add or update the PlexAccount and PlexServer relationships
        var plexAccountTokensResult = await _addOrUpdatePlexAccountServersCommand.ExecuteAsync(plexAccountId, serverAccessTokens, cancellationToken);
        if (plexAccountTokensResult.IsFailed)
            return plexAccountTokensResult;

        _log.Information("Successfully refreshed accessible Plex servers for account {PlexAccountDisplayName}", plexAccount.DisplayName);

        return await _dbContext.GetAllPlexServersByPlexAccountIdQuery(_mapper, plexAccountId, cancellationToken);
    }
}