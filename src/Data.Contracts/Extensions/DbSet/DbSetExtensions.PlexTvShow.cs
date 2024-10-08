using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain;

namespace Data.Contracts;

public static partial class DbSetExtensions
{
    public static IQueryable<PlexTvShow> IncludeAll(this IQueryable<PlexTvShow> plexTvShows) => plexTvShows
        .Include(x => x.PlexServer)
        .Include(x => x.PlexLibrary)
        .Include($"{nameof(PlexTvShow.Seasons)}.{nameof(PlexTvShow.PlexServer)}")
        .Include($"{nameof(PlexTvShow.Seasons)}.{nameof(PlexTvShow.PlexLibrary)}")
        .Include(
            $"{nameof(PlexTvShow.Seasons)}.{nameof(PlexTvShowSeason.Episodes)}.{nameof(PlexTvShow.PlexServer)}"
        )
        .Include(
            $"{nameof(PlexTvShow.Seasons)}.{nameof(PlexTvShowSeason.Episodes)}.{nameof(PlexTvShow.PlexLibrary)}"
        );

    public static IQueryable<PlexTvShowSeason> IncludeAll(this IQueryable<PlexTvShowSeason> plexTvShowSeasons) =>
        plexTvShowSeasons.Include(x => x.TvShow).IncludePlexServer().IncludePlexLibrary().IncludeEpisodes();
}