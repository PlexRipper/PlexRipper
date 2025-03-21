using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using Settings.Contracts;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;
using PlexRipper.Domain;

namespace PlexRipper.Application.Torznab;

/// <summary>
/// Request model for Torznab API requests from Sonarr/Radarr
/// </summary>
public record TorznabSearchRequest
{
    [QueryParam, BindFrom("apikey")]
    public string? ApiKey { get; init; }

    [QueryParam, BindFrom("q")]
    public string? Query { get; init; }

    [QueryParam, BindFrom("t")]
    public string? Type { get; init; } // search tvsearch movie

    [QueryParam, BindFrom("cat")]
    public string? Categories { get; init; }

    [QueryParam, BindFrom("limit")]
    public int? Limit { get; init; }

    [QueryParam, BindFrom("offset")]
    public int? Offset { get; init; }

    [QueryParam, BindFrom("extended")]
    public int? Extended { get; init; }
}

public class TorznabSearchRequestValidator : Validator<TorznabSearchRequest>
{
    public TorznabSearchRequestValidator()
    {
        // In Torznab API 't' parameter is required
        RuleFor(x => x.Type).NotEmpty().WithMessage("Parameter 't' is required");

        // For search operations a query is required
        When(x => x.Type == "search" || x.Type == "tvsearch" || x.Type == "movie", () => {
            RuleFor(x => x.Query).NotEmpty().WithMessage("Parameter 'q' is required for search operations");
        });

        // ApiKey validation can be added if required
        // RuleFor(x => x.ApiKey).NotEmpty().WithMessage("API key is required");
    }
}

/// <summary>
/// Endpoint that handles Torznab API requests from Sonarr/Radarr and returns results in Torznab XML format
/// </summary>
public class TorznabSearchEndpoint : Endpoint<TorznabSearchRequest>
{
    private readonly IPlexRipperDbContext _dbContext;
    private readonly TorznabSettingsModule _torznabSettings;

    public override void Configure()
    {
        Get(ApiRoutes.TorznabSearchEndpoint);
        AllowAnonymous(); // Torznab clients will use API key for auth
        Description(b => 
            b.Produces(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status401Unauthorized));
    }

    public TorznabSearchEndpoint(
        IPlexRipperDbContext dbContext,
        TorznabSettingsModule torznabSettings)
    {
        _dbContext = dbContext;
        _torznabSettings = torznabSettings;
    }

    public override async Task HandleAsync(TorznabSearchRequest req, CancellationToken ct)
    {
        // Validate API key if enabled (check settings)
        if (!IsValidApiKey(req.ApiKey))
        {
            await SendUnauthorizedResponse(ct);
            return;
        }

        // Handle different request types
        switch (req.Type?.ToLowerInvariant())
        {
            case "caps":
                await SendCapabilitiesResponse(ct);
                break;

            case "search":
            case "tvsearch":
            case "movie":
                await HandleSearchRequest(req, ct);
                break;

            default:
                await SendErrorResponse("Function not available", ct);
                break;
        }
    }

    private async Task HandleSearchRequest(TorznabSearchRequest req, CancellationToken ct)
    {
        try
        {
            // Use the existing search functionality to find media
            var searchQuery = req.Query?.ToSearchTitle() ?? string.Empty;

            // Get server filtering settings
            var includedServerIds = _torznabSettings.IncludedServerIds;
            var searchAllServers = _torznabSettings.SearchAllServers;

            // Search for TV Shows and Movies based on the request type
            var tvShowResults = new List<PlexMediaSlim>();
            var movieResults = new List<PlexMediaSlim>();

            // Only search for TV shows if not a movie-only search
            if (req.Type != "movie")
            {
                var tvShowQuery = _dbContext.PlexTvShows.AsQueryable();

                // Apply server filtering if not searching all servers
                if (!searchAllServers && includedServerIds.Any())
                {
                    tvShowQuery = tvShowQuery.Where(p => includedServerIds.Contains(p.PlexServerId));
                }

                // Apply search query filter
                tvShowQuery = tvShowQuery.Where(p => p.SearchTitle.Contains(searchQuery));

                tvShowResults = await tvShowQuery
                    .ProjectToMediaSlim()
                    .ToListAsync(ct);
            }

            // Only search for movies if not a TV-only search
            if (req.Type != "tvsearch")
            {
                var movieQuery = _dbContext.PlexMovies.AsQueryable();

                // Apply server filtering if not searching all servers
                if (!searchAllServers && includedServerIds.Any())
                {
                    movieQuery = movieQuery.Where(p => includedServerIds.Contains(p.PlexServerId));
                }

                // Apply search query filter
                movieQuery = movieQuery.Where(p => p.SearchTitle.Contains(searchQuery));

                movieResults = await movieQuery
                    .ProjectToMediaSlim()
                    .ToListAsync(ct);
            }

            // Combine results
            var results = new List<PlexMediaSlim>();
            results.AddRange(tvShowResults);
            results.AddRange(movieResults);

            // Apply limit and offset
            if (req.Offset.HasValue)
            {
                results = results.Skip(req.Offset.Value).ToList();
            }

            if (req.Limit.HasValue)
            {
                results = results.Take(req.Limit.Value).ToList();
            }

            // Convert results to Torznab format
            var torznabResults = ConvertToTorznabXml(results);

            // Send response
            await SendStringAsync(torznabResults, contentType: "application/xml", cancellation: ct);
        }
        catch (Exception ex)
        {
            await SendErrorResponse($"Error processing search: {ex.Message}", ct);
        }
    }

