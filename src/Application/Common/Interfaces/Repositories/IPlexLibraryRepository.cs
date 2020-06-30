using PlexRipper.Domain;
using PlexRipper.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlexRipper.Application.Common.Interfaces.Repositories
{
    public interface IPlexLibraryRepository : IRepository<PlexLibrary>
    {
        Task<IList<PlexLibrary>> GetLibraries(int serverId);
    }
}
