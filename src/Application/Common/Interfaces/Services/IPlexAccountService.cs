using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Domain;

namespace PlexRipper.Application.Common
{
    public interface IPlexAccountService
    {
        /// <summary>
        /// Checks if this account is valid by querying the Plex API. If Result.Ok(true) then 2FA is enabled,
        /// Result.Ok(false) then the account was valid with no 2FA and will be Result.Fail with any other error.
        /// </summary>
        /// <param name="plexAccount">Requires the Username, Password and ClientId</param>
        /// <returns>Result based on validity of the credentials.</returns>
        Task<Result<bool>> ValidatePlexAccountAsync(PlexAccount plexAccount);

        /// <summary>
        /// Checks if an <see cref="PlexAccount"/> with the same username already exists.
        /// </summary>
        /// <param name="username">The username to check for.</param>
        /// <returns>true if username is available.</returns>
        Task<Result<bool>> CheckIfUsernameIsAvailableAsync(string username);

        /// <summary>
        /// Returns the <see cref="PlexAccount"/> with the corresponding <see cref="PlexAccount"/>
        /// and the accessible <see cref="PlexServer">PlexServers</see>.
        /// </summary>
        /// <param name="accountId">The Id to retrieve the <see cref="PlexAccount"/> by.</param>
        /// <returns>The account found.</returns>
        Task<Result<PlexAccount>> GetPlexAccountAsync(int accountId);

        Task<Result<List<PlexAccount>>> GetAllPlexAccountsAsync(bool onlyEnabled = false);

        /// <summary>
        /// Creates an <see cref="PlexAccount"/> in the Database and performs an SetupAccountAsync().
        /// </summary>
        /// <param name="plexAccount">The unique account.</param>
        /// <returns>Returns the added account after setup.</returns>
        Task<Result<PlexAccount>> CreatePlexAccountAsync(PlexAccount plexAccount);

        /// <summary>
        /// Hard deletes the <see cref="PlexAccount"/> from the Database.
        /// </summary>
        /// <param name="plexAccountId"></param>
        /// <returns></returns>
        Task<Result> DeletePlexAccountAsync(int plexAccountId);

        Task<Result<PlexAccount>> UpdatePlexAccountAsync(PlexAccount plexAccount);

        Task<Result<PlexAccount>> UpdatePlexAccountAsync(dynamic plexAccountDto);

        /// <summary>
        /// Refreshes the <see cref="PlexServer"/> and <see cref="PlexLibrary"/> access of the <see cref="PlexAccount"/>.
        /// </summary>
        /// <param name="plexAccountId">Can be 0 to refresh all enabled PlexAccounts.</param>
        /// <returns>If successful.</returns>
        Task<Result> RefreshPlexAccount(int plexAccountId = 0);

        /// <summary>
        /// This retrieves all the <see cref="PlexAccount"/> related data from the PlexApi.
        /// It's assumed that the <see cref="PlexAccount"/> has already been created in the Database.
        /// </summary>
        /// <param name="plexAccountId">The is of <see cref="PlexAccount"/> to setup.</param>
        /// <returns>The list of <see cref="PlexServer">PlexServers</see> which are accessible by this account.</returns>
        Task<Result<List<PlexServer>>> SetupAccountAsync(int plexAccountId);
    }
}