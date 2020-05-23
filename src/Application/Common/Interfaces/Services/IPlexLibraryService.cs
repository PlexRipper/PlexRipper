using PlexRipper.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlexRipper.Application.Common.Interfaces
{
    public interface IPlexLibraryService
    {
        Task<bool> RefreshLibrariesAsync(PlexServer plexServer);
        Task<List<PlexLibrary>> GetPlexLibrariesAsync(PlexServer plexServer, bool refresh = false);
    }
}
