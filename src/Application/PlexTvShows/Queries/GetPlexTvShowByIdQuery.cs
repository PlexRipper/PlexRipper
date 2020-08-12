using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain.Base;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain.FluentResultExtensions;

namespace PlexRipper.Application.PlexTvShows.Queries
{
    public class GetPlexTvShowByIdQuery : IRequest<Result<PlexTvShow>>
    {
        public GetPlexTvShowByIdQuery(int id)
        {
            Id = id;
        }

        public int Id { get; }
    }

    public class GetPlexTvShowByIdQueryValidator : AbstractValidator<GetPlexTvShowByIdQuery>
    {
        public GetPlexTvShowByIdQueryValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }


    public class GetPlexTvShowByIdQueryHandler : BaseHandler, IRequestHandler<GetPlexTvShowByIdQuery, Result<PlexTvShow>>
    {
        private readonly IPlexRipperDbContext _dbContext;

        public GetPlexTvShowByIdQueryHandler(IPlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<PlexTvShow>> Handle(GetPlexTvShowByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await ValidateAsync<GetPlexTvShowByIdQuery, GetPlexTvShowByIdQueryValidator>(request);
            if (result.IsFailed) return result;
            var plexTvShow = await _dbContext.PlexTvShows
                .Include(x => x.Seasons)
                .ThenInclude(x => x.Episodes)
                .OrderBy(x => x.Title)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (plexTvShow == null)
            {
                return result.Set404NotFoundError();
            }

            plexTvShow.Seasons = plexTvShow.Seasons.OrderBy(x => x.Title).ToList();

            return Result.Ok(plexTvShow);
        }
    }
}