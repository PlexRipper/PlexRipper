using Data.Contracts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Data.Common;

namespace PlexRipper.Data;

public class ClearAllNotificationsValidator : AbstractValidator<ClearAllNotificationsCommand>
{
    public ClearAllNotificationsValidator() { }
}

public class ClearAllNotificationsHandler : BaseHandler, IRequestHandler<ClearAllNotificationsCommand, Result<int>>
{
    public ClearAllNotificationsHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

    public async Task<Result<int>> Handle(ClearAllNotificationsCommand command, CancellationToken cancellationToken)
    {
        // Empty the table
        var deletedNotificationsCount = await _dbContext.Notifications.ExecuteDeleteAsync(cancellationToken);
        return Result.Ok(deletedNotificationsCount);
    }
}