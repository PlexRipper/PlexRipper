using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain;

namespace Data.Contracts;

public static partial class DbSetExtensions
{
    public static IQueryable<PlexTvShowEpisode> IncludeAll(this IQueryable<PlexTvShowEpisode> plexTvShowEpisodes) =>
        plexTvShowEpisodes
            .IncludePlexServer()
            .IncludePlexLibrary()
            .Include($"{nameof(PlexTvShowEpisode.TvShowSeason)}.{nameof(PlexTvShowSeason.PlexServer)}")
            .Include($"{nameof(PlexTvShowEpisode.TvShowSeason)}.{nameof(PlexTvShowSeason.PlexLibrary)}")
            .Include($"{nameof(PlexTvShowEpisode.TvShow)}.{nameof(PlexTvShow.PlexServer)}")
            .Include($"{nameof(PlexTvShowEpisode.TvShow)}.{nameof(PlexTvShow.PlexLibrary)}");

    public static IQueryable<PlexTvShowEpisode> IncludePlexLibrary(
        this IQueryable<PlexTvShowEpisode> plexTvShowEpisode
    ) => plexTvShowEpisode.Include(x => x.PlexLibrary);

    public static IQueryable<PlexTvShowEpisode> IncludePlexServer(
        this IQueryable<PlexTvShowEpisode> plexTvShowEpisode
    ) => plexTvShowEpisode.Include(x => x.PlexServer).ThenInclude(x => x!.PlexServerConnections);
}