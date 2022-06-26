using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.Common;

namespace PlexRipper.Data.PlexTvShows;

public class GetMultiplePlexTvShowEpisodesByIdQueryValidator : AbstractValidator<GetMultiplePlexTvShowEpisodesByIdQuery>
{
    public GetMultiplePlexTvShowEpisodesByIdQueryValidator()
    {
        RuleFor(x => x.Ids.Count).GreaterThan(0);
    }
}

public class GetMultiplePlexTvShowEpisodesByIdQueryHandler : BaseHandler,
    IRequestHandler<GetMultiplePlexTvShowEpisodesByIdQuery, Result<List<PlexTvShowEpisode>>>
{
    public GetMultiplePlexTvShowEpisodesByIdQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

    public async Task<Result<List<PlexTvShowEpisode>>> Handle(GetMultiplePlexTvShowEpisodesByIdQuery request, CancellationToken cancellationToken)
    {
        IQueryable<PlexTvShowEpisode> query = PlexTvShowEpisodesQueryable;

        if (request.IncludeLibrary)
        {
            query = query.IncludePlexLibrary();
        }

        if (request.IncludeServer)
        {
            query = query.IncludePlexServer();
        }

        if (request.IncludeTvShowAndSeason)
        {
            query = query
                .Include(x => x.TvShowSeason)
                .ThenInclude(x => x.TvShow);
        }

        var plexTvShowEpisodes = await query
            .Where(x => request.Ids.Contains(x.Id)).ToListAsync(cancellationToken);

        return Result.Ok(plexTvShowEpisodes);
    }
}