    private async Task SendCapabilitiesResponse(CancellationToken ct)
    {
        // Create capabilities XML
        var caps = new XElement("caps",
            new XElement("server",
                new XAttribute("version", "1.0"),
                new XAttribute("title", "PlexRipper Torznab"),
                new XAttribute("strapline", "Plex Content as Torznab"),
                new XAttribute("email", ""),
                new XAttribute("url", "")),
            new XElement("limits",
                new XAttribute("max", "100"),
                new XAttribute("default", "25")),
            new XElement("searching",
                new XElement("search",
                    new XAttribute("available", "yes"),
                    new XAttribute("supportedParams", "q")),
                new XElement("tv-search",
                    new XAttribute("available", "yes"),
                    new XAttribute("supportedParams", "q,season,ep")),
                new XElement("movie-search",
                    new XAttribute("available", "yes"),
                    new XAttribute("supportedParams", "q"))),
            new XElement("categories",
                new XElement("category",
                    new XAttribute("id", "2000"),
                    new XAttribute("name", "Movies"),
                    new XElement("subcat",
                        new XAttribute("id", "2010"),
                        new XAttribute("name", "Movies")),
                    new XElement("subcat",
                        new XAttribute("id", "2020"),
                        new XAttribute("name", "Movies HD"))),
                new XElement("category",
                    new XAttribute("id", "5000"),
                    new XAttribute("name", "TV"),
                    new XElement("subcat",
                        new XAttribute("id", "5030"),
                        new XAttribute("name", "TV/HD")),
                    new XElement("subcat",
                        new XAttribute("id", "5040"),
                        new XAttribute("name", "TV/SD")))));

        var xml = new XDocument(
            new XDeclaration("1.0", "UTF-8", "yes"),
            caps);

        await SendStringAsync(xml.ToString(), contentType: "application/xml", cancellation: ct);
    }

    private string ConvertToTorznabXml(List<PlexMediaSlim> results)
    {
        // Create RSS XML document
        var rss = new XElement("rss",
            new XAttribute("version", "2.0"),
            new XAttribute(XNamespace.Xmlns + "atom", "http://www.w3.org/2005/Atom"),
            new XAttribute(XNamespace.Xmlns + "torznab", "http://torznab.com/schemas/2015/feed"));

        var channel = new XElement("channel");
        rss.Add(channel);

        // Add channel info
        channel.Add(new XElement("title", "PlexRipper Torznab"));
        channel.Add(new XElement("description", "Plex content as a Torznab feed"));
        channel.Add(new XElement("link", "http://localhost/torznab"));
        channel.Add(new XElement("language", "en-us"));

        // Add items
        foreach (var result in results)
        {
            var item = new XElement("item");
            channel.Add(item);

            // Basic info
            item.Add(new XElement("title", result.Title));
            item.Add(new XElement("guid", result.Id.ToString()));
            item.Add(new XElement("comments", $"PlexRipper media ID: {result.Id}"));

            // Media type specific attributes
            bool isMovie = result.Type == PlexMediaType.Movie;

            // Category
            var categoryId = isMovie ? "2010" : "5030"; // Movies or TV HD
            item.Add(new XElement("category", isMovie ? "Movies" : "TV"));

            // Torznab attributes
            item.Add(new XElement("torznab:attr", new XAttribute("name", "category"), new XAttribute("value", categoryId)));
            item.Add(new XElement("torznab:attr", new XAttribute("name", "type"), new XAttribute("value", isMovie ? "movie" : "series")));

            // Size estimate (placeholder - will need actual file size)
            long estimatedSize = isMovie ? 4000000000 : 2000000000; // ~4GB for movies ~2GB for TV episodes as placeholders
            item.Add(new XElement("size", estimatedSize));
            item.Add(new XElement("torznab:attr", new XAttribute("name", "size"), new XAttribute("value", estimatedSize)));

            // Quality info
            item.Add(new XElement("torznab:attr", new XAttribute("name", "resolution"), new XAttribute("value", "1080p")));

            // Link to download - this will need to be mapped to a real endpoint that triggers the download
            string downloadUrl = $"/api/download?id={result.Id}&apikey={{apikey}}";
            item.Add(new XElement("link", downloadUrl));

            // Publication date
            // Use AddedAt or fallback to UtcNow if it's the default value
            DateTime pubDate = result.AddedAt == default ? DateTime.UtcNow : result.AddedAt;
            item.Add(new XElement("pubDate", pubDate.ToString("r")));

            // Additional info if available
            if (result.Year > 0)
            {
                item.Add(new XElement("torznab:attr", new XAttribute("name", "year"), new XAttribute("value", result.Year)));
            }
        }

        var xml = new XDocument(
            new XDeclaration("1.0", "UTF-8", "yes"),
            rss);

        return xml.ToString();
    }

    private async Task SendErrorResponse(string message, CancellationToken ct)
    {
        var error = new XElement("error",
            new XAttribute("code", "100"),
            new XAttribute("description", message));

        var xml = new XDocument(
            new XDeclaration("1.0", "UTF-8", "yes"),
            error);

        await SendStringAsync(xml.ToString(), 400, "application/xml", ct);
    }

    private async Task SendUnauthorizedResponse(CancellationToken ct)
    {
        var error = new XElement("error",
            new XAttribute("code", "101"),
            new XAttribute("description", "Invalid API Key"));

        var xml = new XDocument(
            new XDeclaration("1.0", "UTF-8", "yes"),
            error);

        await SendStringAsync(xml.ToString(), 401, "application/xml", ct);
    }

    private bool IsValidApiKey(string? apiKey)
    {
        // Use the settings module to validate the API key
        return _torznabSettings.IsValidApiKey(apiKey);
    }
}
