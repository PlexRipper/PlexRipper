using Application.Contracts;
using PlexRipper.Application.Contracts;
using Settings.Contracts;
using Data.Contracts;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain;

namespace PlexRipper.Application;

public class TorznabSearchEndpoint : BaseEndpoint<TorznabSearchRequest, string>
{
    private readonly IUserSettings _userSettings;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly ITorznabAuthenticationService _torznabAuth;

    public TorznabSearchEndpoint(IUserSettings userSettings, IPlexRipperDbContext dbContext, ITorznabAuthenticationService torznabAuth)
    {
        _userSettings = userSettings;
        _dbContext = dbContext;
        _torznabAuth = torznabAuth;
    }

    public override void Configure()
    {
        Get("/api/torznab");
        AllowAnonymous(); // We'll handle API key authentication in the handler
        Summary(s =>
        {
            s.Summary = "Torznab search endpoint";
            s.Description = "Search for media across Plex servers using Torznab protocol";
        });
    }

    public override async Task<string> ExecuteAsync(TorznabSearchRequest req, CancellationToken ct)
    {
        // Handle capabilities request
        if (string.IsNullOrEmpty(req.T) || req.T.ToLower() == "caps")
        {
            var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
            var maxResults = _userSettings.TorznabSettings.MaxResultsPerRequest;
            
            var capabilities = TorznabXmlFormatter.CreateDefaultCapabilities(baseUrl, maxResults);
            var xml = TorznabXmlFormatter.SerializeToXml(capabilities);

            await SendStringAsync(xml, contentType: "application/xml", cancellation: ct);
            return xml;
        }

        // Validate API key for search requests
        if (_torznabAuth.IsEnabled)
        {
            if (!_torznabAuth.ValidateApiKey(req.ApiKey))
            {
                await SendUnauthorizedAsync(ct);
                return string.Empty;
            }
        }
        else
        {
            await SendErrorsAsync(503, ct); // Service Unavailable
            return string.Empty;
        }

        // Handle different search types
        var results = req.T?.ToLower() switch
        {
            "search" => await PerformGeneralSearch(req, ct),
            "movie" => await PerformMovieSearch(req, ct),
            "tvsearch" => await PerformTvSearch(req, ct),
            _ => new List<TorznabItem>()
        };

        var response = new TorznabRss
        {
            Channel = new TorznabChannel
            {
                Title = "PlexRipper Search Results",
                Description = $"Search results for: {req.Q}",
                Items = results
            }
        };

        var xml = TorznabXmlFormatter.SerializeToXml(response);
        await SendStringAsync(xml, contentType: "application/xml", cancellation: ct);
        return xml;
    }

    private async Task<List<TorznabItem>> PerformGeneralSearch(TorznabSearchRequest req, CancellationToken ct)
    {
        var results = new List<TorznabItem>();
        var limit = Math.Min(req.Limit ?? 100, _userSettings.TorznabSettings.MaxResultsPerRequest);
        var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";

        // Search movies
        if (!string.IsNullOrEmpty(req.Q))
        {
            var movies = await SearchMovies(req.Q, limit / 2, ct);
            results.AddRange(movies.Select(m => m.ToTorznabItem($"{baseUrl}/api/torznab/download/{m.Id}", baseUrl)));

            // Search TV shows
            var episodes = await SearchTvEpisodes(req.Q, limit / 2, ct);
            results.AddRange(episodes.Select(e => e.ToTorznabItem($"{baseUrl}/api/torznab/download/{e.Id}", baseUrl)));
        }

        return results.Take(limit).ToList();
    }

    private async Task<List<TorznabItem>> PerformMovieSearch(TorznabSearchRequest req, CancellationToken ct)
    {
        var limit = Math.Min(req.Limit ?? 100, _userSettings.TorznabSettings.MaxResultsPerRequest);
        var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";

        List<PlexMovie> movies;

        if (!string.IsNullOrEmpty(req.ImdbId))
        {
            movies = await SearchMoviesByImdbId(req.ImdbId, limit, ct);
        }
        else if (!string.IsNullOrEmpty(req.Q))
        {
            movies = await SearchMovies(req.Q, limit, ct);
        }
        else
        {
            movies = new List<PlexMovie>();
        }

        return movies.Select(m => m.ToTorznabItem($"{baseUrl}/api/torznab/download/{m.Id}", baseUrl)).ToList();
    }

    private async Task<List<TorznabItem>> PerformTvSearch(TorznabSearchRequest req, CancellationToken ct)
    {
        var limit = Math.Min(req.Limit ?? 100, _userSettings.TorznabSettings.MaxResultsPerRequest);
        var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";

        List<PlexTvShowEpisode> episodes;

        if (!string.IsNullOrEmpty(req.Q))
        {
            episodes = await SearchTvEpisodes(req.Q, limit, ct, req.Season, req.Ep);
        }
        else
        {
            episodes = new List<PlexTvShowEpisode>();
        }

        return episodes.Select(e => e.ToTorznabItem($"{baseUrl}/api/torznab/download/{e.Id}", baseUrl)).ToList();
    }

    private async Task<List<PlexMovie>> SearchMovies(string query, int limit, CancellationToken ct)
    {
        var enabledServerIds = _userSettings.TorznabSettings.EnabledServerIds;
        
        return await _dbContext.PlexMovies
            .Where(m => (enabledServerIds.Count == 0 || enabledServerIds.Contains(m.PlexServerId)) &&
                       m.Title.Contains(query))
            .Take(limit)
            .ToListAsync(ct);
    }

    private async Task<List<PlexMovie>> SearchMoviesByImdbId(string imdbId, int limit, CancellationToken ct)
    {
        var enabledServerIds = _userSettings.TorznabSettings.EnabledServerIds;
        
        return await _dbContext.PlexMovies
            .Where(m => (enabledServerIds.Count == 0 || enabledServerIds.Contains(m.PlexServerId)) &&
                       m.ImdbId == imdbId)
            .Take(limit)
            .ToListAsync(ct);
    }

    private async Task<List<PlexTvShowEpisode>> SearchTvEpisodes(string query, int limit, CancellationToken ct, int? season = null, int? episode = null)
    {
        var enabledServerIds = _userSettings.TorznabSettings.EnabledServerIds;
        
        var queryable = _dbContext.PlexTvShowEpisodes
            .Include(e => e.TvShow)
            .Where(e => (enabledServerIds.Count == 0 || enabledServerIds.Contains(e.PlexServerId)) &&
                       (e.TvShow.Title.Contains(query) || e.Title.Contains(query)));

        if (season.HasValue)
            queryable = queryable.Where(e => e.ParentIndex == season.Value);

        if (episode.HasValue)
            queryable = queryable.Where(e => e.Index == episode.Value);

        return await queryable.Take(limit).ToListAsync(ct);
    }
}

public class TorznabSearchRequest
{
    public string? T { get; set; }
    public string? Q { get; set; }
    public string? ApiKey { get; set; }
    public int? Limit { get; set; }
    public int? Offset { get; set; }
    public string? Cat { get; set; }
    public string? ImdbId { get; set; }
    public string? TmdbId { get; set; }
    public string? TvdbId { get; set; }
    public int? Season { get; set; }
    public int? Ep { get; set; }
}