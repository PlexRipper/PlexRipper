using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain.Interfaces;

namespace PlexRipper.Application.Common.Interfaces.Repositories
{
    public interface IPlexLibraryRepository : IRepository<PlexLibrary>
    {
        Task<IEnumerable<PlexLibrary>> GetLibraries(int serverId);
    }
}
