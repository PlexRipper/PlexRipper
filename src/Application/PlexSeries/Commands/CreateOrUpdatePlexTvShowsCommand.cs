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

namespace PlexRipper.Application.PlexSeries
{
    public class CreateOrUpdatePlexTvShowsCommand : IRequest<Result<PlexLibrary>>
    {
        public PlexLibrary PlexLibrary { get; }
        public List<PlexSerie> PlexTvShows { get; }

        public CreateOrUpdatePlexTvShowsCommand(PlexLibrary plexLibrary, List<PlexSerie> plexTvShows)
        {
            PlexLibrary = plexLibrary;
            PlexTvShows = plexTvShows;
        }
    }

    public class CreateOrUpdatePlexTvShowsValidator : AbstractValidator<CreateOrUpdatePlexTvShowsCommand>
    {
        public CreateOrUpdatePlexTvShowsValidator()
        {
            RuleFor(x => x.PlexLibrary).NotNull();
            RuleFor(x => x.PlexLibrary.Id).GreaterThan(0);
            RuleFor(x => x.PlexLibrary.Title).NotEmpty();
            RuleFor(x => x.PlexTvShows).NotEmpty();
        }
    }

    public class CreateOrUpdatePlexTvShowsHandler : BaseHandler, IRequestHandler<CreateOrUpdatePlexTvShowsCommand, Result<PlexLibrary>>
    {
        private readonly IPlexRipperDbContext _dbContext;

        public CreateOrUpdatePlexTvShowsHandler(IPlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<PlexLibrary>> Handle(CreateOrUpdatePlexTvShowsCommand command, CancellationToken cancellationToken)
        {
            var plexLibrary = command.PlexLibrary;
            var plexTvShows = command.PlexTvShows;

            Log.Debug($"Starting adding or updating tv shows in library: {plexLibrary.Title}");
            // Remove all movies and re-add them
            var currentTvShows = await _dbContext.PlexTvShows
                .AsTracking().Where(x => x.PlexLibraryId == plexLibrary.Id)
                .ToListAsync();
            _dbContext.PlexTvShows.RemoveRange(currentTvShows);
            await _dbContext.SaveChangesAsync(cancellationToken);


            // Ensure the correct ID is added. 
            foreach (var tvShow in plexTvShows)
            {
                tvShow.PlexLibraryId = plexLibrary.Id;
            }

            // TODO update Roles and tags

            await _dbContext.PlexTvShows.AddRangeAsync(plexTvShows);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var entity = await _dbContext.PlexLibraries.Include(x => x.Series)
                .FirstOrDefaultAsync(x => x.Id == plexLibrary.Id);

            ReturnResult(entity, plexLibrary.Id);
            return Result.Ok(entity);

        }
    }
}
