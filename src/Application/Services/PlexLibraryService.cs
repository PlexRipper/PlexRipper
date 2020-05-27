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
        private readonly IPlexMoviesRepository _plexMoviesRepository;
        private readonly IPlexApiService _plexServiceApi;
        private ILogger Log { get; }

        public PlexLibraryService(
            IPlexApiService plexServiceApi,
            IPlexLibraryRepository plexLibraryRepository,
            IPlexMoviesRepository plexMoviesRepository,
            ILogger logger)
        {
            _plexLibraryRepository = plexLibraryRepository;
            _plexMoviesRepository = plexMoviesRepository;
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

            var libraries = await _plexServiceApi.GetLibrarySectionsAsync(plexServer.AccessToken, plexServer.BaseUrl);
            await AddOrUpdatePlexLibrariesAsync(plexServer, libraries);

            return true;
        }

        /// <summary>
        /// Returns the <see cref="PlexLibrary"/> containing the media content.
        /// </summary>
        /// <param name="plexServer"></param>
        /// <param name="libraryKey"></param>
        /// <returns></returns>
        public async Task<PlexLibrary> GetLibraryMediaAsync(PlexServer plexServer, string libraryKey, bool refresh = false)
        {
            var plexLibrary = plexServer.PlexLibraries.Find(x => x.Key == libraryKey);
            plexLibrary = await _plexServiceApi.GetLibraryMediaAsync(plexLibrary, plexServer.AccessToken, plexServer.BaseUrl);

            switch (plexLibrary.Type)
            {
                case "movie":
                    await AddOrUpdateMoviesAsync(plexLibrary, plexLibrary.Movies);
                    break;
            }
            // Create library

            //var metaData = await _plexServiceApi.GetMetadata(plexServer.AccessToken, plexServer.BaseUrl, 5516);
            //string downloadUrl = _plexApi.GetDownloadUrl(plexServer, metaData);
            //string filename = _plexApi.GetDownloadFilename(plexServer, metaData);
            //_plexApi.DownloadMedia(plexServer.AccessToken, downloadUrl, filename);
            return plexLibrary;
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
                var plexLibraryDB = await _plexLibraryRepository.FindAsync(x => x.PlexServerId == plexServer.Id && x.Uuid == plexLibrary.Uuid);

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


        public async Task AddOrUpdateMoviesAsync(PlexLibrary plexLibrary, List<PlexMovies> movies)
        {
            if (plexLibrary == null)
            {
                Log.Warning($"{nameof(AddOrUpdateMoviesAsync)} => plexLibrary was null");
                return;
            }

            if (movies.Count == 0)
            {
                Log.Warning($"{nameof(AddOrUpdateMoviesAsync)} => servers list was empty");
                return;
            }

            Log.Debug($"{ nameof(AddOrUpdateMoviesAsync)} => Starting adding or updating movies in library: {plexLibrary.Title}");
            // Remove all movies and re-add them
            var currentMovies = await _plexMoviesRepository.FindAllAsync(x => x.PlexLibraryId == plexLibrary.Id);
            await _plexMoviesRepository.RemoveRangeAsync(currentMovies);

            // Ensure the correct ID is added. 
            foreach (var movie in movies)
            {
                movie.PlexLibraryId = plexLibrary.Id;
            }

            await _plexMoviesRepository.AddRangeAsync(movies);

        }

        #endregion
    }
}
