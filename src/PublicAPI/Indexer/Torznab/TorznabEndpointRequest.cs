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
    public string? Query { get; init; }

    /// <summary>
    /// Season number for tvsearch queries.
    /// </summary>
    [QueryParam, BindFrom("season")]
    public int? Season { get; init; }

    /// <summary>
    /// Episode number for tvsearch queries.
    /// </summary>
    [QueryParam, BindFrom("ep")]
    public int? Episode { get; init; }

    /// <summary>
    /// Thetvdb.com series ID for tvsearch queries.
    /// </summary>
    [QueryParam, BindFrom("tvdbid")]
    public int? TvdbId { get; init; }

    /// <summary>
    /// TV Rage ID (legacy, rarely used by Sonarr).
    /// </summary>
    [QueryParam, BindFrom("rid")]
    public int? Rid { get; init; }

    /// <summary>
    /// IMDb ID for movie queries (e.g. "tt1234567").
    /// </summary>
    [QueryParam, BindFrom("imdbid")]
    public string? ImdbId { get; init; }

    /// <summary>
    /// TMDb ID for movie queries.
    /// </summary>
    [QueryParam, BindFrom("tmdbid")]
    public int? TmdbId { get; init; }

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
    public string? ApiKey { get; init; }

    /// <summary>
    /// Max number of results to return.
    /// Sonarr/Radarr will pass this for pagination.
    /// </summary>
    [QueryParam, BindFrom("limit")]
    public int? Limit { get; init; }

    /// <summary>
    /// Offset of results for pagination.
    /// Used together with limit.
    /// </summary>
    [QueryParam, BindFrom("offset")]
    public int? Offset { get; init; }
}
