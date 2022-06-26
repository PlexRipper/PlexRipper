using System.Diagnostics;
using EFCore.BulkExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.Common;

namespace PlexRipper.Data.PlexTvShows
{
    public class CreateUpdateOrDeletePlexTvShowsCommandValidator : AbstractValidator<CreateUpdateOrDeletePlexTvShowsCommand>
    {
        public CreateUpdateOrDeletePlexTvShowsCommandValidator()
        {
            RuleFor(x => x.PlexLibrary).NotNull();
            RuleFor(x => x.PlexLibrary.Id).GreaterThan(0);
            RuleFor(x => x.PlexLibrary.Title).NotEmpty();
            RuleFor(x => x.PlexLibrary.TvShows).NotEmpty();

            RuleForEach(x => x.PlexLibrary.TvShows).ChildRules(plexTvShow =>
            {
                plexTvShow.RuleFor(x => x.Key).GreaterThan(0);
                plexTvShow.RuleForEach(x => x.Seasons).ChildRules(plexTvShowSeason =>
                {
                    plexTvShowSeason.RuleFor(x => x.Key).GreaterThan(0);
                });
            });
        }
    }

    public class CreateUpdateOrDeletePlexTvShowsCommandHandler : BaseHandler, IRequestHandler<CreateUpdateOrDeletePlexTvShowsCommand, Result<bool>>
    {
        private readonly BulkConfig _config = new BulkConfig
        {
            SetOutputIdentity = true,
            PreserveInsertOrder = true,
        };

