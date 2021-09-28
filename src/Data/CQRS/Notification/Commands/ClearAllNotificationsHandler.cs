using System;
using System.Threading;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Notifications;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.CQRS
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