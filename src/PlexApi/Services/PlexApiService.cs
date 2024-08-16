using Application.Contracts;
using Data.Contracts;
using Logging.Interface;
using PlexApi.Contracts;
using Settings.Contracts;

namespace PlexRipper.PlexApi.Services;

/// <summary>
/// This service is an extra layer of abstraction to convert incoming DTO's from the PlexAPI to workable entities.
/// This was done in order to keep all PlexApi related DTO's in the infrastructure layer.
/// </summary>
public class PlexApiService : IPlexApiService
{
    #region Fields

    private readonly ILog _log;

    private readonly IPlexRipperDbContext _dbContext;
    private readonly IServerSettingsModule _serverSettingsModule;
    private readonly Api.PlexApi _plexApi;

    #endregion

    #region Constructors

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

    #endregion

    #region Methods

    #region Public

    public async Task<Result<List<PlexTvShowEpisode>>> GetAllEpisodesAsync(
        PlexLibrary plexLibrary,
        CancellationToken cancellationToken = default
    )
    {
        var tokenResult = await _dbContext.GetPlexServerTokenAsync(plexLibrary.PlexServerId, cancellationToken);
        if (tokenResult.IsFailed)
            return tokenResult.ToResult();

        var stepSize = 5000;
        var authToken = tokenResult.Value;
        var plexLibraryKey = plexLibrary.Key;

        var plexServerConnection = await _dbContext.ChoosePlexServerConnection(
            plexLibrary.PlexServerId,
            cancellationToken
        );
        if (plexServerConnection.IsFailed)
            return plexServerConnection.ToResult();

        var serverUrl = plexServerConnection.Value.Url;

        var result = await _plexApi.GetAllEpisodesAsync(authToken, serverUrl, plexLibraryKey, 0, stepSize);
        if (result != null)
        {
            var metaData = result.MediaContainer.Metadata;
            var totalSize = result.MediaContainer.TotalSize;
            if (totalSize > stepSize)
            {
                var loops = (int)Math.Ceiling(totalSize / (double)stepSize);

                for (var i = 1; i < loops; i++)
                {
                    var rangeResult = await _plexApi.GetAllEpisodesAsync(
                        authToken,
                        serverUrl,
                        plexLibraryKey,
                        i * stepSize,
                        stepSize
                    );
                    if (rangeResult?.MediaContainer?.Metadata?.Count > 0)
                        metaData.AddRange(rangeResult.MediaContainer.Metadata);
                }

                var success = metaData.Count == totalSize;
            }

            return Result.Ok(metaData.ToPlexTvShowEpisodes());
        }

        return Result.Fail($"Failed to retrieve episodes for library with key {plexLibraryKey}");
    }

    /// <inheritdoc />
    public async Task<Result<List<PlexTvShowSeason>>> GetAllSeasonsAsync(
        PlexLibrary plexLibrary,
        CancellationToken cancellationToken = default
    )
    {
        var tokenResult = await _dbContext.GetPlexServerTokenAsync(plexLibrary.PlexServerId, cancellationToken);
        if (tokenResult.IsFailed)
            return tokenResult.ToResult();

        var plexServerConnection = await _dbContext.ChoosePlexServerConnection(
            plexLibrary.PlexServerId,
            cancellationToken
        );

        if (plexServerConnection.IsFailed)
            return plexServerConnection.ToResult();

        var serverUrl = plexServerConnection.Value.Url;

        var result = await _plexApi.GetAllSeasonsAsync(tokenResult.Value, serverUrl, plexLibrary.Key);
        if (result != null)
            return Result.Ok(result.MediaContainer.Metadata.ToPlexTvShowSeasons());

        return Result.Fail($"Failed to retrieve seasons for library with key {plexLibrary.Key}");
    }

