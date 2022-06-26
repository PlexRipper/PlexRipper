using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.PlexTvShows
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
            var query = PlexTvShowsQueryable;

            if (request.IncludePlexLibrary)
            {
                query = query.IncludePlexLibrary();
            }

            if (request.IncludePlexServer)
            {
                query = query.IncludePlexServer();
            }

            var plexTvShow = await query.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (plexTvShow == null)
            {
                return ResultExtensions.EntityNotFound(nameof(PlexTvShow), request.Id);
            }

            return Result.Ok(plexTvShow);
        }
    }
}