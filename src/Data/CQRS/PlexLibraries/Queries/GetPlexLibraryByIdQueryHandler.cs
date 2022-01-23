using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.PlexLibraries
{
    public class GetPlexLibraryByIdQueryValidator : AbstractValidator<GetPlexLibraryByIdQuery>
    {
        public GetPlexLibraryByIdQueryValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }

    public class GetPlexLibraryByIdWithMediaHandler : BaseHandler, IRequestHandler<GetPlexLibraryByIdQuery, Result<PlexLibrary>>
    {
        public GetPlexLibraryByIdWithMediaHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<PlexLibrary>> Handle(GetPlexLibraryByIdQuery request, CancellationToken cancellationToken)
        {
            var query = PlexLibraryQueryable;

            var result = await query.FirstOrDefaultAsync(x => x.Id == request.Id);
            if (result == null)
            {
                return ResultExtensions.EntityNotFound(nameof(PlexLibrary), request.Id);
            }

            var plexLibrary = await GetPlexLibraryQueryableByType(result.Type, request.IncludePlexServer, request.IncludeMedia, request.TopLevelMediaOnly)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (plexLibrary == null)
            {
                return ResultExtensions.EntityNotFound(nameof(PlexLibrary), request.Id);
            }

            return Result.Ok(plexLibrary.SortMedia());
        }
    }
}