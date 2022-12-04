using AutoMapper;
using PlexRipper.Application;
using PlexRipper.Application.PlexAuthentication.Queries;

namespace PlexRipper.PlexApi.Services;

/// <summary>
/// This service is an extra layer of abstraction to convert incoming DTO's from the PlexAPI to workable entities.
/// This was done in order to keep all PlexApi related DTO's in the infrastructure layer.
/// </summary>
public class PlexApiService : IPlexApiService
{
    #region Fields

    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    private readonly Api.PlexApi _plexApi;

    #endregion

    #region Constructors

    public PlexApiService(Api.PlexApi plexApi, IMapper mapper, IMediator mediator)
    {
        _plexApi = plexApi;
        _mapper = mapper;
        _mediator = mediator;
    }

    #endregion

    #region Methods

    #region Public

    #region PlexAccount

    public async Task<PlexAccount> GetAccountAsync(string authToken)
    {
        var result = await _plexApi.GetAccountAsync(authToken);
        return _mapper.Map<PlexAccount>(result);
    }

    #endregion


    public async Task<Result<List<PlexTvShowEpisode>>> GetAllEpisodesAsync(PlexLibrary plexLibrary)
    {
        var plexServerResult = await _mediator.Send(new GetPlexServerByIdQuery(plexLibrary.PlexServerId));
        if (plexServerResult.IsFailed)
            return plexServerResult.ToResult();

        var tokenResult = await GetPlexServerTokenAsync(plexServerResult.Value.Id);
        if (tokenResult.IsFailed)
            return tokenResult.ToResult();

        var stepSize = 5000;
        var authToken = tokenResult.Value;
        var plexLibraryKey = plexLibrary.Key;
        var serverUrl = plexServerResult.Value.GetServerUrl();

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
                    var rangeResult = await _plexApi.GetAllEpisodesAsync(authToken, serverUrl, plexLibraryKey, i * stepSize, stepSize);
                    if (rangeResult?.MediaContainer?.Metadata?.Count > 0)
                        metaData.AddRange(rangeResult.MediaContainer.Metadata);
                }

