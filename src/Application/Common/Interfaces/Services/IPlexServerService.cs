using FluentResults;
using PlexRipper.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlexRipper.Application.Common.Interfaces
{
    public interface IPlexServerService
    {
        Task<Result<List<PlexServer>>> GetServersAsync(PlexAccount plexAccount, bool refresh = false);
        Task<Result<List<PlexServer>>> RefreshPlexServersAsync(PlexAccount plexAccount);

        Task<Result<PlexServer>> GetAllLibraryMediaAsync(PlexAccount plexAccount, PlexServer plexServer,
            bool refresh = false);

        Task<Result<PlexServer>> GetServerAsync(int plexServerId);

        /// <summary>
        /// Check if the <see cref="PlexServer"/> is available and log the status
        /// </summary>
        /// <param name="plexAccount"><see cref="PlexAccount"/> to use for authentication</param>
        /// <param name="plexServer">Which <see cref="PlexServer"/> to check</param>
        /// <returns></returns>
        Task<Result<PlexServerStatus>> GetPlexServerStatusAsync(PlexAccount plexAccount, PlexServer plexServer);

        /// <summary>
        /// Retrieves all <see cref="PlexServer"/>s from the Database with the included <see cref="PlexLibrary"/>s.
        /// </summary>
        /// <returns>The list of <see cref="PlexServer"/>s</returns>
        Task<Result<List<PlexServer>>> GetServersAsync();
    }
}
