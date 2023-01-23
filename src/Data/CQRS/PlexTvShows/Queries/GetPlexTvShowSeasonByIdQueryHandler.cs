using Data.Contracts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.Common;

namespace PlexRipper.Data.PlexTvShows;

public class GetPlexTvShowSeasonByIdQueryValidator : AbstractValidator<GetPlexTvShowSeasonByIdQuery>
{
    public GetPlexTvShowSeasonByIdQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class GetPlexTvShowSeasonByIdQueryHandler : BaseHandler, IRequestHandler<GetPlexTvShowSeasonByIdQuery, Result<PlexTvShowSeason>>
{
    public GetPlexTvShowSeasonByIdQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

    public async Task<Result<PlexTvShowSeason>> Handle(GetPlexTvShowSeasonByIdQuery request, CancellationToken cancellationToken)
    {
        var query = PlexTvShowSeasonsQueryable;

        if (request.IncludeLibrary)
            query = query.IncludePlexLibrary();

        if (request.IncludeServer)
            query = query.IncludePlexServer();

        var plexTvShowSeason = await query
            .Include(x => x.TvShow)
            .Include(x => x.Episodes)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (plexTvShowSeason == null)
            return ResultExtensions.EntityNotFound(nameof(PlexTvShowSeason), request.Id);

        return Result.Ok(plexTvShowSeason);
    }
}