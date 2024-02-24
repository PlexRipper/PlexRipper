using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

/// <summary>
/// Retrieves all the  <see cref="PlexServer">PlexServers</see> with all connections currently in the database.
/// </summary>
public record GetAllPlexServersQuery() : IRequest<Result<List<PlexServer>>>;

public class GetAllPlexServersQueryValidator : AbstractValidator<GetAllPlexServersQuery> { }

public class GetAllPlexServersQueryHandler : IRequestHandler<GetAllPlexServersQuery, Result<List<PlexServer>>>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;

    public GetAllPlexServersQueryHandler(ILog log, IPlexRipperDbContext dbContext)
    {
        _log = log;
        _dbContext = dbContext;
    }

    public async Task<Result<List<PlexServer>>> Handle(GetAllPlexServersQuery request, CancellationToken cancellationToken)
    {
        var plexServers = await _dbContext.PlexServers
            .Include(x => x.PlexServerConnections)
            .ThenInclude(x => x.PlexServerStatus.OrderByDescending(y => y.LastChecked).Take(1))
            .ToListAsync(cancellationToken);

        return Result.Ok(plexServers);
    }
}