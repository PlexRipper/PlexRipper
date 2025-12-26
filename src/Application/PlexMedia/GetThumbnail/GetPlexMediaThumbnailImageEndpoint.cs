using System.Net.Mime;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.Application;

public record GetPlexMediaThumbnailImageEndpointRequest
{
    [QueryParam, BindFrom("plexServerId")]
    public required int PlexServerId { get; init; }

    [QueryParam, BindFrom("plexKey")]
    public required int PlexKey { get; init; }

    [QueryParam, BindFrom("metaDataKey")]
    public required int MetaDataKey { get; init; }

    [QueryParam, BindFrom("width")]
    public required int Width { get; init; }

    [QueryParam, BindFrom("height")]
    public required int Height { get; init; }
}

public class GetPlexMediaThumbnailImageEndpointRequestValidator : Validator<GetPlexMediaThumbnailImageEndpointRequest>
{
    public GetPlexMediaThumbnailImageEndpointRequestValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
        RuleFor(x => x.PlexKey).GreaterThan(0);
        RuleFor(x => x.MetaDataKey).GreaterThan(0);
        RuleFor(x => x.Width).InclusiveBetween(1, 4096);
        RuleFor(x => x.Height).InclusiveBetween(1, 4096);
    }
}

/// <summary>
/// High-performance thumbnail proxy endpoint with streaming response and in-memory caching
/// for database lookups. Response caching is enabled for downstream caches (3 days).
/// </summary>
public sealed class GetPlexMediaThumbnailImageEndpoint : BaseEndpoint<GetPlexMediaThumbnailImageEndpointRequest, byte[]>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;

    private static readonly TimeSpan _tokenCacheDuration = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan _connectionCacheDuration = TimeSpan.FromMinutes(5);

    public override string EndpointPath => ApiRoutes.PlexMediaController + "/thumbnail";

    public GetPlexMediaThumbnailImageEndpoint(
        ILogger log,
        IReaparrDbContext dbContext,
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache
    )
    {
        _log = log.ForContext<GetPlexMediaThumbnailImageEndpoint>();
        _dbContext = dbContext;
        _httpClientFactory = httpClientFactory;
        _cache = cache;
    }

    public override void Configure()
    {
        Get(EndpointPath);

        // Enable response caching headers for downstream caches (proxies, CDNs, browsers)
        // 3 days = 259200 seconds, varied by query parameters
        ResponseCache(259200, varyByQueryKeys: ["plexServerId", "plexKey", "metaDataKey", "width", "height"]);

        Summary(s =>
        {
            s.Summary = "Proxy Plex image";
            s.Description = "Proxies image bytes from Plex servers with CORS headers.";
            s.ExampleRequest = new GetPlexMediaThumbnailImageEndpointRequest
            {
                PlexServerId = 1,
                PlexKey = 1756014789,
                MetaDataKey = 57920,
                Width = 200,
                Height = 400,
            };
        });

        Description(x =>
        {
            x.Produces(StatusCodes.Status200OK, typeof(byte[]), MediaTypeNames.Image.Jpeg)
                .Produces(StatusCodes.Status304NotModified)
                .Produces(StatusCodes.Status404NotFound, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status502BadGateway, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO));
        });
    }

    public override async Task HandleAsync(GetPlexMediaThumbnailImageEndpointRequest req, CancellationToken ct)
    {
#pragma warning disable ASP0015
        HttpContext.Response.Headers["Access-Control-Allow-Origin"] = "*";
        HttpContext.Response.Headers["Access-Control-Allow-Methods"] = "GET, OPTIONS";
        HttpContext.Response.Headers["Access-Control-Allow-Headers"] = "*";
#pragma warning restore ASP0015

        // Generate ETag based on the unique thumbnail parameters
        var etag = $"\"{req.PlexServerId}-{req.PlexKey}-{req.MetaDataKey}-{req.Width}x{req.Height}\"";
        HttpContext.Response.Headers.ETag = etag;

        // Check If-None-Match header for conditional request (304 Not Modified)
        var ifNoneMatch = HttpContext.Request.Headers.IfNoneMatch.ToString();
        if (!string.IsNullOrEmpty(ifNoneMatch) && ifNoneMatch == etag)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status304NotModified;
            return;
        }

        var plexServerId = req.PlexServerId;

        // Fetch token and connection in parallel for better performance
        var tokenTask = GetCachedTokenAsync(plexServerId, ct);
        var connectionTask = GetCachedConnectionAsync(plexServerId, ct);

        await Task.WhenAll(tokenTask, connectionTask);

        var tokenResult = await tokenTask;
        if (tokenResult.IsFailed)
        {
            _log.Here()
                .Warning(
                    "Failed to get Plex server token for server {PlexServerId}: {Error}",
                    plexServerId,
                    tokenResult.Errors.FirstOrDefault()?.Message
                );
            await SendFluentResult(tokenResult.ToResult(), ct);
            return;
        }

        var connectionResult = await connectionTask;
        if (connectionResult.IsFailed)
        {
            _log.Here()
                .Warning(
                    "Failed to get Plex server connection for server {PlexServerId}: {Error}",
                    plexServerId,
                    connectionResult.Errors.FirstOrDefault()?.Message
                );
            await SendFluentResult(connectionResult.ToResult(), ct);
            return;
        }

        var token = tokenResult.Value;

        // Use the named PlexThumbnail HttpClient with connection pooling
        var client = _httpClientFactory.CreateClient(HttpClientModule.PlexThumbnailClientName);

        var baseUrl = $"{connectionResult.Value.Url}/photo/:/transcode";

        var query = new Dictionary<string, string?>
        {
            ["width"] = req.Width.ToString(),
            ["height"] = req.Height.ToString(),
            ["minSize"] = "1",
            ["upscale"] = "1",
            ["url"] = $"/library/metadata/{req.PlexKey}/thumb/{req.MetaDataKey}?X-Plex-Token={token}",
            ["X-Plex-Token"] = token,
        };

        var url = QueryHelpers.AddQueryString(baseUrl, query);

        // Use ResponseHeadersRead to start streaming immediately without buffering
        using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
        if (!response.IsSuccessStatusCode)
        {
            _log.Here().Verbose("Failed to fetch Plex thumbnail from {Url}", url);
            await SendFluentResult(Result.Fail("Failed to fetch image").Add502BadGatewayError(), ct);
            return;
        }

        var contentType = response.Content.Headers.ContentType?.ToString() ?? "image/jpeg";
        var contentLength = response.Content.Headers.ContentLength;

        if (contentLength.HasValue)
        {
            HttpContext.Response.ContentLength = contentLength.Value;
        }

        HttpContext.Response.ContentType = contentType;

        // Stream directly to response without buffering
        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        await stream.CopyToAsync(HttpContext.Response.Body, ct);
    }

    private async Task<Result<string>> GetCachedTokenAsync(int plexServerId, CancellationToken ct)
    {
        var cacheKey = $"plex_token_{plexServerId}";

        // Check cache first
        if (_cache.TryGetValue<Result<string>>(cacheKey, out var cachedResult) && cachedResult is not null)
            return cachedResult;

        // Fetch from database
        var tokenResult = await _dbContext.GetPlexServerTokenAsync(plexServerId, ct);

        // Only cache successful results
        if (tokenResult.IsSuccess)
        {
            _cache.Set(cacheKey, tokenResult, _tokenCacheDuration);
        }

        return tokenResult;
    }

    private async Task<Result<PlexServerConnection>> GetCachedConnectionAsync(int plexServerId, CancellationToken ct)
    {
        var cacheKey = $"plex_connection_{plexServerId}";

        // Check cache first
        if (
            _cache.TryGetValue<Result<PlexServerConnection>>(cacheKey, out var cachedResult) && cachedResult is not null
        )
            return cachedResult;

        // Fetch from a database
        var connectionResult = await _dbContext.ChoosePlexServerConnection(plexServerId, ct);

        // Only cache successful results
        if (connectionResult.IsSuccess)
        {
            _cache.Set(cacheKey, connectionResult, _connectionCacheDuration);
        }

        return connectionResult;
    }
}
