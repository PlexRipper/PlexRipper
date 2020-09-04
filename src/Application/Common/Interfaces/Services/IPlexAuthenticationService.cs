using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Domain.Entities;

namespace PlexRipper.Application.Common
{
    public interface IPlexAuthenticationService
    {
        Task<string> GetPlexTokenAsync(PlexAccount plexAccount);

        /// <summary>
        /// Returns the authentication token needed to communicate with a <see cref="PlexServer"/>
        /// </summary>
        /// <param name="plexAccountId"></param>
        /// <param name="plexServerId"></param>
        /// <returns></returns>
        Task<Result<string>> GetPlexServerTokenAsync(int plexAccountId, int plexServerId);

        Task<Result<string>> GetPlexServerTokenWithUrl(int plexAccountId, int plexServerId, string downloadUrl);
    }
}