                var success = metaData.Count == totalSize;
            }

            return Result.Ok(_mapper.Map<List<PlexTvShowEpisode>>(metaData));
        }

        return Result.Fail($"Failed to retrieve episodes for library with key {plexLibraryKey}");
    }

    /// <inheritdoc/>
    public async Task<Result<List<PlexTvShowSeason>>> GetAllSeasonsAsync(PlexLibrary plexLibrary)
    {
        var plexServerResult = await _mediator.Send(new GetPlexServerByIdQuery(plexLibrary.PlexServerId));
        if (plexServerResult.IsFailed)
            return plexServerResult.ToResult();

        var tokenResult = await GetPlexServerTokenAsync(plexServerResult.Value.Id);
        if (tokenResult.IsFailed)
            return tokenResult.ToResult();

        var result = await _plexApi.GetAllSeasonsAsync(tokenResult.Value, plexServerResult.Value.GetServerUrl(), plexLibrary.Key);
        if (result != null)
            return Result.Ok(_mapper.Map<List<PlexTvShowSeason>>(result.MediaContainer.Metadata));

        return Result.Fail($"Failed to retrieve seasons for library with key {plexLibrary.Key}");
    }

    /// <inheritdoc/>
    public async Task<Result<PlexLibrary>> GetLibraryMediaAsync(PlexLibrary plexLibrary, PlexAccount plexAccount = null)
    {
        var plexServerResult = await _mediator.Send(new GetPlexServerByIdQuery(plexLibrary.PlexServerId));
        if (plexServerResult.IsFailed)
            return plexServerResult.ToResult();

        var tokenResult = await GetPlexServerTokenAsync(plexLibrary.PlexServerId, plexAccount?.Id ?? 0);
        if (tokenResult.IsFailed)
            return tokenResult.ToResult();

        var serverUrl = plexServerResult.Value.GetServerUrl();

        // Retrieve updated version of the PlexLibrary
        var plexLibraries = await GetLibrarySectionsAsync(plexServerResult.Value, plexAccount);

        if (plexLibraries.IsFailed)
            return plexLibraries.ToResult();

        var updatedPlexLibrary = plexLibraries.Value.Find(x => x.Key == plexLibrary.Key);
        updatedPlexLibrary.Id = plexLibrary.Id;
        updatedPlexLibrary.PlexServerId = plexLibrary.PlexServerId;
        updatedPlexLibrary.SyncedAt = DateTime.UtcNow;

        // Retrieve the media for this library
        var result = await _plexApi.GetMetadataForLibraryAsync(tokenResult.Value, serverUrl, plexLibrary.Key);

        if (result == null)
            return null;

        var mediaList = result.MediaContainer.Metadata;

        // Determine how to map based on the Library type.
        switch (result.MediaContainer.ViewGroup)
        {
            case "movie":
                updatedPlexLibrary.Movies = _mapper.Map<List<PlexMovie>>(mediaList);
                break;
            case "show":
                updatedPlexLibrary.TvShows = _mapper.Map<List<PlexTvShow>>(mediaList);
                break;
        }

        return Result.Ok(updatedPlexLibrary);
    }

    /// <inheritdoc/>
    public async Task<Result<List<PlexLibrary>>> GetLibrarySectionsAsync(PlexServer plexServer, PlexAccount plexAccount = null)
    {
        var tokenResult = await GetPlexServerTokenAsync(plexServer.Id, plexAccount?.Id ?? 0);
        if (tokenResult.IsFailed)
            return tokenResult.ToResult();

        var result = await _plexApi.GetLibrarySectionsAsync(tokenResult.Value, plexServer.GetServerUrl());
        if (result.IsFailed)
        {
            Log.Warning($"Plex server: {plexServer.Name} returned no libraries");
            return result.ToResult();
        }

        var directories = result.Value.MediaContainer.Directory;

        return Result.Ok(_mapper.Map<List<PlexLibrary>>(directories));
    }

    public async Task<PlexMediaMetaData> GetMediaMetaDataAsync(string serverAuthToken, string plexFullHost, int ratingKey)
    {
        var result = await _plexApi.GetMetadataAsync(serverAuthToken, plexFullHost, ratingKey);
        return _mapper.Map<PlexMediaMetaData>(result);
    }

    public async Task<PlexMediaMetaData> GetMediaMetaDataAsync(string serverAuthToken, string metaDataUrl)
    {
        var result = await _plexApi.GetMetadataAsync(serverAuthToken, metaDataUrl);
        return _mapper.Map<PlexMediaMetaData>(result);
    }

    public async Task<Result<PlexServerStatus>> GetPlexServerStatusAsync(int plexServerId, Action<PlexApiClientProgress> action = null)
    {
        var plexServerResult = await _mediator.Send(new GetPlexServerByIdQuery(plexServerId));
        if (plexServerResult.IsFailed)
            return plexServerResult.ToResult();

        var tokenResult = await GetPlexServerTokenAsync(plexServerId);
        if (tokenResult.IsFailed)
            return tokenResult.ToResult();

        var serverStatusResult = await _plexApi.GetServerStatusAsync(tokenResult.Value, plexServerResult.Value.GetServerUrl(), action);
        if (serverStatusResult.IsFailed)
            return serverStatusResult;

        serverStatusResult.Value.PlexServer = plexServerResult.Value;
        serverStatusResult.Value.PlexServerId = plexServerResult.Value.Id;
        return serverStatusResult;
    }

    public async Task<List<PlexTvShowSeason>> GetSeasonsAsync(string serverAuthToken, string plexFullHost, PlexTvShow plexTvShow)
    {
        var result = await _plexApi.GetSeasonsAsync(serverAuthToken, plexFullHost, plexTvShow.Key);
        return result != null ? _mapper.Map<List<PlexTvShowSeason>>(result.MediaContainer.Metadata) : new List<PlexTvShowSeason>();
    }

    /// <inheritdoc/>
    public async Task<(Result<List<PlexServer>> servers, Result<List<ServerAccessTokenDTO>> tokens)> GetServersAsync(PlexAccount plexAccount)
    {
        var plexAccountToken = await GetPlexApiTokenAsync(plexAccount);
        if (plexAccountToken.IsFailed)
            return (plexAccountToken.ToResult(), plexAccountToken.ToResult());

        var result = await _plexApi.GetServerAsync(plexAccountToken.Value);
        if (result.IsFailed)
        {
            Log.Warning("Failed to retrieve PlexServers");
            return (result.ToResult(), result.ToResult());
        }

        var mapServersResult = _mapper.Map<List<PlexServer>>(result.Value);
        var mapAccessResult = _mapper.Map<List<ServerAccessTokenDTO>>(result.Value);

        // The servers have an OwnerId of 0 when it belongs to the PlexAccount that was used to request it.
        foreach (var plexServer in mapServersResult.Where(plexServer => plexServer.OwnerId == 0))
            plexServer.OwnerId = plexAccount.PlexId;

        // Ensure every token has the PlexAccountId assigned
        foreach (var serverAccessTokenDto in mapAccessResult)
            serverAccessTokenDto.PlexAccountId = plexAccount.Id;

        return (Result.Ok(mapServersResult), Result.Ok(mapAccessResult));
    }

    #region Images

    /// <inheritdoc/>
    public async Task<Result<byte[]>> GetPlexMediaImageAsync(PlexServer plexServer, string thumbPath, int width = 0, int height = 0)
    {
        var tokenResult = await GetPlexServerTokenAsync(plexServer.Id);
        if (tokenResult.IsFailed)
            return tokenResult.ToResult();

        return await _plexApi.GetPlexMediaImageAsync(plexServer.GetServerUrl() + thumbPath, tokenResult.Value, width, height);
    }

    #endregion


    #region Authentication

    #region PlexSignIn

    public async Task<Result<PlexAccount>> PlexSignInAsync(PlexAccount plexAccount)
    {
        var result = await _plexApi.PlexSignInAsync(plexAccount);
        if (result.IsSuccess)
        {
            var mapResult = _mapper.Map<PlexAccount>(result.Value);
            mapResult.Id = plexAccount.Id;
            mapResult.ClientId = plexAccount.ClientId;
            mapResult.DisplayName = plexAccount.DisplayName;
            mapResult.Username = plexAccount.Username;
            mapResult.Password = plexAccount.Password;
            mapResult.IsEnabled = plexAccount.IsEnabled;
            mapResult.IsMain = plexAccount.IsMain;
            mapResult.IsValidated = true;
            mapResult.ValidatedAt = DateTime.UtcNow;
            mapResult.VerificationCode = "";

            Log.Information($"Successfully retrieved the PlexAccount data for user {plexAccount.DisplayName} from the PlexApi");
            return Result.Ok(mapResult);
        }

        return result.ToResult();
    }

    public Task<Result<AuthPin>> Get2FAPin(string clientId)
    {
        return _plexApi.Get2FAPin(clientId);
    }

    public Task<Result<AuthPin>> Check2FAPin(int pinId, string clientId)
    {
        return _plexApi.Check2FAPin(pinId, clientId);
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
                Log.Information("Plex AuthToken was still valid, using from local DB.");
                return plexAccount.AuthenticationToken;
            }

            Log.Information("Plex AuthToken has expired, refreshing Plex AuthToken now.");

            // TODO Account for 2FA
            return await _plexApi.RefreshPlexAuthTokenAsync(plexAccount);
        }

        return Result.Fail($"PlexAccount with Id: {plexAccount.Id} contained an empty AuthToken!").LogError();
    }

    /// <summary>
    /// Returns the authentication token needed to authenticate communication with the <see cref="PlexServer"/>.
    /// Note: If no plexAccountId is specified then it will search for a valid <see cref="PlexAccount"/> automatically.
    /// </summary>
    /// <param name="plexServerId">The id of the <see cref="PlexServer"/> to retrieve a token for.</param>
    /// <param name="plexAccountId">The id of the <see cref="PlexAccount"/> to authenticate with.</param>
    /// <returns>The authentication token.</returns>
    private Task<Result<string>> GetPlexServerTokenAsync(int plexServerId, int plexAccountId = 0)
    {
        // TODO if there is no token then it should refresh a token
        return _mediator.Send(new GetPlexServerTokenQuery(plexServerId, plexAccountId));
    }

    public async Task<Result<string>> GetPlexServerTokenWithUrl(int plexServerId, string serverUrl, int plexAccountId = 0)
    {
        if (string.IsNullOrEmpty(serverUrl))
            return ResultExtensions.IsNull(nameof(serverUrl)).LogWarning();

        var token = await GetPlexServerTokenAsync(plexServerId, plexAccountId);
        if (token.IsFailed)
            return token.ToResult();

        // TODO verify that download=1 is not needed.
        return Result.Ok($"{serverUrl}?X-Plex-Token={token.Value}");
    }

    #endregion

    #endregion

    #endregion
}