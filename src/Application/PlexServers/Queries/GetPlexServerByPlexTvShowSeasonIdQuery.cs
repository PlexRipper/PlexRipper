using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.Base;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexServers.Queries
{
    public class GetPlexServerByPlexTvShowSeasonIdQuery : IRequest<Result<PlexServer>>
    {
        public GetPlexServerByPlexTvShowSeasonIdQuery(int id)
        {
            Id = id;
        }

        public int Id { get; }
    }

    public class GetPlexServerByPlexTvShowSeasonIdQueryValidator : AbstractValidator<GetPlexServerByPlexTvShowSeasonIdQuery>
    {
        public GetPlexServerByPlexTvShowSeasonIdQueryValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }


    public class GetPlexServerByPlexTvShowSeasonIdQueryHandler : BaseHandler,
        IRequestHandler<GetPlexServerByPlexTvShowSeasonIdQuery, Result<PlexServer>>
    {
        public GetPlexServerByPlexTvShowSeasonIdQueryHandler(IPlexRipperDbContext dbContext): base(dbContext) { }

        public async Task<Result<PlexServer>> Handle(GetPlexServerByPlexTvShowSeasonIdQuery request,
            CancellationToken cancellationToken)
        {
            var plexTvShowEpisode = await _dbContext.PlexTvShowSeason
                .Include(x => x.TvShow)
                .ThenInclude(x => x.PlexLibrary)
                .ThenInclude(x => x.PlexServer)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (plexTvShowEpisode == null)
            {
                return ResultExtensions.Create404NotFoundResult();
            }

            var plexServer = plexTvShowEpisode?.TvShow?.PlexLibrary?.PlexServer ?? null;
            if (plexServer == null)
            {
                return ResultExtensions.GetEntityNotFound(nameof(PlexServer), request.Id)
                    .LogError(null, $"Could not retrieve the PlexServer from PlexTvShowSeason with id: {request.Id}");
            }
            return Result.Ok(plexServer);
        }
    }
}