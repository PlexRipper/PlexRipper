using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Data.Common;

namespace PlexRipper.Data.PlexLibraries;

public class GetPlexLibraryByIdWithServerQueryValidator : AbstractValidator<GetPlexLibraryByIdWithServerQuery>
{
    public GetPlexLibraryByIdWithServerQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class GetPlexLibraryByIdWithServerQueryHandler
    : BaseHandler,
        IRequestHandler<GetPlexLibraryByIdWithServerQuery, Result<PlexLibrary>>
{
    public GetPlexLibraryByIdWithServerQueryHandler(ILog log, PlexRipperDbContext dbContext)
        : base(log, dbContext) { }

    public async Task<Result<PlexLibrary>> Handle(
        GetPlexLibraryByIdWithServerQuery request,
        CancellationToken cancellationToken
    )
    {
        var plexLibrary = await PlexLibraryQueryable
            .IncludePlexServer()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        return ReturnResult(plexLibrary, request.Id);
    }
}
