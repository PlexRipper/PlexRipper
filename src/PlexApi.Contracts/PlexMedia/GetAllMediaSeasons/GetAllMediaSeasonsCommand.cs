using FastEndpoints;

namespace Reaparr.PlexApi.Contracts;

/// <summary>
/// Fetches all the <see cref="PlexTvShowSeason">Plex TvShow Seasons</see> from the Plex api with the given <see cref="PlexLibrary"/>.
/// </summary>
/// <param name="PlexLibrary"> The <see cref="PlexLibrary"/> to fetch the seasons from.</param>
public record GetAllMediaSeasonsCommand(PlexLibrary PlexLibrary) : ICommand<Result<List<PlexTvShowSeason>>>;
