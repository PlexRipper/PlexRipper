using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain;
using PlexRipper.Domain.Base;
using PlexRipper.Domain.Entities;

namespace PlexRipper.Application.PlexTvShows.Queries
{
    public class GetPlexTvShowEpisodeByIdQuery : IRequest<Result<PlexTvShowEpisode>>
    {
        public GetPlexTvShowEpisodeByIdQuery(int id)
        {
            Id = id;
        }

        public int Id { get; }
    }

    public class GetPlexTvShowEpisodeByIdQueryValidator : AbstractValidator<GetPlexTvShowEpisodeByIdQuery>
    {
        public GetPlexTvShowEpisodeByIdQueryValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }


    public class GetPlexTvShowEpisodeByIdQueryHandler : BaseHandler, IRequestHandler<GetPlexTvShowEpisodeByIdQuery, Result<PlexTvShowEpisode>>
    {
        private readonly IPlexRipperDbContext _dbContext;

        public GetPlexTvShowEpisodeByIdQueryHandler(IPlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<PlexTvShowEpisode>> Handle(GetPlexTvShowEpisodeByIdQuery request, CancellationToken cancellationToken)
        {
            var plexTvShowEpisode = await _dbContext.PlexTvShowEpisodes
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (plexTvShowEpisode == null)
            {
                return ResultExtensions.Get404NotFoundResult();
            }

            return Result.Ok(plexTvShowEpisode);
        }
    }
}