using Data.Contracts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public record GetPlexServerConnectionByIdQuery(int Id) : IRequest<Result<PlexServerConnection>>;

public class GetPlexServerConnectionByIdQueryValidator : AbstractValidator<GetPlexServerConnectionByIdQuery>
{
    public GetPlexServerConnectionByIdQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class GetPlexServerConnectionByIdQueryHandler : IRequestHandler<GetPlexServerConnectionByIdQuery, Result<PlexServerConnection>>
{
    private readonly IPlexRipperDbContext _dbContext;

    public GetPlexServerConnectionByIdQueryHandler(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PlexServerConnection>> Handle(GetPlexServerConnectionByIdQuery request, CancellationToken cancellationToken)
    {
        var plexServerConnection = await _dbContext
            .PlexServerConnections
            .Include(x => x.PlexServerStatus.OrderByDescending(y => y.LastChecked).Take(5))
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (plexServerConnection == null)
            return ResultExtensions.EntityNotFound(nameof(PlexServerConnection), request.Id);

        return Result.Ok(plexServerConnection);
    }
}