using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.PlexServers;
using PlexRipper.Data.Common;

namespace PlexRipper.Data.CQRS.PlexServers
{
    public class TrimPlexServerStatusCommandHandlerValidator : AbstractValidator<TrimPlexServerStatusCommand> { }

    public class TrimPlexServerStatusCommandHandler : BaseHandler, IRequestHandler<TrimPlexServerStatusCommand, Result<bool>>
    {
        public TrimPlexServerStatusCommandHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<bool>> Handle(TrimPlexServerStatusCommand command, CancellationToken cancellationToken)
        {
            if (command.PlexServerId > 0)
            {
                var serverStatusList = await _dbContext
                    .PlexServerStatuses
                    .AsTracking()
                    .Where(x => x.PlexServerId == command.PlexServerId)
                    .ToListAsync();
                if (serverStatusList.Count > 100)
                {
                    // All server status are stored chronologically, which means instead of sorting by LastChecked we can do sort by Id.
                    serverStatusList = serverStatusList.OrderByDescending(x => x.Id).ToList();
                    _dbContext.PlexServerStatuses.RemoveRange(serverStatusList.Skip(100));
                    await SaveChangesAsync();
                }
            }
            else
            {
                var serverStatusList = await _dbContext
                    .PlexServerStatuses
                    .AsTracking()
                    .ToListAsync();

                var uniquePlexServerIds = serverStatusList.Select(x => x.PlexServerId).Distinct().ToList();
                if (uniquePlexServerIds.Any())
                {
                    foreach (var plexServerId in uniquePlexServerIds)
                    {
                        var statusList = serverStatusList.Where(x => x.PlexServerId == plexServerId).ToList();
                        if (statusList.Count > 100)
                        {
                            // All server status are stored chronologically, which means instead of sorting by LastChecked we can do sort by Id.
                            serverStatusList = statusList.OrderByDescending(x => x.Id).ToList();
                            _dbContext.PlexServerStatuses.RemoveRange(serverStatusList.Skip(100));
                        }
                    }

                    await SaveChangesAsync();
                }
            }

            return Result.Ok(true);
        }
    }
}