using Application.Contracts;
using Data.Contracts;
using Logging.Interface;
using LukeHagar.PlexAPI.SDK.Models.Requests;
using PlexApi.Contracts;
using Settings.Contracts;
using Type = LukeHagar.PlexAPI.SDK.Models.Requests.Type;

namespace PlexRipper.PlexApi.Services;

/// <summary>
/// This service is an extra layer of abstraction to convert incoming DTO's from the PlexAPI to workable entities.
/// This was done in order to keep all PlexApi related DTO's in the infrastructure layer.
/// </summary>
public class PlexApiService : IPlexApiService
{
    private readonly ILog _log;

    private readonly IPlexRipperDbContext _dbContext;
    private readonly IServerSettingsModule _serverSettingsModule;
    private readonly Api.PlexApi _plexApi;

    public PlexApiService(
        ILog log,
        IPlexRipperDbContext dbContext,
        IServerSettingsModule serverSettingsModule,
        Api.PlexApi plexApi
    )
    {
        _log = log;
        _plexApi = plexApi;
        _dbContext = dbContext;
        _serverSettingsModule = serverSettingsModule;
    }

    #region Methods

    #region Public

    /// <inheritdoc />
    public async Task<Result<PlexLibrary>> GetLibraryMediaAsync(
        PlexLibrary plexLibrary,
        CancellationToken cancellationToken = default
    )
    {
        // Retrieve updated version of the PlexLibrary
        var plexLibraries = await GetLibrarySectionsAsync(
            plexLibrary.PlexServerId,
            cancellationToken: cancellationToken
        );

        if (plexLibraries.IsFailed)
            return plexLibraries.ToResult();

        var updatedPlexLibrary = plexLibraries.Value.Find(x => x.Key == plexLibrary.Key);
        if (updatedPlexLibrary is null)
            return ResultExtensions.IsNull(nameof(updatedPlexLibrary));

        updatedPlexLibrary.Id = plexLibrary.Id;
        updatedPlexLibrary.PlexServerId = plexLibrary.PlexServerId;

        var mediaListResult = await SyncMedia(plexLibrary, cancellationToken: cancellationToken);

        if (mediaListResult.IsFailed)
            return mediaListResult.ToResult();

        var mediaList = mediaListResult.Value;

        // Determine how to map based on the Library type.
        switch (updatedPlexLibrary.Type)
        {
            case PlexMediaType.Movie:
                updatedPlexLibrary.Movies = mediaList.ToPlexMovies();
                break;
            case PlexMediaType.TvShow:
                updatedPlexLibrary.TvShows = mediaList.ToPlexTvShows();
                break;
        }

        return Result.Ok(updatedPlexLibrary);
    }

    /// <inheritdoc />
    public async Task<Result<List<PlexTvShowSeason>>> GetAllSeasonsAsync(
        PlexLibrary plexLibrary,
        CancellationToken cancellationToken = default
    )
    {
        var mediaListResult = await SyncMedia(plexLibrary, Type.Season, cancellationToken: cancellationToken);

        if (mediaListResult.IsFailed)
            return mediaListResult.ToResult();

        var mediaList = mediaListResult.Value.ToPlexTvShowSeasons();
        return Result.Ok(mediaList);
    }

    public async Task<Result<List<PlexTvShowEpisode>>> GetAllEpisodesAsync(
        PlexLibrary plexLibrary,
        CancellationToken cancellationToken = default
    )
    {
        var mediaListResult = await SyncMedia(plexLibrary, Type.Episode, 5000, cancellationToken);

        if (mediaListResult.IsFailed)
            return mediaListResult.ToResult();

        var mediaList = mediaListResult.Value.ToPlexTvShowEpisodes();
        return Result.Ok(mediaList);
    }

    /// <inheritdoc />
    public async Task<Result<List<PlexLibrary>>> GetLibrarySectionsAsync(
        int plexServerId,
        int plexAccountId = 0,
        CancellationToken cancellationToken = default
    )
    {
        var tokenResult = await _dbContext.GetPlexServerTokenAsync(plexServerId, plexAccountId, cancellationToken);
        if (tokenResult.IsFailed)
            return tokenResult.ToResult();

        var plexServerConnection = await _dbContext.ChoosePlexServerConnection(plexServerId, cancellationToken);
        if (plexServerConnection.IsFailed)
            return plexServerConnection.ToResult();

        var plexServer = plexServerConnection.Value.PlexServer;

        if (plexServer is null)
            return ResultExtensions.EntityNotFound(nameof(PlexServer), plexServerId);

        return await _plexApi.GetAccessibleLibraryInPlexServerAsync(tokenResult.Value, plexServerConnection.Value);
    }