    /// <inheritdoc />
    public async Task<Result<PlexLibrary>> GetLibraryMediaAsync(
        PlexLibrary plexLibrary,
        CancellationToken cancellationToken = default
    )
    {
        var tokenResult = await _dbContext.GetPlexServerTokenAsync(plexLibrary.PlexServerId, cancellationToken);
        if (tokenResult.IsFailed)
            return tokenResult.ToResult();

        var plexServerConnection = await _dbContext.ChoosePlexServerConnection(
            plexLibrary.PlexServerId,
            cancellationToken
        );

        if (plexServerConnection.IsFailed)
            return plexServerConnection.ToResult();

        var serverUrl = plexServerConnection.Value.Url;

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

        // Retrieve the media for this library
        var result = await _plexApi.GetMetadataForLibraryAsync(tokenResult.Value, serverUrl, plexLibrary.Key);

        if (result.IsFailed)
            return result.ToResult().LogError();

        var mediaList = result.Value.MediaContainer.Metadata;

        // Set the TitleSort if it is empty
        foreach (var metadata in mediaList)
            metadata.TitleSort = !string.IsNullOrEmpty(metadata.TitleSort)
                ? metadata.TitleSort
                : metadata.Title.ToSortTitle();

        // Determine how to map based on the Library type.
        switch (result.Value.MediaContainer.ViewGroup)
        {
            case "movie":
                updatedPlexLibrary.Movies = mediaList.ToPlexMovies();
                break;
            case "show":
                updatedPlexLibrary.TvShows = mediaList.ToPlexTvShows();
                break;
        }

        return Result.Ok(updatedPlexLibrary);
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

        var serverUrl = plexServerConnection.Value.Url;
        var plexServer = plexServerConnection.Value.PlexServer;

        if (plexServer is null)
            return ResultExtensions.EntityNotFound(nameof(PlexServer), plexServerId);

        var result = await _plexApi.GetAccessibleLibraryInPlexServerAsync(tokenResult.Value, serverUrl);
        if (result.IsFailed)
        {
            _log.Warning("Plex server with name: {PlexServerName} returned no libraries", plexServer.Name);
            return result.ToResult();
        }

        if (result.Value?.MediaContainer?.Directory is null)
        {
            _log.Error(
                "Plex server: {PlexServerName} returned an empty response when libraries were requested",
                plexServer.Name
            );
            return result.ToResult();
        }

        var directories = result.Value.MediaContainer.Directory;

        var mappedLibraries = directories
            .Select(x => new PlexLibrary
            {
                Id = 0,
                Type = x.Type.ToPlexMediaTypeFromPlexApi(),
                Title = x.Title,
                Key = x.Key,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                ScannedAt = x.ScannedAt,
                SyncedAt = null,
                Uuid = Guid.Parse(x.Uuid),
                MetaData = new PlexLibraryMetaData
                {
                    TvShowCount = 0,
                    TvShowSeasonCount = 0,
                    TvShowEpisodeCount = 0,
                    MovieCount = 0,
                    MediaSize = 0,
                },
                PlexServer = null,
                PlexServerId = 0,
                DefaultDestination = null,
                DefaultDestinationId = null,
                Movies = [],
                TvShows = [],
                PlexAccountLibraries = [],
            })
            .ToList();

        // Ensure every library has the Plex server id set
        foreach (var mappedLibrary in mappedLibraries)
            mappedLibrary.PlexServerId = plexServerId;

        return Result.Ok(mappedLibraries);
    }

