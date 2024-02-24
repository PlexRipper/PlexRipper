using Data.Contracts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public record GetAllNotificationsQuery : IRequest<Result<List<Notification>>> { }

public class GetAllNotificationsValidator : AbstractValidator<GetAllNotificationsQuery> { }

public class GetAllNotificationsHandler : IRequestHandler<GetAllNotificationsQuery, Result<List<Notification>>>
{
    private readonly IPlexRipperDbContext _dbContext;

    public GetAllNotificationsHandler(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<Notification>>> Handle(GetAllNotificationsQuery request, CancellationToken cancellationToken)
    {
        var list = await _dbContext.Notifications.ToListAsync(cancellationToken);
        return Result.Ok(list);
    }
}