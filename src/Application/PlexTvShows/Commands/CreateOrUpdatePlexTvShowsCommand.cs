using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Base;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain;
using PlexRipper.Domain.Entities;

namespace PlexRipper.Application.PlexTvShows
{
    public class CreateOrUpdatePlexTvShowsCommand : IRequest<Result<bool>>
    {
        public PlexLibrary PlexLibrary { get; }
        public List<PlexTvShow> PlexTvShows { get; }

        public CreateOrUpdatePlexTvShowsCommand(PlexLibrary plexLibrary, List<PlexTvShow> plexTvShows)
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

    public class CreateOrUpdatePlexTvShowsHandler : BaseHandler, IRequestHandler<CreateOrUpdatePlexTvShowsCommand, Result<bool>>
    {
        public CreateOrUpdatePlexTvShowsHandler(IPlexRipperDbContext dbContext): base(dbContext) { }

        public async Task<Result<bool>> Handle(CreateOrUpdatePlexTvShowsCommand command, CancellationToken cancellationToken)
        {
            var result = await ValidateAsync<CreateOrUpdatePlexTvShowsCommand, CreateOrUpdatePlexTvShowsValidator>(command);
            if (result.IsFailed) return result;

            var plexLibrary = command.PlexLibrary;
            var plexTvShows = command.PlexTvShows;

            Log.Debug($"Starting adding or updating tv shows in library: {plexLibrary.Title}");
            // Remove all movies and re-add them
            var currentTvShows = await _dbContext.PlexTvShows
                .AsTracking().Where(x => x.PlexLibraryId == plexLibrary.Id)
                .ToListAsync(cancellationToken);
            _dbContext.PlexTvShows.RemoveRange(currentTvShows);
            await _dbContext.SaveChangesAsync(cancellationToken);


            // Ensure the correct ID is added.
            foreach (var tvShow in plexTvShows)
            {
                tvShow.PlexLibraryId = plexLibrary.Id;
            }

            // TODO update Roles and tags

            await _dbContext.PlexTvShows.AddRangeAsync(plexTvShows, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok(true);

        }
    }
}
