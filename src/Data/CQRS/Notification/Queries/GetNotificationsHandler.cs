using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Data.Common;

namespace PlexRipper.Data.Queries;

public class GetNotificationsValidator : AbstractValidator<GetNotificationsQuery> { }

public class GetNotificationsHandler : BaseHandler, IRequestHandler<GetNotificationsQuery, Result<List<Notification>>>
{
    public GetNotificationsHandler(ILog log, PlexRipperDbContext dbContext) : base(log, dbContext) { }

    public async Task<Result<List<Notification>>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var list = await _dbContext.Notifications.ToListAsync(cancellationToken);
        return Result.Ok(list);
    }
}