using System;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.PlexLibraries;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.CQRS.PlexLibraries
{
    public class DeleteMediaFromPlexLibraryCommandValidator : AbstractValidator<DeleteMediaFromPlexLibraryCommand>
    {
        public DeleteMediaFromPlexLibraryCommandValidator()
        {
            RuleFor(x => x.PlexLibraryId).GreaterThan(0);
        }
    }

    public class DeleteMediaFromPlexLibraryCommandHandler : BaseHandler, IRequestHandler<DeleteMediaFromPlexLibraryCommand, Result<bool>>
    {
        public DeleteMediaFromPlexLibraryCommandHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<bool>> Handle(DeleteMediaFromPlexLibraryCommand command, CancellationToken cancellationToken)
        {
            try
            {
                // First retrieve only the plexLibrary to determine the media type.
                var entity = await _dbContext.PlexLibraries.AsNoTracking().FirstOrDefaultAsync(x => x.Id == command.PlexLibraryId, cancellationToken);
                if (entity == null)
                {
                    return ResultExtensions.GetEntityNotFound(nameof(PlexLibrary), command.PlexLibraryId);
                }

                // Then construct the database query,
                // this improves performance such as not to check the tables which will return no result anyway.
                var plexLibraryQuery = GetPlexLibraryQueryableByType(entity.Type, false, true);

                // We only want to delete the media and preserve the PlexLibrary entry in the Db.
                var plexLibrary = await plexLibraryQuery.AsTracking().FirstOrDefaultAsync(x => x.Id == command.PlexLibraryId, cancellationToken);
                switch (plexLibrary.Type)
                {
                    case PlexMediaType.Movie:
                        _dbContext.PlexMovies.RemoveRange(plexLibrary.Movies);
                        break;
                    case PlexMediaType.TvShow:
                        _dbContext.PlexTvShows.RemoveRange(plexLibrary.TvShows);
                        break;
                    default:
                        return Result.Fail($"PlexLibrary with Id {plexLibrary.Id} and MediaType {plexLibrary.Type} is currently not supported");
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                return Result.Ok(true);
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                return Result.Fail(new ExceptionalError(e));
            }
        }
    }
}