using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Data.Common;

namespace PlexRipper.Data.PlexLibraries;

public class GetAllPlexLibrariesQueryValidator : AbstractValidator<GetAllPlexLibrariesQuery>
{
    public GetAllPlexLibrariesQueryValidator() { }
}

public class GetAllPlexLibrariesQueryHandler : BaseHandler, IRequestHandler<GetAllPlexLibrariesQuery, Result<List<PlexLibrary>>>
{
    public GetAllPlexLibrariesQueryHandler(ILog log, PlexRipperDbContext dbContext) : base(log, dbContext) { }

    public async Task<Result<List<PlexLibrary>>> Handle(GetAllPlexLibrariesQuery request, CancellationToken cancellationToken)
    {
        var query = PlexLibraryQueryable;

        if (request.IncludePlexServer)
            query = query.IncludePlexServer();

        var plexLibraries = await query.ToListAsync(cancellationToken);

        return Result.Ok(plexLibraries);
    }
}