        public CreateUpdateOrDeletePlexTvShowsCommandHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<bool>> Handle(CreateUpdateOrDeletePlexTvShowsCommand command, CancellationToken cancellationToken)
        {
            var plexLibrary = command.PlexLibrary;

            Log.Debug($"Starting adding, updating or deleting of tv shows in library: {plexLibrary.Title} - {plexLibrary.Id}");
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            // Ensure all tvShows, seasons and episodes have the correct plexLibraryId assigned and other values
            var plexTvShowsDict = new Dictionary<int, PlexTvShow>();
            command.PlexLibrary.TvShows.ForEach(plexTvShow =>
            {
                plexTvShow.PlexLibraryId = plexLibrary.Id;
                plexTvShow.PlexServerId = plexLibrary.PlexServerId;

                plexTvShow.Seasons?.ForEach(season =>
                {
                    season.PlexLibraryId = plexLibrary.Id;
                    season.PlexServerId = plexLibrary.PlexServerId;
                    season.ParentKey = plexTvShow.Key;
                    season.TvShow = plexTvShow;

                    season.Episodes?.ForEach(episode =>
                    {
                        episode.PlexLibraryId = plexLibrary.Id;
                        episode.PlexServerId = plexLibrary.PlexServerId;
                        episode.ParentKey = season.Key;
                        episode.TvShowSeason = season;
                    });
                });

                // Add to dictionary to later compare against DB data
                plexTvShowsDict.Add(plexTvShow.Key, plexTvShow);
            });

            // Retrieve current tv shows
            var tvShowsInDb = await PlexTvShowsQueryable.IncludeEpisodes()
                .Where(x => x.PlexLibraryId == plexLibrary.Id)
                .ToListAsync(cancellationToken);

            // Create compare dictionary based on the rating key of the current data in the database.
            Dictionary<int, PlexTvShow> PlexTvShowDbDict = new();
            tvShowsInDb.ForEach(x => PlexTvShowDbDict.Add(x.Key, x));

            // Create dictionaries on how to update the database.
            var addTvShowDict = plexTvShowsDict.Where(x => !PlexTvShowDbDict.ContainsKey(x.Key)).ToDictionary(k => k.Key, v => v.Value);
            var deleteTvShowDict = PlexTvShowDbDict.Where(x => !plexTvShowsDict.ContainsKey(x.Key)).ToDictionary(k => k.Key, v => v.Value);
            var updateTvShowDict = plexTvShowsDict.Where(x => !deleteTvShowDict.ContainsKey(x.Key) && !addTvShowDict.ContainsKey(x.Key))
                .ToDictionary(k => k.Key, v => v.Value);

            var addSeasonDict = new Dictionary<int, PlexTvShowSeason>();
            var deleteSeasonDict = new Dictionary<int, PlexTvShowSeason>();
            var updateSeasonDict = new Dictionary<int, PlexTvShowSeason>();

            var addEpisodeDict = new Dictionary<int, PlexTvShowEpisode>();
            var deleteEpisodeDict = new Dictionary<int, PlexTvShowEpisode>();
            var updateEpisodeDict = new Dictionary<int, PlexTvShowEpisode>();

            // Generate add dictionaries for seasons and episodes
            addTvShowDict.SelectMany(x => x.Value.Seasons).ToList().ForEach(x => addSeasonDict.Add(x.Key, x));
            addSeasonDict.SelectMany(x => x.Value.Episodes).ToList().ForEach(x => addEpisodeDict.Add(x.Key, x));

            // Generate delete dictionaries for seasons and episodes
            deleteTvShowDict.SelectMany(x => x.Value.Seasons).ToList().ForEach(x => deleteSeasonDict.Add(x.Key, x));
            deleteSeasonDict.SelectMany(x => x.Value.Episodes).ToList().ForEach(x => deleteEpisodeDict.Add(x.Key, x));

            // Further filter down what should be updated
            foreach (KeyValuePair<int, PlexTvShow> keyValuePair in updateTvShowDict)
            {
                var plexTvShowDb = PlexTvShowDbDict[keyValuePair.Key];
                var newPlexTvShow = keyValuePair.Value;

                // Remove from list if it has not been updated
                if (newPlexTvShow.UpdatedAt <= plexTvShowDb.UpdatedAt)
                {
                    updateTvShowDict.Remove(keyValuePair.Key);
                    continue;
                }

                // Copy over the Id of the current db entry
                newPlexTvShow.Id = plexTvShowDb.Id;
                var seasonList = plexTvShowDb.Seasons;
                var episodeList = seasonList.SelectMany(x => x.Episodes).ToList();

                // Filter the seasons by Add, update or delete.
                foreach (var newPlexTvSeason in newPlexTvShow.Seasons)
                {
                    var searchSeasonResult = plexTvShowDb.Seasons.Find(x => x.Key == newPlexTvSeason.Key);
                    if (searchSeasonResult is null)
                    {
                        // Add season
                        addSeasonDict.Add(newPlexTvSeason.Key, newPlexTvSeason);

                        // Add all episodes because the season is new.
                        newPlexTvSeason.Episodes.ForEach(newEpisode => addEpisodeDict.Add(newEpisode.Key, newEpisode));
                    }
                    else
                    {
                        //Update seasons
                        newPlexTvSeason.Id = searchSeasonResult.Id;
                        newPlexTvSeason.TvShowId = searchSeasonResult.TvShowId;
                        updateSeasonDict.Add(newPlexTvSeason.Key, newPlexTvSeason);

                        // Filter the episodes by Add, update or delete.
                        newPlexTvSeason.Episodes.ForEach(newPlexTvShowEpisode =>
                        {
                            // Set tvShowId and tvShowSeasonId in every episode
                            newPlexTvShowEpisode.TvShowId = newPlexTvShow.Id;
                            newPlexTvShowEpisode.TvShowSeasonId = newPlexTvSeason.Id;
                            var searchEpisodeResult = episodeList.Find(x => x.Key == newPlexTvShowEpisode.Key);
                            if (searchEpisodeResult is null)
                            {
                                // Add new episodes
                                addEpisodeDict.Add(newPlexTvShowEpisode.Key, newPlexTvShowEpisode);
                            }
                            else
                            {
                                // Update existing episodes
                                newPlexTvShowEpisode.Id = searchEpisodeResult.Id;
                                updateEpisodeDict.Add(newPlexTvShowEpisode.Key, newPlexTvShowEpisode);
                            }
                        });
                    }
                }

                // Delete seasons
                var seasonHashSet = new HashSet<int>();
                newPlexTvShow.Seasons.ForEach(x => seasonHashSet.Add(x.Key));
                seasonList.Where(seasonDb => !seasonHashSet.Contains(seasonDb.Key)).ToList().ForEach(x => deleteSeasonDict.Add(x.Key, x));

                // Delete episodes
                var episodeHashSet = new HashSet<int>();
                newPlexTvShow.Seasons.ForEach(x => x.Episodes.ForEach(y => episodeHashSet.Add(y.Key)));
                episodeList.Where(episodeDb => !episodeHashSet.Contains(episodeDb.Key)).ToList().ForEach(x => deleteEpisodeDict.Add(x.Key, x));
            }

            // Update database
            await BulkInsertTvShows(addTvShowDict, addSeasonDict, addEpisodeDict, cancellationToken);

            await _dbContext.BulkUpdateAsync(updateTvShowDict.Select(x => x.Value).ToList(), _config, cancellationToken: cancellationToken);
            await _dbContext.BulkUpdateAsync(updateSeasonDict.Select(x => x.Value).ToList(), _config, cancellationToken: cancellationToken);
            await _dbContext.BulkUpdateAsync(updateEpisodeDict.Select(x => x.Value).ToList(), _config, cancellationToken: cancellationToken);

            await _dbContext.BulkDeleteAsync(deleteTvShowDict.Select(x => x.Value).ToList(), _config, cancellationToken: cancellationToken);
            await _dbContext.BulkDeleteAsync(deleteSeasonDict.Select(x => x.Value).ToList(), _config, cancellationToken: cancellationToken);
            await _dbContext.BulkDeleteAsync(deleteEpisodeDict.Select(x => x.Value).ToList(), _config, cancellationToken: cancellationToken);

            stopWatch.Stop();
            Log.Information(
                $"Finished updating plexLibrary: {plexLibrary.Title} with id: {plexLibrary.Id} in {stopWatch.Elapsed.TotalSeconds} seconds.");
            return Result.Ok(true);
        }

