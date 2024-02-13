using Data.Contracts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

/// <summary>
/// Retrieves all the <see cref="PlexLibrary">PlexLibraries </see> from the database.
/// </summary>
/// <param name="IncludePlexServer">Should the <see cref="PlexServer"/> of each library be included too.</param>
public record GetAllPlexLibrariesQuery(bool IncludePlexServer = false) : IRequest<Result<List<PlexLibrary>>>;

public class GetAllPlexLibrariesQueryValidator : AbstractValidator<GetAllPlexLibrariesQuery>
{
    public GetAllPlexLibrariesQueryValidator() { }
}

public class GetAllPlexLibrariesQueryHandler : IRequestHandler<GetAllPlexLibrariesQuery, Result<List<PlexLibrary>>>
{
    private readonly IPlexRipperDbContext _dbContext;

    public GetAllPlexLibrariesQueryHandler(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<PlexLibrary>>> Handle(GetAllPlexLibrariesQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.PlexLibraries.AsQueryable();

        if (request.IncludePlexServer)
            query = query.Include(x => x.PlexServer);

        var plexLibraries = await query.ToListAsync(cancellationToken);

        return Result.Ok(plexLibraries);
    }
}