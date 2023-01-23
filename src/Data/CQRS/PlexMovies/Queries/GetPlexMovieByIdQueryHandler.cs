using Data.Contracts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Data.Common;

namespace PlexRipper.Data.PlexMovies;

public class GetPlexMovieByIdQueryValidator : AbstractValidator<GetPlexMovieByIdQuery>
{
    public GetPlexMovieByIdQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class GetPlexMovieByIdQueryHandler : BaseHandler, IRequestHandler<GetPlexMovieByIdQuery, Result<PlexMovie>>
{
    public GetPlexMovieByIdQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

    public async Task<Result<PlexMovie>> Handle(GetPlexMovieByIdQuery request, CancellationToken cancellationToken)
    {
        var query = PlexMoviesQueryable;

        if (request.IncludeLibrary)
            query = query.IncludePlexLibrary();

        if (request.IncludeServer)
            query = query.IncludePlexServer();

        var plexMovie = await query.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (plexMovie == null)
            return ResultExtensions.EntityNotFound(nameof(PlexMovie), request.Id);

        return Result.Ok(plexMovie);
    }
}