    public async Task<Result<List<PlexLibrary>>> GetLibrarySectionByPlexServerAsync(
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

        var serverUrl = plexServerConnection.Value.Url;
        var plexServer = plexServerConnection.Value.PlexServer;

        if (plexServer is null)
            return ResultExtensions.EntityNotFound(nameof(PlexServer), plexServerId);

        var result = await _plexApi.GetServerProviderDataAsync(tokenResult.Value, serverUrl);
        if (result.IsFailed)
        {
            _log.Warning("Plex server with name: {PlexServerName} returned no providers data", plexServer.Name);
            return result.ToResult();
        }

        var mediaProvider = result.Value?.MediaContainer.MediaProvider;
        var directories =
            mediaProvider
                ?.FirstOrDefault(x => x.Title == "Library")
                ?.Feature.FirstOrDefault(x => x.Key == "/library/sections")
                ?.Directory ?? [];

        if (!directories.Any())
        {
            _log.Error(
                "Plex server: {PlexServerName} returned an empty response when libraries were requested",
                plexServer.Name
            );
            return result.ToResult();
        }

        var mappedLibraries = directories
            .Where(x => x.Type?.ToPlexMediaTypeFromPlexApi() != PlexMediaType.Unknown)
            .Select(x => new PlexLibrary
            {
                Id = 0,
                Type = x.Type.ToPlexMediaTypeFromPlexApi(),
                Title = x.Title,
                Key = x.Key,
                CreatedAt = DateTime.MinValue, // TODO See if we can get this data from the API
                UpdatedAt = x.UpdatedAt,
                ScannedAt = x.ScannedAt,
                SyncedAt = null,
                Uuid = Guid.Parse(x.Uuid),
                MetaData = new PlexLibraryMetaData
                {
                    TvShowCount = 0,
                    TvShowSeasonCount = 0,
                    TvShowEpisodeCount = 0,
                    MovieCount = 0,
                    MediaSize = 0,
                },
                PlexServer = null,
                PlexServerId = 0,
                DefaultDestination = null,
                DefaultDestinationId = null,
                Movies = [],
                TvShows = [],
                PlexAccountLibraries = [],
            })
            .ToList();

        // Ensure every library has the Plex server id set
        foreach (var mappedLibrary in mappedLibraries)
            mappedLibrary.PlexServerId = plexServerId;

        return Result.Ok(mappedLibraries);
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
        var serverStatusResult = await _plexApi.GetServerStatusAsync(connection.Url, action);
        if (serverStatusResult.IsFailed)
            return serverStatusResult;

        serverStatusResult.Value.PlexServerId = connection.PlexServerId;
        serverStatusResult.Value.PlexServerConnectionId = connection.Id;
        return serverStatusResult;
    }

    public async Task<List<PlexTvShowSeason>> GetSeasonsAsync(
        string serverAuthToken,
        string plexFullHost,
        PlexTvShow plexTvShow
    )
    {
        var result = await _plexApi.GetSeasonsAsync(serverAuthToken, plexFullHost, plexTvShow.Key);
        return result?.MediaContainer?.Metadata.ToPlexTvShowSeasons() ?? new List<PlexTvShowSeason>();
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
                CreatedAt = DateTime.Parse(x.CreatedAt),
                LastSeenAt = DateTime.Parse(x.LastSeenAt),
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
                        Protocol = y.Protocol,
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

    #region Images

    /// <inheritdoc />
    public async Task<Result<byte[]>> GetPlexMediaImageAsync(
        PlexServer plexServer,
        string thumbPath,
        int width = 0,
        int height = 0,
        CancellationToken cancellationToken = default
    )
    {
        var tokenResult = await _dbContext.GetPlexServerTokenAsync(plexServer.Id, cancellationToken);
        if (tokenResult.IsFailed)
            return tokenResult.ToResult();

        var plexServerConnection = await _dbContext.ChoosePlexServerConnection(plexServer.Id, cancellationToken);
        if (plexServerConnection.IsFailed)
            return plexServerConnection.ToResult();

        return await _plexApi.GetPlexMediaImageAsync(
            plexServerConnection.Value.GetThumbUrl(thumbPath),
            tokenResult.Value,
            width,
            height
        );
    }

    #endregion

    #endregion

    #endregion

    #region Authentication

    #region PlexSignIn

    public async Task<Result<PlexAccount>> PlexSignInAsync(PlexAccount plexAccount)
    {
        var result = await _plexApi.PlexSignInAsync(plexAccount);
        if (result.IsSuccess)
        {
            var x = result.Value;
            var refreshedAccount = new PlexAccount
            {
                Id = plexAccount.Id,
                DisplayName = plexAccount.DisplayName,
                Username = plexAccount.Username,
                Password = plexAccount.Password,
                IsEnabled = plexAccount.IsEnabled,
                IsAuthTokenMode = plexAccount.IsAuthTokenMode,
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
            };

            _log.Information(
                "Successfully retrieved the PlexAccount data for user {PlexAccountDisplayName} from the PlexApi",
                plexAccount.DisplayName
            );
            return Result.Ok(refreshedAccount);
        }

        return result.ToResult();
    }

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

    #endregion
}
