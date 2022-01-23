using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Notifications.Queries;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.Queries
{
    public class GetNotificationsValidator : AbstractValidator<GetNotificationsQuery> { }

    public class GetNotificationsHandler : BaseHandler, IRequestHandler<GetNotificationsQuery, Result<List<Notification>>>
    {
        public GetNotificationsHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<List<Notification>>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
        {
            var list = await _dbContext.Notifications.ToListAsync();
            return Result.Ok(list);
        }
    }
}