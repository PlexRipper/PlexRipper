using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Domain;

namespace PlexRipper.Application.Common
{
    public interface IPlexServerService
    {
        Task<Result<List<PlexServer>>> GetServersAsync(PlexAccount plexAccount, bool refresh = false);

        Task<Result<bool>> RefreshPlexServersAsync(PlexAccount plexAccount);

        Task<Result<PlexServer>> GetServerAsync(int plexServerId);

        Task<Result<PlexServerStatus>> CheckPlexServerStatusAsync(int plexServerId, int plexAccountId = 0);

        /// <summary>
        /// Retrieves all <see cref="PlexServer"/>s from the Database with the included <see cref="PlexLibrary"/>.
        /// </summary>
        /// <param name="plexAccountId">Retrieve only the <see cref="PlexServer"/> which are accessible by this <see cref="PlexAccount"/>.</param>
        /// <returns>The list of <see cref="PlexServer"/>s.</returns>
        Task<Result<List<PlexServer>>> GetServersAsync(int plexAccountId = 0);
    }
}