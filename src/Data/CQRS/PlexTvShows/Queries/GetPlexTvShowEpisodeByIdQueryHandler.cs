using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.Common;

namespace PlexRipper.Data.PlexTvShows;

public class GetPlexTvShowEpisodeByIdQueryValidator : AbstractValidator<GetPlexTvShowEpisodeByIdQuery>
{
    public GetPlexTvShowEpisodeByIdQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class GetPlexTvShowEpisodeByIdQueryHandler : BaseHandler, IRequestHandler<GetPlexTvShowEpisodeByIdQuery, Result<PlexTvShowEpisode>>
{
    public GetPlexTvShowEpisodeByIdQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

    public async Task<Result<PlexTvShowEpisode>> Handle(GetPlexTvShowEpisodeByIdQuery request, CancellationToken cancellationToken)
    {
        var plexTvShowEpisode = await PlexTvShowEpisodesQueryable.IncludeAll()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (plexTvShowEpisode == null)
        {
            return ResultExtensions.EntityNotFound(nameof(PlexTvShowEpisode), request.Id);
        }

        return Result.Ok(plexTvShowEpisode);
    }
}