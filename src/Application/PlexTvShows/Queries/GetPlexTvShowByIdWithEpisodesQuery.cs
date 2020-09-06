using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.Base;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexTvShows.Queries
{
    public class GetPlexTvShowByIdWithEpisodesQuery : IRequest<Result<PlexTvShow>>
    {
        public GetPlexTvShowByIdWithEpisodesQuery(int id)
        {
            Id = id;
        }

        public int Id { get; }
    }

    public class GetPlexTvShowByIdWithEpisodesQueryValidator : AbstractValidator<GetPlexTvShowByIdWithEpisodesQuery>
    {
        public GetPlexTvShowByIdWithEpisodesQueryValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }


    public class GetPlexTvShowByIdWithEpisodesQueryHandler : BaseHandler, IRequestHandler<GetPlexTvShowByIdWithEpisodesQuery, Result<PlexTvShow>>
    {
        public GetPlexTvShowByIdWithEpisodesQueryHandler(IPlexRipperDbContext dbContext): base(dbContext) { }

        public async Task<Result<PlexTvShow>> Handle(GetPlexTvShowByIdWithEpisodesQuery request, CancellationToken cancellationToken)
        {
            var result = await ValidateAsync<GetPlexTvShowByIdWithEpisodesQuery, GetPlexTvShowByIdWithEpisodesQueryValidator>(request);
            if (result.IsFailed) return result;
            var plexTvShow = await _dbContext.PlexTvShows
                .Include(x => x.Seasons)
                .ThenInclude(x => x.Episodes)
                .OrderBy(x => x.RatingKey)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (plexTvShow == null)
            {
                return ResultExtensions.GetEntityNotFound(nameof(PlexTvShow), request.Id);
            }

            plexTvShow.Seasons = plexTvShow.Seasons.OrderBy(x => x.Title).ToList();

            return Result.Ok(plexTvShow);
        }
    }
}