    /// <inheritdoc />
    public async Task<Result<PlexServerStatus>> GetPlexServerStatusAsync(
        int plexServerConnectionId,
        Action<PlexApiClientProgress>? action = null
    )
    {
        var connection = await _dbContext.PlexServerConnections.GetAsync(plexServerConnectionId);
        if (connection is null)
            return ResultExtensions.EntityNotFound(nameof(PlexServerConnection), plexServerConnectionId);

        var plexServerName = await _dbContext.GetPlexServerNameById(connection.PlexServerId);
        _log.Debug("Getting PlexServerStatus for server: {PlexServerName}", plexServerName);

        return await _plexApi.GetServerStatusAsync(connection, action);
    }

    /// <inheritdoc />
    public async Task<(
        Result<List<PlexServer>> servers,
        Result<List<ServerAccessTokenDTO>> tokens
    )> GetAccessiblePlexServersAsync(int plexAccountId)
    {
        var plexAccount = await _dbContext.PlexAccounts.GetAsync(plexAccountId);
        if (plexAccount is null)
        {
            var failedResult = ResultExtensions.EntityNotFound(nameof(PlexAccount), plexAccountId);
            return (failedResult, failedResult);
        }

        var plexAccountToken = await GetPlexApiTokenAsync(plexAccount);
        if (plexAccountToken.IsFailed)
            return (plexAccountToken.ToResult(), plexAccountToken.ToResult());

        var result = await _plexApi.GetAccessibleServers(plexAccountToken.Value);
        if (result.IsFailed)
        {
            _log.Warning(
                "Failed to retrieve PlexServers for PlexAccount: {PlexAccountDisplayName}",
                plexAccount.DisplayName
            );
            return (result.ToResult(), result.ToResult());
        }

        var plexServers = result
            .Value.FindAll(x => x.Provides.Contains("server"))
            .Select(x => new PlexServer
            {
                Id = 0,
                Name = x.Name,

                // The servers have an OwnerId of 0 when it belongs to the PlexAccount that was used to request it.
                OwnerId = x.OwnerId ?? plexAccount.PlexId,
                PlexServerOwnerUsername = x.SourceTitle ?? plexAccount.Username,
                Device = x.Device ?? string.Empty,
                Platform = x.Platform ?? string.Empty,
                PlatformVersion = x.PlatformVersion ?? string.Empty,
                Product = x.Product,
                ProductVersion = x.ProductVersion,
                Provides = x.Provides,
                CreatedAt = x.CreatedAt,
                LastSeenAt = x.LastSeenAt,
                MachineIdentifier = x.ClientIdentifier,
                PublicAddress = x.PublicAddress,
                PreferredConnectionId = 0,
                IsEnabled = !_serverSettingsModule.GetIsHidden(x.ClientIdentifier),
                Owned = x.Owned,
                Home = x.Home,
                Synced = x.Synced,
                Relay = x.Relay,
                Presence = x.Presence,
                HttpsRequired = x.HttpsRequired,
                PublicAddressMatches = x.PublicAddressMatches,
                DnsRebindingProtection = x.DnsRebindingProtection,
                NatLoopbackSupported = x.NatLoopbackSupported,
                ServerFixApplyDNSFix = false,
                PlexAccountServers = [],
                PlexLibraries = [],
                ServerStatus = [],
                PlexServerConnections = x
                    .Connections.Select(y => new PlexServerConnection
                    {
                        Id = 0,
                        Protocol = y.Protocol.ToString(),
                        Address = y.Address,
                        Port = y.Port,
                        Local = y.Local,
                        Relay = y.Relay,
                        IPv4 = y.Address.IsIpAddress() && !y.IPv6,
                        IPv6 = y.IPv6,
                        Uri = y.Uri,

                        // The port fix is when we don't want to use the port when Address is a domain name
                        PortFix = !y.Address.IsIpAddress() && !y.IPv6 && y.Address != "localhost",
                        PlexServer = null,
                        PlexServerId = 0,
                        PlexServerStatus = [],
                    })
                    .ToList(),
            })
            .ToList();

        var mapAccess = result
            .Value.Select(x => new ServerAccessTokenDTO
            {
                PlexAccountId = plexAccountId,
                MachineIdentifier = x.ClientIdentifier,
                AccessToken = x.AccessToken,
            })
            .ToList();

        return (Result.Ok(plexServers), Result.Ok(mapAccess));
    }

    #endregion

    #endregion

    #region Authentication

    #region PlexSignIn

    public Task<Result<PlexAccount>> PlexSignInAsync(PlexAccount plexAccount) => _plexApi.PlexSignInAsync(plexAccount);

