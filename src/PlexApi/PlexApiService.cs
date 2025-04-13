using Application.Contracts;
using Data.Contracts;
using Logging.Interface;
using PlexApi.Contracts;
using Settings.Contracts;

namespace PlexRipper.PlexApi;

/// <summary>
/// This service is an extra layer of abstraction to convert incoming DTO's from the PlexAPI to workable entities.
/// This was done in order to keep all PlexApi related DTO's in the infrastructure layer.
/// </summary>
public class PlexApiService : IPlexApiService
{
    private readonly ILog _log;

    private readonly IPlexRipperDbContext _dbContext;
    private readonly IServerSettingsModule _serverSettingsModule;
    private readonly IPlexApiMediaService _plexApiMediaService;
    private readonly PlexApiWrapper _plexApiWrapper;

    public PlexApiService(
        ILog log,
        IPlexRipperDbContext dbContext,
        IServerSettingsModule serverSettingsModule,
        IPlexApiMediaService plexApiMediaService,
        PlexApiWrapper plexApiWrapper
    )
    {
        _log = log;
        _plexApiWrapper = plexApiWrapper;
        _dbContext = dbContext;
        _serverSettingsModule = serverSettingsModule;
        _plexApiMediaService = plexApiMediaService;
    }

    /// <inheritdoc />
    public async Task<Result<LibraryMetadata>> GetLibraryMediaAsync(
        PlexLibrary plexLibrary,
        Action<MediaSyncProgress>? action = null,
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

        // Set the default folder path id for the destination
        updatedPlexLibrary.DefaultDestinationId = updatedPlexLibrary.Type.ToDefaultDestinationFolderId();

        var mediaListResult = await _plexApiMediaService.SyncMedia(
            plexLibrary,
            plexLibrary.Type,
            action: action,
            cancellationToken: cancellationToken
        );

        if (mediaListResult.IsFailed)
            return mediaListResult.ToResult();

        // Pre sort the media list
        var mediaList = mediaListResult.Value.OrderByNatural(x => x.TitleSort).ToList();

        // Determine how to map based on the Library type.
        switch (updatedPlexLibrary.Type)
        {
            case PlexMediaType.Movie:
                updatedPlexLibrary.Movies.AddRange(mediaList.ToPlexMovies());
                break;
            case PlexMediaType.TvShow:
                updatedPlexLibrary.TvShows.AddRange(mediaList.ToPlexTvShows());
                break;
            default:
                return Result.Fail($"Unknown PlexLibrary type: {updatedPlexLibrary.Type}").LogError();
        }

        return Result.Ok(
            new LibraryMetadata
            {
                Library = updatedPlexLibrary,
                Countries = mediaList.ToUniquePlexCountry(),
                Genres = mediaList.ToUniquePlexGenre(),
                Roles = mediaList.ToUniquePlexRole(),
            }
        );
    }

    /// <inheritdoc />
    public async Task<Result<List<PlexTvShowSeason>>> GetAllSeasonsAsync(
        PlexLibrary plexLibrary,
        Action<MediaSyncProgress>? action = null,
        CancellationToken cancellationToken = default
    )
    {
        var mediaListResult = await _plexApiMediaService.SyncMedia(
            plexLibrary,
            PlexMediaType.Season,
            action: action,
            cancellationToken: cancellationToken
        );

        if (mediaListResult.IsFailed)
            return mediaListResult.ToResult();

        var mediaList = mediaListResult.Value.ToPlexTvShowSeasons();
        return Result.Ok(mediaList);
    }

    public async Task<Result<List<PlexTvShowEpisode>>> GetAllEpisodesAsync(
        PlexLibrary plexLibrary,
        Action<MediaSyncProgress>? action = null,
        CancellationToken cancellationToken = default
    )
    {
        var mediaListResult = await _plexApiMediaService.SyncMedia(
            plexLibrary,
            PlexMediaType.Episode,
            action: action,
            cancellationToken: cancellationToken
        );

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

        return await _plexApiWrapper.GetAccessibleLibraryInPlexServerAsync(
            tokenResult.Value,
            plexServerConnection.Value
        );
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

        return await _plexApiWrapper.GetServerStatusAsync(connection, action);
    }

