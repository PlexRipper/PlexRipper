using Application.Contracts;
using FluentResults;
using PlexRipper.Domain;

namespace PlexApi.Contracts;

public interface IPlexApiService
{
    /// <summary>
    /// Fetches the PlexLibrary container with either Movies, Series, Music or Photos media depending on the type.
    /// Id and PlexServerId are copied over from the input parameter.
    /// </summary>
    /// <param name="plexLibrary"> The <see cref="PlexLibrary"/> to fetch the media from.</param>
    /// <param name="action"> Progress action callback to notify of connection attempt progress.</param>
    /// <param name="cancellationToken"> The <see cref="CancellationToken"/> to use.</param>
    /// <returns></returns>
    Task<Result<LibraryMetadata>> GetLibraryMediaAsync(
        PlexLibrary plexLibrary,
        Action<MediaSyncProgress>? action = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Fetches all the <see cref="PlexTvShowSeason">Plex TvShow Seasons</see> from the Plex api with the given <see cref="PlexLibrary"/>.
    /// </summary>
    /// <param name="plexLibrary"> The <see cref="PlexLibrary"/> to fetch the seasons from.</param>
    /// <param name="action"> Progress action callback to notify of connection attempt progress.</param>
    /// <param name="cancellationToken"> The <see cref="CancellationToken"/> to use.</param>
    /// <returns></returns>
    Task<Result<List<PlexTvShowSeason>>> GetAllSeasonsAsync(
        PlexLibrary plexLibrary,
        Action<MediaSyncProgress>? action = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Fetches all the <see cref="PlexTvShowEpisode">Plex TvShow Episodes</see> from the Plex api with the given <see cref="PlexLibrary"/>.
    /// </summary>
    /// <param name="plexLibrary"> The <see cref="PlexLibrary"/> to fetch the episodes from.</param>
    /// <param name="action"> Progress action callback to notify of connection attempt progress.</param>
    /// <param name="cancellationToken"> The <see cref="CancellationToken"/> to use.</param>
    /// <returns></returns>
    Task<Result<List<PlexTvShowEpisode>>> GetAllEpisodesAsync(
        PlexLibrary plexLibrary,
        Action<MediaSyncProgress>? action = null,
        CancellationToken cancellationToken = default
    );
}
