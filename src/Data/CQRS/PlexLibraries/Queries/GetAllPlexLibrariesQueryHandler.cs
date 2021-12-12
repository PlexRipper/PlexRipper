using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.CQRS.PlexLibraries
{
    public class GetAllPlexLibrariesQueryValidator : AbstractValidator<GetAllPlexLibrariesQuery>
    {
        public GetAllPlexLibrariesQueryValidator()
        {
        }
    }

    public class GetAllPlexLibrariesQueryHandler : BaseHandler, IRequestHandler<GetAllPlexLibrariesQuery, Result<List<PlexLibrary>>>
    {
        public GetAllPlexLibrariesQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<List<PlexLibrary>>> Handle(GetAllPlexLibrariesQuery request, CancellationToken cancellationToken)
        {
            var query = PlexLibraryQueryable;


            if (request.IncludePlexServer)
            {
                query = query.IncludePlexServer();
            }

            var plexLibraries = await query.ToListAsync(cancellationToken);

            return Result.Ok(plexLibraries);
        }
    }
}