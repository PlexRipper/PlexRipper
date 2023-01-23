using System.Diagnostics;
using Data.Contracts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
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

        RuleForEach(x => x.PlexLibrary.Movies).ChildRules(plexMovie => { plexMovie.RuleFor(x => x.Key).GreaterThan(0); });
    }
}

public class CreateUpdateOrDeletePlexMoviesHandler : BaseHandler, IRequestHandler<CreateUpdateOrDeletePlexMoviesCommand, Result>
{
    public CreateUpdateOrDeletePlexMoviesHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

    public async Task<Result> Handle(CreateUpdateOrDeletePlexMoviesCommand command, CancellationToken cancellationToken)
    {
        var plexLibrary = command.PlexLibrary;
        var apiPlexMovies = plexLibrary.Movies;

        Log.Debug($"Starting adding or updating movies in library: {plexLibrary.Title}");
        var stopWatch = new Stopwatch();
        stopWatch.Start();

        apiPlexMovies.ForEach(x =>
        {
            x.PlexLibraryId = plexLibrary.Id;
            x.PlexServerId = plexLibrary.PlexServerId;
        });

        // Retrieve current Movies
        var plexMoviesInDb = await _dbContext.PlexMovies
            .Where(x => x.PlexLibraryId == plexLibrary.Id)
            .ToListAsync(cancellationToken);

        if (!plexMoviesInDb.Any())
        {
            _dbContext.PlexMovies.AddRange(apiPlexMovies);
            await SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }

        var addCount = 0;
        var updateCount = 0;
        var apiHash = apiPlexMovies.Select(x => x.Key).ToHashSet();
        foreach (var apiMovie in apiPlexMovies)
        {
            var existingMovie = plexMoviesInDb.FirstOrDefault(x => x.Key == apiMovie.Key);
            if (existingMovie == null)
            {
                _dbContext.PlexMovies.Add(apiMovie);
                addCount++;
            }
            else
            {
                apiMovie.Id = existingMovie.Id;
                _dbContext.PlexMovies.Update(apiMovie);
                updateCount++;
            }
        }

        var deleteMovies = plexMoviesInDb.Where(x => !apiHash.Contains(x.Key)).ToList();
        if (deleteMovies.Any())
            _dbContext.PlexMovies.RemoveRange(deleteMovies);

        if (addCount == 0 && updateCount == 0 && deleteMovies.Count == 0)
        {
            Log.Information($"No changes for library \"{plexLibrary.Title}\"");
            return Result.Ok();
        }

        await SaveChangesAsync(cancellationToken);

        Log.Debug($"Finished syncing plexLibrary: {plexLibrary.Title} with id: {plexLibrary.Id} in {stopWatch.Elapsed.TotalSeconds} seconds.");
        Log.Debug($"Add count: {addCount}, Update count: {updateCount}, Delete count: {deleteMovies.Count}");
        return Result.Ok();
    }
}