using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRipper.Domain.Entities;

namespace PlexRipper.Application.Common.Interfaces
{
    public interface IPlexMovieService
    {
        Task AddOrUpdatePlexMoviesAsync(PlexLibrary plexLibrary, List<PlexMovie> movies);
    }
}