using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public record GetAllPlexServerConnectionsQuery() : IRequest<Result<List<PlexServerConnection>>>;

public class GetAllPlexServerConnectionsQueryValidator : AbstractValidator<GetAllPlexServerConnectionsQuery> { }

public class GetAllPlexServerConnectionsQueryHandlerHandler : IRequestHandler<GetAllPlexServerConnectionsQuery, Result<List<PlexServerConnection>>>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;

    public GetAllPlexServerConnectionsQueryHandlerHandler(ILog log, IPlexRipperDbContext dbContext)
    {
        _log = log;
        _dbContext = dbContext;
    }

    public async Task<Result<List<PlexServerConnection>>> Handle(GetAllPlexServerConnectionsQuery request, CancellationToken cancellationToken)
    {
        var plexServerConnections = await _dbContext
            .PlexServerConnections
            .Include(x => x.PlexServerStatus.OrderByDescending(y => y.LastChecked).Take(5))
            .ToListAsync(cancellationToken);

        return Result.Ok(plexServerConnections);
    }
}