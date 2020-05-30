using AutoMapper;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Domain.Entities;
using PlexRipper.Infrastructure.Common.DTO;
using PlexRipper.Infrastructure.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlexRipper.Infrastructure.Services
{
    /// <summary>
    /// This service is an extra layer of abstraction to convert incoming DTO's from the PlexAPI to workable entities.
    /// This was done in order to keep all PlexApi related DTO's in the infrastructure layer. 
    /// </summary>
    public class PlexApiService : IPlexApiService
    {
        private readonly IPlexApi _plexApi;
        private readonly IMapper _mapper;

        public PlexApiService(IPlexApi plexApi, IMapper mapper)
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

        public Task<PlexStatusDTO> GetStatus(string authToken, string uri)
        {
            return _plexApi.GetStatus(authToken, uri);
        }

        public Task<PlexAccount> GetAccountAsync(string authToken)
        {
            return _plexApi.GetAccount(authToken);
        }

        public async Task<List<PlexServer>> GetServerAsync(string authToken)
        {
            var result = await _plexApi.GetServer(authToken);
            return result == null ?
                new List<PlexServer>() :
                _mapper.Map<List<PlexServer>>(result.Server);
        }

        public async Task<List<PlexLibrary>> GetLibrarySectionsAsync(string authToken, string plexLibraryUrl)
        {
            var result = await _plexApi.GetLibrarySections(authToken, plexLibraryUrl);
            if (result == null)
            {
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
    }
}