    public Task<Result<AuthPin>> Get2FAPin(string clientId) => _plexApi.Get2FAPin(clientId);

    public Task<Result<AuthPin>> Check2FAPin(int pinId, string clientId) => _plexApi.Check2FAPin(pinId, clientId);

    public async Task<Result<PlexAccount>> ValidatePlexToken(PlexAccount plexAccount)
    {
        var result = await _plexApi.ValidatePlexToken(plexAccount.AuthenticationToken);

        if (result.IsSuccess)
        {
            var x = result.Value;
            var refreshedAccount = new PlexAccount
            {
                Id = x.Id,
                DisplayName = plexAccount.DisplayName,
                Username = !string.IsNullOrEmpty(x.Email) ? x.Email : x.Username,
                Password = plexAccount.Password,
                IsEnabled = plexAccount.IsEnabled,
                IsValidated = true,
                ValidatedAt = DateTime.UtcNow,
                PlexId = x.Id,
                Uuid = x.Uuid,
                ClientId = plexAccount.ClientId,
                Title = x.Title,
                Email = x.Email,
                HasPassword = x.HasPassword,
                AuthenticationToken = x.AuthToken,
                IsMain = plexAccount.IsMain,
                PlexAccountServers = [],
                PlexAccountLibraries = [],
                Is2Fa = x.TwoFactorEnabled,
                VerificationCode = string.Empty,
                IsAuthTokenMode = plexAccount.IsAuthTokenMode,
            };

            _log.Information(
                "Successfully validated the PlexAccount Authentication Token for user {PlexAccountDisplayName} from the PlexApi",
                plexAccount.DisplayName
            );
            return Result.Ok(refreshedAccount);
        }

        return result.ToResult();
    }

    #endregion

    private async Task<Result<string>> GetPlexApiTokenAsync(PlexAccount plexAccount)
    {
        if (plexAccount == null)
            return ResultExtensions.IsNull(nameof(plexAccount));

        if (plexAccount.AuthenticationToken != string.Empty)
        {
            // TODO Make the token refresh limit configurable
            if ((plexAccount.ValidatedAt - DateTime.UtcNow).TotalDays < 30)
            {
                _log.InformationLine("Plex AuthToken was still valid, using from local DB");
                return plexAccount.AuthenticationToken;
            }

            _log.InformationLine("Plex AuthToken has expired, refreshing Plex AuthToken now");

            // TODO Account for 2FA
            return await _plexApi.RefreshPlexAuthTokenAsync(plexAccount);
        }

        return Result.Fail($"PlexAccount with Id: {plexAccount.Id} contained an empty AuthToken!").LogError();
    }

    private async Task<Result<List<GetLibraryItemsMetadata>>> SyncMedia(
        PlexLibrary plexLibrary,
        Type? plexType = null,
        int batchSize = 500,
        CancellationToken cancellationToken = default
    )
    {
        var tokenResult = await _dbContext.GetPlexServerTokenAsync(plexLibrary.PlexServerId, cancellationToken);
        if (tokenResult.IsFailed)
            return tokenResult.ToResult();

        var plexServerConnectionResult = await _dbContext.ChoosePlexServerConnection(
            plexLibrary.PlexServerId,
            cancellationToken
        );

        if (plexServerConnectionResult.IsFailed)
            return plexServerConnectionResult.ToResult();

        var plexServerConnection = plexServerConnectionResult.Value;

        var mediaList = new List<GetLibraryItemsMetadata>();

        plexType ??= plexLibrary.Type switch
        {
            PlexMediaType.Movie => Type.Movie,
            PlexMediaType.TvShow => Type.TvShow,
            var _ => null,
        };

        var index = 0;
        while (true)
        {
            // Retrieve the media for this library
            var result = await _plexApi.GetMetadataForLibraryAsync(
                plexServerConnection,
                tokenResult.Value,
                plexLibrary.Key,
                index,
                batchSize,
                plexType
            );

            if (result.IsFailed)
                return result.ToResult().LogError();

            var mediaContainer = result.Value;
            mediaList.AddRange(mediaContainer.Metadata);

            var totalSize = mediaContainer.TotalSize;
            index += mediaContainer.Size;

            // If the size is less than the batch size, we have reached the end
            if (mediaContainer.Size < batchSize)
                break;

            if (index >= totalSize)
                break;
        }

        if (!mediaList.Any())
            return ResultExtensions.IsEmpty(nameof(mediaList));

        // Set the TitleSort if it is empty
        foreach (var metadata in mediaList)
            metadata.TitleSort = !string.IsNullOrEmpty(metadata.TitleSort)
                ? metadata.TitleSort
                : metadata.Title.ToSortTitle();

        return Result.Ok(mediaList);
    }

    #endregion
}
