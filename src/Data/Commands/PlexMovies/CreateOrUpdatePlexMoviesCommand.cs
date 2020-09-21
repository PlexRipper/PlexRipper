using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.Base;
using PlexRipper.Application.PlexMovies;
using PlexRipper.Domain;

namespace PlexRipper.Data.Commands.PlexMovies
{
    public class CreateOrUpdatePlexMoviesValidator : AbstractValidator<CreateOrUpdatePlexMoviesCommand>
    {
        public CreateOrUpdatePlexMoviesValidator()
        {
            RuleFor(x => x.PlexLibrary).NotNull();
            RuleFor(x => x.PlexLibrary.Id).GreaterThan(0);
            RuleFor(x => x.PlexLibrary.Title).NotEmpty();
            RuleFor(x => x.PlexMovies).NotEmpty();
        }
    }

    public class CreateOrUpdatePlexMoviesHandler : IRequestHandler<CreateOrUpdatePlexMoviesCommand, Result<bool>>
    {
        private protected readonly PlexRipperDbContext _dbContext;

        private readonly BulkConfig _config = new BulkConfig
        {
            SetOutputIdentity = true,
            PreserveInsertOrder = true,
        };

        public CreateOrUpdatePlexMoviesHandler(PlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<bool>> Handle(CreateOrUpdatePlexMoviesCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var plexLibrary = command.PlexLibrary;
                var plexMovies = command.PlexMovies;
                plexMovies.ForEach(x => x.PlexLibraryId = plexLibrary.Id);

                Log.Debug($"Starting adding or updating movies in library: {plexLibrary.Title}");

                // Retrieve current movies
                var plexMoviesInDb = await _dbContext.PlexMovies
                    .Include(x => x.PlexMovieDatas)
                    .ThenInclude(x => x.Parts)
                    .AsTracking().Where(x => x.PlexLibraryId == plexLibrary.Id)
                    .ToListAsync(cancellationToken);

                if (!plexMoviesInDb.Any())
                {
                    BulkInsert(plexMovies);

                    return Result.Ok(true);
                }

                // TODO Test to remove some rows
                plexMovies.RemoveAll(x => x.Title.StartsWith("a", StringComparison.OrdinalIgnoreCase));
                for (int i = 0; i < 120; i++)
                {
                    plexMovies.Add(new PlexMovie
                    {
                        Key = (99999999 + i).ToString(),
                        Title = $"Fake movie {i}",
                        PlexLibraryId = plexLibrary.Id,
                        PlexMovieDatas = new List<PlexMovieData>
                        {
                            new PlexMovieData
                            {
                                MediaFormat = "ADDED TROLOLOL",
                                Parts = new List<PlexMovieDataPart>
                                {
                                    new PlexMovieDataPart
                                    {
                                        File = "ADDED HACKED FILE!",
                                    },
                                },
                            },
                        },
                    });
                }

                plexMovies.ForEach(x =>
                {
                    if (!x.Title.StartsWith("a", StringComparison.OrdinalIgnoreCase) && !x.Title.Contains("Fake movie"))
                    {
                        x.Title = "UPDATED!!!!";
                        x.UpdatedAt = DateTime.Now;
                        if (x.PlexMovieDatas?.First() != null)
                        {
                            x.PlexMovieDatas.First().MediaFormat = "UPDATED TROLOLOL";
                            if (x.PlexMovieDatas.First().Parts?.First() != null)
                            {
                                x.PlexMovieDatas.First().Parts.First().File = " UPDATED HACKED FILE!";
                            }
                        }
                    }
                });

                // Create lists on how to update the database.
                var deleteList = plexMoviesInDb.Where(p => plexMovies.All(l => p.RatingKey != l.RatingKey)).ToList();
                var addList = plexMovies.Where(p => plexMoviesInDb.All(l => p.RatingKey != l.RatingKey)).ToList();
                var updateList = plexMovies
                    .Where(p => addList.All(l => p.RatingKey != l.RatingKey) && deleteList.All(l => p.RatingKey != l.RatingKey))
                    .ToList();

                // Remove any that are not updated
                for (int i = updateList.Count - 1; i >= 0; i--)
                {
                    var plexMovieDb = plexMoviesInDb.Find(y => y.RatingKey == updateList[i].RatingKey);
                    updateList[i].Id = plexMovieDb.Id;
                    if (updateList[i].UpdatedAt <= plexMovieDb.UpdatedAt)
                    {
                        updateList.RemoveAt(i);
                    }
                }

                BulkInsert(addList);
                BulkUpdate(updateList);

                _dbContext.PlexMovies.RemoveRange(deleteList);
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

            _dbContext.BulkUpdate(plexMovies, _config);

            // Update the PlexMovieId of every PlexMovieData
            plexMovies.ForEach(x => x.PlexMovieDatas.ForEach(y => y.PlexMovieId = x.Id));
            var plexMovieDataList = plexMovies.SelectMany(x => x.PlexMovieDatas.Select(y => y)).ToList();
            _dbContext.BulkUpdate(plexMovieDataList, _config);

            plexMovieDataList.ForEach(x => x.Parts.ForEach(y => y.PlexMovieDataId = x.Id));
            var plexMovieDataPartList = plexMovieDataList.SelectMany(x => x.Parts.Select(y => y)).ToList();
            _dbContext.BulkUpdate(plexMovieDataPartList, _config);
        }
    }
}