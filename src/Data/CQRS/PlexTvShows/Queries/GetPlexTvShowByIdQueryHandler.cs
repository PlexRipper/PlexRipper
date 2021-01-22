using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.PlexTvShows;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.CQRS.PlexTvShows
{
    public class GetPlexTvShowByIdQueryValidator : AbstractValidator<GetPlexTvShowByIdQuery>
    {
        public GetPlexTvShowByIdQueryValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }

    public class GetPlexTvShowByIdQueryHandler : BaseHandler, IRequestHandler<GetPlexTvShowByIdQuery, Result<PlexTvShow>>
    {
        public GetPlexTvShowByIdQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<PlexTvShow>> Handle(GetPlexTvShowByIdQuery request, CancellationToken cancellationToken)
        {
            IQueryable<PlexTvShow> query = PlexTvShowsQueryable;

            if (request.IncludeLibrary)
            {
                query = query.IncludePlexLibrary();
            }

            if (request.IncludeServer)
            {
                query = query.IncludeServer();
            }

            var plexTvShow = await query.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (plexTvShow == null)
            {
                return ResultExtensions.GetEntityNotFound(nameof(PlexTvShow), request.Id);
            }

            return Result.Ok(plexTvShow);
        }
    }
}