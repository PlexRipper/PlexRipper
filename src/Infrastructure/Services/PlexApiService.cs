using AutoMapper;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Domain.Entities;
using PlexRipper.Infrastructure.Common.DTO;
using PlexRipper.Infrastructure.Common.Interfaces;
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
            var plexAuthentication = await _plexApi.PlexSignInAsync(username, password);
            var map = _mapper.Map<PlexAccount>(plexAuthentication?.User);
            return map;
        }

        public async Task<string> RefreshPlexAuthTokenAsync(Account account)
        {
            return await _plexApi.RefreshPlexAuthTokenAsync(account);
        }

        public async Task<PlexStatusDTO> GetStatus(string authToken, string uri)
        {
            return await _plexApi.GetStatus(authToken, uri);
        }

        public async Task<PlexAccount> GetAccountAsync(string authToken)
        {
            return await _plexApi.GetAccount(authToken);
        }

        public async Task<List<PlexServer>> GetServerAsync(string authToken)
        {
            var result = await _plexApi.GetServer(authToken);
            return _mapper.Map<List<PlexServer>>(result.Server);
        }

        public async Task<List<PlexLibrary>> GetLibrarySections(string authToken, string plexLibraryUrl)
        {
            var result = await _plexApi.GetLibrarySections(authToken, plexLibraryUrl);
            var librariesDTOs = result.MediaContainer.Directory.ToList();
            return _mapper.Map<List<PlexLibrary>>(librariesDTOs);
        }

        public async Task<PlexLibrary> GetLibraryAsync(string authToken, string plexFullHost, string libraryId)
        {
            var result = await _plexApi.GetLibrary(authToken, plexFullHost, libraryId);
            return _mapper.Map<PlexLibrary>(result.MediaContainer);
        }
    }
}
