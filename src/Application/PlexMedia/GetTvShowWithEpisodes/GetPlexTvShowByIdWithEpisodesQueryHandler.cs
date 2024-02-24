using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public record GetPlexTvShowByIdWithEpisodesQuery(int PlexTvShowId) : IRequest<Result<PlexTvShow>>;

public class GetPlexTvShowByIdWithEpisodesQueryValidator : AbstractValidator<GetPlexTvShowByIdWithEpisodesQuery>
{
    public GetPlexTvShowByIdWithEpisodesQueryValidator()
    {
        RuleFor(x => x.PlexTvShowId).GreaterThan(0);
    }
}

public class GetPlexTvShowByIdWithEpisodesQueryHandler : IRequestHandler<GetPlexTvShowByIdWithEpisodesQuery, Result<PlexTvShow>>
{
    private readonly IPlexRipperDbContext _dbContext;

    public GetPlexTvShowByIdWithEpisodesQueryHandler(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PlexTvShow>> Handle(GetPlexTvShowByIdWithEpisodesQuery request, CancellationToken cancellationToken)
    {
        var plexTvShow = await _dbContext.PlexTvShows.IncludeEpisodes().GetAsync(request.PlexTvShowId, cancellationToken);

        if (plexTvShow == null)
            return ResultExtensions.EntityNotFound(nameof(PlexTvShow), request.PlexTvShowId);

        plexTvShow.Seasons = plexTvShow.Seasons.OrderByNatural(x => x.Title).ToList();

        return Result.Ok(plexTvShow);
    }
}