        private async Task BulkInsertTvShows(Dictionary<int, PlexTvShow> tvShowDict, Dictionary<int, PlexTvShowSeason> seasonDict,
            Dictionary<int, PlexTvShowEpisode> episodeDict, CancellationToken token)
        {
            // Add tvShows to DB.
            var tvShowAddList = tvShowDict.Select(x => x.Value).ToList();
            await _dbContext.BulkInsertAsync(tvShowAddList, _config, cancellationToken: token);

            // Add tvShowId to every season and episode and then insert seasons in DB.
            var tvShowSeasonAddList = seasonDict.Select(x => x.Value).ToList();
            tvShowAddList.ForEach(tvShow => tvShow.Seasons.ForEach(season =>
            {
                season.TvShowId = tvShow.Id;
                season.Episodes.ForEach(episode => episode.TvShowId = tvShow.Id);
            }));
            tvShowSeasonAddList.ForEach(season =>
            {
                season.TvShowId = season.TvShow.Id;
                season.Episodes.ForEach(episode => episode.TvShowId = season.TvShow.Id);
            });
            await _dbContext.BulkInsertAsync(tvShowSeasonAddList, _config, cancellationToken: token);

            // Add season id to every episode and then insert episodes in DB.
            var tvShowEpisodeAddList = episodeDict.Select(x => x.Value).ToList();
            tvShowSeasonAddList.ForEach(season => season.Episodes.ForEach(episode => episode.TvShowSeasonId = season.Id));
            tvShowEpisodeAddList.ForEach(episode =>
            {
                episode.TvShowSeasonId = episode.TvShowSeason.Id;
                episode.TvShowId = episode.TvShowSeason.TvShowId;
            });
            await _dbContext.BulkInsertAsync(tvShowEpisodeAddList, _config, cancellationToken: token);
        }
    }
}