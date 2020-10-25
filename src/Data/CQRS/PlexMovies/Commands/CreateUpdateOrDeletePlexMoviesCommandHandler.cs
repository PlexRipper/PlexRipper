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
using PlexRipper.Application.PlexMovies;
using PlexRipper.Domain;

namespace PlexRipper.Data.CQRS.PlexMovies.Commands
{
    public class CreateUpdateOrDeletePlexMoviesValidator : AbstractValidator<CreateUpdateOrDeletePlexMoviesCommand>
    {
        public CreateUpdateOrDeletePlexMoviesValidator()
        {
            RuleFor(x => x.PlexLibrary).NotNull();
            RuleFor(x => x.PlexLibrary.Id).GreaterThan(0);
            RuleFor(x => x.PlexLibrary.Title).NotEmpty();
            RuleFor(x => x.PlexMovies).NotEmpty();
            RuleForEach(x => x.PlexMovies).ChildRules(plexMovie =>
            {
                plexMovie.RuleFor(x => x.RatingKey).GreaterThan(0);
                plexMovie.RuleFor(x => x.PlexMovieDatas).NotEmpty();
                plexMovie.RuleForEach(x => x.PlexMovieDatas).ChildRules(plexMovieData =>
                {
                    plexMovieData.RuleFor(x => x.Height).GreaterThan(0);
                    plexMovieData.RuleFor(x => x.Width).GreaterThan(0);
                    plexMovieData.RuleFor(x => x.Parts).NotEmpty();
                    plexMovieData.RuleForEach(x => x.Parts).ChildRules(part =>
                    {
                        part.RuleFor(x => x.Key).NotEmpty();
                        part.RuleFor(x => x.Container).NotEmpty();
                    });
                });
            });
        }
    }

    public class CreateUpdateOrDeletePlexMoviesHandler : IRequestHandler<CreateUpdateOrDeletePlexMoviesCommand, Result<bool>>
    {
        private protected readonly PlexRipperDbContext _dbContext;

        private readonly BulkConfig _config = new BulkConfig
        {
            SetOutputIdentity = true,
            PreserveInsertOrder = true,
        };

        public CreateUpdateOrDeletePlexMoviesHandler(PlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<bool>> Handle(CreateUpdateOrDeletePlexMoviesCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var plexLibrary = command.PlexLibrary;
                var plexMoviesDict = new Dictionary<int, PlexMovie>();
                Log.Debug($"Starting adding or updating movies in library: {plexLibrary.Title}");

                command.PlexMovies.ForEach(x =>
                {
                    x.PlexLibraryId = plexLibrary.Id;
                    plexMoviesDict.Add(x.RatingKey, x);
                });

                // Retrieve current movies
                var plexMoviesInDb = await _dbContext.PlexMovies
                    .Include(x => x.PlexMovieDatas)
                    .ThenInclude(x => x.Parts)
                    .Where(x => x.PlexLibraryId == plexLibrary.Id)
                    .ToListAsync(cancellationToken);

                if (!plexMoviesInDb.Any())
                {
                    BulkInsert(plexMoviesDict.Values.ToList());

                    return Result.Ok(true);
                }

                Dictionary<int, PlexMovie> plexMoviesDbDict = new Dictionary<int, PlexMovie>();
                plexMoviesInDb.ForEach(x => plexMoviesDbDict.Add(x.RatingKey, x));

                // Create dictionaries on how to update the database.
                var addDict = plexMoviesDict.Where(x => !plexMoviesDbDict.ContainsKey(x.Key)).ToDictionary(k => k.Key, v => v.Value);
                var deleteDict = plexMoviesDbDict.Where(x => !plexMoviesDict.ContainsKey(x.Key)).ToDictionary(k => k.Key, v => v.Value);
                var updateDict = plexMoviesDict.Where(x => !deleteDict.ContainsKey(x.Key) && !addDict.ContainsKey(x.Key))
                    .ToDictionary(k => k.Key, v => v.Value);

                // Remove any that are not updated based on UpdatedAt
                foreach (var keyValuePair in updateDict)
                {
                    var plexMovieDb = plexMoviesDbDict[keyValuePair.Key];
                    keyValuePair.Value.Id = plexMovieDb.Id;
                    if (keyValuePair.Value.UpdatedAt <= plexMovieDb.UpdatedAt)
                    {
                        updateDict.Remove(keyValuePair.Key);
                    }
                }

                // Update database
                BulkInsert(addDict.Select(x => x.Value).ToList());
                BulkUpdate(updateDict.Select(x => x.Value).ToList());

                _dbContext.PlexMovies.RemoveRange(deleteDict.Select(x => x.Value).ToList());
                await _dbContext.SaveChangesAsync(cancellationToken);

                return Result.Ok(true);
            }
            catch (Exception e)
            {
                return Result.Fail(new ExceptionalError(e)).LogError();
            }
        }

        private void BulkInsert(List<PlexMovie> plexMovies)
        {
            if (!plexMovies.Any())
            {
                return;
            }

            _dbContext.BulkInsert(plexMovies, _config);

            // Update the PlexMovieId of every PlexMovieData
            plexMovies.ForEach(x => x.PlexMovieDatas.ForEach(y => y.PlexMovieId = x.Id));
            var plexMovieDataList = plexMovies.SelectMany(x => x.PlexMovieDatas.Select(y => y)).ToList();
            _dbContext.BulkInsert(plexMovieDataList, _config);

            plexMovieDataList.ForEach(x => x.Parts.ForEach(y => y.PlexMovieDataId = x.Id));
            var plexMovieDataPartList = plexMovieDataList.SelectMany(x => x.Parts.Select(y => y)).ToList();
            _dbContext.BulkInsert(plexMovieDataPartList, _config);
        }

        private void BulkUpdate(List<PlexMovie> plexMovies)
        {
            if (!plexMovies.Any())
            {
                return;
            }

            _dbContext.BulkUpdate(plexMovies);

            // Update the PlexMovieId of every PlexMovieData
            plexMovies.ForEach(x => x.PlexMovieDatas.ForEach(y => y.PlexMovieId = x.Id));

            // Remove old data and re-add PlexMovieData
            var plexMovieDataDeleteList = new List<PlexMovieData>();
            plexMovies.ForEach(x => plexMovieDataDeleteList.AddRange(_dbContext.PlexMovieData.Where(z => z.PlexMovieId == x.Id).ToList()));
            _dbContext.PlexMovieData.RemoveRange(plexMovieDataDeleteList);

            // Select all PlexMovieData and insert them
            var plexMovieDataList = plexMovies.SelectMany(x => x.PlexMovieDatas.Select(y => y)).ToList();
            _dbContext.BulkInsert(plexMovieDataList, _config);

            // Remove old data and re-add PlexMovieDataPart
            var plexMovieDataPartDeleteList = new List<PlexMovieDataPart>();
            plexMovieDataList.ForEach(x =>
                plexMovieDataPartDeleteList.AddRange(_dbContext.PlexMovieDataParts.Where(z => z.PlexMovieDataId == x.Id).ToList()));
            plexMovieDataList.ForEach(x => x.Parts.ForEach(y => y.PlexMovieDataId = x.Id));
            _dbContext.PlexMovieData.RemoveRange(plexMovieDataDeleteList);
            _dbContext.SaveChanges();

            // Select all PlexMovieDataPart and insert them
            var plexMovieDataPartList = plexMovieDataList.SelectMany(x => x.Parts.Select(y => y)).ToList();
            _dbContext.BulkInsert(plexMovieDataPartList);
        }
    }
}