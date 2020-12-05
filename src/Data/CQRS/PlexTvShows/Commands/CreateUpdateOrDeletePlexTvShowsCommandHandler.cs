using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.PlexTvShows;
using PlexRipper.Domain;

namespace PlexRipper.Data.CQRS.PlexTvShows
{
    public class CreateUpdateOrDeletePlexTvShowsCommandValidator : AbstractValidator<CreateUpdateOrDeletePlexTvShowsCommand>
    {
        public CreateUpdateOrDeletePlexTvShowsCommandValidator()
        {
            RuleFor(x => x.PlexLibrary).NotNull();
            RuleFor(x => x.PlexLibrary.Id).GreaterThan(0);
            RuleFor(x => x.PlexLibrary.Title).NotEmpty();
            RuleFor(x => x.PlexLibrary.TvShows).NotEmpty();
        }
    }

    public class CreateUpdateOrDeletePlexTvShowsCommandHandler : IRequestHandler<CreateUpdateOrDeletePlexTvShowsCommand, Result<bool>>
    {
        private PlexRipperDbContext _dbContext { get; }

        private readonly BulkConfig _config = new BulkConfig
        {
            SetOutputIdentity = true,
            PreserveInsertOrder = true,
        };

        public CreateUpdateOrDeletePlexTvShowsCommandHandler(PlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<bool>> Handle(CreateUpdateOrDeletePlexTvShowsCommand command, CancellationToken cancellationToken)
        {
            try
            {
                // TODO Currently this handler only creates as it is expected to delete everything beforehand
                var plexLibrary = command.PlexLibrary;

                Log.Debug($"Starting adding or updating tv shows in library: {plexLibrary.Title}");

                // Ensure all tvShows, seasons and episodes have the correct plexLibraryId assigned
                var plexTvShowsDict = new Dictionary<int, PlexTvShow>();
                command.PlexLibrary.TvShows.ForEach(plexTvShow =>
                {
                    plexTvShow.PlexLibraryId = plexLibrary.Id;
                    plexTvShow.Seasons.ForEach(x =>
                    {
                        x.PlexLibraryId = plexLibrary.Id;
                        if (x.Episodes != null && !x.Episodes.Any())
                        {
                            x.Episodes.ForEach(y => y.PlexLibraryId = plexLibrary.Id);
                        }
                    });

                    // Add to dictionary to later compare against DB data
                    plexTvShowsDict.Add(plexTvShow.RatingKey, plexTvShow);
                });

                // Retrieve current tv shows
                var tvShowsInDb = await _dbContext.PlexTvShows
                    .Include(x => x.Seasons)
                    .ThenInclude(x => x.Episodes)
                    .ThenInclude(x => x.EpisodeData)
                    .ThenInclude(x => x.Parts)
                    .Where(x => x.PlexLibraryId == plexLibrary.Id)
                    .ToListAsync(cancellationToken);

                if (!tvShowsInDb.Any())
                {
                    BulkInsert(plexTvShowsDict.Values.ToList());
                    return Result.Ok(true);
                }

                // Create compare dictionary based on the rating key of the current data in the database.
                Dictionary<int, PlexTvShow> PlexTvShowDbDict = new Dictionary<int, PlexTvShow>();
                tvShowsInDb.ForEach(x => PlexTvShowDbDict.Add(x.RatingKey, x));

                // Create dictionaries on how to update the database.
                var addDict = plexTvShowsDict.Where(x => !PlexTvShowDbDict.ContainsKey(x.Key)).ToDictionary(k => k.Key, v => v.Value);
                var deleteDict = PlexTvShowDbDict.Where(x => !plexTvShowsDict.ContainsKey(x.Key)).ToDictionary(k => k.Key, v => v.Value);
                var updateDict = plexTvShowsDict.Where(x => !deleteDict.ContainsKey(x.Key) && !addDict.ContainsKey(x.Key))
                    .ToDictionary(k => k.Key, v => v.Value);

                // Remove any that are not updated based on UpdatedAt
                foreach (var keyValuePair in updateDict)
                {
                    var plexTvShowDb = PlexTvShowDbDict[keyValuePair.Key];
                    var plexTvShow = keyValuePair.Value;

                    // Remove from list if it has not been updated
                    if (plexTvShow.UpdatedAt <= plexTvShowDb.UpdatedAt)
                    {
                        updateDict.Remove(keyValuePair.Key);
                    }
                    else
                    {
                        // Copy over the Id of the current db entry
                        plexTvShow.Id = plexTvShowDb.Id;
                    }
                }

                // Update database
                BulkInsert(addDict.Select(x => x.Value).ToList());
                BulkUpdate(updateDict.Select(x => x.Value).ToList());

                _dbContext.PlexTvShows.RemoveRange(deleteDict.Select(x => x.Value).ToList());
                await _dbContext.SaveChangesAsync(cancellationToken);

                return Result.Ok(true);
            }
            catch (Exception e)
            {
                return Result.Fail(new ExceptionalError(e)).LogError();
            }
        }

        private void BulkInsert(List<PlexTvShow> plexTvShows)
        {
            if (!plexTvShows.Any())
            {
                return;
            }

            try
            {
                _dbContext.BulkInsert(plexTvShows, _config);

                // Update the TvShowId of every plexTvShow
                plexTvShows.ForEach(x => x.Seasons?.ForEach(y => y.TvShowId = x.Id));
                var plexTvShowSeasons = plexTvShows.SelectMany(x => x.Seasons.Select(y => y)).ToList();
                if (plexTvShowSeasons.Count == 0)
                {
                    return;
                }

                _dbContext.BulkInsert(plexTvShowSeasons, _config);

                // Update the TvShowSeasonId of every plexTvShowSeason
                plexTvShowSeasons.ForEach(x => x.Episodes?.ForEach(y => y.TvShowSeasonId = x.Id));
                var plexTvShowEpisodes = plexTvShowSeasons?.SelectMany(x => x.Episodes.Select(y => y))?.ToList();
                if (plexTvShowEpisodes.Count == 0)
                {
                    return;
                }

                _dbContext.BulkInsert(plexTvShowEpisodes, _config);

                // Update the PlexTvShowEpisodeId of every plexTvShowEpisode
                plexTvShowEpisodes.ForEach(x => x.EpisodeData?.ForEach(y => y.PlexTvShowEpisodeId = x.Id));
                var plexTvShowEpisodeDataList = plexTvShowEpisodes?.SelectMany(x => x.EpisodeData?.Select(y => y))?.ToList();
                if (plexTvShowEpisodeDataList.Count == 0)
                {
                    return;
                }

                _dbContext.BulkInsert(plexTvShowEpisodeDataList, _config);

                // Update the PlexTvShowEpisodeDataId of every plexTvShowEpisodeData
                plexTvShowEpisodeDataList?.ForEach(x => x.Parts?.ForEach(y => y.PlexTvShowEpisodeDataId = x.Id));
                var plexTvShowEpisodeDataPartList = plexTvShowEpisodeDataList?.SelectMany(x => x.Parts?.Select(y => y))?.ToList();
                if (plexTvShowEpisodeDataPartList.Count == 0)
                {
                    return;
                }

                _dbContext.BulkInsert(plexTvShowEpisodeDataPartList);
            }
            catch (Exception e)
            {
                Log.Fatal(e);
                throw;
            }
        }

        private void BulkUpdate(List<PlexTvShow> plexTvShows)
        {
            if (!plexTvShows.Any())
            {
                return;
            }

            _dbContext.PlexTvShows.UpdateRange(plexTvShows);

            // TODO Currently, seasons and episodes that are changed will not be saved to the database. A different approach will be required for tvShows with the goal of maintaining the already assigned Id's

            //
            // _dbContext.BulkUpdate(plexTvShows);
            // // Update the PlexMovieId of every PlexMovieData
            // plexTvShows.ForEach(x => x.plex.ForEach(y => y.PlexMovieId = x.Id));
            //
            // // Remove old data and re-add PlexMovieData
            // var plexMovieDataDeleteList = new List<PlexMovieData>();
            // plexTvShows.ForEach(x => plexMovieDataDeleteList.AddRange(_dbContext.PlexMovieData.Where(z => z.PlexMovieId == x.Id).ToList()));
            // _dbContext.PlexMovieData.RemoveRange(plexMovieDataDeleteList);
            //
            // // Select all PlexMovieData and insert them
            // var plexMovieDataList = plexTvShows.SelectMany(x => x.PlexMovieDatas.Select(y => y)).ToList();
            // _dbContext.BulkInsert(plexMovieDataList, _config);
            //
            // // Remove old data and re-add PlexMovieDataPart
            // var plexMovieDataPartDeleteList = new List<PlexMovieDataPart>();
            // plexMovieDataList.ForEach(x =>
            //     plexMovieDataPartDeleteList.AddRange(_dbContext.PlexMovieDataParts.Where(z => z.PlexMovieDataId == x.Id).ToList()));
            // plexMovieDataList.ForEach(x => x.Parts.ForEach(y => y.PlexMovieDataId = x.Id));
            // _dbContext.PlexMovieData.RemoveRange(plexMovieDataDeleteList);
            // _dbContext.SaveChanges();
            //
            // // Select all PlexMovieDataPart and insert them
            // var plexMovieDataPartList = plexMovieDataList.SelectMany(x => x.Parts.Select(y => y)).ToList();
            // _dbContext.BulkInsert(plexMovieDataPartList);
        }
    }
}