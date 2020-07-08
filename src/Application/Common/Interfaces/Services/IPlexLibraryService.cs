using FluentResults;
using PlexRipper.Domain.Entities;
using System.Threading.Tasks;

namespace PlexRipper.Application.Common.Interfaces
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
        /// <param name="plexLibrary"></param>
        /// <returns>Returns the PlexLibrary with the containing media</returns>
        Task<Result<PlexLibrary>> RefreshLibraryMediaAsync(PlexAccount plexAccount, PlexLibrary plexLibrary);

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
        /// <param name="plexAccountId"></param>
        /// <param name="libraryId"></param>
        /// <returns></returns>
        Task<Result<PlexLibrary>> GetPlexLibraryAsync(int plexAccountId, int libraryId);

        Task<Result<PlexLibrary>> RefreshLibraryMediaAsync(int plexAccountId,
            int plexLibraryId);
    }
}
