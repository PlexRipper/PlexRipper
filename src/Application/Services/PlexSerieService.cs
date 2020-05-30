using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Application.Common.Interfaces.Repositories;
using PlexRipper.Domain.Entities;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlexRipper.Application.Services
{
    public class PlexSerieService : IPlexSerieService
    {
        private readonly IPlexLibraryRepository _plexLibraryRepository;
        private readonly IPlexSerieRepository _plexSerieRepository;
        private readonly IPlexApiService _plexServiceApi;
        private ILogger Log { get; }

        public PlexSerieService(
            IPlexApiService plexServiceApi,
            IPlexLibraryRepository plexLibraryRepository,
            IPlexSerieRepository plexSerieRepository,
            ILogger logger)
        {
            _plexLibraryRepository = plexLibraryRepository;
            _plexSerieRepository = plexSerieRepository;
            _plexServiceApi = plexServiceApi;
            Log = logger;
        }

        public async Task AddOrUpdatePlexSeriesAsync(PlexLibrary plexLibrary, List<PlexSerie> series)
        {
            if (plexLibrary == null)
            {
                Log.Warning($"{nameof(AddOrUpdatePlexSeriesAsync)} => plexLibrary was null");
                return;
            }

            if (series.Count == 0)
            {
                Log.Warning($"{nameof(AddOrUpdatePlexSeriesAsync)} => servers list was empty");
                return;
            }

            Log.Debug($"{ nameof(AddOrUpdatePlexSeriesAsync)} => Starting adding or updating series in library: {plexLibrary.Title}");
            // Remove all series and re-add them
            var currentMovies = await _plexSerieRepository.FindAllAsync(x => x.PlexLibraryId == plexLibrary.Id);
            await _plexSerieRepository.RemoveRangeAsync(currentMovies);

            // Ensure the correct ID is added. 
            foreach (var serie in series)
            {
                serie.PlexLibraryId = plexLibrary.Id;
            }

            // TODO update Roles and tags

            await _plexSerieRepository.AddRangeAsync(series);
        }

    }
}
