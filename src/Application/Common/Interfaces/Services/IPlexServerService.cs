using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Domain;

namespace PlexRipper.Application.Common
{
    public interface IPlexServerService
    {
        Task<Result> RetrieveAccessiblePlexServersAsync(PlexAccount plexAccount);

        Task<Result<PlexServer>> GetServerAsync(int plexServerId);

        Task<Result<PlexServerStatus>> CheckPlexServerStatusAsync(int plexServerId, int plexAccountId = 0, bool trimEntries = true);


        Task<Result> RemoveInaccessibleServers();

        /// <summary>
        /// Retrieves all <see cref="PlexServer"/>s from the Database with the included <see cref="PlexLibrary"/>.
        /// </summary>
        /// <param name="plexAccountId">Retrieve only the <see cref="PlexServer"/> which are accessible by this <see cref="PlexAccount"/>.</param>
        /// <returns>The list of <see cref="PlexServer"/>s.</returns>
        Task<Result<List<PlexServer>>> GetAllPlexServersAsync(bool includeLibraries, int plexAccountId = 0);

        Task<Result> InspectPlexServers(int plexAccountId, List<int> plexServerIds);

        Task<Result> SyncPlexServers();

        Task<Result> SyncPlexServer(int plexServerId);
    }
}