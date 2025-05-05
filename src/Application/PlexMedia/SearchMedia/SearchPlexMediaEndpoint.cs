using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public record SearchPlexMediaRequest
{
    [QueryParam, BindFrom("query")]
    public required string Query { get; init; }
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

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<List<PlexMediaSlimDTO>>))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(SearchPlexMediaRequest req, CancellationToken ct)
    {
        var q = req.Query.ToSearchTitle();

        // Search for TV Shows and Movies
        var tvShowSearchResults = _dbContext
            .PlexTvShows.Where(p => p.SearchTitle.Contains(q))
            .ProjectToMediaSlimDTO()
            .ToListAsync(ct);

        var movieSearchResults = _dbContext
            .PlexMovies.Where(p => p.SearchTitle.Contains(q))
            .ProjectToMediaSlimDTO()
            .ToListAsync(ct);

        var results = await Task.WhenAll(tvShowSearchResults, movieSearchResults);

        // Flatten the results
        var entities = results.SelectMany(x => x).ToList();

        await SendFluentResult(Result.Ok(entities), ct);
    }
}
