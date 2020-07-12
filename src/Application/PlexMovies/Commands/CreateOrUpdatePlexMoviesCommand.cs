using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain;
using PlexRipper.Domain.Base;
using PlexRipper.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PlexRipper.Application.PlexMovies
{
    public class CreateOrUpdatePlexMoviesCommand : IRequest<Result<PlexLibrary>>
    {
        public PlexLibrary PlexLibrary { get; }
        public List<PlexMovie> PlexMovies { get; }

        public CreateOrUpdatePlexMoviesCommand(PlexLibrary plexLibrary, List<PlexMovie> plexMovies)
        {
            PlexLibrary = plexLibrary;
            PlexMovies = plexMovies;
        }
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

    public class CreateOrUpdatePlexMoviesHandler : BaseHandler, IRequestHandler<CreateOrUpdatePlexMoviesCommand, Result<PlexLibrary>>
    {
        private readonly IPlexRipperDbContext _dbContext;

        public CreateOrUpdatePlexMoviesHandler(IPlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<PlexLibrary>> Handle(CreateOrUpdatePlexMoviesCommand command, CancellationToken cancellationToken)
        {
            var plexLibrary = command.PlexLibrary;
            var plexMovies = command.PlexMovies;

            Log.Debug($"Starting adding or updating movies in library: {plexLibrary.Title}");
            // Remove all movies and re-add them
            var currentMovies = await _dbContext.PlexMovies
                .AsTracking().Where(x => x.PlexLibraryId == plexLibrary.Id)
                .ToListAsync();
            _dbContext.PlexMovies.RemoveRange(currentMovies);
            await _dbContext.SaveChangesAsync(cancellationToken);


            // Ensure the correct ID is added. 
            foreach (var movie in plexMovies)
            {
                movie.PlexLibraryId = plexLibrary.Id;
            }

            // TODO update Roles and tags

            await _dbContext.PlexMovies.AddRangeAsync(plexMovies);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var entity = await _dbContext.PlexLibraries.Include(x => x.Movies)
                .FirstOrDefaultAsync(x => x.Id == plexLibrary.Id);

            ReturnResult(entity, plexLibrary.Id);
            return Result.Ok(entity);

        }
    }
}
