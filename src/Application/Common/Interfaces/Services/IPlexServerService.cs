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
        /// <param name="includeLibraries">Include the nested <see cref="PlexLibrary">PlexLibraries</see>.</param>
        /// <param name="plexAccountId">Retrieve only the <see cref="PlexServer"/> which are accessible by this <see cref="PlexAccount"/>.</param>
        /// <returns>The list of <see cref="PlexServer">PlexServers</see>.</returns>
        Task<Result<List<PlexServer>>> GetAllPlexServersAsync(bool includeLibraries, int plexAccountId = 0);

        Task<Result> InspectPlexServers(int plexAccountId, List<int> plexServerIds);

        Task<Result> SyncPlexServers(bool forceSync = false);

        Task<Result> SyncPlexServers(List<int> plexServerIds, bool forceSync = false);

        /// <summary>
        /// Take all <see cref="PlexLibrary">PlexLibraries.</see> and retrieve all data to then store in the database.
        /// </summary>
        /// <param name="plexServerId">The id of the <see cref="PlexServer"/> to use.</param>
        /// <param name="forceSync">By default, the libraries which have been synced less than 6 hours ago will be skipped. </param>
        /// <returns><see cref="Result"/></returns>
        Task<Result> SyncPlexServer(int plexServerId, bool forceSync = false);
    }
}