using FluentResults;
using PlexRipper.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlexRipper.Application.Common.Interfaces
{
    public interface IPlexLibraryService
    {
        Task<List<PlexLibrary>> GetPlexLibrariesAsync(PlexAccount plexAccount, PlexServer plexServer, bool refresh = false);

        /// <summary>
        /// Returns the <see cref="PlexLibrary"/> containing the media content.
        /// </summary>
        /// <param name="plexServer"></param>
        /// <param name="libraryKey"></param>
        /// <returns></returns>
        Task<PlexLibrary> GetLibraryMediaAsync(PlexServer plexServer, string libraryKey, bool refresh = false);

        /// <summary>
        /// Retrieves the new media metadata from the PlexApi
        /// </summary>
        /// <param name="plexLibrary"></param>
        /// <returns>Returns the PlexLibrary with the containing media</returns>
        Task<Result<PlexLibrary>> RefreshLibraryMediaAsync(PlexLibrary plexLibrary);

        Task<PlexLibrary> GetPlexLibraryAsync(int libraryId, bool refresh = false);

        /// <summary>
        /// Returns the <see cref="PlexLibrary"/> with the media content.
        /// </summary>
        /// <param name="plexLibrary"></param>
        /// <param name="refresh"></param>
        /// <returns></returns>
        Task<PlexLibrary> GetLibraryMediaAsync(PlexLibrary plexLibrary, bool refresh = false);

        Task<PlexMediaMetaData> GetMetaDataAsync(PlexMovie movie);

        /// <summary>
        /// Retrieve the latest <see cref="PlexLibrary">PlexLibraries</see> for this <see cref="PlexServer"/> which the <see cref="PlexAccount"/> has access to and update the database.
        /// </summary>
        /// <param name="plexAccount"></param>
        /// <param name="plexServer"></param>
        /// <returns>If successful</returns>
        Task<Result<bool>> RefreshLibrariesAsync(PlexAccount plexAccount, PlexServer plexServer);
    }
}
