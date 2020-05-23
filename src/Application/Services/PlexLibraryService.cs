using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Application.Common.Interfaces.Repositories;
using PlexRipper.Domain.Entities;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlexRipper.Application.Services
{
    public class PlexLibraryService : IPlexLibraryService
    {
        private readonly IPlexLibraryRepository _plexLibraryRepository;
        private readonly IPlexApiService _plexServiceApi;
        private ILogger Log { get; }

        public PlexLibraryService(
            IPlexApiService plexServiceApi,
            IPlexLibraryRepository plexLibraryRepository,
            ILogger logger)
        {
            _plexLibraryRepository = plexLibraryRepository;
            _plexServiceApi = plexServiceApi;
            Log = logger;
        }


        public async Task<bool> RefreshLibrariesAsync(PlexServer plexServer)
        {
            if (plexServer == null)
            {
                Log.Warning($"{nameof(RefreshLibrariesAsync)} => plexServer was null");
                return false;
            }

            Log.Debug($"{nameof(RefreshLibrariesAsync)} => Refreshing PlexLibraries for plexServer: {plexServer.Id}");

            var libraries = await _plexServiceApi.GetLibrarySections(plexServer.AccessToken, plexServer.LibraryUrl);
            await AddOrUpdatePlexLibrariesAsync(plexServer, libraries);

            return true;
        }

        #region CRUD
        public async Task<List<PlexLibrary>> GetPlexLibrariesAsync(PlexServer plexServer, bool refresh = false)
        {
            if (plexServer == null)
            {
                Log.Warning($"{nameof(GetPlexLibrariesAsync)} => The PlexLibrary was null");
                return new List<PlexLibrary>();
            }

            // Retrieve all plexLibraries
            var plexLibraryList = await _plexLibraryRepository.GetLibraries(plexServer.Id);

            if (refresh || !plexLibraryList.Any())
            {
                if (!plexLibraryList.Any())
                {
                    Log.Warning($"{nameof(GetPlexLibrariesAsync)} => PlexAccount {plexServer.Id} did not have any PlexServers assigned. Forcing {nameof(RefreshLibrariesAsync)} was null");
                }

                var refreshSuccess = await RefreshLibrariesAsync(plexServer);
                if (refreshSuccess)
                {
                    plexLibraryList = await _plexLibraryRepository.GetLibraries(plexServer.Id);
                }
            }

            return plexLibraryList.ToList();
        }

        private async Task AddOrUpdatePlexLibrariesAsync(PlexServer plexServer, List<PlexLibrary> libraries)
        {

            if (plexServer == null)
            {
                Log.Warning($"{nameof(AddOrUpdatePlexLibrariesAsync)} => plexServer was null");
                return;
            }

            if (libraries.Count == 0)
            {
                Log.Warning($"{nameof(AddOrUpdatePlexLibrariesAsync)} => libraries list was empty");
                return;
            }

            Log.Debug($"{ nameof(AddOrUpdatePlexLibrariesAsync)} => Starting adding or updating plex libraries");

            // Add or update the plex libraries
            foreach (var plexLibrary in libraries)
            {
                // Create
                var plexLibraryDB = await _plexLibraryRepository.FindAsync(x => x.PlexServerId == plexServer.Id && x.Key == plexLibrary.Key);

                plexLibrary.PlexServerId = plexServer.Id;

                if (plexLibraryDB != null)
                {
                    // Update
                    plexLibrary.Id = plexLibraryDB.Id;
                    await _plexLibraryRepository.UpdateAsync(plexLibrary);
                }
                else
                {
                    //Create
                    await _plexLibraryRepository.AddAsync(plexLibrary);
                }
            }

            // TODO Add check if it was successful


        }




        #endregion
    }
}
