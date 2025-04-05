using System.Net;

namespace PlexRipper.BaseTests;

public class PlexApiDataConfig : BaseConfig<PlexApiDataConfig>
{
    public Seed Seed { get; set; } = new(9999);

    public int MovieLibraryCount { get; set; } = 0;

    public int TvShowLibraryCount { get; set; } = 0;

    public int MoviesPerLibraryCount { get; set; } = 0;

    public int TvShowsPerLibraryCount { get; set; } = 0;

    public int SeasonsPerTvShowCount { get; set; } = 0;

    public int EpisodesPerSeasonCount { get; set; } = 0;

    public int PlexServerAccessCount { get; set; } = 5;

    public int PlexServerAccessConnectionsCount { get; set; } = 5;

    public bool PlexServerAccessConnectionsIncludeHttps { get; set; } = false;

    public HttpStatusCode SetServerResourcesResponse { get; set; } = HttpStatusCode.OK;

    public int LibraryCount(PlexMediaType type = PlexMediaType.Unknown)
    {
        return type switch
        {
            PlexMediaType.Movie => MovieLibraryCount,
            PlexMediaType.TvShow => TvShowLibraryCount,
            _ => Math.Max(MovieLibraryCount, TvShowLibraryCount),
        };
    }
}
