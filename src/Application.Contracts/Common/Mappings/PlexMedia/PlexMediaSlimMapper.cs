using PlexRipper.Domain;

namespace Application.Contracts;

public static class PlexMediaSlimMapper
{
    #region PlexMovie

    public static PlexMediaSlim ToSlim(this PlexMovie source) => source;

    public static IQueryable<PlexMediaSlim> ProjectToMediaSlim(this IQueryable<PlexMovie> source) =>
        source.Select(x => ToSlim(x));

    #endregion

    #region PlexTvShow

    public static PlexMediaSlim ToSlim(this PlexTvShow source) => source;

    public static IQueryable<PlexMediaSlim> ProjectToMediaSlim(this IQueryable<PlexTvShow> source) =>
        source.Select(x => ToSlimMapper(x));

    private static PlexMediaSlim ToSlimMapper(this PlexTvShow source) => source;

    #endregion

    #region PlexSeason

    public static PlexMediaSlim ToSlim(this PlexTvShowSeason source) => source;

    public static IQueryable<PlexMediaSlim> ProjectToMediaSlim(this IQueryable<PlexTvShowSeason> source) =>
        source.Select(x => ToSlim(x));

    private static PlexMediaSlim ToSlimMapper(this PlexTvShowSeason source) => source;

    #endregion

    #region PlexEpisode

    public static PlexMediaSlim ToSlim(this PlexTvShowEpisode source) => source;

    public static IQueryable<PlexMediaSlim> ProjectToMediaSlim(this IQueryable<PlexTvShowEpisode> source) =>
        source.Select(x => ToSlim(x));

    #endregion
}
