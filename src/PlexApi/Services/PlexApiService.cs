using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FluentResults;
using PlexRipper.Application.Common;
using PlexRipper.Domain;

namespace PlexRipper.PlexApi.Services
{
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

        #region Private

        /// <summary>
        /// Some PlexServers are misconfigured so we have to fix that.
        /// </summary>
        /// <param name="plexServers"></param>
        /// <returns></returns>
        private List<PlexServer> CleanupPlexServers(List<PlexServer> plexServers)
        {
            if (plexServers.Count > 0)
            {
                foreach (var plexServer in plexServers)
                {
                    if (plexServer.Port == 443 && plexServer.Scheme == "http")
                    {
                        plexServer.Scheme = "https";
                    }
                }
            }

            return plexServers;
        }

        #endregion

        #region Public

        public async Task<PlexAccount> GetAccountAsync(string authToken)
        {
            var result = await _plexApi.GetAccountAsync(authToken);
            return _mapper.Map<PlexAccount>(result);
        }

        public async Task<List<PlexTvShowEpisode>> GetEpisodesAsync(string serverAuthToken, string plexFullHost, PlexTvShowSeason plexTvShowSeason)
        {
            var result = await _plexApi.GetAllEpisodesAsync(serverAuthToken, plexFullHost, plexTvShowSeason.RatingKey);
            return _mapper.Map<List<PlexTvShowEpisode>>(result.MediaContainer.Metadata);
        }

        /// <summary>
        /// Returns the latest version of the <see cref="PlexLibrary"/> with the included media. Id and PlexServerId are copied over from the input parameter.
        /// </summary>
        /// <param name="plexLibrary"></param>
        /// <param name="authToken">The token used to authenticate with the <see cref="PlexServer"/>.</param>
        /// <param name="plexServerBaseUrl"></param>
        /// <returns></returns>
        public async Task<PlexLibrary> GetLibraryMediaAsync(PlexLibrary plexLibrary, string authToken)
        {
            // Retrieve updated version of the PlexLibrary
            var plexLibraries = await GetLibrarySectionsAsync(authToken, plexLibrary.ServerUrl);

            var updatedPlexLibrary = plexLibraries.Find(x => x.Key == plexLibrary.Key);
            updatedPlexLibrary.Id = plexLibrary.Id;
            updatedPlexLibrary.PlexServerId = plexLibrary.PlexServerId;
            updatedPlexLibrary.CheckedAt = DateTime.Now;

            var result = await _plexApi.GetMetadataForLibraryAsync(authToken, plexLibrary.ServerUrl, plexLibrary.Key);

            if (result == null)
            {
                return null;
            }

            // Determine how to map based on the Library type.
            switch (result.MediaContainer.ViewGroup)
            {
                case "movie":
                    updatedPlexLibrary.Movies = _mapper.Map<List<PlexMovie>>(result.MediaContainer.Metadata);
                    break;
                case "show":
                    updatedPlexLibrary.TvShows = _mapper.Map<List<PlexTvShow>>(result.MediaContainer.Metadata);
                    break;
            }

            return updatedPlexLibrary;
        }

        /// <summary>
        /// Retrieves all accessible <see cref="PlexLibrary"/> from this <see cref="PlexServer"/> by this AuthToken.
        /// </summary>
        /// <param name="authToken">The token used to authenticate with the <see cref="PlexServer"/>.</param>
        /// <param name="plexServerBaseUrl">The full PlexServer Url.</param>
        /// <returns>List of accessible <see cref="PlexLibrary"/>.</returns>
        public async Task<List<PlexLibrary>> GetLibrarySectionsAsync(string authToken, string plexServerBaseUrl)
        {
            var result = await _plexApi.GetLibrarySectionsAsync(authToken, plexServerBaseUrl);
            if (result == null)
            {
                Log.Warning($"{plexServerBaseUrl} returned no libraries");
                return new List<PlexLibrary>();
            }

            var directories = result.MediaContainer.Directory;

            return _mapper.Map<List<PlexLibrary>>(directories);
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

        public Task<PlexServerStatus> GetPlexServerStatusAsync(string authToken, string serverBaseUrl)
        {
            return _plexApi.GetServerStatusAsync(authToken, serverBaseUrl);
        }

        public async Task<List<PlexTvShowSeason>> GetSeasonsAsync(string serverAuthToken, string plexFullHost, PlexTvShow plexTvShow)
        {
            var result = await _plexApi.GetSeasonsAsync(serverAuthToken, plexFullHost, plexTvShow.RatingKey);
            return _mapper.Map<List<PlexTvShowSeason>>(result.MediaContainer.Metadata);
        }

        public async Task<List<PlexServer>> GetServerAsync(string authToken)
        {
            var result = await _plexApi.GetServerAsync(authToken);
            if (result != null)
            {
                var convertedList = _mapper.Map<List<PlexServer>>(result);
                return CleanupPlexServers(convertedList);
            }

            Log.Warning("Failed to retrieve PlexServers");
            return new List<PlexServer>();
        }

        public async Task<PlexAccount> PlexSignInAsync(string username, string password)
        {
            var result = await _plexApi.PlexSignInAsync(username, password);
            if (result != null)
            {
                var mapResult = _mapper.Map<PlexAccount>(result.User);
                if (mapResult != null)
                {
                    mapResult.IsValidated = true;
                    mapResult.ValidatedAt = DateTime.Now;
                    Log.Information($"Successfully retrieved the PlexAccount data for user {username} from the PlexApi");
                    return mapResult;
                }
            }

            Log.Warning("The result from the PlexSignIn was null");
            return null;
        }

        public Task<string> RefreshPlexAuthTokenAsync(PlexAccount account)
        {
            return _plexApi.RefreshPlexAuthTokenAsync(account);
        }

        public async Task<byte[]> GetThumbnailAsync(string thumbUrl, string authToken, int width = 0, int height = 0)
        {
            return await _plexApi.GetThumbnailAsync(thumbUrl, authToken, width, height);
        }

        #endregion

        #endregion
    }
}