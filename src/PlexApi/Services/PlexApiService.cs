using AutoMapper;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Domain.Entities;
using PlexRipper.PlexApi.Common.DTO.PlexGetStatus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlexRipper.Domain;

namespace PlexRipper.PlexApi.Services
{
    /// <summary>
    /// This service is an extra layer of abstraction to convert incoming DTO's from the PlexAPI to workable entities.
    /// This was done in order to keep all PlexApi related DTO's in the infrastructure layer. 
    /// </summary>
    public class PlexApiService : IPlexApiService
    {
        private readonly Api.PlexApi _plexApi;
        private readonly IMapper _mapper;

        public PlexApiService(Api.PlexApi plexApi, IMapper mapper)
        {
            _plexApi = plexApi;
            _mapper = mapper;
        }

        public async Task<PlexAccount> PlexSignInAsync(string username, string password)
        {
            var result = await _plexApi.PlexSignInAsync(username, password);
            return result == null ? null : _mapper.Map<PlexAccount>(result.User);
        }

        public Task<string> RefreshPlexAuthTokenAsync(Account account)
        {
            return _plexApi.RefreshPlexAuthTokenAsync(account);
        }

        public Task<PlexServerStatus> GetPlexServerStatusAsync(string authToken, string serverBaseUrl)
        {
            return _plexApi.GetServerStatusAsync(authToken, serverBaseUrl);
        }

        public async Task<PlexAccount> GetAccountAsync(string authToken)
        {
            var result = await _plexApi.GetAccountAsync(authToken);
            return _mapper.Map<PlexAccount>(result);
        }

        public async Task<List<PlexServer>> GetServerAsync(string authToken)
        {
            var result = await _plexApi.GetServerAsync(authToken);
            return result == null ?
                new List<PlexServer>() :
                _mapper.Map<List<PlexServer>>(result.Server);
        }

        public async Task<List<PlexLibrary>> GetLibrarySectionsAsync(string authToken, string plexLibraryUrl)
        {
            var result = await _plexApi.GetLibrarySectionsAsync(authToken, plexLibraryUrl);
            if (result == null)
            {
                Log.Warning($"{plexLibraryUrl} returned no libraries");
                return new List<PlexLibrary>();
            }

            var librariesDTOs = result.MediaContainer.Directory.ToList();
            var map = new List<PlexLibrary>();
            try
            {
                map = _mapper.Map<List<PlexLibrary>>(librariesDTOs);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            return map;
        }

        /// <summary>
        /// Returns a PlexLibrary container with either Movies, Series, Music or Photos depending on the type. 
        /// </summary>
        /// <param name="library"></param>
        /// <param name="authToken"></param>
        /// <param name="plexFullHost"></param>
        /// <returns></returns>
        public async Task<PlexLibrary> GetLibraryMediaAsync(PlexLibrary library, string authToken, string plexFullHost)
        {
            var result = await _plexApi.GetLibraryMediaAsync(authToken, plexFullHost, library.Key);

            if (result == null) { return null; }

            var libraryContainer = new PlexLibrary
            {
                Id = library.Id,
                Type = result.MediaContainer.ViewGroup
            };

            // Determine how to map based on the Library type.
            switch (result.MediaContainer.ViewGroup)
            {
                case "movie":
                    libraryContainer.Movies = _mapper.Map<List<PlexMovie>>(result.MediaContainer.Metadata);
                    break;
                case "show":
                    libraryContainer.Series = _mapper.Map<List<PlexSerie>>(result.MediaContainer.Metadata);
                    break;
            }

            return libraryContainer;
        }


        public async Task<PlexMediaMetaData> GetMediaMetaDataAsync(string serverAuthToken, string plexFullHost, int ratingKey)
        {
            PlexMediaMetaDataDTO result = await _plexApi.GetMetadataAsync(serverAuthToken, plexFullHost, ratingKey);
            return _mapper.Map<PlexMediaMetaData>(result);
        }

        public async Task<PlexMediaMetaData> GetMediaMetaDataAsync(string serverAuthToken, string metaDataUrl)
        {
            PlexMediaMetaDataDTO result = await _plexApi.GetMetadataAsync(serverAuthToken, metaDataUrl);
            return _mapper.Map<PlexMediaMetaData>(result);
        }
    }
}
