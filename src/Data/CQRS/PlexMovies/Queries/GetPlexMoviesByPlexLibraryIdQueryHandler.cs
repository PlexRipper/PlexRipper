using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Data.Common;

namespace PlexRipper.Data.PlexMovies;

public class GetPlexMoviesByPlexLibraryIdValidator : AbstractValidator<GetPlexMoviesByPlexLibraryId>
{
    public GetPlexMoviesByPlexLibraryIdValidator()
    {
        RuleFor(x => x.PlexLibraryId).GreaterThan(0);
    }
}

public class GetPlexMoviesByPlexLibraryIdHandler
    : BaseHandler,
        IRequestHandler<GetPlexMoviesByPlexLibraryId, Result<List<PlexMovie>>>
{
    public GetPlexMoviesByPlexLibraryIdHandler(ILog log, PlexRipperDbContext dbContext)
        : base(log, dbContext) { }

    public async Task<Result<List<PlexMovie>>> Handle(
        GetPlexMoviesByPlexLibraryId request,
        CancellationToken cancellationToken
    )
    {
        var plexMovies = await PlexMoviesQueryable
            .Where(x => x.PlexLibraryId == request.PlexLibraryId)
            .ToListAsync(cancellationToken);

        return Result.Ok(plexMovies);
    }
}
