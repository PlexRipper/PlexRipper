using FastEndpoints;
using Microsoft.AspNetCore.WebUtilities;
using Reaparr.Data.Contracts;
using Reaparr.PlexApi.Contracts;

namespace Reaparr.PlexApi;

public class GetThumbnailImageCommandHandler : ICommandHandler<GetThumbnailImageCommand, Result<ThumbnailImageResponse>>
{
    private readonly IReaparrDbContext _dbContext;
    private readonly IHttpClientFactory _httpClientFactory;

    public GetThumbnailImageCommandHandler(IReaparrDbContext dbContext, IHttpClientFactory httpClientFactory)
    {
        _dbContext = dbContext;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<Result<ThumbnailImageResponse>> ExecuteAsync(
        GetThumbnailImageCommand command,
        CancellationToken ct
    )
    {
        var plexServerId = command.PlexServerId;

        var tokenResult = await _dbContext.GetPlexServerTokenAsync(plexServerId, ct);
        if (tokenResult.IsFailed)
            return tokenResult.ToResult();

        var token = tokenResult.Value;

        var plexServerConnectionResult = await _dbContext.ChoosePlexServerConnection(plexServerId, ct);
        if (plexServerConnectionResult.IsFailed)
            return plexServerConnectionResult.ToResult();

        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(30);

        var baseUrl = $"{plexServerConnectionResult.Value.Url}/photo/:/transcode";

        var query = new Dictionary<string, string?>
        {
            ["width"] = command.Width.ToString(),
            ["height"] = command.Height.ToString(),
            ["minSize"] = "1",
            ["upscale"] = "1",
            ["url"] = $"/library/metadata/{command.PlexKey}/thumb/{command.MetaDataKey}?X-Plex-Token={token}",
            ["X-Plex-Token"] = token,
        };

        var url = QueryHelpers.AddQueryString(baseUrl, query);
        using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
        if (!response.IsSuccessStatusCode)
            return Result.Fail("Failed to fetch image").Add502BadGatewayError();

        var contentType = response.Content.Headers.ContentType?.ToString() ?? "image/jpeg";
        var data = await response.Content.ReadAsByteArrayAsync(ct);

        return Result.Ok(new ThumbnailImageResponse { Data = data, ContentType = contentType });
    }
}
