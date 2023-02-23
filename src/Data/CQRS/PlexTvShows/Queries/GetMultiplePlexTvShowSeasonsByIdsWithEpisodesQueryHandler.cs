using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Data.Common;

namespace PlexRipper.Data.PlexTvShows;

public class GetMultiplePlexTvShowSeasonsByIdsWithEpisodesQueryValidator : AbstractValidator<GetMultiplePlexTvShowSeasonsByIdsWithEpisodesQuery>
{
    public GetMultiplePlexTvShowSeasonsByIdsWithEpisodesQueryValidator()
    {
        RuleFor(x => x.Ids.Count).GreaterThan(0);
    }
}

public class GetMultiplePlexTvShowSeasonsByIdsWithEpisodesQueryHandler : BaseHandler,
    IRequestHandler<GetMultiplePlexTvShowSeasonsByIdsWithEpisodesQuery, Result<List<PlexTvShowSeason>>>
{
    public GetMultiplePlexTvShowSeasonsByIdsWithEpisodesQueryHandler(ILog log, PlexRipperDbContext dbContext) : base(log, dbContext) { }

    public async Task<Result<List<PlexTvShowSeason>>> Handle(GetMultiplePlexTvShowSeasonsByIdsWithEpisodesQuery request, CancellationToken cancellationToken)
    {
        var query = PlexTvShowSeasonsQueryable;

        if (request.IncludeData)
        {
            query = query
                .Include(x => x.TvShow)
                .Include(x => x.Episodes);
        }

        if (request.IncludeLibrary)
            query = query.IncludePlexLibrary();

        if (request.IncludeServer)
            query = query.IncludePlexServer();

        var plexTvShowSeason = await query
            .Where(x => request.Ids.Contains(x.Id))
            .ToListAsync(cancellationToken);

        return Result.Ok(plexTvShowSeason);
    }
}