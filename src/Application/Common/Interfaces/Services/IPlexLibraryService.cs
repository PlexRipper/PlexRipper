using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Domain.Entities;

namespace PlexRipper.Application.Common
{
    public interface IPlexLibraryService
    {

        /// <summary>
        /// Returns the <see cref="PlexLibrary"/> containing the media content.
        /// </summary>
        /// <param name="plexAccount"></param>
        /// <param name="plexServer"></param>
        /// <param name="libraryKey"></param>
        /// <param name="refresh"></param>
        /// <returns></returns>
        Task<PlexLibrary> GetLibraryMediaAsync(PlexAccount plexAccount, PlexServer plexServer, string libraryKey,
            bool refresh = false);

        /// <summary>
        /// Retrieves the new media metadata from the PlexApi
        /// </summary>
        /// <param name="plexAccount"></param>
        /// <param name="newPlexLibrary"></param>
        /// <returns>Returns the PlexLibrary with the containing media</returns>
        Task<Result<PlexLibrary>> RefreshLibraryMediaAsync(PlexAccount plexAccount, PlexLibrary newPlexLibrary);

        /// <summary>
        /// Returns the <see cref="PlexLibrary"/> with the media content.
        /// </summary>
        /// <param name="plexAccount"></param>
        /// <param name="plexLibrary"></param>
        /// <param name="refresh"></param>
        /// <returns></returns>
        Task<PlexLibrary> GetLibraryMediaAsync(PlexAccount plexAccount, PlexLibrary plexLibrary, bool refresh = false);

        /// <summary>
        /// Retrieve the latest <see cref="PlexLibrary">PlexLibraries</see> for this <see cref="PlexServer"/> which the <see cref="PlexAccount"/> has access to and update the database.
        /// </summary>
        /// <param name="plexAccount"></param>
        /// <param name="plexServer"></param>
        /// <returns>If successful</returns>
        Task<Result<bool>> RefreshLibrariesAsync(PlexAccount plexAccount, PlexServer plexServer);

        Task<Result<PlexMediaMetaData>> GetMetaDataAsync(PlexAccount plexAccount, PlexMovie movie);


        /// <summary>
        /// Return the PlexLibrary by the Id, will refresh if the library has no media assigned.
        /// </summary>
        /// <param name="libraryId"></param>
        /// <param name="plexAccountId"></param>
        /// <returns></returns>
        Task<Result<PlexLibrary>> GetPlexLibraryAsync(int libraryId, int plexAccountId);

        Task<Result<PlexLibrary>> RefreshLibraryMediaAsync(int plexAccountId,
            int plexLibraryId);
    }
}
