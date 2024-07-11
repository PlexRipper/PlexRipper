using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Data.Common;

namespace PlexRipper.Data.PlexMovies;

public class GetMultiplePlexMoviesByIdsQueryHandlerValidator : AbstractValidator<GetMultiplePlexMoviesByIdsQuery>
{
    public GetMultiplePlexMoviesByIdsQueryHandlerValidator()
    {
        RuleFor(x => x.Ids.Count).GreaterThan(0);
    }
}

public class GetMultiplePlexMoviesByIdsQueryHandlerHandler
    : BaseHandler,
        IRequestHandler<GetMultiplePlexMoviesByIdsQuery, Result<List<PlexMovie>>>
{
    public GetMultiplePlexMoviesByIdsQueryHandlerHandler(ILog log, PlexRipperDbContext dbContext)
        : base(log, dbContext) { }

    public async Task<Result<List<PlexMovie>>> Handle(
        GetMultiplePlexMoviesByIdsQuery request,
        CancellationToken cancellationToken
    )
    {
        var query = PlexMoviesQueryable;

        if (request.IncludeLibrary)
            query = query.IncludePlexLibrary();

        if (request.IncludeServer)
            query = query.IncludePlexServer();

        var plexMovies = await query.Where(x => request.Ids.Contains(x.Id)).ToListAsync(cancellationToken);

        return Result.Ok(plexMovies);
    }
}
