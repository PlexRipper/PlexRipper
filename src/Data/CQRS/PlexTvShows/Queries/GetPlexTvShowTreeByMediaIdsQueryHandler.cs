using System.Collections.Generic;
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

namespace PlexRipper.Data.PlexTvShows
{
    public class GetPlexTvShowTreeByMediaIdsQueryValidator : AbstractValidator<GetPlexTvShowTreeByMediaIdsQuery>
    {
        public GetPlexTvShowTreeByMediaIdsQueryValidator() { }
    }

    public class GetPlexTvShowTreeByMediaIdsQueryHandler : BaseHandler, IRequestHandler<GetPlexTvShowTreeByMediaIdsQuery, Result<List<PlexTvShow>>>
    {
        public GetPlexTvShowTreeByMediaIdsQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<List<PlexTvShow>>> Handle(GetPlexTvShowTreeByMediaIdsQuery request, CancellationToken cancellationToken)
        {
            List<PlexTvShowEpisode> episodes = new List<PlexTvShowEpisode>();
            var existingEpisodeIds = new List<int>();

            if (request.TvShowIds.Any())
            {
                var episodeIdsFromTvShows =
                    await PlexTvShowsQueryable
                        .IncludeServer()
                        .IncludePlexLibrary()
                        .Include(x => x.Seasons)
                        .ThenInclude(x => x.Episodes)
                        .ThenInclude(x => x.TvShowSeason)
                        .ThenInclude(x => x.TvShow)

                        // Include PlexLibrary
                        .Include(x => x.Seasons)
                        .ThenInclude(x => x.Episodes)
                        .ThenInclude(x => x.PlexLibrary)

                        // Include PlexServer
                        .Include(x => x.Seasons)
                        .ThenInclude(x => x.Episodes)
                        .ThenInclude(x => x.PlexServer)

                        // Filter on only the requested ones
                        .Where(x => request.TvShowIds.Contains(x.Id))
                        .SelectMany(x => x.Seasons.SelectMany(y => y.Episodes))
                        .ToListAsync(cancellationToken);

                episodes.AddRange(episodeIdsFromTvShows);
                existingEpisodeIds.AddRange(episodes.Select(x => x.Id).ToList());
            }

            if (request.TvShowSeasonIds.Any())
            {
                var episodeIdsFromTvShowSeasons =
                    await PlexTvShowSeasonsQueryable
                        .Include(x => x.Episodes)
                        .ThenInclude(x => x.TvShowSeason)
                        .ThenInclude(x => x.TvShow)
                        .Where(x => request.TvShowSeasonIds.Contains(x.Id)
                                    && !existingEpisodeIds.Contains(x.Id))
                        .SelectMany(y => y.Episodes)
                        .ToListAsync(cancellationToken);

                episodes.AddRange(episodeIdsFromTvShowSeasons);
                existingEpisodeIds.AddRange(episodes.Select(x => x.Id).ToList());
            }

            if (request.TvShowEpisodeIds.Any())
            {
                var episodesFromTvShowEpisodes = await PlexTvShowEpisodesQueryable
                    .Include(x => x.TvShowSeason)
                    .ThenInclude(x => x.TvShow)
                    .Where(x => request.TvShowEpisodeIds.Contains(x.Id)
                                && !existingEpisodeIds.Contains(x.Id))
                    .ToListAsync(cancellationToken);

                episodes.AddRange(episodesFromTvShowEpisodes);
            }

            var shows = episodes.Select(x => x.TvShowSeason.TvShow)
                .GroupBy(x => x.Id)
                .Select(g => g.First())
                .ToList();

            return Result.Ok(shows);
        }
    }
}