    /// <inheritdoc />
    public async Task<Result<List<PlexServerAccessDTO>>> GetAccessiblePlexServersAsync(int plexAccountId)
    {
        var plexAccount = await _dbContext.PlexAccounts.GetAsync(plexAccountId);
        if (plexAccount is null)
        {
            return ResultExtensions.EntityNotFound(nameof(PlexAccount), plexAccountId);
        }

        var plexAccountToken = await GetPlexApiTokenAsync(plexAccount);
        if (plexAccountToken.IsFailed)
            return plexAccountToken.ToResult();

        var result = await _plexApiWrapper.GetAccessibleServers(plexAccountToken.Value);
        if (result.IsFailed)
        {
            return result.ToResult().LogError();
        }

        var plexServers = result
            .Value.FindAll(x => x.Provides.Contains("server"))
            .Select(x => new PlexServerAccessDTO
            {
                AccessToken = new ServerAccessTokenDTO
                {
                    PlexAccountId = plexAccountId,
                    MachineIdentifier = x.ClientIdentifier,
                    AccessToken = x.AccessToken,
                    IsServerOwned = x.Owned,
                },
                PlexServer = new PlexServer
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
                    Home = x.Home,
                    Synced = x.Synced,
                    Relay = x.Relay,
                    Presence = x.Presence,
                    HttpsRequired = x.HttpsRequired,
                    PublicAddressMatches = x.PublicAddressMatches,
                    DnsRebindingProtection = x.DnsRebindingProtection,
                    NatLoopbackSupported = x.NatLoopbackSupported,
                    PlexAccountServers = [],
                    PlexLibraries = [],
                    ServerStatus = [],
                    PlexServerConnections = x
                        .Connections.Select(y => new PlexServerConnection
                        {
                            Id = 0,
                            Protocol = y.Protocol.ToString().ToLower(),
                            Address = y.Address,
                            Port = y.Port,
                            Local = y.Local,
                            Relay = y.Relay,
                            IPv4 = y.Address.IsIpAddress() && !y.IPv6,
                            IPv6 = y.IPv6,
                            Url = y.Uri,
                            PlexServer = null,
                            PlexServerId = 0,
                            LatestConnectionStatus = null,
                            IsCustom = false,
                        })
                        .ToList(),
                },
            })
            .ToList();

        return Result.Ok(plexServers);
    }

    public Task<Result<PlexAccount>> PlexSignInAsync(PlexAccount plexAccount) =>
        _plexApiWrapper.PlexSignInAsync(plexAccount);

    public async Task<Result<PlexAccount>> ValidatePlexToken(PlexAccount plexAccount) =>
        await _plexApiWrapper.ValidatePlexToken(plexAccount, plexAccount.GetAuthToken);

    public async Task<Result<ServerIdentityDTO>> ValidatePlexConnection(string plexServerConnection)
    {
        var response = await _plexApiWrapper.ValidatePlexConnectionUrl(plexServerConnection);
        if (response.IsFailed)
        {
            return response.ToResult();
        }

        var mediaContainer = response.Value.Object?.MediaContainer ?? null;
        if (mediaContainer is null)
        {
            return ResultExtensions.IsNull(nameof(mediaContainer));
        }

        return Result.Ok(
            new ServerIdentityDTO
            {
                Claimed = mediaContainer.Claimed ?? false,
                MachineIdentifier = mediaContainer.MachineIdentifier ?? string.Empty,
                Version = mediaContainer.Version ?? string.Empty,
            }
        );
    }

    private async Task<Result<string>> GetPlexApiTokenAsync(PlexAccount? plexAccount)
    {
        if (plexAccount == null)
            return ResultExtensions.IsNull(nameof(plexAccount));

        if (plexAccount.GetAuthToken != string.Empty)
        {
            // TODO:Make the token refresh limit configurable
            if ((plexAccount.ValidatedAt - DateTime.UtcNow)?.TotalDays < 30)
            {
                _log.InformationLine("Plex AuthToken was still valid, using from local DB");
                return plexAccount.GetAuthToken;
            }

            _log.InformationLine("Plex AuthToken has expired, refreshing Plex AuthToken now");

            // TODO:Account for 2FA
            return await _plexApiWrapper.RefreshPlexAuthTokenAsync(plexAccount);
        }

        return Result.Fail($"PlexAccount with Id: {plexAccount.Id} contained an empty AuthToken!").LogError();
    }
}
