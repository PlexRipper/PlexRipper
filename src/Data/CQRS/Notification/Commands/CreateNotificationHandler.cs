using System;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using PlexRipper.Application.Notifications;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.CQRS
{
    public class CreateNotificationValidator : AbstractValidator<CreateNotificationCommand>
    {
        public CreateNotificationValidator()
        {
            RuleFor(x => x.Notification).NotNull();
            RuleFor(x => x.Notification.Message).NotEmpty();
            RuleFor(x => x.Notification.Level).NotEqual(NotificationLevel.None);
            RuleFor(x => x.Notification.CreatedAt).NotEqual(DateTime.MinValue);
        }
    }

    public class CreateNotificationHandler : BaseHandler, IRequestHandler<CreateNotificationCommand, Result<int>>
    {
        public CreateNotificationHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<int>> Handle(CreateNotificationCommand command, CancellationToken cancellationToken)
        {
            await _dbContext.Notifications.AddAsync(command.Notification);
            await _dbContext.SaveChangesAsync();
            return Result.Ok(command.Notification.Id);
        }
    }
}