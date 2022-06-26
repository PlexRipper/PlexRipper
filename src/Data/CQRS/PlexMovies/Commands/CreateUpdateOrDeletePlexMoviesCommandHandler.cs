using System.Diagnostics;
using EFCore.BulkExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.Common;

namespace PlexRipper.Data.PlexMovies.Commands;

public class CreateUpdateOrDeletePlexMoviesValidator : AbstractValidator<CreateUpdateOrDeletePlexMoviesCommand>
{
    public CreateUpdateOrDeletePlexMoviesValidator()
    {
        RuleFor(x => x.PlexLibrary).NotNull();
        RuleFor(x => x.PlexLibrary.Id).GreaterThan(0);
        RuleFor(x => x.PlexLibrary.Title).NotEmpty();
        RuleFor(x => x.PlexLibrary.Movies).NotEmpty();

        RuleForEach(x => x.PlexLibrary.Movies).ChildRules(plexMovie =>
        {
            plexMovie.RuleFor(x => x.Key).GreaterThan(0);
        });
    }
}

public class CreateUpdateOrDeletePlexMoviesHandler : BaseHandler, IRequestHandler<CreateUpdateOrDeletePlexMoviesCommand, Result<bool>>
{
    public CreateUpdateOrDeletePlexMoviesHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

    public async Task<Result<bool>> Handle(CreateUpdateOrDeletePlexMoviesCommand command, CancellationToken cancellationToken)
    {
        var plexLibrary = command.PlexLibrary;
        var plexMoviesDict = new Dictionary<int, PlexMovie>();
        Log.Debug($"Starting adding or updating movies in library: {plexLibrary.Title}");
        var stopWatch = new Stopwatch();
        stopWatch.Start();

        command.PlexLibrary.Movies.ForEach(x =>
        {
            x.PlexLibraryId = plexLibrary.Id;
            x.PlexServerId = plexLibrary.PlexServerId;
            plexMoviesDict.Add(x.Key, x);
        });

        // Retrieve current tvShows
        var plexMoviesInDb = await _dbContext.PlexMovies
            .Where(x => x.PlexLibraryId == plexLibrary.Id)
            .ToListAsync(cancellationToken);

        if (!plexMoviesInDb.Any())
        {
            BulkInsert(plexMoviesDict.Values.ToList());

            return Result.Ok(true);
        }

        Dictionary<int, PlexMovie> plexMoviesDbDict = new Dictionary<int, PlexMovie>();
        plexMoviesInDb.ForEach(x => plexMoviesDbDict.Add(x.Key, x));

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

        Log.Information($"Finished updating plexLibrary: {plexLibrary.Title} with id: {plexLibrary.Id} in {stopWatch.Elapsed.TotalSeconds} seconds.");

        return Result.Ok(true);
    }

    private void BulkInsert(List<PlexMovie> plexMovies)
    {
        if (!plexMovies.Any())
        {
            return;
        }

        _dbContext.BulkInsert(plexMovies, _bulkConfig);
    }

    private void BulkUpdate(List<PlexMovie> plexMovies)
    {
        if (!plexMovies.Any())
        {
            return;
        }

        _dbContext.BulkUpdate(plexMovies);
    }
}