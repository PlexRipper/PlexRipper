using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Application.Common.Interfaces.Repositories;
using PlexRipper.Domain.Entities;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlexRipper.Application.Services
{
    public class PlexMovieService : IPlexMovieService
    {
        private readonly IPlexLibraryRepository _plexLibraryRepository;
        private readonly IPlexMoviesRepository _plexMoviesRepository;
        private readonly IPlexApiService _plexServiceApi;
        private ILogger Log { get; }

        public PlexMovieService(
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

        public async Task AddOrUpdatePlexMoviesAsync(PlexLibrary plexLibrary, List<PlexMovie> movies)
        {
            if (plexLibrary == null)
            {
                Log.Warning($"{nameof(AddOrUpdatePlexMoviesAsync)} => plexLibrary was null");
                return;
            }

            if (movies.Count == 0)
            {
                Log.Warning($"{nameof(AddOrUpdatePlexMoviesAsync)} => servers list was empty");
                return;
            }

            Log.Debug($"{ nameof(AddOrUpdatePlexMoviesAsync)} => Starting adding or updating movies in library: {plexLibrary.Title}");
            // Remove all movies and re-add them
            var currentMovies = await _plexMoviesRepository.FindAllAsync(x => x.PlexLibraryId == plexLibrary.Id);
            await _plexMoviesRepository.RemoveRangeAsync(currentMovies);

            // Ensure the correct ID is added. 
            foreach (var movie in movies)
            {
                movie.PlexLibraryId = plexLibrary.Id;
            }

            // TODO update Roles and tags

            await _plexMoviesRepository.AddRangeAsync(movies);
        }
    }
}
