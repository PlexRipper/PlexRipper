using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain.Base;
using PlexRipper.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly IPlexRipperDbContext _dbContext;

        public GetPlexServerByPlexTvShowSeasonIdQueryHandler(IPlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

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
                return ResultExtensions.Get404NotFoundResult();
            }

            var plexServer = plexTvShowEpisode?.TvShow?.PlexLibrary?.PlexServer ?? null;
            if (plexServer == null)
            {
                return Result.Fail($"Could not retrieve the PlexServer from PlexTvShowSeason with id: {request.Id}");
            }
            return Result.Ok(plexServer);
        }
    }
}