﻿using System.Threading;
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
            var plexTvShowEpisode = await _dbContext.PlexTvShowEpisodes
                .Include(x => x.TvShowSeason)
                .ThenInclude(x => x.TvShow)
                .Include(x => x.PlexLibrary)
                .ThenInclude(x => x.PlexServer)
                .Include(x => x.EpisodeData)
                .ThenInclude(x => x.Parts)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (plexTvShowEpisode == null)
            {
                return ResultExtensions.GetEntityNotFound(nameof(PlexTvShowEpisode), request.Id);
            }

            return Result.Ok(plexTvShowEpisode);
        }
    }
}