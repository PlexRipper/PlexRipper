using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.Base;

namespace PlexRipper.Application.PlexMovies
{
    public class CreateOrUpdatePlexMoviesCommand : IRequest<Result<bool>>
    {
        public CreateOrUpdatePlexMoviesCommand(PlexLibrary plexLibrary, List<PlexMovie> plexMovies)
        {
            PlexLibrary = plexLibrary;
            PlexMovies = plexMovies;
        }

        public PlexLibrary PlexLibrary { get; }

        public List<PlexMovie> PlexMovies { get; }
    }

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

    public class CreateOrUpdatePlexMoviesHandler : BaseHandler, IRequestHandler<CreateOrUpdatePlexMoviesCommand, Result<bool>>
    {
        public CreateOrUpdatePlexMoviesHandler(IPlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<bool>> Handle(CreateOrUpdatePlexMoviesCommand command, CancellationToken cancellationToken)
        {
            var plexLibrary = command.PlexLibrary;
            var plexMovies = command.PlexMovies;

            Log.Debug($"Starting adding or updating movies in library: {plexLibrary.Title}");

            // Remove all movies and re-add them
            // TODO Write a better method of only removing the movies that are missing and adding only the new ones
            var currentMovies = await _dbContext.PlexMovies
                .AsTracking().Where(x => x.PlexLibraryId == plexLibrary.Id)
                .ToListAsync(cancellationToken);
            _dbContext.PlexMovies.RemoveRange(currentMovies);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Ensure the correct ID is added.
            foreach (var movie in plexMovies)
            {
                movie.PlexLibraryId = plexLibrary.Id;
            }

            // TODO update Roles and tags

            await _dbContext.PlexMovies.AddRangeAsync(plexMovies, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok(true);
        }
    }
}