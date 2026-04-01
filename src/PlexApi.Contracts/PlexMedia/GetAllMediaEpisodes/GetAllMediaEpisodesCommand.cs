namespace Reaparr.PlexApi.Contracts;

/// <summary>
/// Fetches all the <see cref="PlexTvShowEpisode">Plex TvShow Episodes</see> from the Plex api with the given <see cref="PlexLibrary"/>.
/// </summary>
/// <param name="PlexLibrary"> The <see cref="PlexLibrary"/> to fetch the episodes from.</param>
public record GetAllMediaEpisodesCommand(PlexLibrary PlexLibrary) : ICommand<Result<List<PlexTvShowEpisode>>>;
