using PlexRipper.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlexRipper.Application.Common.Interfaces
{
    public interface IPlexLibraryService
    {
        Task<bool> RefreshLibrariesAsync(PlexServer plexServer);
        Task<List<PlexLibrary>> GetPlexLibrariesAsync(PlexServer plexServer, bool refresh = false);

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
        Task<PlexLibrary> RefreshLibraryMediaAsync(PlexLibrary plexLibrary);

        Task<PlexLibrary> GetPlexLibraryAsync(int libraryId, bool refresh = false);

        /// <summary>
        /// Returns the <see cref="PlexLibrary"/> with the media content.
        /// </summary>
        /// <param name="plexLibrary"></param>
        /// <param name="refresh"></param>
        /// <returns></returns>
        Task<PlexLibrary> GetLibraryMediaAsync(PlexLibrary plexLibrary, bool refresh = false);

        Task<PlexMediaMetaData> GetMetaDataAsync(PlexMovie movie);
    }
}
