using Data.Contracts;
using FluentValidation;

namespace PlexRipper.Application;

public record GetPlexServerByIdQuery(int PlexServerId) : IRequest<Result<PlexServer>>;

public class GetPlexServerByIdQueryValidator : AbstractValidator<GetPlexServerByIdQuery>
{
    public GetPlexServerByIdQueryValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
    }
}

public class GetPlexServerByIdQueryHandler : IRequestHandler<GetPlexServerByIdQuery, Result<PlexServer>>
{
    private readonly IPlexRipperDbContext _dbContext;

    public GetPlexServerByIdQueryHandler(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PlexServer>> Handle(GetPlexServerByIdQuery request, CancellationToken cancellationToken)
    {
        var plexServer = await _dbContext.PlexServers.IncludeConnections().GetAsync(request.PlexServerId, cancellationToken);

        if (plexServer is null)
            return ResultExtensions.EntityNotFound(nameof(PlexServer), request.PlexServerId);

        return Result.Ok(plexServer);
    }
}