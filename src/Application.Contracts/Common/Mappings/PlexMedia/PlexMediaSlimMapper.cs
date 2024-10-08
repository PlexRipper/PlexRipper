using PlexRipper.Domain;

namespace Application.Contracts;

public static class PlexMediaSlimMapper
{
    public static PlexMediaSlim ToSlim(this PlexMovie source) => source;

    public static IQueryable<PlexMediaSlim> ProjectToMediaSlim(this IQueryable<PlexMovie> source) =>
        source.Select(x => ToSlim(x));

    public static IQueryable<PlexMediaSlim> ProjectToMediaSlim(this IQueryable<PlexTvShow> source) =>
        source.Select(x => ToSlimMapper(x));

    private static PlexMediaSlim ToSlimMapper(this PlexTvShow source) => source;
}
