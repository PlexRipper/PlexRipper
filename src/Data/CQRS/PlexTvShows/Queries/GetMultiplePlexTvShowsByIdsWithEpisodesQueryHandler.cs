using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.CQRS.PlexTvShows
{
    public class GetMultiplePlexTvShowsByIdsWithEpisodesQueryValidator : AbstractValidator<GetMultiplePlexTvShowsByIdsWithEpisodesQuery>
    {
        public GetMultiplePlexTvShowsByIdsWithEpisodesQueryValidator()
        {
            RuleFor(x => x.Ids.Count).GreaterThan(0);
        }
    }

    public class GetMultiplePlexTvShowsByIdsWithEpisodesQueryHandler : BaseHandler,
        IRequestHandler<GetMultiplePlexTvShowsByIdsWithEpisodesQuery, Result<List<PlexTvShow>>>
    {
        public GetMultiplePlexTvShowsByIdsWithEpisodesQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<List<PlexTvShow>>> Handle(GetMultiplePlexTvShowsByIdsWithEpisodesQuery request, CancellationToken cancellationToken)
        {
            IQueryable<PlexTvShow> query = PlexTvShowsQueryable;

            if (request.IncludeLibrary)
            {
                query = query.IncludePlexLibrary();
            }

            if (request.IncludeServer)
            {
                query = query.IncludePlexServer();
            }

            if (request.IncludeData)
            {
                query = query.Include(x => x.Seasons)
                    .ThenInclude(x => x.Episodes);
            }

            var plexTvShow = await query
                .Where(x => request.Ids.Contains(x.Id))
                .ToListAsync(cancellationToken);

            return Result.Ok(plexTvShow);
        }
    }
}