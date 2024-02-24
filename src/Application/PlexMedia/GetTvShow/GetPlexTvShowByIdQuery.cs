using Data.Contracts;
using FluentValidation;
using Logging.Interface;

namespace PlexRipper.Application;

public record GetPlexTvShowByIdQuery(int PlexTvShowId) : IRequest<Result<PlexTvShow>>;

public class GetPlexTvShowByIdQueryValidator : AbstractValidator<GetPlexTvShowByIdQuery>
{
    public GetPlexTvShowByIdQueryValidator()
    {
        RuleFor(x => x.PlexTvShowId).GreaterThan(0);
    }
}

public class GetPlexTvShowByIdQueryHandler : IRequestHandler<GetPlexTvShowByIdQuery, Result<PlexTvShow>>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;

    public GetPlexTvShowByIdQueryHandler(ILog log, IPlexRipperDbContext dbContext)
    {
        _log = log;
        _dbContext = dbContext;
    }

    public async Task<Result<PlexTvShow>> Handle(GetPlexTvShowByIdQuery request, CancellationToken cancellationToken)
    {
        var plexTvShow = await _dbContext.PlexTvShows.GetAsync(request.PlexTvShowId, cancellationToken);
        if (plexTvShow == null)
            return ResultExtensions.EntityNotFound(nameof(PlexTvShow), request.PlexTvShowId);

        return Result.Ok(plexTvShow);
    }
}