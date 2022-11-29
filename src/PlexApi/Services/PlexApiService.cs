using AutoMapper;
using PlexRipper.Application;

namespace PlexRipper.PlexApi.Services;

/// <summary>
/// This service is an extra layer of abstraction to convert incoming DTO's from the PlexAPI to workable entities.
/// This was done in order to keep all PlexApi related DTO's in the infrastructure layer.
/// </summary>
public class PlexApiService : IPlexApiService
{
    #region Fields

    private readonly IMapper _mapper;

    private readonly Api.PlexApi _plexApi;

    #endregion

    #region Constructors

    public PlexApiService(Api.PlexApi plexApi, IMapper mapper)
    {
        _plexApi = plexApi;
        _mapper = mapper;
    }

    #endregion

    #region Methods

    #region Public

    public async Task<PlexAccount> GetAccountAsync(string authToken)
    {
        var result = await _plexApi.GetAccountAsync(authToken);
        return _mapper.Map<PlexAccount>(result);
    }

    public async Task<Result<List<PlexTvShowEpisode>>> GetAllEpisodesAsync(string serverAuthToken, string plexFullHost, string plexLibraryKey)
    {
        var stepSize = 5000;
        var result = await _plexApi.GetAllEpisodesAsync(serverAuthToken, plexFullHost, plexLibraryKey, 0, stepSize);
        if (result != null)
        {
            var metaData = result.MediaContainer.Metadata;
            var totalSize = result.MediaContainer.TotalSize;
            if (totalSize > stepSize)
            {
                var loops = (int)Math.Ceiling(totalSize / (double)stepSize);

                for (var i = 1; i < loops; i++)
                {
                    var rangeResult = await _plexApi.GetAllEpisodesAsync(serverAuthToken, plexFullHost, plexLibraryKey, i * stepSize, stepSize);
                    if (rangeResult?.MediaContainer?.Metadata?.Count > 0)
                        metaData.AddRange(rangeResult.MediaContainer.Metadata);
                }

                var success = metaData.Count == totalSize;
            }

            return Result.Ok(_mapper.Map<List<PlexTvShowEpisode>>(metaData));
        }

        return Result.Fail($"Failed to retrieve episodes for library with key {plexLibraryKey}");
    }

    public async Task<Result<List<PlexTvShowSeason>>> GetAllSeasonsAsync(
        string serverAuthToken,
        string plexFullHost,
        string plexLibraryKey)
    {
        var result = await _plexApi.GetAllSeasonsAsync(serverAuthToken, plexFullHost, plexLibraryKey);
        if (result != null)
            return Result.Ok(_mapper.Map<List<PlexTvShowSeason>>(result.MediaContainer.Metadata));

        return Result.Fail($"Failed to retrieve seasons for library with key {plexLibraryKey}");
    }

    /// <summary>
    /// Returns the latest version of the <see cref="PlexLibrary"/> with the included media. Id and PlexServerId are copied over from the input parameter.
    /// </summary>
    /// <param name="plexLibrary"></param>
    /// <param name="authToken">The token used to authenticate with the <see cref="PlexServer"/>.</param>
    /// <param name="plexServerBaseUrl"></param>
    /// <returns></returns>
    public async Task<Result<PlexLibrary>> GetLibraryMediaAsync(PlexLibrary plexLibrary, string authToken)
    {
        // Retrieve updated version of the PlexLibrary
        var plexLibraries = await GetLibrarySectionsAsync(authToken, plexLibrary.ServerUrl);

        if (plexLibraries.IsFailed)
            return plexLibraries.ToResult();

        var updatedPlexLibrary = plexLibraries.Value.Find(x => x.Key == plexLibrary.Key);
        updatedPlexLibrary.Id = plexLibrary.Id;
        updatedPlexLibrary.PlexServerId = plexLibrary.PlexServerId;
        updatedPlexLibrary.SyncedAt = DateTime.Now;

        // Retrieve the media for this library
        var result = await _plexApi.GetMetadataForLibraryAsync(authToken, plexLibrary.ServerUrl, plexLibrary.Key);

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

    /// <summary>
    /// Retrieves all accessible <see cref="PlexLibrary"/> from this <see cref="PlexServer"/> by this AuthToken.
    /// </summary>
    /// <param name="authToken">The token used to authenticate with the <see cref="PlexServer"/>.</param>
    /// <param name="plexServerBaseUrl">The full PlexServer Url.</param>
    /// <returns>List of accessible <see cref="PlexLibrary"/>.</returns>
    public async Task<Result<List<PlexLibrary>>> GetLibrarySectionsAsync(string authToken, string plexServerBaseUrl)
    {
        var result = await _plexApi.GetLibrarySectionsAsync(authToken, plexServerBaseUrl);
        if (result.IsFailed)
        {
            Log.Warning($"{plexServerBaseUrl} returned no libraries");
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

    public Task<PlexServerStatus> GetPlexServerStatusAsync(string authToken, string serverBaseUrl, Action<PlexApiClientProgress> action = null)
    {
        return _plexApi.GetServerStatusAsync(authToken, serverBaseUrl, action);
    }

    public async Task<List<PlexTvShowSeason>> GetSeasonsAsync(string serverAuthToken, string plexFullHost, PlexTvShow plexTvShow)
    {
        var result = await _plexApi.GetSeasonsAsync(serverAuthToken, plexFullHost, plexTvShow.Key);
        return result != null ? _mapper.Map<List<PlexTvShowSeason>>(result.MediaContainer.Metadata) : new List<PlexTvShowSeason>();
    }

    /// <inheritdoc/>
    public async Task<List<PlexServer>> GetServersAsync(string plexAccountToken)
    {
        var result = await _plexApi.GetServerAsync(plexAccountToken);
        if (result != null)
            return _mapper.Map<List<PlexServer>>(result);

        Log.Warning("Failed to retrieve PlexServers");
        return new List<PlexServer>();
    }

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
            mapResult.ValidatedAt = DateTime.Now;
            mapResult.VerificationCode = "";

            Log.Information($"Successfully retrieved the PlexAccount data for user {plexAccount.DisplayName} from the PlexApi");
            return Result.Ok(mapResult);
        }

        return result.ToResult();
    }

    public Task<string> RefreshPlexAuthTokenAsync(PlexAccount account)
    {
        return _plexApi.RefreshPlexAuthTokenAsync(account);
    }

    public Task<Result<byte[]>> GetPlexMediaImageAsync(string thumbUrl, string authToken, int width = 0, int height = 0)
    {
        return _plexApi.GetPlexMediaImageAsync(thumbUrl, authToken, width, height);
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

    #endregion
}