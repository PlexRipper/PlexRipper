using FluentResults;
using PlexRipper.Domain;

namespace Application.Contracts;

public interface IPlexLibraryService
{
    /// <summary>
    /// Returns the PlexLibrary by the Id, will refresh if the library has no media assigned.
    /// Note: this will not include the media.
    /// </summary>
    /// <param name="libraryId">The id of the <see cref="PlexLibrary"/> to retrieve.</param>
    /// <returns>Valid result if found.</returns>
    Task<Result<PlexLibrary>> GetPlexLibraryAsync(int libraryId);
}