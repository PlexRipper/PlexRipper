using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Data.Common;

namespace PlexRipper.Data;

public class HideNotificationValidator : AbstractValidator<HideNotificationCommand>
{
    public HideNotificationValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class HideNotificationHandler : BaseHandler, IRequestHandler<HideNotificationCommand, Result>
{
    public HideNotificationHandler(ILog log, PlexRipperDbContext dbContext) : base(log, dbContext) { }

    public async Task<Result> Handle(HideNotificationCommand command, CancellationToken cancellationToken)
    {
        var notification = _dbContext.Notifications.AsTracking().FirstOrDefault(x => x.Id == command.Id);
        if (notification == null)
            return ResultExtensions.EntityNotFound(nameof(Notification), command.Id);

        notification.Hidden = true;
        await SaveChangesAsync();
        return Result.Ok();
    }
}