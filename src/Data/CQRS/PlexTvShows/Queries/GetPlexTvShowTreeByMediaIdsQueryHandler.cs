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

namespace PlexRipper.Data.CQRS.PlexTvShows
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
            var plexTvShows =
                await PlexTvShowsQueryable
                    .IncludeSeasons()
                    .IncludeEpisodes()
                    .IncludeServer()
                    .IncludePlexLibrary()
                    .Where(x => request.TvShowIds.Contains(x.Id))
                    .ToListAsync(cancellationToken);

            var plexTvShowSeasons =
                await PlexTvShowSeasonsQueryable
                    .IncludeEpisodes()
                    .IncludeServer()
                    .IncludePlexLibrary()
                    .Where(x => request.TvShowSeasonIds.Contains(x.Id))
                    .ToListAsync(cancellationToken);

            var plexTvShowEpisodes =
                await PlexTvShowEpisodesQueryable
                    .Include(x => x.TvShowSeason)
                    .ThenInclude(x => x.TvShow)
                    .IncludeServer()
                    .IncludePlexLibrary()
                    .Where(x => request.TvShowEpisodeIds.Contains(x.Id))
                    .ToListAsync(cancellationToken);

            var mergedPlexTvShows = new List<PlexTvShow>();

            // Add all TvShows as the contains all seasons and episodes
            mergedPlexTvShows.AddRange(plexTvShows);

            // Create hierarchy with the TvShow as the root with seasons and episodes
            var seasonTvShows = plexTvShowSeasons.Select(x => x.TvShow).ToList();
            foreach (var season in plexTvShowSeasons)
            {
                var tvShow = seasonTvShows.Find(x => x.Id == season.TvShowId);
                tvShow?.Seasons.Add(season);
            }

            mergedPlexTvShows.AddRange(seasonTvShows);

            // Create hierarchy with the TvShow as the root with seasons and episodes

            var episodesTvShows = plexTvShowEpisodes.Select(x => x.TvShow).ToList();
            foreach (var episode in plexTvShowEpisodes)
            {
                var tvShow = seasonTvShows.Find(x => x.Id == episode.TvShowId);
            }

            await _dbContext.PlexTvShows
                .Where(x => request.TvShowIds.Contains(x.Id))
                .Select(x => new
                {
                    Seasons = x.Seasons.Where(y => request.TvShowSeasonIds.Contains(y.Id))
                        .Select(y => new
                        {
                            Episodes = y.Episodes.Where(z => request.TvShowEpisodeIds.Contains(z.Id)).ToList();
                        }).ToList(),
                })
                .ToListAsync();

            return ReturnResult(entity, request.Id);
        }
    }
}