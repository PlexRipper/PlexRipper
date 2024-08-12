using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public record SearchPlexMediaRequest()
{
    [QueryParam]
    public string Query { get; init; }
}

public class SearchPlexMediaRequestValidator : Validator<SearchPlexMediaRequest>
{
    public SearchPlexMediaRequestValidator()
    {
        RuleFor(x => x.Query.Length).GreaterThan(0);
    }
}

public class SearchPlexMediaEndpoint : BaseEndpoint<SearchPlexMediaRequest, ResultDTO<List<PlexMediaSlimDTO>>>
{
    private readonly IPlexRipperDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.PlexMediaController + "/search";

    public SearchPlexMediaEndpoint(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(EndpointPath);
        AllowAnonymous();
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<List<PlexMediaSlimDTO>>))
                .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO))
        );
    }

    public override async Task HandleAsync(SearchPlexMediaRequest req, CancellationToken ct)
    {
        var q = req.Query.ToSearchTitle();

        // Search for TV Shows and Movies
        var tvShowSearchResults = _dbContext
            .PlexTvShows.Where(p => p.SearchTitle.Contains(q))
            .ProjectToMediaSlim()
            .ToListAsync(ct);

        var movieSearchResults = _dbContext
            .PlexMovies.Where(p => p.SearchTitle.Contains(q))
            .ProjectToMediaSlim()
            .ToListAsync(ct);

        var results = await Task.WhenAll(tvShowSearchResults, movieSearchResults);

        var response = results.SelectMany(x => x).ToList();

        // Get the Plex connection and token for the Plex Servers
        var plexServerIds = response.Select(x => x.PlexServerId).Distinct();
        var tokenDict = new Dictionary<int, (string, string)>();

        foreach (var plexServerId in plexServerIds)
        {
            var tokenResult = await _dbContext.GetPlexServerTokenAsync(plexServerId, ct);
            if (tokenResult.IsFailed)
                continue;

            var serverConnectionResult = await _dbContext.GetValidPlexServerConnection(plexServerId, ct);
            if (serverConnectionResult.IsFailed)
                continue;

            tokenDict.Add(plexServerId, (serverConnectionResult.Value.Url, tokenResult.Value));
        }

        // Set the full thumbnail url and convert to slim DTO
        var entities = new List<PlexMediaSlimDTO>();
        foreach (var media in response)
        {
            if (media.HasThumb && tokenDict.ContainsKey(media.PlexServerId))
                media.SetFullThumbnailUrl(tokenDict[media.PlexServerId].Item1, tokenDict[media.PlexServerId].Item2);

            entities.Add(media.ToSlimDTO());
        }

        await SendFluentResult(Result.Ok(entities.SetIndex()), ct);
    }
}
