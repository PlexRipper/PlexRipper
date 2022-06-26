using EFCore.BulkExtensions;
using FluentResults;
using FluentValidation;
using MediatR;
using PlexRipper.Application.Notifications;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data
{
    public class ClearAllNotificationsValidator : AbstractValidator<ClearAllNotificationsCommand>
    {
        public ClearAllNotificationsValidator() { }
    }

    public class ClearAllNotificationsHandler : BaseHandler, IRequestHandler<ClearAllNotificationsCommand, Result>
    {
        public ClearAllNotificationsHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result> Handle(ClearAllNotificationsCommand command, CancellationToken cancellationToken)
        {
            // Empty table
            await _dbContext.TruncateAsync<Notification>();
            await _dbContext.SaveChangesAsync();
            return Result.Ok();
        }
    }
}