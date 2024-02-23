using Application.Contracts;
using AutoMapper;
using Data.Contracts;
using Logging.Interface;
using PlexApi.Contracts;
using Quartz;
using Settings.Contracts;

namespace PlexRipper.Application;

public class RefreshPlexServersAccessJob : IJob
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IServerSettingsModule _serverSettingsModule;
    private readonly IAddOrUpdatePlexServersCommand _addOrUpdatePlexServersCommand;
    private readonly IAddOrUpdatePlexAccountServersCommand _addOrUpdatePlexAccountServersCommand;
    private readonly IPlexApiService _plexServiceApi;

    public static string PlexAccountIdParameter => "plexAccountId";

    public static JobKey GetJobKey(int id)
    {
        return new JobKey($"{PlexAccountIdParameter}_{id}", nameof(RefreshPlexServersAccessJob));
    }

    public RefreshPlexServersAccessJob(
        ILog log,
        IPlexRipperDbContext dbContext,
        IMapper mapper,
        IServerSettingsModule serverSettingsModule,
        IAddOrUpdatePlexServersCommand addOrUpdatePlexServersCommand,
        IAddOrUpdatePlexAccountServersCommand addOrUpdatePlexAccountServersCommand,
        IPlexApiService plexServiceApi)
    {
        _log = log;
        _dbContext = dbContext;
        _mapper = mapper;
        _serverSettingsModule = serverSettingsModule;
        _addOrUpdatePlexServersCommand = addOrUpdatePlexServersCommand;
        _addOrUpdatePlexAccountServersCommand = addOrUpdatePlexAccountServersCommand;
        _plexServiceApi = plexServiceApi;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var dataMap = context.JobDetail.JobDataMap;
        var plexAccountId = dataMap.GetIntValue(PlexAccountIdParameter);
        var cancellationToken = context.CancellationToken;

        _log.Debug("Executing job: {NameOfRefreshAccessiblePlexServersJob)} for {NameOfPlexAccountId)} with id: {PlexAccountId}",
            nameof(RefreshPlexServersAccessJob),
            nameof(plexAccountId), plexAccountId);

        // Jobs should swallow exceptions as otherwise Quartz will keep re-executing it
        // https://www.quartz-scheduler.net/documentation/best-practices.html#throwing-exceptions
        try
        {
            var plexAccount = await _dbContext.PlexAccounts.GetAsync(plexAccountId, cancellationToken);
            if (plexAccount is null)
            {
                ResultExtensions.EntityNotFound(nameof(PlexAccount), plexAccountId).LogError();
                return;
            }

            _log.Debug("Refreshing Plex servers for PlexAccount: {PlexAccountId}", plexAccountId);

            var tupleResult = await _plexServiceApi.GetAccessiblePlexServersAsync(plexAccountId);
            var serversResult = tupleResult.servers;
            var tokensResult = tupleResult.tokens;

            if (serversResult.IsFailed || tokensResult.IsFailed)
            {
                serversResult.LogError();
                return;
            }

            if (!serversResult.Value.Any())
            {
                _log.Warning("No Plex servers found for PlexAccount: {PlexAccountId}", plexAccountId);
                return;
            }

            var serverList = serversResult.Value;
            var serverAccessTokens = tokensResult.Value;

            // Add PlexServers and their PlexServerConnections
            var updateResult = await _addOrUpdatePlexServersCommand.ExecuteAsync(serverList, cancellationToken);
            if (updateResult.IsFailed)
            {
                updateResult.LogError();
                return;
            }

            // Check if every server has a settings entry
            _serverSettingsModule.EnsureAllServersHaveASettingsEntry(serverList);

            // Add or update the PlexAccount and PlexServer relationships
            var plexAccountTokensResult = await _addOrUpdatePlexAccountServersCommand.ExecuteAsync(plexAccountId, serverAccessTokens, cancellationToken);
            if (plexAccountTokensResult.IsFailed)
            {
                plexAccountTokensResult.LogError();
                return;
            }

            _log.Information("Successfully refreshed accessible Plex servers for account {PlexAccountDisplayName}", plexAccount.DisplayName);

            await _dbContext.GetAllPlexServersByPlexAccountIdQuery(_mapper, plexAccountId, cancellationToken);
        }
        catch (Exception e)
        {
            _log.Error(e);
        }
    }
}