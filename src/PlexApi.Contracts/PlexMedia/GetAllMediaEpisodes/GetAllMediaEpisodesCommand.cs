using FastEndpoints;
using FluentResults;
using PlexRipper.Domain;

namespace PlexApi.Contracts;

/// <summary>
/// Fetches all the <see cref="PlexTvShowEpisode">Plex TvShow Episodes</see> from the Plex api with the given <see cref="PlexLibrary"/>.
/// </summary>
/// <param name="PlexLibrary"> The <see cref="PlexLibrary"/> to fetch the episodes from.</param>
/// <param name="Action"> Progress action callback to notify of connection attempt progress.</param>
/// <returns></returns>
public record GetAllMediaEpisodesCommand(PlexLibrary PlexLibrary, Action<MediaSyncProgress>? Action = null)
    : ICommand<Result<List<PlexTvShowEpisode>>>;
