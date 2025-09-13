using FastEndpoints;

namespace Reaparr.PublicAPI;

public record TorznabEndpointRequest
{
    /// <summary>
    /// Operation type selector.
    /// Possible values: "caps", "search", "tvsearch", "movie".
    /// </summary>
    [QueryParam, BindFrom("t")]
    public string? Type { get; init; }

    /// <summary>
    /// Free text search query.
    /// Used in search, tvsearch, and movie queries.
    /// </summary>
    [QueryParam, BindFrom("q")]
    public string Query { get; init; } = string.Empty;

    /// <summary>
    /// Season number for tvsearch queries.
    /// </summary>
    [QueryParam, BindFrom("season")]
    public int Season { get; init; } = 0;

    /// <summary>
    /// Episode number for tvsearch queries.
    /// </summary>
    [QueryParam, BindFrom("ep")]
    public int Episode { get; init; } = 0;

    /// <summary>
    /// Thetvdb.com series ID for tvsearch queries.
    /// </summary>
    [QueryParam, BindFrom("tvdbid")]
    public int TvdbId { get; init; } = 0;

    /// <summary>
    /// TV Rage ID.
    /// </summary>
    [QueryParam, BindFrom("rid")]
    public int Rid { get; init; } = 0;

    /// <summary>
    /// IMDb ID for movie queries (e.g. "tt1234567").
    /// </summary>
    [QueryParam, BindFrom("imdbid")]
    public int ImdbId { get; init; } = 0;

    /// <summary>
    /// TMDb ID for movie queries.
    /// </summary>
    [QueryParam, BindFrom("tmdbid")]
    public int TmdbId { get; init; } = 0;

    /// <summary>
    /// Optional author field (used by Readarr for book search).
    /// Not required for Sonarr/Radarr.
    /// </summary>
    [QueryParam, BindFrom("author")]
    public string? Author { get; init; }

    /// <summary>
    /// Optional title field (used by Readarr for book search).
    /// Not required for Sonarr/Radarr.
    /// </summary>
    [QueryParam, BindFrom("title")]
    public string? Title { get; init; }

    /// <summary>
    /// Optional ISBN field (used by Readarr for book search).
    /// Not required for Sonarr/Radarr.
    /// </summary>
    [QueryParam, BindFrom("isbn")]
    public string? Isbn { get; init; }

    /// <summary>
    /// API key provided by the requesting client (Sonarr/Radarr).
    /// </summary>
    [QueryParam, BindFrom("apikey")]
    public string ApiKey { get; init; } = string.Empty;

    /// <summary>
    /// Max number of results to return.
    /// Sonarr/Radarr will pass this for pagination.
    /// </summary>
    [QueryParam, BindFrom("limit")]
    public int Limit { get; init; } = 50;

    /// <summary>
    /// Offset of results for pagination.
    /// Used together with limit.
    /// </summary>
    [QueryParam, BindFrom("offset")]
    public int Offset { get; init; } = 0;
}
