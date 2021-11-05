using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Domain;

namespace PlexRipper.Application.Common
{
    public interface IPlexAuthenticationService
    {
        Task<string> GetPlexApiTokenAsync(PlexAccount plexAccount);

        /// <summary>
        /// Returns the authentication token needed to authenticate communication with the <see cref="PlexServer"/>.
        /// Note: If no plexAccountId is specified then it will search for a valid <see cref="PlexAccount"/> automatically.
        /// </summary>
        /// <param name="plexServerId">The id of the <see cref="PlexServer"/> to retrieve a token for.</param>
        /// <param name="plexAccountId">The id of the <see cref="PlexAccount"/> to authenticate with.</param>
        /// <returns>The authentication token.</returns>
        Task<Result<string>> GetPlexServerTokenAsync(int plexServerId, int plexAccountId = 0);

        Task<Result<string>> GetPlexServerTokenWithUrl(int plexServerId, string serverUrl, int plexAccountId = 0);
    }
}