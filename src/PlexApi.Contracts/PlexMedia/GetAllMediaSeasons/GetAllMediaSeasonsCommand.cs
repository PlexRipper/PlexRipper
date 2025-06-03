using FastEndpoints;
using FluentResults;
using PlexRipper.Domain;

namespace PlexApi.Contracts;

/// <summary>
/// Fetches all the <see cref="PlexTvShowSeason">Plex TvShow Seasons</see> from the Plex api with the given <see cref="PlexLibrary"/>.
/// </summary>
/// <param name="PlexLibrary"> The <see cref="PlexLibrary"/> to fetch the seasons from.</param>
/// <param name="Action"> Progress action callback to notify of connection attempt progress.</param>
/// <returns></returns>
public record GetAllMediaSeasonsCommand(PlexLibrary PlexLibrary, Action<MediaSyncProgress> Action)
    : ICommand<Result<List<PlexTvShowSeason>>>;
