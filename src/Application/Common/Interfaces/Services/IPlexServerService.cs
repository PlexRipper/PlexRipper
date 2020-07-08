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
        /// <param name="plexServer"></param>
        /// <returns></returns>
        Task<Result<PlexServerStatus>> GetPlexServerStatusAsync(PlexAccount plexAccount, PlexServer plexServer);
    }
}
