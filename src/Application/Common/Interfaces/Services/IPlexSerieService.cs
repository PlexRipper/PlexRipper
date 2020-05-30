using PlexRipper.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlexRipper.Application.Common.Interfaces
{
    public interface IPlexSerieService
    {
        Task AddOrUpdatePlexSeriesAsync(PlexLibrary plexLibrary, List<PlexSerie> series);
    }
}
